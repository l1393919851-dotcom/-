using System;

namespace Functional.SceneManage
{
    /// <summary>
    /// 场景流程中的一步：场景名 + 加载模式 + 加载完成回调。
    /// </summary>
    [Serializable]
    public class SceneFlowStep
    {
        /// <summary>Unity Build Settings 中的场景名（不含路径）。</summary>
        public string SceneName;

        /// <summary>加载模式。</summary>
        public SceneLoadMode Mode = SceneLoadMode.Single;

        /// <summary>是否显示加载 UI（由外部监听 OnProgress 自行处理）。</summary>
        public bool ShowLoading = true;

        public SceneFlowStep() { }

        public SceneFlowStep(string sceneName, SceneLoadMode mode = SceneLoadMode.Single, bool showLoading = true)
        {
            SceneName = sceneName;
            Mode = mode;
            ShowLoading = showLoading;
        }
    }
}
