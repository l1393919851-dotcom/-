using System;
using UnityEngine;
using Base.Common;

namespace Base.ResourceFramework
{
    /// <summary>
    /// 资源管理总入口。所有资源相关操作（加载、释放、池化）都通过此类对外暴露。
    /// 内部封装了：加载器（IResourceLoader）、引用计数缓存（AssetCache）、GameObject 池。
    /// 
    /// 典型用法：
    ///   var prefab = ResourceManager.Instance.Load<GameObject>("UI/MainPanel");
    ///   ResourceManager.Instance.LoadAsync<Sprite>("Icons/Hero", sprite => image.sprite = sprite);
    ///   var go = ResourceManager.Instance.SpawnAsync("Effects/Boom", parent);
    /// </summary>
    public class ResourceManager : MonoSingleton<ResourceManager>
    {
        private IResourceLoader _loader;
        private AssetCache _cache;
        private GameObjectPool _pool;
        private Transform _poolRoot;

        /// <summary>
        /// 当前底层加载器（默认使用 ResourcesLoader，可以通过 SetLoader 替换为 Addressables）。
        /// </summary>
        public IResourceLoader Loader => _loader;

        /// <summary>
        /// 当前缓存模块（外部一般不直接使用，调试时可访问）。
        /// </summary>
        public AssetCache Cache => _cache;

        /// <summary>
        /// 当前 GameObject 对象池。
        /// </summary>
        public GameObjectPool Pool => _pool;

        /// <summary>
        /// 初始化：创建默认加载器、缓存和对象池根节点。
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();
            _loader = new ResourcesLoader(this);
            _cache = new AssetCache();
            var poolGo = new GameObject("[Pool]");
            poolGo.transform.SetParent(transform);
            poolGo.SetActive(false);
            _poolRoot = poolGo.transform;
            _pool = new GameObjectPool(_poolRoot);
        }

        /// <summary>
        /// 替换底层加载器（一般在游戏启动时调用一次）。
        /// 例如切换到 Addressables 时：ResourceManager.Instance.SetLoader(new AddressablesLoader());
        /// </summary>
        public void SetLoader(IResourceLoader loader)
        {
            if (loader == null) return;
            _cache.Clear(_loader);
            _loader = loader;
        }

        /// <summary>
        /// 同步加载资源。会自动增加引用计数；同名资源会复用缓存。
        /// </summary>
        public T Load<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path)) return null;
            var assetRef = _cache.Get(path);
            if (assetRef == null || !assetRef.IsAlive)
            {
                var asset = _loader.Load<T>(path);
                if (asset == null) return null;
                assetRef = _cache.Add(path, asset);
            }
            assetRef.Retain();
            return assetRef.Asset as T;
        }

        /// <summary>
        /// 异步加载资源。如果缓存命中则同帧回调，否则走底层异步加载。
        /// </summary>
        public LoadRequest<T> LoadAsync<T>(string path, Action<T> onComplete = null) where T : UnityEngine.Object
        {
            var request = new LoadRequest<T>(path);
            if (onComplete != null) request.OnComplete += onComplete;
            var assetRef = _cache.Get(path);
            if (assetRef != null && assetRef.IsAlive)
            {
                assetRef.Retain();
                request.Complete(assetRef.Asset as T);
                return request;
            }
            _loader.LoadAsync<T>(path, asset =>
            {
                if (asset != null)
                {
                    var newRef = _cache.Add(path, asset);
                    newRef.Retain();
                }
                request.Complete(asset);
            });
            return request;
        }

        /// <summary>
        /// 释放一次资源引用。引用计数归零后会进入延迟卸载流程。
        /// </summary>
        public void Release(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            var assetRef = _cache.Get(path);
            if (assetRef != null) assetRef.Release();
        }

        /// <summary>
        /// 立即销毁指定资源（不走延迟卸载）。慎用，注意有没有其他引用。
        /// </summary>
        public void ForceRelease(string path)
        {
            var assetRef = _cache.Get(path);
            if (assetRef != null)
            {
                _loader.Release(path, assetRef.Asset);
                _cache.Remove(path);
            }
        }

        /// <summary>
        /// 从对象池实例化一个 GameObject。如果池中没有则先加载预制体再实例化。
        /// </summary>
        public GameObject Spawn(string path, Transform parent = null)
        {
            var prefab = Load<GameObject>(path);
            if (prefab == null) return null;
            return _pool.Spawn(path, prefab, parent);
        }

        /// <summary>
        /// 异步版的 Spawn。
        /// </summary>
        public void SpawnAsync(string path, Transform parent, Action<GameObject> onComplete)
        {
            LoadAsync<GameObject>(path, prefab =>
            {
                if (prefab == null)
                {
                    onComplete?.Invoke(null);
                    return;
                }
                var go = _pool.Spawn(path, prefab, parent);
                onComplete?.Invoke(go);
            });
        }

        /// <summary>
        /// 回收一个对象到池中。
        /// </summary>
        public void Despawn(string path, GameObject go)
        {
            _pool.Despawn(path, go);
        }

        /// <summary>
        /// 卸载所有资源（切场景时调用）。
        /// </summary>
        public void ReleaseAll()
        {
            _pool.ClearAll();
            _cache.Clear(_loader);
            _loader.ReleaseAll();
        }

        /// <summary>
        /// 每帧推进缓存的延迟卸载计时器。
        /// </summary>
        private void Update()
        {
            _cache?.Tick(Time.unscaledDeltaTime, _loader);
        }
    }
}
