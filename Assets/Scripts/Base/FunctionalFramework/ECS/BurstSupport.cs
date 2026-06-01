using UnityEngine;

namespace Functional.ECS
{
    /// <summary>
    /// Burst 支持状态查询。用于启动时确认 ECS 是否走 Burst 路径。
    /// </summary>
    public static class BurstSupport
    {
        /// <summary>项目是否已安装并编译 Burst 相关代码。</summary>
        public static bool IsPackageAvailable
        {
            get
            {
#if UNITY_BURST
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// 在 Console 输出当前 Burst 状态（可在 Bootstrap 中调用）。
        /// </summary>
        public static void LogStatus()
        {
#if UNITY_BURST
            Debug.Log("[BurstSupport] ✓ Burst 包已安装（UNITY_BURST 已定义），ECSWorldRunner 可启用 BurstMovementSystem");
#else
            Debug.LogWarning("[BurstSupport] ✗ Burst 未就绪。请确认 Packages/manifest.json 含 com.unity.burst，并重新打开 Unity 等待包下载完成");
#endif
        }
    }
}
