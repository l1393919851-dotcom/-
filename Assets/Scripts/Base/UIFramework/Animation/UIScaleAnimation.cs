using System;
using System.Collections;
using UnityEngine;

namespace Base.UIFramework.Animation
{
    /// <summary>
    /// 缩放动画。从 0.8 → 1（入场）和从 1 → 0.8（出场），配合 alpha 实现弹出效果。
    /// 常用于弹窗类界面。
    /// </summary>
    public class UIScaleAnimation : IUIAnimation
    {
        /// <summary>入场动画初始缩放。</summary>
        public Vector3 InStartScale { get; set; } = new Vector3(0.8f, 0.8f, 1f);

        /// <summary>入场动画结束缩放。</summary>
        public Vector3 InEndScale { get; set; } = Vector3.one;

        /// <summary>出场动画结束缩放。</summary>
        public Vector3 OutEndScale { get; set; } = new Vector3(0.8f, 0.8f, 1f);

        /// <summary>入场动画时长。</summary>
        public float InDuration { get; set; } = 0.25f;

        /// <summary>出场动画时长。</summary>
        public float OutDuration { get; set; } = 0.15f;

        /// <summary>
        /// 播放入场：缩放 InStartScale → InEndScale，alpha 0 → 1。
        /// </summary>
        public void PlayIn(UIBase ui, Action onFinish)
        {
            var cg = GetCanvasGroup(ui);
            ui.transform.localScale = InStartScale;
            ui.StartCoroutine(Run(ui.transform, cg, InStartScale, InEndScale, 0f, 1f, InDuration, onFinish));
        }

        /// <summary>
        /// 播放出场：缩放当前 → OutEndScale，alpha 当前 → 0。
        /// </summary>
        public void PlayOut(UIBase ui, Action onFinish)
        {
            var cg = GetCanvasGroup(ui);
            var startScale = ui.transform.localScale;
            ui.StartCoroutine(Run(ui.transform, cg, startScale, OutEndScale, cg.alpha, 0f, OutDuration, onFinish));
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
        /// 协程：同时插值 scale 和 alpha，带回弹缓动（OutBack 风格）。
        /// </summary>
        private static IEnumerator Run(Transform tr, CanvasGroup cg, Vector3 fromS, Vector3 toS, float fromA, float toA, float duration, Action onFinish)
        {
            if (duration <= 0f)
            {
                tr.localScale = toS;
                cg.alpha = toA;
                onFinish?.Invoke();
                yield break;
            }
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / duration);
                float ease = EaseOutBack(p);
                tr.localScale = Vector3.LerpUnclamped(fromS, toS, ease);
                cg.alpha = Mathf.Lerp(fromA, toA, p);
                yield return null;
            }
            tr.localScale = toS;
            cg.alpha = toA;
            onFinish?.Invoke();
        }

        /// <summary>
        /// 缓动函数：OutBack（结尾有轻微回弹）。
        /// </summary>
        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
