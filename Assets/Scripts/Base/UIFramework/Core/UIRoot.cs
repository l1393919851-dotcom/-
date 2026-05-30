using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Base.UIFramework
{
    /// <summary>
    /// UI 根节点。负责创建并管理：
    /// - 一个根 Canvas（持有 CanvasScaler、GraphicRaycaster）
    /// - 每个 UILayer 对应一个子 Canvas（独立 sortingOrder，避免合批断裂）
    /// - 一个 EventSystem
    /// 一般由 UIManager 在初始化时自动创建，外部不直接使用。
    /// </summary>
    public class UIRoot : MonoBehaviour
    {
        /// <summary>设计分辨率宽度，可在 Inspector 或代码中修改。</summary>
        public Vector2 ReferenceResolution = new Vector2(1920, 1080);

        /// <summary>匹配模式：0 完全匹配宽度，1 完全匹配高度，0.5 取中间。</summary>
        [Range(0, 1)] public float MatchWidthOrHeight = 0.5f;

        private Canvas _rootCanvas;
        private readonly Dictionary<UILayer, Transform> _layers = new Dictionary<UILayer, Transform>();

        /// <summary>
        /// 获取根 Canvas。
        /// </summary>
        public Canvas RootCanvas => _rootCanvas;

        /// <summary>
        /// 初始化：创建根 Canvas、每层 Canvas、EventSystem。
        /// </summary>
        public void Init()
        {
            BuildRootCanvas();
            BuildLayers();
            BuildEventSystem();
        }

        /// <summary>
        /// 获取指定层级的父 Transform。
        /// </summary>
        public Transform GetLayer(UILayer layer)
        {
            _layers.TryGetValue(layer, out var t);
            return t;
        }

        /// <summary>
        /// 创建根 Canvas + CanvasScaler + GraphicRaycaster。
        /// </summary>
        private void BuildRootCanvas()
        {
            _rootCanvas = gameObject.GetComponent<Canvas>();
            if (_rootCanvas == null) _rootCanvas = gameObject.AddComponent<Canvas>();
            _rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _rootCanvas.sortingOrder = 0;

            var scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = MatchWidthOrHeight;

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// 为每个 UILayer 创建一个子 Canvas，独立 sortingOrder。
        /// 每层加 GraphicRaycaster，保证可以接收输入。
        /// </summary>
        private void BuildLayers()
        {
            foreach (UILayer layer in System.Enum.GetValues(typeof(UILayer)))
            {
                var go = new GameObject(layer.ToString(), typeof(RectTransform));
                go.transform.SetParent(transform, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.localScale = Vector3.one;

                var canvas = go.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = (int)layer * 100;
                go.AddComponent<GraphicRaycaster>();
                _layers[layer] = go.transform;
            }
        }

        /// <summary>
        /// 如果场景中没有 EventSystem，则自动创建。
        /// </summary>
        private void BuildEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(es);
        }
    }
}
