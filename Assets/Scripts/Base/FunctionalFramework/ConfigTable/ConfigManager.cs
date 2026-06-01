using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Base.Common;
using Base.ResourceFramework;

namespace Functional.ConfigTable
{
    /// <summary>
    /// 配置表管理总入口。从 Resources 或 TextAsset 加载 JSON/CSV 配置。
    /// 
    /// 典型用法：
    ///   ConfigManager.Instance.LoadAll();
    ///   var hero = ConfigManager.Instance.GetTable<HeroConfig>().Get(1001);
    /// </summary>
    public class ConfigManager : Singleton<ConfigManager>
    {
        private readonly Dictionary<Type, object> _tables = new Dictionary<Type, object>();

        /// <summary>
        /// 加载所有带 [Config] 特性的配置表类型。
        /// </summary>
        public void LoadAll()
        {
            foreach (var type in FindConfigTypes())
            {
                Load(type);
            }
        }

        /// <summary>
        /// 加载指定类型的配置表。
        /// </summary>
        public ConfigTable<T> Load<T>() where T : class, IConfigRow, new()
        {
            return Load(typeof(T)) as ConfigTable<T>;
        }

        /// <summary>
        /// 获取已加载的配置表。
        /// </summary>
        public ConfigTable<T> GetTable<T>() where T : class, IConfigRow
        {
            if (_tables.TryGetValue(typeof(T), out var table))
            {
                return table as ConfigTable<T>;
            }
            Debug.LogWarning($"[ConfigManager] 配置表未加载：{typeof(T).Name}");
            return null;
        }

        /// <summary>
        /// 卸载所有配置表。
        /// </summary>
        public void UnloadAll()
        {
            _tables.Clear();
        }

        private object Load(Type rowType)
        {
            var attr = rowType.GetCustomAttribute<ConfigAttribute>();
            if (attr == null)
            {
                Debug.LogError($"[ConfigManager] {rowType.Name} 缺少 [Config] 特性");
                return null;
            }

            var text = LoadText(attr.Path);
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogError($"[ConfigManager] 找不到配置：{attr.Path}");
                return null;
            }

            var list = ParseRows(rowType, text, attr.Format);
            var tableType = typeof(ConfigTable<>).MakeGenericType(rowType);
            var table = Activator.CreateInstance(tableType);
            var buildMethod = tableType.GetMethod("Build");
            buildMethod.Invoke(table, new object[] { list });

            _tables[rowType] = table;
            Debug.Log($"[ConfigManager] 已加载 {rowType.Name}，共 {((System.Collections.ICollection)list).Count} 行");
            return table;
        }

        private static List<object> ParseRows(Type rowType, string text, ConfigFormat format)
        {
            if (format == ConfigFormat.Json)
            {
                return ParseJsonList(rowType, text);
            }
            return ParseCsvList(rowType, text);
        }

        private static List<object> ParseJsonList(Type rowType, string text)
        {
            var wrapperType = typeof(JsonListWrapper<>).MakeGenericType(rowType);
            var wrapper = JsonUtility.FromJson(text, wrapperType);
            if (wrapper == null)
            {
                var single = JsonUtility.FromJson(text, rowType);
                var list = new List<object>();
                if (single != null) list.Add(single);
                return list;
            }
            var itemsField = wrapperType.GetField("items");
            var arr = itemsField.GetValue(wrapper) as Array;
            var result = new List<object>();
            if (arr != null)
            {
                foreach (var item in arr) result.Add(item);
            }
            return result;
        }

        private static List<object> ParseCsvList(Type rowType, string text)
        {
            var method = typeof(CsvParser).GetMethod("Parse").MakeGenericMethod(rowType);
            var list = method.Invoke(null, new object[] { text });
            var result = new List<object>();
            foreach (var item in (System.Collections.IEnumerable)list)
            {
                result.Add(item);
            }
            return result;
        }

        private static string LoadText(string path)
        {
            if (ResourceManager.HasInstance)
            {
                var asset = ResourceManager.Instance.Load<TextAsset>(path);
                if (asset != null) return asset.text;
            }
            var ta = Resources.Load<TextAsset>(path);
            return ta != null ? ta.text : null;
        }

        private static IEnumerable<Type> FindConfigTypes()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch { continue; }
                foreach (var t in types)
                {
                    if (t.IsClass && !t.IsAbstract && typeof(IConfigRow).IsAssignableFrom(t)
                        && t.GetCustomAttribute<ConfigAttribute>() != null)
                    {
                        yield return t;
                    }
                }
            }
        }

        [Serializable]
        private class JsonListWrapper<T>
        {
            public T[] items;
        }
    }
}
