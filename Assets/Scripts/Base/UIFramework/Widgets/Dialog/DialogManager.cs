using System;
using UnityEngine;
using UnityEngine.UI;
using Base.Common;

namespace Base.UIFramework.Widgets
{
    /// <summary>
    /// 通用确认对话框管理器。无需预制体，运行时动态构建。
    /// 支持单按钮（OK）或双按钮（OK / Cancel）。
    /// 
    /// 用法：
    ///   DialogManager.Instance.Show("提示", "确定要退出吗？", () => Application.Quit(), () => Debug.Log("取消"));
    /// </summary>
    public class DialogManager : MonoSingleton<DialogManager>
    {
        /// <summary>
        /// 显示一个对话框（双按钮）。
        /// </summary>
        /// <param name="title">标题文字。</param>
        /// <param name="content">正文内容。</param>
        /// <param name="onConfirm">点击确定按钮回调。</param>
        /// <param name="onCancel">点击取消按钮回调，传 null 表示单按钮模式。</param>
        public void Show(string title, string content, Action onConfirm = null, Action onCancel = null)
        {
            var layer = UIManager.Instance.Root.GetLayer(UILayer.Popup);
            var root = CreateRoot(layer);
            CreateMask(root.transform);
            var panel = CreatePanel(root.transform);
            CreateTitle(panel.transform, title);
            CreateContent(panel.transform, content);

            if (onCancel == null)
            {
                CreateButton(panel.transform, "确定", new Vector2(0, -180), () =>
                {
                    onConfirm?.Invoke();
                    Destroy(root);
                });
            }
            else
            {
                CreateButton(panel.transform, "取消", new Vector2(-130, -180), () =>
                {
                    onCancel.Invoke();
                    Destroy(root);
                });
                CreateButton(panel.transform, "确定", new Vector2(130, -180), () =>
                {
                    onConfirm?.Invoke();
                    Destroy(root);
                });
            }
        }

        /// <summary>
        /// 创建对话框根节点（铺满整个 Popup 层）。
        /// </summary>
        private GameObject CreateRoot(Transform parent)
        {
            var go = new GameObject("Dialog", typeof(RectTransform));
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
            var img = go.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0.6f);
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
        /// 创建标题文本。
        /// </summary>
        private void CreateTitle(Transform parent, string title)
        {
            var go = new GameObject("Title", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 80);
            rt.anchoredPosition = new Vector2(0, 0);
            var txt = go.AddComponent<Text>();
            txt.text = title;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.fontSize = 40;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        /// <summary>
        /// 创建正文文本。
        /// </summary>
        private void CreateContent(Transform parent, string content)
        {
            var go = new GameObject("Content", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(-60, 200);
            rt.anchoredPosition = new Vector2(0, 20);
            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.fontSize = 30;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        /// <summary>
        /// 创建一个按钮，按钮文字 + 点击事件。
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
            var txt = textGo.AddComponent<Text>();
            txt.text = label;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.fontSize = 32;
            txt.raycastTarget = false;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
