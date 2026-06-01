using System.Collections.Generic;

namespace Functional.BehaviorTree
{
    /// <summary>
    /// 行为树节点基类。
    /// </summary>
    public abstract class BTNode
    {
        public string Name { get; set; }
        protected readonly List<BTNode> Children = new List<BTNode>();

        /// <summary>
        /// 添加子节点（组合节点使用）。
        /// </summary>
        public BTNode AddChild(BTNode child)
        {
            Children.Add(child);
            return child;
        }

        /// <summary>
        /// 执行节点逻辑。
        /// </summary>
        public abstract BTStatus Tick(BTContext context);

        /// <summary>
        /// 重置节点状态（Running 节点在树重启时调用）。
        /// </summary>
        public virtual void Reset()
        {
            foreach (var c in Children) c.Reset();
        }
    }
}
