using System;
using System.Collections;
using UnityEngine;

namespace Base.ResourceFramework
{
    /// <summary>
    /// 基于 Unity Resources 系统的加载器实现。
    /// 适合中小型项目或快速原型，路径以 Resources 文件夹为根。
    /// 如果项目后续切换到 Addressables，只需要新建一个 AddressablesLoader 并实现 IResourceLoader 即可。
    /// </summary>
    public class ResourcesLoader : IResourceLoader
    {
        private readonly MonoBehaviour _coroutineRunner;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="coroutineRunner">用于跑协程的宿主（一般是 ResourceManager 自身）。</param>
        public ResourcesLoader(MonoBehaviour coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
        }

        /// <summary>
        /// 同步加载资源。底层使用 Resources.Load。
        /// </summary>
        public T Load<T>(string path) where T : UnityEngine.Object
        {
            var asset = Resources.Load<T>(path);
            if (asset == null)
            {
                Debug.LogError($"[ResourcesLoader] 资源加载失败: {path}");
            }
            return asset;
        }

        /// <summary>
        /// 异步加载资源。底层使用 Resources.LoadAsync，通过协程等待完成。
        /// </summary>
        public void LoadAsync<T>(string path, Action<T> callback) where T : UnityEngine.Object
        {
            _coroutineRunner.StartCoroutine(LoadAsyncCoroutine(path, callback));
        }

        /// <summary>
        /// 协程版本的异步加载（内部使用）。
        /// </summary>
        private IEnumerator LoadAsyncCoroutine<T>(string path, Action<T> callback) where T : UnityEngine.Object
        {
            var request = Resources.LoadAsync<T>(path);
            yield return request;
            var asset = request.asset as T;
            if (asset == null)
            {
                Debug.LogError($"[ResourcesLoader] 异步加载失败: {path}");
            }
            callback?.Invoke(asset);
        }

        /// <summary>
        /// 释放单个资源。Resources 系统对 GameObject 不能直接 Unload，所以一般什么都不做，
        /// 真正的卸载由 ReleaseAll 调用的 Resources.UnloadUnusedAssets 完成。
        /// </summary>
        public void Release(string path, UnityEngine.Object asset)
        {
            // Resources 加载的 GameObject 不能单独卸载，只能等到 ReleaseAll
        }

        /// <summary>
        /// 卸载所有未使用的资源（一般在切场景时调用，会有耗时）。
        /// </summary>
        public void ReleaseAll()
        {
            Resources.UnloadUnusedAssets();
        }
    }
}
