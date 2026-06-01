using UnityEngine;

namespace Functional.StateMachine
{
    /// <summary>
    /// 挂在场景对象上，每帧驱动 Fsm 更新。子类实现 CreateContext 与 BuildFsm。
    /// </summary>
    public abstract class FsmRunner<TContext> : MonoBehaviour
    {
        protected Fsm<TContext> Fsm { get; private set; }
        protected TContext Context { get; private set; }

        protected virtual void Awake()
        {
            Context = CreateContext();
            Fsm = BuildFsm();
        }

        protected virtual void Start()
        {
            OnFsmReady();
        }

        protected virtual void Update()
        {
            if (Fsm != null && Context != null)
            {
                Fsm.Update(Context, Time.deltaTime);
            }
        }

        protected virtual void OnDestroy()
        {
            if (Fsm != null && Context != null)
            {
                Fsm.Stop(Context);
            }
        }

        /// <summary>创建状态机上下文。</summary>
        protected abstract TContext CreateContext();

        /// <summary>构建并配置状态机（注册状态与转换）。</summary>
        protected abstract Fsm<TContext> BuildFsm();

        /// <summary>状态机构建完成后的启动入口（子类在此调用 Fsm.Start）。</summary>
        protected virtual void OnFsmReady() { }
    }
}
