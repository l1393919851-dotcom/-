using System;
using System.Collections.Generic;
using UnityEngine;

namespace Functional.StateMachine
{
    /// <summary>
    /// 有限状态机。支持注册多个状态、条件转换、强制切换。
    /// 
    /// 典型用法：
    ///   var fsm = new Fsm<PlayerContext>();
    ///   fsm.AddState(new IdleState());
    ///   fsm.AddTransition<IdleState, RunState>(ctx => ctx.IsMoving);
    ///   fsm.Start<IdleState>(context);
    ///   fsm.Update(context, Time.deltaTime);
    /// </summary>
    public class Fsm<TContext>
    {
        private readonly Dictionary<Type, IState<TContext>> _states = new Dictionary<Type, IState<TContext>>();
        private readonly List<StateTransition<TContext>> _transitions = new List<StateTransition<TContext>>();

        /// <summary>当前状态。</summary>
        public IState<TContext> CurrentState { get; private set; }

        /// <summary>当前状态类型。</summary>
        public Type CurrentStateType => CurrentState?.GetType();

        /// <summary>状态切换事件（旧状态, 新状态）。</summary>
        public event Action<IState<TContext>, IState<TContext>> OnStateChanged;

        /// <summary>
        /// 注册一个状态实例。
        /// </summary>
        public Fsm<TContext> AddState(IState<TContext> state)
        {
            _states[state.GetType()] = state;
            return this;
        }

        /// <summary>
        /// 添加条件转换（每帧检测，按 Priority 从高到低）。
        /// </summary>
        public Fsm<TContext> AddTransition<TFrom, TTo>(Func<TContext, bool> condition, int priority = 0)
            where TFrom : IState<TContext>
            where TTo : IState<TContext>
        {
            _transitions.Add(new StateTransition<TContext>(typeof(TFrom), typeof(TTo), condition, priority));
            _transitions.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            return this;
        }

        /// <summary>
        /// 启动状态机并进入初始状态。
        /// </summary>
        public void Start<TInitial>(TContext context) where TInitial : IState<TContext>
        {
            ChangeState(typeof(TInitial), context);
        }

        /// <summary>
        /// 每帧驱动：先检测转换，再更新当前状态。
        /// </summary>
        public void Update(TContext context, float deltaTime)
        {
            if (CurrentState == null) return;

            foreach (var t in _transitions)
            {
                if (t.FromState != CurrentStateType) continue;
                if (t.Condition != null && t.Condition(context))
                {
                    ChangeState(t.ToState, context);
                    break;
                }
            }

            CurrentState?.OnUpdate(context, deltaTime);
        }

        /// <summary>
        /// 强制切换到指定状态（不检测条件）。
        /// </summary>
        public void ChangeState<TState>(TContext context) where TState : IState<TContext>
        {
            ChangeState(typeof(TState), context);
        }

        /// <summary>
        /// 强制切换到指定状态类型。
        /// </summary>
        public void ChangeState(Type stateType, TContext context)
        {
            if (!_states.TryGetValue(stateType, out var next))
            {
                Debug.LogError($"[Fsm] 未注册状态：{stateType.Name}");
                return;
            }
            if (CurrentState == next) return;

            var prev = CurrentState;
            prev?.OnExit(context);
            CurrentState = next;
            next.OnEnter(context);
            OnStateChanged?.Invoke(prev, next);
        }

        /// <summary>
        /// 停止状态机。
        /// </summary>
        public void Stop(TContext context)
        {
            CurrentState?.OnExit(context);
            CurrentState = null;
        }
    }
}
