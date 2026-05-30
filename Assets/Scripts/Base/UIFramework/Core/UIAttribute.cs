using System;

namespace Base.UIFramework
{
    /// <summary>
    /// UI 面板配置特性。标记在 UIBase 子类上，用于声明该面板的资源路径、所在层级等元信息。
    /// 通过特性的方式而不是硬编码字典，可以让每个面板自描述，新增/移除面板都不用改 UIManager。
    /// 
    /// 示例：
    /// [UI("UI/MainPanel", UILayer.Normal)]
    /// public class MainPanel : UIBase { }
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class UIAttribute : Attribute
    {
        /// <summary>预制体资源路径（相对 Resources 目录或 Addressables 地址）。</summary>
        public string PrefabPath { get; }

        /// <summary>所在层级，默认 Normal。</summary>
        public UILayer Layer { get; }

        /// <summary>是否全屏（全屏界面打开后会自动隐藏下层 Normal 界面以省 DrawCall）。</summary>
        public bool FullScreen { get; }

        /// <summary>关闭后是否缓存（true=进入隐藏池，false=立即销毁）。</summary>
        public bool Cacheable { get; }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="prefabPath">预制体路径。</param>
        /// <param name="layer">UI 所在的层级。</param>
        /// <param name="fullScreen">是否全屏，全屏界面会让下层界面隐藏渲染。</param>
        /// <param name="cacheable">关闭时是否进入对象池，下一次打开可以复用。</param>
        public UIAttribute(string prefabPath, UILayer layer = UILayer.Normal, bool fullScreen = false, bool cacheable = true)
        {
            PrefabPath = prefabPath;
            Layer = layer;
            FullScreen = fullScreen;
            Cacheable = cacheable;
        }
    }
}
