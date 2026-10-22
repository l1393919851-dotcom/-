namespace LiteEcs
{
    /// <summary>
    /// 状态节点。一张图被同类型的所有实体共享，所以节点本身不存实体状态，进出都把实体传进来。
    /// </summary>
    public abstract class StateGraphNode
    {
        public StateGraph Graph { get; internal set; }

        public abstract string State { get; }

        /// <param name="arg">StateGraphComponent.Goto 透传的参数，可为 null</param>
        public virtual void Enter(LEntity entity, object arg)
        {
        }

        public virtual void Exit(LEntity entity)
        {
        }
    }

    /// <summary>
    /// 进入时给实体加上与状态同名的标签，退出时去掉。其他系统可以直接用 HasTag(状态名) 判断实体当前状态。
    /// 需要实体挂 TagsComponent；子类重写 Enter / Exit 时调用 base。
    /// </summary>
    public class TaggedStateNode : StateGraphNode
    {
        private readonly string _stateId;

        public override string State => _stateId;

        public TaggedStateNode(string stateId)
        {
            _stateId = stateId;
        }

        public override void Enter(LEntity entity, object arg)
        {
            entity.GetComponent<TagsComponent>()?.AddTag(_stateId);
        }

        public override void Exit(LEntity entity)
        {
            entity.GetComponent<TagsComponent>()?.RemoveTag(_stateId);
        }
    }
}
