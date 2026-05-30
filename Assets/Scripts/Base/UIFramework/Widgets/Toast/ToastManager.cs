using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Base.Common;

namespace Base.UIFramework.Widgets
{
    /// <summary>
    /// 轻量 Toast 管理器。在屏幕中央/底部显示一行短文字提示。
    /// 内部按队列依次显示，避免重叠。
    /// 
    /// 用法：ToastManager.Instance.Show("保存成功");
    /// </summary>
    public class ToastManager : MonoSingleton<ToastManager>
    {
        /// <summary>Toast 显示时长（秒）。</summary>
        public float DisplayDuration = 1.5f;

        /// <summary>淡入淡出时长（秒）。</summary>
        public float FadeDuration = 0.25f;

        private Transform _toastLayer;
        private readonly Queue<string> _queue = new Queue<string>();
        private bool _running;

        /// <summary>
        /// 显示一条 Toast。可连续调用，按队列播放。
        /// </summary>
        public void Show(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            _queue.Enqueue(message);
            if (!_running) StartCoroutine(RunQueue());
        }

        /// <summary>
        /// 协程：依次处理队列中的 Toast。
        /// </summary>
        private IEnumerator RunQueue()
        {
            _running = true;
            EnsureLayer();
            while (_queue.Count > 0)
            {
                var msg = _queue.Dequeue();
                yield return ShowOne(msg);
            }
            _running = false;
        }

        /// <summary>
        /// 显示单条 Toast：创建临时 GameObject，淡入 → 等待 → 淡出 → 销毁。
        /// </summary>
        private IEnumerator ShowOne(string msg)
        {
            var go = new GameObject("Toast", typeof(RectTransform));
            go.transform.SetParent(_toastLayer, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.3f);
            rt.anchorMax = new Vector2(0.5f, 0.3f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(600, 80);

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);
            bg.raycastTarget = false;

            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(20, 10);
            trt.offsetMax = new Vector2(-20, -10);
            var txt = textGo.AddComponent<Text>();
            txt.text = msg;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.fontSize = 32;
            txt.raycastTarget = false;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;

            yield return Fade(cg, 0f, 1f, FadeDuration);
            yield return new WaitForSecondsRealtime(DisplayDuration);
            yield return Fade(cg, 1f, 0f, FadeDuration);
            Destroy(go);
        }

        /// <summary>
        /// 协程：CanvasGroup alpha 插值。
        /// </summary>
        private static IEnumerator Fade(CanvasGroup cg, float from, float to, float duration)
        {
            if (duration <= 0f) { cg.alpha = to; yield break; }
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            cg.alpha = to;
        }

        /// <summary>
        /// 确保 Toast 挂在 System 层下。
        /// </summary>
        private void EnsureLayer()
        {
            if (_toastLayer != null) return;
            var manager = UIManager.Instance;
            _toastLayer = manager.Root.GetLayer(UILayer.System);
        }
    }
}
