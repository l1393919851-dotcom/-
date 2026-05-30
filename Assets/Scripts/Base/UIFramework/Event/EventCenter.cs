using System;
using System.Collections.Generic;
using UnityEngine;
using Base.Common;

namespace Base.UIFramework.Event
{
    /// <summary>
    /// 全局事件中心。提供 string 类型事件名的发布/订阅机制。
    /// 适合解耦"数据变化 → UI 刷新"、"按钮点击 → 业务逻辑"等跨模块通信。
    /// 
    /// 用法：
    ///   EventCenter.Instance.Subscribe("OnGoldChange", OnGoldChange);
    ///   EventCenter.Instance.Publish("OnGoldChange", 100);
    ///   EventCenter.Instance.Unsubscribe("OnGoldChange", OnGoldChange);
    /// </summary>
    public class EventCenter : Singleton<EventCenter>
    {
        private readonly Dictionary<string, Delegate> _events = new Dictionary<string, Delegate>();

        /// <summary>
        /// 订阅无参事件。
        /// </summary>
        public void Subscribe(string eventName, Action callback)
        {
            AddHandler(eventName, callback);
        }

        /// <summary>
        /// 订阅带 1 个参数的事件。
        /// </summary>
        public void Subscribe<T>(string eventName, Action<T> callback)
        {
            AddHandler(eventName, callback);
        }

        /// <summary>
        /// 订阅带 2 个参数的事件。
        /// </summary>
        public void Subscribe<T1, T2>(string eventName, Action<T1, T2> callback)
        {
            AddHandler(eventName, callback);
        }

        /// <summary>
        /// 取消订阅无参事件。
        /// </summary>
        public void Unsubscribe(string eventName, Action callback)
        {
            RemoveHandler(eventName, callback);
        }

        /// <summary>
        /// 取消订阅带 1 个参数的事件。
        /// </summary>
        public void Unsubscribe<T>(string eventName, Action<T> callback)
        {
            RemoveHandler(eventName, callback);
        }

        /// <summary>
        /// 取消订阅带 2 个参数的事件。
        /// </summary>
        public void Unsubscribe<T1, T2>(string eventName, Action<T1, T2> callback)
        {
            RemoveHandler(eventName, callback);
        }

        /// <summary>
        /// 发布无参事件。
        /// </summary>
        public void Publish(string eventName)
        {
            if (_events.TryGetValue(eventName, out var d) && d is Action action)
            {
                try { action.Invoke(); }
                catch (Exception e) { UnityEngine.Debug.LogException(e); }
            }
        }

        /// <summary>
        /// 发布带 1 个参数的事件。
        /// </summary>
        public void Publish<T>(string eventName, T arg)
        {
            if (_events.TryGetValue(eventName, out var d) && d is Action<T> action)
            {
                try { action.Invoke(arg); }
                catch (Exception e) { UnityEngine.Debug.LogException(e); }
            }
        }

        /// <summary>
        /// 发布带 2 个参数的事件。
        /// </summary>
        public void Publish<T1, T2>(string eventName, T1 arg1, T2 arg2)
        {
            if (_events.TryGetValue(eventName, out var d) && d is Action<T1, T2> action)
            {
                try { action.Invoke(arg1, arg2); }
                catch (Exception e) { UnityEngine.Debug.LogException(e); }
            }
        }

        /// <summary>
        /// 清空所有事件（一般在切场景或关闭框架时使用）。
        /// </summary>
        public void Clear()
        {
            _events.Clear();
        }

        // ----------------- 内部实现 -----------------

        /// <summary>
        /// 内部：添加事件回调（合并到现有 Delegate）。
        /// </summary>
        private void AddHandler(string eventName, Delegate callback)
        {
            if (callback == null) return;
            _events.TryGetValue(eventName, out var d);
            _events[eventName] = Delegate.Combine(d, callback);
        }

        /// <summary>
        /// 内部：移除事件回调，全部移除后清空 key。
        /// </summary>
        private void RemoveHandler(string eventName, Delegate callback)
        {
            if (callback == null) return;
            if (!_events.TryGetValue(eventName, out var d)) return;
            var newDelegate = Delegate.Remove(d, callback);
            if (newDelegate == null) _events.Remove(eventName);
            else _events[eventName] = newDelegate;
        }
    }
}
