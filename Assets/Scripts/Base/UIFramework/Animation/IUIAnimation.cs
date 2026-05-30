using System;

namespace Base.UIFramework.Animation
{
    /// <summary>
    /// UI 动画接口。每个面板可以通过 UIBase.CreateAnimation 返回不同的动画实现。
    /// 框架内置 Fade（淡入淡出）、Scale（缩放）两种，业务可自定义实现。
    /// </summary>
    public interface IUIAnimation
    {
        /// <summary>
        /// 播放入场动画。
        /// </summary>
        /// <param name="ui">目标 UI。</param>
        /// <param name="onFinish">动画播放完成回调（必须调用一次以触发 OnShow）。</param>
        void PlayIn(UIBase ui, Action onFinish);

        /// <summary>
        /// 播放出场动画。
        /// </summary>
        /// <param name="ui">目标 UI。</param>
        /// <param name="onFinish">动画播放完成回调（必须调用一次以触发 OnClose）。</param>
        void PlayOut(UIBase ui, Action onFinish);
    }
}
