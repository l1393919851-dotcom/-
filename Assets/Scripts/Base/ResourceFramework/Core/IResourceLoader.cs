using System;
using UnityEngine;

namespace Base.ResourceFramework
{
    /// <summary>
    /// 资源加载器接口。所有具体加载器（Resources / AssetBundle / Addressables）都要实现此接口。
    /// 设计目标：统一上层调用，下层可自由切换底层加载机制。
    /// </summary>
    public interface IResourceLoader
    {
        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <param name="path">资源路径（Resources 相对路径或 Addressables 地址）。</param>
        /// <typeparam name="T">资源类型，必须继承自 UnityEngine.Object。</typeparam>
        /// <returns>加载到的资源对象，失败返回 null。</returns>
        T Load<T>(string path) where T : UnityEngine.Object;

        /// <summary>
        /// 异步加载资源，通过回调返回结果。
        /// </summary>
        /// <param name="path">资源路径。</param>
        /// <param name="callback">加载完成回调，参数为加载到的资源。</param>
        void LoadAsync<T>(string path, Action<T> callback) where T : UnityEngine.Object;

        /// <summary>
        /// 释放资源（卸载或减引用），具体行为由底层实现决定。
        /// </summary>
        /// <param name="path">资源路径，用于定位要释放的资源。</param>
        /// <param name="asset">要释放的资源对象（部分加载器需要此参数）。</param>
        void Release(string path, UnityEngine.Object asset);

        /// <summary>
        /// 释放当前加载器所有资源（一般用于切换场景或退出游戏）。
        /// </summary>
        void ReleaseAll();
    }
}
