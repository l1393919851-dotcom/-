using UnityEngine;

namespace Base.UIFramework.Adapter
{
    /// <summary>
    /// 安全区适配器。挂在 UI 节点上，自动让该节点的 anchor 跟随 Screen.safeArea。
    /// 用于刘海屏、Home 指示条等机型的 UI 边距适配。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaAdapter : MonoBehaviour
    {
        /// <summary>是否每帧检查安全区（用于横竖屏切换的情况）。</summary>
        public bool CheckEveryFrame = false;

        private RectTransform _rt;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        /// <summary>
        /// Awake：缓存 RectTransform 引用并立即应用一次。
        /// </summary>
        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            Apply();
        }

        /// <summary>
        /// Update：可选地每帧检查 SafeArea 变化（横竖屏切换）。
        /// </summary>
        private void Update()
        {
            if (!CheckEveryFrame) return;
            if (Screen.safeArea != _lastSafeArea
                || Screen.width != _lastScreenSize.x
                || Screen.height != _lastScreenSize.y)
            {
                Apply();
            }
        }

        /// <summary>
        /// 根据当前 Screen.safeArea 设置 anchor，使节点贴合安全区域。
        /// </summary>
        public void Apply()
        {
            if (_rt == null) _rt = GetComponent<RectTransform>();
            var safe = Screen.safeArea;
            _lastSafeArea = safe;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            _rt.anchorMin = anchorMin;
            _rt.anchorMax = anchorMax;
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;
        }
    }
}
