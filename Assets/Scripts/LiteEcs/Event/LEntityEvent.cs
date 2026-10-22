namespace LiteEcs
{
    /// <summary>
    /// 关于某个实体的池化事件基类。用 LWorld.TakeEvent 取、LWorld.TriggerAndRecycle 发。
    /// 子类加字段时在 OnRecycle 里清掉，并调用 base.OnRecycle()。
    /// </summary>
    public abstract class LEntityEvent : IPoolable
    {
        public LEntity Entity { get; private set; }

        public void Apply(LEntity entity)
        {
            Entity = entity;
        }

        public virtual void OnRecycle()
        {
            Entity = null;
        }
    }
}
