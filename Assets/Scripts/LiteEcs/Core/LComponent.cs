using System;

namespace LiteEcs
{
    /// <summary>
    /// 组件基类。生命周期由所属实体驱动：
    /// Init(创建参数) → 进世界那帧 Awake → Start → 每帧 Tick → 移除那帧 End → Destroy → 回池 OnRecycle。
    /// 子类重写 OnRecycle 时必须调用 base.OnRecycle()。
    /// </summary>
    public abstract class LComponent : IPoolable
    {
        /// <summary>
        /// 组件在实体上占用的槽位。默认是具体类型；想让子类顶替基类槽位时重写成基类类型，
        /// 此时只能用 GetComponent&lt;基类&gt;() 取到它。
        /// </summary>
        public virtual Type ComponentKey => GetType();

        public LEntity Entity { get; internal set; }

        /// <summary>
        /// 实体创建时调用。同一个实体的所有组件拿到的是同一个参数对象，各组件按自己关心的接口去取。
        /// </summary>
        public virtual void Init(object createParam)
        {
        }

        public virtual void Tick(float dt)
        {
        }

        public virtual void OnComponentAwake()
        {
        }

        public virtual void OnComponentStart()
        {
        }

        public virtual void OnComponentEnd()
        {
        }

        public virtual void OnComponentDestroy()
        {
        }

        public virtual void OnRecycle()
        {
            Entity = null;
        }
    }
}
