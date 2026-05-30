// =============================================================================
// 启用条件：
//   1) 导入 "DOTween (HOTween v2)" 资源包（Asset Store 免费版）或 DOTween Pro；
//   2) 在 Project Settings → Player → Scripting Define Symbols 添加 BASE_DOTWEEN_SUPPORT。
// 使用方式：在 UIBase 子类里重写 CreateAnimation 返回一个 UIDotweenAnimation 实例即可。
//   protected override IUIAnimation CreateAnimation() => new UIDotweenAnimation();
// =============================================================================
#if BASE_DOTWEEN_SUPPORT
using System;
using UnityEngine;
using DG.Tweening;

namespace Base.UIFramework.Animation
{
    /// <summary>
    /// 基于 DOTween 的 UI 入场/出场动画。
    /// 通过 Tween 同时驱动 scale 和 alpha，自带缓动曲线，比自写协程更流畅。
    /// </summary>
    public class UIDotweenAnimation : IUIAnimation
    {
        /// <summary>入场起始缩放。</summary>
        public Vector3 InStartScale { get; set; } = new Vector3(0.8f, 0.8f, 1f);

        /// <summary>入场结束缩放。</summary>
        public Vector3 InEndScale { get; set; } = Vector3.one;

        /// <summary>出场结束缩放。</summary>
        public Vector3 OutEndScale { get; set; } = new Vector3(0.8f, 0.8f, 1f);

        /// <summary>入场时长（秒）。</summary>
        public float InDuration { get; set; } = 0.25f;

        /// <summary>出场时长（秒）。</summary>
        public float OutDuration { get; set; } = 0.15f;

        /// <summary>入场缓动曲线（默认 OutBack 带回弹）。</summary>
        public Ease InEase { get; set; } = Ease.OutBack;

        /// <summary>出场缓动曲线（默认 InQuad）。</summary>
        public Ease OutEase { get; set; } = Ease.InQuad;

        /// <summary>是否忽略 Time.timeScale（暂停游戏时仍能播放）。</summary>
        public bool IgnoreTimeScale { get; set; } = true;

        /// <summary>
        /// 播放入场：缩放与 alpha 并行。
        /// </summary>
        public void PlayIn(UIBase ui, Action onFinish)
        {
            var tr = ui.transform;
            var cg = GetCanvasGroup(ui);
            tr.localScale = InStartScale;
            cg.alpha = 0f;
            var seq = DOTween.Sequence().SetUpdate(IgnoreTimeScale);
            seq.Join(tr.DOScale(InEndScale, InDuration).SetEase(InEase));
            seq.Join(cg.DOFade(1f, InDuration));
            seq.OnComplete(() => onFinish?.Invoke());
        }

        /// <summary>
        /// 播放出场：缩放变小同时淡出。
        /// </summary>
        public void PlayOut(UIBase ui, Action onFinish)
        {
            var tr = ui.transform;
            var cg = GetCanvasGroup(ui);
            var seq = DOTween.Sequence().SetUpdate(IgnoreTimeScale);
            seq.Join(tr.DOScale(OutEndScale, OutDuration).SetEase(OutEase));
            seq.Join(cg.DOFade(0f, OutDuration));
            seq.OnComplete(() => onFinish?.Invoke());
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
    }
}
#endif
