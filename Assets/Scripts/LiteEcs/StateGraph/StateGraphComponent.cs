namespace LiteEcs
{
    /// <summary>
    /// 创建参数里指定状态图时实现这个接口。两个值都可以为空。
    /// </summary>
    public interface IStateGraphCreateParam
    {
        /// <summary>为空时按实体标签去 StateGraphSystem 里自动匹配</summary>
        string StateGraphKey { get; }

        /// <summary>Start 时自动进入的状态，为空则停在无状态</summary>
        string InitialState { get; }
    }

    /// <summary>
    /// 实体的当前状态。Start 时从 StateGraphSystem 拿图（世界里必须先 AddSystem(StateGraphSystem)），
    /// 实体 End / 回池时退出当前节点。
    /// </summary>
    public class StateGraphComponent : LComponent
    {
        private StateGraph _graph;
        private StateGraphNode _node;
        private string _graphKey;
        private string _initialState;

        public StateGraph Graph => _graph;

        /// <summary>当前状态名，没有为 null</summary>
        public string State => _node?.State;

        public override void Init(object createParam)
        {
            if (createParam is IStateGraphCreateParam param)
            {
                _graphKey = param.StateGraphKey;
                _initialState = param.InitialState;
            }
        }

        public override void OnComponentStart()
        {
            var system = Entity.World.GetSystem<StateGraphSystem>();
            if (system == null)
            {
                EcsLog.Error("StateGraphComponent needs StateGraphSystem in world, entity " + Entity.Id);
                return;
            }
            var key = string.IsNullOrEmpty(_graphKey) ? system.ResolveKey(Entity) : _graphKey;
            if (key == null)
            {
                return;
            }
            _graph = system.Get(key);
            if (_graph != null && !string.IsNullOrEmpty(_initialState))
            {
                Goto(_initialState);
            }
        }

        /// <summary>
        /// 切到目标状态：同状态直接返回；先 Exit 旧节点，再 Enter 新节点。
        /// </summary>
        public void Goto(string state, object arg = null)
        {
            if (_graph == null)
            {
                EcsLog.Error("StateGraph missing, entity " + Entity.Id + " state " + state);
                return;
            }
            if (_node != null && _node.State == state)
            {
                return;
            }
            var node = _graph.GetNode(state);
            if (node == null)
            {
                EcsLog.Error("StateGraph node missing, entity " + Entity.Id + " state " + state);
                return;
            }
            ExitNode();
            _node = node;
            node.Enter(Entity, arg);
        }

        public override void OnComponentEnd()
        {
            ExitNode();
        }

        public override void OnComponentDestroy()
        {
            ExitNode();
            _graph = null;
        }

        public override void OnRecycle()
        {
            ExitNode();
            _graph = null;
            _graphKey = null;
            _initialState = null;
            base.OnRecycle();
        }

        private void ExitNode()
        {
            var node = _node;
            if (node == null)
            {
                return;
            }
            _node = null;
            node.Exit(Entity);
        }
    }
}
