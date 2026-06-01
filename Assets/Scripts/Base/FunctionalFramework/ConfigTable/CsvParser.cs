using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Functional.ConfigTable
{
    /// <summary>
    /// 简易 CSV 解析器。第一行为表头，后续为数据行。
    /// 支持用 [CsvColumn("列名")] 映射字段，或按字段名匹配表头。
    /// </summary>
    public static class CsvParser
    {
        /// <summary>
        /// 将 CSV 文本解析为配置行列表。
        /// </summary>
        public static List<T> Parse<T>(string csvText) where T : class, IConfigRow, new()
        {
            var result = new List<T>();
            if (string.IsNullOrWhiteSpace(csvText)) return result;

            var lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) return result;

            var headers = SplitLine(lines[0]);
            var headerIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                headerIndex[headers[i].Trim()] = i;
            }

            var type = typeof(T);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int row = 1; row < lines.Length; row++)
            {
                var cells = SplitLine(lines[row]);
                if (cells.Length == 0) continue;
                var item = new T();
                FillFields(item, fields, props, headerIndex, cells);
                result.Add(item);
            }
            return result;
        }

        private static void FillFields(object item, FieldInfo[] fields, PropertyInfo[] props,
            Dictionary<string, int> headerIndex, string[] cells)
        {
            foreach (var f in fields)
            {
                if (f.Name == "Id" || f.GetCustomAttribute<CsvColumnAttribute>() != null)
                {
                    var colName = f.GetCustomAttribute<CsvColumnAttribute>()?.ColumnName ?? f.Name;
                    if (headerIndex.TryGetValue(colName, out int idx) && idx < cells.Length)
                    {
                        SetValue(item, f.FieldType, f.Name, cells[idx], isField: true, f, null);
                    }
                }
            }
            foreach (var p in props)
            {
                if (!p.CanWrite) continue;
                var colName = p.GetCustomAttribute<CsvColumnAttribute>()?.ColumnName ?? p.Name;
                if (headerIndex.TryGetValue(colName, out int idx) && idx < cells.Length)
                {
                    SetValue(item, p.PropertyType, p.Name, cells[idx], isField: false, null, p);
                }
            }
        }

        private static void SetValue(object item, Type type, string name, string raw, bool isField, FieldInfo field, PropertyInfo prop)
        {
            try
            {
                object val = ConvertValue(type, raw);
                if (isField) field.SetValue(item, val);
                else prop.SetValue(item, val);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CsvParser] 解析失败 {item.GetType().Name}.{name} = '{raw}': {e.Message}");
            }
        }

        private static object ConvertValue(Type type, string raw)
        {
            if (type == typeof(int)) return int.Parse(raw);
            if (type == typeof(float)) return float.Parse(raw);
            if (type == typeof(double)) return double.Parse(raw);
            if (type == typeof(bool)) return raw == "1" || raw.Equals("true", StringComparison.OrdinalIgnoreCase);
            if (type == typeof(string)) return raw;
            return Convert.ChangeType(raw, type);
        }

        private static string[] SplitLine(string line)
        {
            return line.Split(',');
        }
    }

    /// <summary>
    /// CSV 列名映射。字段名与表头不一致时使用。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class CsvColumnAttribute : Attribute
    {
        public string ColumnName { get; }
        public CsvColumnAttribute(string columnName) => ColumnName = columnName;
    }
}
