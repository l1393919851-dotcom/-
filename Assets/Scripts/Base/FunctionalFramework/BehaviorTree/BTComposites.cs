namespace Functional.BehaviorTree
{
    /// <summary>
    /// 顺序节点：依次执行子节点，任一失败则失败，全部成功则成功。
    /// </summary>
    public class SequenceNode : BTNode
    {
        private int _index;

        public override BTStatus Tick(BTContext context)
        {
            if (Children.Count == 0) return BTStatus.Success;

            while (_index < Children.Count)
            {
                var status = Children[_index].Tick(context);
                if (status == BTStatus.Running) return BTStatus.Running;
                if (status == BTStatus.Failure)
                {
                    _index = 0;
                    return BTStatus.Failure;
                }
                _index++;
            }
            _index = 0;
            return BTStatus.Success;
        }

        public override void Reset()
        {
            _index = 0;
            base.Reset();
        }
    }

    /// <summary>
    /// 选择节点：依次尝试子节点，任一成功则成功，全部失败则失败。
    /// </summary>
    public class SelectorNode : BTNode
    {
        private int _index;

        public override BTStatus Tick(BTContext context)
        {
            if (Children.Count == 0) return BTStatus.Failure;

            while (_index < Children.Count)
            {
                var status = Children[_index].Tick(context);
                if (status == BTStatus.Running) return BTStatus.Running;
                if (status == BTStatus.Success)
                {
                    _index = 0;
                    return BTStatus.Success;
                }
                _index++;
            }
            _index = 0;
            return BTStatus.Failure;
        }

        public override void Reset()
        {
            _index = 0;
            base.Reset();
        }
    }

    /// <summary>
    /// 并行节点：同时 tick 所有子节点。Policy 决定成功/失败条件。
    /// </summary>
    public class ParallelNode : BTNode
    {
        public enum Policy
        {
            /// <summary>任一子节点成功即成功。</summary>
            OneSuccess,
            /// <summary>全部子节点成功才成功。</summary>
            AllSuccess,
            /// <summary>任一子节点失败即失败。</summary>
            OneFailure
        }

        public Policy SuccessPolicy = Policy.OneSuccess;

        public override BTStatus Tick(BTContext context)
        {
            int success = 0, failure = 0, running = 0;
            foreach (var child in Children)
            {
                var s = child.Tick(context);
                if (s == BTStatus.Success) success++;
                else if (s == BTStatus.Failure) failure++;
                else running++;
            }

            if (running > 0) return BTStatus.Running;

            switch (SuccessPolicy)
            {
                case Policy.OneSuccess:
                    return success > 0 ? BTStatus.Success : BTStatus.Failure;
                case Policy.AllSuccess:
                    return failure == 0 && success == Children.Count ? BTStatus.Success : BTStatus.Failure;
                case Policy.OneFailure:
                    return failure > 0 ? BTStatus.Failure : BTStatus.Success;
                default:
                    return BTStatus.Failure;
            }
        }
    }
}
