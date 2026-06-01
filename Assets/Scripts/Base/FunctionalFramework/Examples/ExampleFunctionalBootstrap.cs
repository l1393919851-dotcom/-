using UnityEngine;
using Functional.SceneManage;
using Functional.ConfigTable;
using Functional.SoundManage;
using Functional.ECS;

namespace Functional.Examples
{
    /// <summary>
    /// 功能框架启动示例。可与 Base.Examples.ExampleBootstrap 同场景使用。
    /// </summary>
    public class ExampleFunctionalBootstrap : MonoBehaviour
    {
        private void Start()
        {
            BurstSupport.LogStatus();
            ConfigManager.Instance.LoadAll();
            SoundManager.Instance.SetVolume(SoundChannel.BGM, 0.8f);

            SceneFlowManager.Instance.OnLoadProgress += (name, p) =>
                Debug.Log($"[Scene] 加载 {name} 进度 {p:P0}");
        }
    }
}
