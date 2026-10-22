using System;

namespace LiteEcs
{
    /// <summary>
    /// 状态图上的转移规则。加入图时注册监听，图 Clear 时反注册。
    /// 一张图里每种 EventKey 只能有一条规则。
    /// </summary>
    public abstract class StateGraphEvent
    {
        public abstract Type EventKey { get; }

        public StateGraph Graph { get; internal set; }

        protected internal virtual void OnRegister()
        {
        }

        protected internal virtual void OnUnregister()
        {
        }
    }

    /// <summary>
    /// 监听世界事件 TEvent，在 OnEvent 里决定把事件里的实体切到哪个状态。
    /// 例：收到点击事件 → entity.GetComponent&lt;StateGraphComponent&gt;().Goto("focused")。
    /// </summary>
    public abstract class StateGraphEvent<TEvent> : StateGraphEvent
    {
        private Action<TEvent> _handler;

        public override Type EventKey => typeof(TEvent);

        protected internal override void OnRegister()
        {
            if (_handler == null)
            {
                _handler = OnEvent;
            }
            Graph.World.EventManager.AddListener(_handler);
        }

        protected internal override void OnUnregister()
        {
            if (_handler != null)
            {
                Graph.World.EventManager.RemoveListener(_handler);
            }
        }

        protected abstract void OnEvent(TEvent evt);
    }
}
