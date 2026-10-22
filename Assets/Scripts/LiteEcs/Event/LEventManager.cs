using System;
using System.Collections.Generic;

namespace LiteEcs
{
    /// <summary>
    /// 世界内的同步事件，按事件类型 T 分发。
    /// - 同一个监听重复注册会被忽略（委托按目标对象 + 方法判等，方法组每次传进来都能对上）。
    /// - 派发前拷贝当前监听名单：派发中移除的监听本轮仍会被调用，新加的本轮不调用。
    /// - 派发中同类型事件再次触发，会按当时的名单立刻再跑一轮（可重入）。
    /// - Release 后正在进行的派发立刻停止。
    /// - 每个监听单独 try/catch。
    /// </summary>
    public sealed class LEventManager
    {
        private sealed class ListenerList
        {
            public readonly List<Delegate> Items = new List<Delegate>();
            public readonly Dictionary<Delegate, int> Index = new Dictionary<Delegate, int>();
        }

        private Dictionary<Type, ListenerList> _listeners = new Dictionary<Type, ListenerList>();
        private readonly Stack<List<Delegate>> _snapshotPool = new Stack<List<Delegate>>();
        private int _epoch;

        public int ListenerCount<T>()
        {
            return _listeners.TryGetValue(typeof(T), out var list) ? list.Items.Count : 0;
        }

        public void AddListener<T>(Action<T> listener)
        {
            if (listener == null)
            {
                EcsLog.Warn("AddListener null listener for " + typeof(T).Name);
                return;
            }
            if (!_listeners.TryGetValue(typeof(T), out var list))
            {
                list = new ListenerList();
                _listeners.Add(typeof(T), list);
            }
            if (list.Index.ContainsKey(listener))
            {
                return;
            }
            list.Index.Add(listener, list.Items.Count);
            list.Items.Add(listener);
        }

        public void RemoveListener<T>(Action<T> listener)
        {
            if (listener == null)
            {
                EcsLog.Warn("RemoveListener null listener for " + typeof(T).Name);
                return;
            }
            if (!_listeners.TryGetValue(typeof(T), out var list))
            {
                return;
            }
            if (!list.Index.TryGetValue(listener, out var index))
            {
                return;
            }
            int last = list.Items.Count - 1;
            if (index != last)
            {
                var moved = list.Items[last];
                list.Items[index] = moved;
                list.Index[moved] = index;
            }
            list.Items.RemoveAt(last);
            list.Index.Remove(listener);
        }

        public void Trigger<T>(T evt)
        {
            if (!_listeners.TryGetValue(typeof(T), out var list))
            {
                return;
            }
            int count = list.Items.Count;
            if (count == 0)
            {
                return;
            }
            if (count == 1)
            {
                Invoke((Action<T>)list.Items[0], evt);
                return;
            }
            var snapshot = _snapshotPool.Count > 0 ? _snapshotPool.Pop() : new List<Delegate>(count);
            snapshot.AddRange(list.Items);
            int epoch = _epoch;
            for (int i = 0; i < count; i++)
            {
                if (_epoch != epoch)
                {
                    break;
                }
                Invoke((Action<T>)snapshot[i], evt);
            }
            snapshot.Clear();
            _snapshotPool.Push(snapshot);
        }

        /// <summary>
        /// 清空全部监听，不回调。
        /// </summary>
        public void Release()
        {
            _epoch++;
            _listeners = new Dictionary<Type, ListenerList>();
        }

        private static void Invoke<T>(Action<T> listener, T evt)
        {
            try
            {
                listener(evt);
            }
            catch (Exception ex)
            {
                EcsLog.LogException(ex);
            }
        }
    }
}
