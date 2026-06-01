using Functional.StateMachine;
using UnityEngine;

namespace Functional.Examples
{
    /// <summary>
    /// 玩家状态机示例。
    /// </summary>
    public class ExamplePlayerFsm : FsmRunner<ExamplePlayerContext>
    {
        protected override ExamplePlayerContext CreateContext() => new ExamplePlayerContext();

        protected override Fsm<ExamplePlayerContext> BuildFsm()
        {
            return new Fsm<ExamplePlayerContext>()
                .AddState(new PlayerIdleState())
                .AddState(new PlayerRunState())
                .AddTransition<PlayerIdleState, PlayerRunState>(ctx => ctx.IsMoving)
                .AddTransition<PlayerRunState, PlayerIdleState>(ctx => !ctx.IsMoving);
        }

        protected override void OnFsmReady()
        {
            Fsm.Start<PlayerIdleState>(Context);
        }

        private void OnGUI()
        {
            Context.IsMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A)
                || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        }
    }

    public class ExamplePlayerContext
    {
        public bool IsMoving;
        public string CurrentAnim = "Idle";
    }

    public class PlayerIdleState : IState<ExamplePlayerContext>
    {
        public string Name => "Idle";
        public void OnEnter(ExamplePlayerContext ctx) => ctx.CurrentAnim = "Idle";
        public void OnUpdate(ExamplePlayerContext ctx, float dt) { }
        public void OnExit(ExamplePlayerContext ctx) { }
    }

    public class PlayerRunState : IState<ExamplePlayerContext>
    {
        public string Name => "Run";
        public void OnEnter(ExamplePlayerContext ctx) => ctx.CurrentAnim = "Run";
        public void OnUpdate(ExamplePlayerContext ctx, float dt) { }
        public void OnExit(ExamplePlayerContext ctx) { }
    }
}
