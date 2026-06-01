namespace Functional.BehaviorTree
{
    /// <summary>
    /// 反转装饰器：Success ↔ Failure。
    /// </summary>
    public class InverterNode : BTNode
    {
        private readonly BTNode _child;

        public InverterNode(BTNode child) => _child = child;

        public override BTStatus Tick(BTContext context)
        {
            var s = _child.Tick(context);
            if (s == BTStatus.Success) return BTStatus.Failure;
            if (s == BTStatus.Failure) return BTStatus.Success;
            return BTStatus.Running;
        }

        public override void Reset() => _child.Reset();
    }

    /// <summary>
    /// 重复装饰器：重复执行子节点 count 次。
    /// </summary>
    public class RepeatNode : BTNode
    {
        private readonly BTNode _child;
        private readonly int _count;
        private int _done;

        public RepeatNode(BTNode child, int count)
        {
            _child = child;
            _count = count;
        }

        public override BTStatus Tick(BTContext context)
        {
            while (_done < _count)
            {
                var s = _child.Tick(context);
                if (s == BTStatus.Running) return BTStatus.Running;
                if (s == BTStatus.Failure) return BTStatus.Failure;
                _child.Reset();
                _done++;
            }
            _done = 0;
            return BTStatus.Success;
        }

        public override void Reset()
        {
            _done = 0;
            _child.Reset();
        }
    }

    /// <summary>
    /// 直到成功：反复执行子节点直到返回 Success。
    /// </summary>
    public class UntilSuccessNode : BTNode
    {
        private readonly BTNode _child;

        public UntilSuccessNode(BTNode child) => _child = child;

        public override BTStatus Tick(BTContext context)
        {
            var s = _child.Tick(context);
            if (s == BTStatus.Success) return BTStatus.Success;
            if (s == BTStatus.Failure) _child.Reset();
            return BTStatus.Running;
        }

        public override void Reset() => _child.Reset();
    }
}
