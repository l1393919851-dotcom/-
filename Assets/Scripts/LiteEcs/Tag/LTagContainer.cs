using System;
using System.Collections.Generic;

namespace LiteEcs
{
    /// <summary>
    /// 引用计数的字符串标签集合。Add 两次要 Remove 两次才真正去掉，
    /// 方便多个来源（状态节点、玩法逻辑）各自加减同一个标签而互不干扰。
    /// </summary>
    public sealed class LTagContainer
    {
        public int Count => _tags.Count;

        private readonly Dictionary<string, int> _tags = new Dictionary<string, int>(StringComparer.Ordinal);

        public LTagContainer()
        {
        }

        public LTagContainer(IReadOnlyList<string> tags)
        {
            AddTags(tags);
        }

        /// <returns>该标签当前的引用计数，没有为 0</returns>
        public int Get(string tag)
        {
            return tag != null && _tags.TryGetValue(tag, out var count) ? count : 0;
        }

        public void Add(string tag)
        {
            if (tag == null)
            {
                return;
            }
            _tags.TryGetValue(tag, out var count);
            _tags[tag] = count + 1;
        }

        public void AddTags(IReadOnlyList<string> tags)
        {
            if (tags == null)
            {
                return;
            }
            for (int i = 0; i < tags.Count; i++)
            {
                Add(tags[i]);
            }
        }

        public void Remove(string tag)
        {
            if (tag == null || !_tags.TryGetValue(tag, out var count))
            {
                return;
            }
            if (count <= 1)
            {
                _tags.Remove(tag);
            }
            else
            {
                _tags[tag] = count - 1;
            }
        }

        public void RemoveTags(IReadOnlyList<string> tags)
        {
            if (tags == null)
            {
                return;
            }
            for (int i = 0; i < tags.Count; i++)
            {
                Remove(tags[i]);
            }
        }

        public bool Has(string tag)
        {
            return tag != null && _tags.ContainsKey(tag);
        }

        public bool HasAny(IReadOnlyList<string> tags)
        {
            for (int i = 0; i < tags.Count; i++)
            {
                if (Has(tags[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasAll(IReadOnlyList<string> tags)
        {
            for (int i = 0; i < tags.Count; i++)
            {
                if (!Has(tags[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 不管计数，直接去掉这个标签。
        /// </summary>
        public void Clear(string tag)
        {
            if (tag != null)
            {
                _tags.Remove(tag);
            }
        }

        public void ClearAll()
        {
            _tags.Clear();
        }
    }
}
