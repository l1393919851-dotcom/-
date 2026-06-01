using System.Collections.Generic;
using UnityEngine;

namespace Functional.ECS
{
    /// <summary>
    /// ECS 实体与 GameObject 表现层的桥接注册表。
    /// </summary>
    public class ECSViewRegistry
    {
        private readonly Dictionary<int, GameObject> _views = new Dictionary<int, GameObject>();
        private int _nextId = 1;

        /// <summary>
        /// 注册一个视图并返回 ViewId。
        /// </summary>
        public int Register(GameObject go)
        {
            int id = _nextId++;
            _views[id] = go;
            return id;
        }

        /// <summary>
        /// 获取视图 GameObject。
        /// </summary>
        public GameObject Get(int viewId)
        {
            _views.TryGetValue(viewId, out var go);
            return go;
        }

        /// <summary>
        /// 注销视图。
        /// </summary>
        public void Unregister(int viewId)
        {
            _views.Remove(viewId);
        }

        /// <summary>
        /// 清空所有视图引用。
        /// </summary>
        public void Clear() => _views.Clear();
    }
}
