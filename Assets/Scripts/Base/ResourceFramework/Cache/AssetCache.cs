using System.Collections.Generic;
using UnityEngine;

namespace Base.ResourceFramework
{
    /// <summary>
    /// 资源缓存模块。
    /// 以路径为 key 缓存所有已加载的 AssetRef，负责引用计数的归零延迟卸载。
    /// </summary>
    public class AssetCache
    {
        private readonly Dictionary<string, AssetRef> _cache = new Dictionary<string, AssetRef>();

        /// <summary>未使用资源的存活时间（秒）。设为 0 表示立即卸载，建议 30~120 秒。</summary>
        public float UnusedLifetime { get; set; } = 60f;

        /// <summary>
        /// 尝试从缓存中获取资源（不增加引用计数）。
        /// </summary>
        public AssetRef Get(string path)
        {
            _cache.TryGetValue(path, out var asset);
            return asset;
        }

        /// <summary>
        /// 新增一个资源到缓存。一般在第一次加载完成时调用。
        /// </summary>
        public AssetRef Add(string path, Object asset)
        {
            if (_cache.TryGetValue(path, out var exist)) return exist;
            var assetRef = new AssetRef(path, asset);
            _cache.Add(path, assetRef);
            return assetRef;
        }

        /// <summary>
        /// 从缓存中移除一个资源。
        /// </summary>
        public bool Remove(string path)
        {
            return _cache.Remove(path);
        }

        /// <summary>
        /// 每帧调用，递增"未使用资源"的存活时间。超过 UnusedLifetime 后通过 loader 真正卸载。
        /// </summary>
        public void Tick(float deltaTime, IResourceLoader loader)
        {
            if (_cache.Count == 0) return;
            List<string> toRemove = null;
            foreach (var kv in _cache)
            {
                var assetRef = kv.Value;
                if (assetRef.RefCount > 0) continue;
                assetRef.UnusedTime += deltaTime;
                if (assetRef.UnusedTime >= UnusedLifetime)
                {
                    (toRemove ??= new List<string>()).Add(kv.Key);
                }
            }
            if (toRemove != null)
            {
                foreach (var p in toRemove)
                {
                    var assetRef = _cache[p];
                    loader.Release(p, assetRef.Asset);
                    _cache.Remove(p);
                }
            }
        }

        /// <summary>
        /// 清空所有缓存（一般在切场景时调用）。
        /// </summary>
        public void Clear(IResourceLoader loader)
        {
            foreach (var kv in _cache)
            {
                loader.Release(kv.Key, kv.Value.Asset);
            }
            _cache.Clear();
        }

        /// <summary>
        /// 获取当前缓存数量（用于调试）。
        /// </summary>
        public int Count => _cache.Count;
    }
}
