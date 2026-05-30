using System;
using System.Collections.Generic;
using Base.Common;

namespace Base.UIFramework.Widgets
{
    /// <summary>
    /// 红点系统管理器。负责管理红点节点的树状结构和路径访问。
    /// 
    /// 用法：
    ///   RedDotManager.Instance.Register("Mail/Reward");
    ///   RedDotManager.Instance.SetCount("Mail/Reward", 3);  // Mail 节点自动变成 3
    ///   RedDotManager.Instance.AddListener("Mail", count => redDotImage.SetActive(count > 0));
    /// </summary>
    public class RedDotManager : Singleton<RedDotManager>
    {
        private RedDotNode _root;

        /// <summary>
        /// 初始化：创建一个空的根节点。
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();
            _root = new RedDotNode("Root", null);
        }

        /// <summary>
        /// 注册一条红点路径（路径用 / 分隔），如 "Mail/Reward"。
        /// 中间不存在的节点会自动创建。
        /// </summary>
        public RedDotNode Register(string path)
        {
            return FindOrCreate(path);
        }

        /// <summary>
        /// 设置某个红点的数值。会自动冒泡到父节点。
        /// </summary>
        public void SetCount(string path, int count)
        {
            var node = FindOrCreate(path);
            node.SetSelfCount(count);
        }

        /// <summary>
        /// 监听某个红点的数值变化。
        /// </summary>
        public void AddListener(string path, Action<int> onChanged)
        {
            var node = FindOrCreate(path);
            node.OnChanged += onChanged;
            try { onChanged?.Invoke(node.Count); }
            catch (Exception e) { UnityEngine.Debug.LogException(e); }
        }

        /// <summary>
        /// 取消某个红点的数值变化监听。
        /// </summary>
        public void RemoveListener(string path, Action<int> onChanged)
        {
            var node = Find(path);
            if (node != null) node.OnChanged -= onChanged;
        }

        /// <summary>
        /// 查找节点，找不到返回 null。
        /// </summary>
        public RedDotNode Find(string path)
        {
            if (string.IsNullOrEmpty(path)) return _root;
            var parts = path.Split('/');
            RedDotNode node = _root;
            foreach (var p in parts)
            {
                node = node.GetChild(p);
                if (node == null) return null;
            }
            return node;
        }

        /// <summary>
        /// 查找节点，不存在则按路径自动创建。
        /// </summary>
        private RedDotNode FindOrCreate(string path)
        {
            if (string.IsNullOrEmpty(path)) return _root;
            var parts = path.Split('/');
            RedDotNode node = _root;
            foreach (var p in parts)
            {
                node = node.AddChild(p);
            }
            return node;
        }
    }
}
