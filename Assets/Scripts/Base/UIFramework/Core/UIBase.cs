using System;
using UnityEngine;
using Base.UIFramework.Animation;

namespace Base.UIFramework
{
    /// <summary>
    /// 所有 UI 面板的基类。
    /// 定义统一的生命周期：Init → Open → Show → Hide → Close → Destroy。
    /// 子类只需要重写需要的回调，并通过 [UI(...)] 特性指定预制体路径。
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        /// <summary>UI 唯一标识，默认为类型全名，UIManager 用它做 key。</summary>
        public string UIName { get; internal set; }

        /// <summary>是否已初始化（OnInit 只会调用一次，避免重复初始化）。</summary>
        public bool IsInitialized { get; private set; }

        /// <summary>当前是否处于打开状态。</summary>
        public bool IsOpen { get; private set; }

        /// <summary>面板配置（来自类上的 [UI] 特性）。</summary>
        public UIAttribute Attribute { get; internal set; }

        /// <summary>该面板的入场/出场动画（可选，子类可重写 CreateAnimation 返回不同的动画）。</summary>
        protected IUIAnimation Animation;

        /// <summary>
        /// 创建该面板使用的动画。子类可重写以返回自定义动画（如缩放/淡入）。
        /// 返回 null 表示无动画。
        /// </summary>
        protected virtual IUIAnimation CreateAnimation() => null;

        // ----------------- 生命周期：由 UIManager 调用，不要主动调用 -----------------

        /// <summary>
        /// 内部调用：第一次创建时触发（只调用一次）。
        /// 一般用来绑定组件、订阅事件。
        /// </summary>
        internal void InternalInit()
        {
            if (IsInitialized) return;
            IsInitialized = true;
            Animation = CreateAnimation();
            OnInit();
        }

        /// <summary>
        /// 内部调用：每次打开时触发，可以接收任意参数。
        /// </summary>
        internal void InternalOpen(object args)
        {
            IsOpen = true;
            gameObject.SetActive(true);
            OnOpen(args);
            if (Animation != null)
            {
                Animation.PlayIn(this, OnShow);
            }
            else
            {
                OnShow();
            }
        }

        /// <summary>
        /// 内部调用：关闭时触发，先播放出场动画再触发 OnClose。
        /// </summary>
        internal void InternalClose(Action onFinish)
        {
            if (!IsOpen)
            {
                onFinish?.Invoke();
                return;
            }
            OnHide();
            void Finish()
            {
                IsOpen = false;
                OnClose();
                onFinish?.Invoke();
            }
            if (Animation != null) Animation.PlayOut(this, Finish);
            else Finish();
        }

        /// <summary>
        /// 内部调用：销毁前触发。
        /// </summary>
        internal void InternalDestroy()
        {
            OnDestroyUI();
        }

        // ----------------- 子类可重写的生命周期 -----------------

        /// <summary>
        /// 初始化回调，整个生命周期只调用一次。
        /// 用来：绑定组件引用、初始化数据、订阅事件。
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// 打开回调，每次打开都会触发。
        /// 用来：根据传入参数刷新界面数据。
        /// </summary>
        protected virtual void OnOpen(object args) { }

        /// <summary>
        /// 显示完成回调，入场动画结束后触发。
        /// 用来：开始播放音效、自动播放教学等。
        /// </summary>
        protected virtual void OnShow() { }

        /// <summary>
        /// 隐藏回调，关闭动画开始前触发。
        /// 用来：停止协程、保存数据。
        /// </summary>
        protected virtual void OnHide() { }

        /// <summary>
        /// 关闭回调，关闭动画结束后触发。
        /// 用来：清理一次性的状态（不要在这里销毁缓存数据）。
        /// </summary>
        protected virtual void OnClose() { }

        /// <summary>
        /// 销毁回调，面板真正被销毁前触发（不可缓存或主动 Destroy 时）。
        /// 用来：反注册事件、释放资源。
        /// </summary>
        protected virtual void OnDestroyUI() { }

        /// <summary>
        /// 获得焦点（被顶层界面盖住后又重新成为最上层时调用）。
        /// </summary>
        public virtual void OnFocus() { }

        /// <summary>
        /// 失去焦点（被新界面盖住时调用）。
        /// </summary>
        public virtual void OnBlur() { }

        /// <summary>
        /// 数据刷新（外部主动调用 UIManager.Refresh 时触发）。
        /// </summary>
        public virtual void OnRefresh(object args) { }

        // ----------------- 工具方法 -----------------

        /// <summary>
        /// 关闭自身的快捷方法（等价于 UIManager.Instance.Close(this)）。
        /// </summary>
        public void Close()
        {
            UIManager.Instance.Close(UIName);
        }
    }
}
