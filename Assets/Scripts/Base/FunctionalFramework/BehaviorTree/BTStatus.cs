namespace Functional.BehaviorTree
{
    /// <summary>
    /// 行为树节点执行结果。
    /// </summary>
    public enum BTStatus
    {
        /// <summary>执行成功。</summary>
        Success,

        /// <summary>执行失败。</summary>
        Failure,

        /// <summary>下一帧继续执行（异步/多帧任务）。</summary>
        Running
    }
}
