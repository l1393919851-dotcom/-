namespace Base.UIFramework
{
    /// <summary>
    /// UI 层级枚举。
    /// 通过分层来管理界面的渲染顺序，每一层是一个独立的子 Canvas，互不打断合批。
    /// 数值越大，越靠上显示。
    /// </summary>
    public enum UILayer
    {
        /// <summary>背景层：场景背景、主城背景图等，最底层。</summary>
        Background = 0,

        /// <summary>主界面层：主城、关卡选择、商店等常驻或主要界面。</summary>
        Normal = 1,

        /// <summary>弹窗层：所有弹出式窗口（购买、设置等）。</summary>
        Popup = 2,

        /// <summary>系统层：Toast、飘字、轻量提示。</summary>
        System = 3,

        /// <summary>顶层：Loading、新手引导遮罩、断线重连等最高优先级 UI。</summary>
        Top = 4
    }
}
