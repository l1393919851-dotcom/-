using System.Collections.Generic;
using UnityEngine;

namespace Base.ResourceFramework
{
    /// <summary>
    /// GameObject 对象池。
    /// 用于频繁创建销毁的对象（子弹、特效、列表项等）。
    /// 池内对象会被设为 inactive 并挂在统一的 PoolRoot 节点下。
    /// </summary>
    public class GameObjectPool
    {
        private readonly Dictionary<string, Queue<GameObject>> _pool = new Dictionary<string, Queue<GameObject>>();
        private readonly Transform _poolRoot;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="poolRoot">池中对象的统一父节点，建议是一个 inactive 的空 GameObject。</param>
        public GameObjectPool(Transform poolRoot)
        {
            _poolRoot = poolRoot;
        }

        /// <summary>
        /// 从池中取出一个对象。如果池中没有，则根据 prefab 实例化一个新的。
        /// </summary>
        /// <param name="key">池的 key，通常用资源路径或预制体名称。</param>
        /// <param name="prefab">资源原型，池空时用来 Instantiate。</param>
        /// <param name="parent">取出后挂到哪里。</param>
        public GameObject Spawn(string key, GameObject prefab, Transform parent = null)
        {
            GameObject go;
            if (_pool.TryGetValue(key, out var queue) && queue.Count > 0)
            {
                go = queue.Dequeue();
            }
            else
            {
                go = Object.Instantiate(prefab);
                go.name = prefab.name;
            }
            go.transform.SetParent(parent, false);
            go.SetActive(true);
            return go;
        }

        /// <summary>
        /// 把对象回收到池中。对象会被 SetActive(false) 并挂到 PoolRoot。
        /// </summary>
        /// <param name="key">池 key（必须与 Spawn 时一致）。</param>
        /// <param name="go">要回收的对象。</param>
        public void Despawn(string key, GameObject go)
        {
            if (go == null) return;
            go.SetActive(false);
            go.transform.SetParent(_poolRoot, false);
            if (!_pool.TryGetValue(key, out var queue))
            {
                queue = new Queue<GameObject>();
                _pool[key] = queue;
            }
            queue.Enqueue(go);
        }

        /// <summary>
        /// 预热：提前创建 count 个对象放到池中。常用于战斗开始前提前实例化好子弹。
        /// </summary>
        public void Prewarm(string key, GameObject prefab, int count)
        {
            if (!_pool.TryGetValue(key, out var queue))
            {
                queue = new Queue<GameObject>();
                _pool[key] = queue;
            }
            for (int i = 0; i < count; i++)
            {
                var go = Object.Instantiate(prefab, _poolRoot, false);
                go.name = prefab.name;
                go.SetActive(false);
                queue.Enqueue(go);
            }
        }

        /// <summary>
        /// 清空指定 key 的池，把所有缓存对象销毁。
        /// </summary>
        public void Clear(string key)
        {
            if (_pool.TryGetValue(key, out var queue))
            {
                while (queue.Count > 0)
                {
                    var go = queue.Dequeue();
                    if (go != null) Object.Destroy(go);
                }
                _pool.Remove(key);
            }
        }

        /// <summary>
        /// 清空整个对象池（一般在切场景时调用）。
        /// </summary>
        public void ClearAll()
        {
            foreach (var queue in _pool.Values)
            {
                while (queue.Count > 0)
                {
                    var go = queue.Dequeue();
                    if (go != null) Object.Destroy(go);
                }
            }
            _pool.Clear();
        }
    }
}
