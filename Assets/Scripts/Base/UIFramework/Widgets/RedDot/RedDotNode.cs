using System;
using System.Collections.Generic;

namespace Base.UIFramework.Widgets
{
    /// <summary>
    /// 红点节点。组成树形结构：
    /// - 叶子节点（如"未领取邮件"）的红点数由业务直接设置
    /// - 父节点（如"邮件"）的红点数 = 所有子节点之和
    /// - 任何节点的红点变化都会自动冒泡到父节点
    /// </summary>
    public class RedDotNode
    {
        /// <summary>节点 key（如 "Mail/Reward"）。</summary>
        public string Key { get; private set; }

        /// <summary>父节点，可为 null（根节点）。</summary>
        public RedDotNode Parent { get; private set; }

        /// <summary>红点数。> 0 表示需要显示红点。</summary>
        public int Count { get; private set; }

        /// <summary>红点数变化回调。</summary>
        public event Action<int> OnChanged;

        private readonly Dictionary<string, RedDotNode> _children = new Dictionary<string, RedDotNode>();
        private int _selfCount;

        /// <summary>
        /// 构造函数。一般通过 RedDotManager 调用而不是直接 new。
        /// </summary>
        public RedDotNode(string key, RedDotNode parent)
        {
            Key = key;
            Parent = parent;
        }

        /// <summary>
        /// 设置当前节点自身的红点数（不影响子节点的合计）。
        /// </summary>
        public void SetSelfCount(int count)
        {
            if (count < 0) count = 0;
            if (_selfCount == count) return;
            _selfCount = count;
            Recalculate();
        }

        /// <summary>
        /// 添加一个子节点。
        /// </summary>
        public RedDotNode AddChild(string key)
        {
            if (_children.TryGetValue(key, out var exist)) return exist;
            var node = new RedDotNode(key, this);
            _children[key] = node;
            node.OnChanged += _ => Recalculate();
            return node;
        }

        /// <summary>
        /// 获取子节点（不存在返回 null）。
        /// </summary>
        public RedDotNode GetChild(string key)
        {
            _children.TryGetValue(key, out var node);
            return node;
        }

        /// <summary>
        /// 重新计算红点数 = 自身红点 + 所有子节点的红点总和。
        /// </summary>
        private void Recalculate()
        {
            int total = _selfCount;
            foreach (var c in _children.Values) total += c.Count;
            if (Count == total) return;
            Count = total;
            try { OnChanged?.Invoke(Count); }
            catch (Exception e) { UnityEngine.Debug.LogException(e); }
        }
    }
}
