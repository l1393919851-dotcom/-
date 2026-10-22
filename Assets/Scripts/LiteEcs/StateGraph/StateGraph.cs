using System;
using System.Collections.Generic;

namespace LiteEcs
{
    /// <summary>
    /// 状态图：一组节点 + 一组转移规则。同一世界里同一类实体共用一张图，当前状态存在各自的 StateGraphComponent 上。
    /// 子类在构造函数里 AddNode / AddEvent 即可。
    /// </summary>
    public class StateGraph
    {
        private readonly Dictionary<string, StateGraphNode> _nodes = new Dictionary<string, StateGraphNode>();
        private readonly Dictionary<Type, StateGraphEvent> _events = new Dictionary<Type, StateGraphEvent>();

        public LWorld World { get; }

        public StateGraph(LWorld world)
        {
            World = world;
        }

        public void AddNode(StateGraphNode node)
        {
            node.Graph = this;
            _nodes[node.State] = node;
        }

        /// <summary>
        /// 加入并立刻注册监听。同 EventKey 已有规则时先反注册旧的。
        /// </summary>
        public void AddEvent(StateGraphEvent evt)
        {
            if (_events.TryGetValue(evt.EventKey, out var old))
            {
                old.OnUnregister();
            }
            evt.Graph = this;
            _events[evt.EventKey] = evt;
            evt.OnRegister();
        }

        public StateGraphNode GetNode(string state)
        {
            return state != null && _nodes.TryGetValue(state, out var node) ? node : null;
        }

        public StateGraphEvent GetEvent(Type eventKey)
        {
            return _events.TryGetValue(eventKey, out var evt) ? evt : null;
        }

        /// <summary>
        /// 反注册全部规则并清空节点。
        /// </summary>
        public void Clear()
        {
            foreach (var evt in _events.Values)
            {
                try
                {
                    evt.OnUnregister();
                }
                catch (Exception ex)
                {
                    EcsLog.LogException(ex);
                }
            }
            _events.Clear();
            _nodes.Clear();
        }
    }
}
