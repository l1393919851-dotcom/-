namespace Functional.BehaviorTree
{
    /// <summary>
    /// 行为树容器。持有根节点，负责 Tick 与重置。
    /// </summary>
    public class BehaviorTreeAsset
    {
        public BTNode Root { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// 执行一次行为树 Tick。
        /// </summary>
        public BTStatus Tick(BTContext context)
        {
            if (Root == null) return BTStatus.Failure;
            return Root.Tick(context);
        }

        /// <summary>
        /// 重置整棵树。
        /// </summary>
        public void Reset()
        {
            Root?.Reset();
        }
    }
}
