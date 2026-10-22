using System;
using System.Collections.Generic;

namespace LiteEcs
{
    /// <summary>
    /// 本世界的状态图注册表。按 key 注册工厂，第一次用到时创建并缓存，世界销毁时 Clear 掉全部图。
    /// key 习惯上就用实体类型标签（如 "building"），这样 StateGraphComponent 可以按标签自动找图。
    /// </summary>
    public class StateGraphSystem : LSystem
    {
        private readonly Dictionary<string, Func<LWorld, StateGraph>> _factories = new Dictionary<string, Func<LWorld, StateGraph>>();
        private readonly List<string> _keys = new List<string>();
        private readonly Dictionary<string, StateGraph> _graphs = new Dictionary<string, StateGraph>();

        /// <summary>
        /// 在实体 Start 之前注册。注册顺序决定按标签自动匹配时的优先级。
        /// </summary>
        public void Register(string graphKey, Func<LWorld, StateGraph> factory)
        {
            if (!_factories.ContainsKey(graphKey))
            {
                _keys.Add(graphKey);
            }
            _factories[graphKey] = factory;
        }

        /// <returns>没有注册该 key 时报错并返回 null</returns>
        public StateGraph Get(string graphKey)
        {
            if (_graphs.TryGetValue(graphKey, out var graph))
            {
                return graph;
            }
            if (!_factories.TryGetValue(graphKey, out var factory))
            {
                EcsLog.Error("StateGraph not registered: " + graphKey);
                return null;
            }
            graph = factory(World);
            _graphs.Add(graphKey, graph);
            return graph;
        }

        /// <returns>按注册顺序第一个实体身上有同名标签的 key；没有 TagsComponent 或都不匹配时为 null</returns>
        public string ResolveKey(LEntity entity)
        {
            var tags = entity.GetComponent<TagsComponent>();
            if (tags == null)
            {
                return null;
            }
            for (int i = 0; i < _keys.Count; i++)
            {
                if (tags.HasTag(_keys[i]))
                {
                    return _keys[i];
                }
            }
            return null;
        }

        protected override void OnDispose()
        {
            foreach (var graph in _graphs.Values)
            {
                graph.Clear();
            }
            _graphs.Clear();
            _factories.Clear();
            _keys.Clear();
        }
    }
}
