using System;

namespace Functional.StateMachine
{
    /// <summary>
    /// 状态转换条件：从 fromState 到 toState，当 condition 为 true 时触发。
    /// </summary>
    public class StateTransition<TContext>
    {
        public Type FromState { get; }
        public Type ToState { get; }
        public Func<TContext, bool> Condition { get; }
        public int Priority { get; }

        public StateTransition(Type from, Type to, Func<TContext, bool> condition, int priority = 0)
        {
            FromState = from;
            ToState = to;
            Condition = condition;
            Priority = priority;
        }
    }
}
