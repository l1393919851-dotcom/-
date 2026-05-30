// =============================================================================
// 启用条件：
//   1) 通过 Package Manager 安装 "TextMeshPro" 包；
//   2) 在 Project Settings → Player → Scripting Define Symbols 添加 BASE_TMP_SUPPORT。
// 与原 DialogManager 并行存在，调用方式：DialogManagerTMP.Instance.Show(...)
// =============================================================================
#if BASE_TMP_SUPPORT
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Base.Common;

namespace Base.UIFramework.Widgets
{
    /// <summary>
    /// TextMeshPro 版本的通用对话框管理器。
    /// 与 DialogManager 等价，但内部文本使用 TextMeshProUGUI，支持富文本和高级排版。
    /// </summary>
    public class DialogManagerTMP : MonoSingleton<DialogManagerTMP>
    {
        /// <summary>可选：指定使用的 TMP 字体资源。为 null 时使用 TMP 默认字体。</summary>
        public TMP_FontAsset FontAsset;

        /// <summary>
        /// 显示一个对话框（单按钮或双按钮）。
        /// </summary>
        /// <param name="title">标题。</param>
        /// <param name="content">正文。</param>
        /// <param name="onConfirm">点击确定按钮回调。</param>
        /// <param name="onCancel">点击取消按钮回调，传 null 表示单按钮模式。</param>
        public void Show(string title, string content, Action onConfirm = null, Action onCancel = null)
        {
            var layer = UIManager.Instance.Root.GetLayer(UILayer.Popup);
            var root = CreateRoot(layer);
            CreateMask(root.transform);
            var panel = CreatePanel(root.transform);
            CreateText(panel.transform, title, 40, new Vector2(0, 1), new Vector2(0, 0), new Vector2(0, 80), 0);
            CreateText(panel.transform, content, 30, new Vector2(0, 0.5f), new Vector2(0, 20), new Vector2(-60, 200), 0);

            if (onCancel == null)
            {
                CreateButton(panel.transform, "确定", new Vector2(0, -180), () => { onConfirm?.Invoke(); Destroy(root); });
            }
            else
            {
                CreateButton(panel.transform, "取消", new Vector2(-130, -180), () => { onCancel.Invoke(); Destroy(root); });
                CreateButton(panel.transform, "确定", new Vector2(130, -180), () => { onConfirm?.Invoke(); Destroy(root); });
            }
        }

        /// <summary>
        /// 创建对话框根节点（铺满整个 Popup 层）。
        /// </summary>
        private GameObject CreateRoot(Transform parent)
        {
            var go = new GameObject("DialogTMP", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        /// <summary>
        /// 创建半透明黑色遮罩，拦截下层点击。
        /// </summary>
        private void CreateMask(Transform parent)
        {
            var go = new GameObject("Mask", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);
        }

        /// <summary>
        /// 创建居中的对话框面板。
        /// </summary>
        private GameObject CreatePanel(Transform parent)
        {
            var go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(700, 450);
            go.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
            return go;
        }

        /// <summary>
        /// 通用 TMP 文本节点创建。anchor、pivot、size、position 都可控。
        /// </summary>
        private void CreateText(Transform parent, string content, int fontSize, Vector2 anchorY, Vector2 pos, Vector2 size, float pivotY)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, anchorY.x);
            rt.anchorMax = new Vector2(1, anchorY.y == 0 ? anchorY.x : anchorY.y);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = content;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
            txt.fontSize = fontSize;
            if (FontAsset != null) txt.font = FontAsset;
        }

        /// <summary>
        /// 创建按钮（含文字 + 点击回调）。
        /// </summary>
        private void CreateButton(Transform parent, string label, Vector2 anchoredPos, Action onClick)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(220, 80);
            rt.anchoredPosition = new Vector2(anchoredPos.x, 40);
            go.GetComponent<Image>().color = new Color(0.3f, 0.5f, 0.9f);
            go.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());

            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            var txt = textGo.AddComponent<TextMeshProUGUI>();
            txt.text = label;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
            txt.fontSize = 32;
            txt.raycastTarget = false;
            if (FontAsset != null) txt.font = FontAsset;
        }
    }
}
#endif
