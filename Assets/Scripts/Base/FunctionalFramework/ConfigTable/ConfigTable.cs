using System.Collections.Generic;

namespace Functional.ConfigTable
{
    /// <summary>
    /// 单张配置表的运行时容器。以 Id 为 key 索引。
    /// </summary>
    public class ConfigTable<T> where T : class, IConfigRow
    {
        private readonly Dictionary<int, T> _rows = new Dictionary<int, T>();
        private readonly List<T> _list = new List<T>();

        /// <summary>所有行（只读列表）。</summary>
        public IReadOnlyList<T> All => _list;

        /// <summary>行数量。</summary>
        public int Count => _list.Count;

        /// <summary>
        /// 从列表构建表。
        /// </summary>
        public void Build(IEnumerable<T> rows)
        {
            _rows.Clear();
            _list.Clear();
            foreach (var row in rows)
            {
                if (row == null) continue;
                _rows[row.Id] = row;
                _list.Add(row);
            }
        }

        /// <summary>
        /// 按 Id 获取配置，不存在返回 null。
        /// </summary>
        public T Get(int id)
        {
            _rows.TryGetValue(id, out var row);
            return row;
        }

        /// <summary>
        /// 尝试按 Id 获取配置。
        /// </summary>
        public bool TryGet(int id, out T row) => _rows.TryGetValue(id, out row);
    }
}
