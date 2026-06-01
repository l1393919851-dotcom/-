using Functional.ECS.Systems;
using UnityEngine;

namespace Functional.ECS
{
    /// <summary>
    /// 挂在场景中驱动 ECS World 更新。可配置使用 Burst 移动系统。
    /// </summary>
    public class ECSWorldRunner : MonoBehaviour
    {
        [SerializeField] private bool _useBurstWhenAvailable = true;

        public World World { get; private set; }
        public ECSViewRegistry ViewRegistry { get; private set; }

        /// <summary>当前是否使用 Burst 移动系统。</summary>
        public bool IsUsingBurst { get; private set; }

        protected virtual void Awake()
        {
            ViewRegistry = new ECSViewRegistry();
            World = new World();
            RegisterSystems();
        }

        protected virtual void Update()
        {
            World?.Update(Time.deltaTime);
        }

        protected virtual void OnDestroy()
        {
            World?.Dispose();
            ViewRegistry?.Clear();
        }

        /// <summary>
        /// 注册系统。子类可重写以添加自定义系统。
        /// </summary>
        protected virtual void RegisterSystems()
        {
#if UNITY_BURST
            if (_useBurstWhenAvailable)
            {
                World.AddSystem(new BurstMovementSystem(ViewRegistry));
                IsUsingBurst = true;
                Debug.Log("[ECSWorldRunner] Burst 已启用 → BurstMovementSystem");
                return;
            }
#endif
            IsUsingBurst = false;
            World.AddSystem(new MovementSystem(ViewRegistry));
#if UNITY_BURST
            Debug.Log("[ECSWorldRunner] Burst 包已安装，但 Use Burst When Available 未勾选，使用 MovementSystem");
#else
            Debug.LogWarning("[ECSWorldRunner] 未检测到 Burst 包，使用 MovementSystem。请在 manifest.json 安装 com.unity.burst 后重新打开 Unity");
#endif
        }
    }
}
