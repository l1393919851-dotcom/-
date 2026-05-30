using UnityEngine;

namespace Base.ResourceFramework
{
    /// <summary>
    /// 资源引用计数数据结构。每个被加载的资源都会被包装为 AssetRef，记录被引用次数。
    /// 引用计数归零时，由 ResourceManager 决定是否真正卸载。
    /// </summary>
    public class AssetRef
    {
        /// <summary>资源路径，作为唯一标识。</summary>
        public string Path { get; private set; }

        /// <summary>底层加载到的 Unity 资源对象。</summary>
        public Object Asset { get; private set; }

        /// <summary>当前引用计数。</summary>
        public int RefCount { get; private set; }

        /// <summary>引用计数归零后的存活时间（秒），用于实现"延迟卸载"以提高复用率。</summary>
        public float UnusedTime { get; set; }

        /// <summary>
        /// 构造函数。一般由 ResourceManager 调用。
        /// </summary>
        public AssetRef(string path, Object asset)
        {
            Path = path;
            Asset = asset;
            RefCount = 0;
            UnusedTime = 0f;
        }

        /// <summary>
        /// 增加一次引用计数（通常在 Load 成功后调用）。
        /// </summary>
        public void Retain()
        {
            RefCount++;
            UnusedTime = 0f;
        }

        /// <summary>
        /// 减少一次引用计数（通常在 Release 时调用）。
        /// 不会小于 0，避免外部错误调用。
        /// </summary>
        public void Release()
        {
            if (RefCount > 0) RefCount--;
        }

        /// <summary>
        /// 判断当前引用是否已经"无效"（资源被外部销毁或丢失）。
        /// </summary>
        public bool IsAlive => Asset != null;
    }
}
