using System;
using System.Collections.Generic;

namespace LiteEcs
{
    /// <summary>
    /// 单一类型的对象池。池空时用工厂新建；回收时先调 OnRecycle 再入栈。
    /// 不防重复回收，同一个对象回收两次会被取出两次。
    /// </summary>
    public sealed class LObjectPool
    {
        public int CachedCount => _caches.Count;

        private readonly Func<IPoolable> _factory;
        private readonly Stack<IPoolable> _caches = new Stack<IPoolable>();

        public Type ItemType { get; }

        public LObjectPool(Type itemType, Func<IPoolable> factory)
        {
            ItemType = itemType;
            _factory = factory;
        }

        public IPoolable Take()
        {
            return _caches.Count > 0 ? _caches.Pop() : _factory();
        }

        public void Recycle(IPoolable obj)
        {
            obj.OnRecycle();
            _caches.Push(obj);
        }

        public void Clear()
        {
            _caches.Clear();
        }
    }
}
