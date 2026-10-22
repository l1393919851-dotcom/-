using System.Collections.Generic;

namespace LiteEcs
{
    /// <summary>
    /// 字符串键的运行时 KV 表。值类型会装箱，适合放 id、标志位这类低频读写的数据。
    /// </summary>
    public sealed class Blackboard
    {
        private readonly Dictionary<string, object> _pairs = new Dictionary<string, object>();

        /// <param name="force">false 时键已存在会报错并拒绝写入</param>
        /// <returns>是否写入成功</returns>
        public bool Write(string key, object value, bool force = false)
        {
            if (key == null)
            {
                EcsLog.Error("Blackboard key can't be null");
                return false;
            }
            if (!force && _pairs.ContainsKey(key))
            {
                EcsLog.Error("Blackboard key " + key + " is duplicated");
                return false;
            }
            _pairs[key] = value;
            return true;
        }

        /// <param name="clear">读完顺手删掉</param>
        public object Read(string key, bool clear = false)
        {
            if (key == null)
            {
                EcsLog.Error("Blackboard key can't be null");
                return null;
            }
            if (!_pairs.TryGetValue(key, out var value))
            {
                return null;
            }
            if (clear)
            {
                _pairs.Remove(key);
            }
            return value;
        }

        /// <returns>键不存在或类型不符时返回 defaultValue</returns>
        public T Read<T>(string key, T defaultValue = default)
        {
            return TryRead<T>(key, out var value) ? value : defaultValue;
        }

        public bool TryRead<T>(string key, out T value)
        {
            if (key != null && _pairs.TryGetValue(key, out var raw) && raw is T typed)
            {
                value = typed;
                return true;
            }
            value = default;
            return false;
        }

        public bool Contains(string key)
        {
            return key != null && _pairs.ContainsKey(key);
        }

        /// <summary>
        /// key 为 null 时清空全部。
        /// </summary>
        public void ClearByKey(string key)
        {
            if (key == null)
            {
                _pairs.Clear();
                return;
            }
            _pairs.Remove(key);
        }

        public void Clear()
        {
            _pairs.Clear();
        }
    }
}
