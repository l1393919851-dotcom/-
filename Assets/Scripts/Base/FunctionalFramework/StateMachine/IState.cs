namespace Functional.StateMachine
{
    /// <summary>
    /// 状态接口。每个状态实现进入、更新、退出逻辑。
    /// </summary>
    public interface IState<TContext>
    {
        /// <summary>状态名称（用于调试）。</summary>
        string Name { get; }

        /// <summary>进入状态时调用。</summary>
        void OnEnter(TContext context);

        /// <summary>每帧更新。</summary>
        void OnUpdate(TContext context, float deltaTime);

        /// <summary>退出状态时调用。</summary>
        void OnExit(TContext context);
    }
}
