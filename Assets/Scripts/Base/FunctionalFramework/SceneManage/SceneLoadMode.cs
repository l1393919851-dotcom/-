namespace Functional.SceneManage
{
    /// <summary>
    /// 场景加载模式。
    /// </summary>
    public enum SceneLoadMode
    {
        /// <summary>单场景：卸载当前场景后加载目标场景。</summary>
        Single,

        /// <summary>叠加：保留当前场景，追加加载目标场景。</summary>
        Additive
    }
}
