using System;
using System.Collections.Generic;

namespace LiteEcs
{
    /// <summary>
    /// 按运行时类型分池。实体、组件、池化事件都从这里取和还。
    /// 回收时按对象的实际类型找池，所以子类对象会进子类自己的池。
    /// </summary>
    public sealed class LObjectPoolManager
    {
        private readonly Dictionary<Type, LObjectPool> _pools = new Dictionary<Type, LObjectPool>();

        public T Take<T>() where T : class, IPoolable, new()
        {
            if (!_pools.TryGetValue(typeof(T), out var pool))
            {
                pool = new LObjectPool(typeof(T), () => new T());
                _pools.Add(typeof(T), pool);
            }
            return (T)pool.Take();
        }

        public void Recycle(IPoolable obj)
        {
            if (obj == null)
            {
                return;
            }
            var type = obj.GetType();
            if (!_pools.TryGetValue(type, out var pool))
            {
                pool = new LObjectPool(type, () => (IPoolable)Activator.CreateInstance(type));
                _pools.Add(type, pool);
            }
            pool.Recycle(obj);
        }

        public void ClearPool<T>()
        {
            if (_pools.TryGetValue(typeof(T), out var pool))
            {
                pool.Clear();
            }
        }

        public void Dispose()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
            _pools.Clear();
        }
    }
}
