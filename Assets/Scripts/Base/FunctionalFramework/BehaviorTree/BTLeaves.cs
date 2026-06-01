using System;

namespace Functional.BehaviorTree
{
    /// <summary>
    /// 动作叶子节点：执行委托逻辑。
    /// </summary>
    public class ActionNode : BTNode
    {
        private readonly Func<BTContext, BTStatus> _action;

        public ActionNode(Func<BTContext, BTStatus> action) => _action = action;

        public override BTStatus Tick(BTContext context) => _action?.Invoke(context) ?? BTStatus.Failure;
    }

    /// <summary>
    /// 条件叶子节点：条件为 true 返回 Success，否则 Failure。
    /// </summary>
    public class ConditionNode : BTNode
    {
        private readonly Func<BTContext, bool> _condition;

        public ConditionNode(Func<BTContext, bool> condition) => _condition = condition;

        public override BTStatus Tick(BTContext context)
        {
            return _condition != null && _condition(context) ? BTStatus.Success : BTStatus.Failure;
        }
    }

    /// <summary>
    /// 等待节点：等待指定秒数后返回 Success。
    /// </summary>
    public class WaitNode : BTNode
    {
        private readonly float _duration;
        private float _elapsed;

        public WaitNode(float duration) => _duration = duration;

        public override BTStatus Tick(BTContext context)
        {
            _elapsed += context.DeltaTime;
            if (_elapsed >= _duration)
            {
                _elapsed = 0f;
                return BTStatus.Success;
            }
            return BTStatus.Running;
        }

        public override void Reset() => _elapsed = 0f;
    }
}
