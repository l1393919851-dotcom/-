using System;
using System.Collections;
using UnityEngine;

namespace Base.UIFramework.Animation
{
    /// <summary>
    /// 淡入淡出动画。基于 CanvasGroup 修改 alpha。
    /// 如果目标 UI 没有 CanvasGroup 组件，会自动添加。
    /// </summary>
    public class UIFadeAnimation : IUIAnimation
    {
        /// <summary>淡入耗时（秒）。</summary>
        public float InDuration { get; set; } = 0.2f;

        /// <summary>淡出耗时（秒）。</summary>
        public float OutDuration { get; set; } = 0.15f;

        /// <summary>
        /// 播放淡入：alpha 从 0 到 1。
        /// </summary>
        public void PlayIn(UIBase ui, Action onFinish)
        {
            var cg = GetCanvasGroup(ui);
            ui.StartCoroutine(Fade(cg, 0f, 1f, InDuration, onFinish));
        }

        /// <summary>
        /// 播放淡出：alpha 从当前值到 0。
        /// </summary>
        public void PlayOut(UIBase ui, Action onFinish)
        {
            var cg = GetCanvasGroup(ui);
            ui.StartCoroutine(Fade(cg, cg.alpha, 0f, OutDuration, onFinish));
        }

        /// <summary>
        /// 获取或自动添加 CanvasGroup 组件。
        /// </summary>
        private static CanvasGroup GetCanvasGroup(UIBase ui)
        {
            var cg = ui.GetComponent<CanvasGroup>();
            if (cg == null) cg = ui.gameObject.AddComponent<CanvasGroup>();
            return cg;
        }

        /// <summary>
        /// 协程：按时间插值 alpha。
        /// </summary>
        private static IEnumerator Fade(CanvasGroup cg, float from, float to, float duration, Action onFinish)
        {
            if (duration <= 0f)
            {
                cg.alpha = to;
                onFinish?.Invoke();
                yield break;
            }
            float t = 0f;
            cg.alpha = from;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            cg.alpha = to;
            onFinish?.Invoke();
        }
    }
}
