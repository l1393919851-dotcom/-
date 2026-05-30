// =============================================================================
// 启用条件：
//   1) 通过 Package Manager 安装 "Addressables" 包；
//   2) 在 Project Settings → Player → Scripting Define Symbols 添加 BASE_ADDRESSABLES。
// 启用后，在游戏启动时调用：
//   ResourceManager.Instance.SetLoader(new AddressablesLoader());
// 即可让整个项目走 Addressables，业务层无需任何修改。
// =============================================================================
#if BASE_ADDRESSABLES
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Base.ResourceFramework
{
    /// <summary>
    /// 基于 Unity Addressables 的加载器实现。
    /// 通过 AsyncOperationHandle 句柄管理资源生命周期，自动处理引用计数与卸载。
    /// </summary>
    public class AddressablesLoader : IResourceLoader
    {
        private readonly Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();

        /// <summary>
        /// 同步加载资源（内部使用 WaitForCompletion 阻塞等待，仅推荐用于启动期）。
        /// 运行时尽量使用 LoadAsync 避免卡顿。
        /// </summary>
        public T Load<T>(string path) where T : UnityEngine.Object
        {
            if (_handles.TryGetValue(path, out var existing) && existing.IsValid())
            {
                return existing.Result as T;
            }
            var handle = Addressables.LoadAssetAsync<T>(path);
            var asset = handle.WaitForCompletion();
            if (asset == null)
            {
                Debug.LogError($"[AddressablesLoader] 同步加载失败: {path}");
                Addressables.Release(handle);
                return null;
            }
            _handles[path] = handle;
            return asset;
        }

        /// <summary>
        /// 异步加载资源。完成后通过回调返回结果。
        /// </summary>
        public void LoadAsync<T>(string path, Action<T> callback) where T : UnityEngine.Object
        {
            if (_handles.TryGetValue(path, out var existing) && existing.IsValid() && existing.IsDone)
            {
                callback?.Invoke(existing.Result as T);
                return;
            }
            var handle = Addressables.LoadAssetAsync<T>(path);
            _handles[path] = handle;
            handle.Completed += op =>
            {
                if (op.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[AddressablesLoader] 异步加载失败: {path}");
                    callback?.Invoke(null);
                    return;
                }
                callback?.Invoke(op.Result as T);
            };
        }

        /// <summary>
        /// 释放资源（减句柄引用，Addressables 内部会维护真正的内存回收时机）。
        /// </summary>
        public void Release(string path, UnityEngine.Object asset)
        {
            if (_handles.TryGetValue(path, out var handle))
            {
                if (handle.IsValid()) Addressables.Release(handle);
                _handles.Remove(path);
            }
        }

        /// <summary>
        /// 释放所有持有的 Addressables 句柄。一般在切换大场景时调用。
        /// </summary>
        public void ReleaseAll()
        {
            foreach (var kv in _handles)
            {
                if (kv.Value.IsValid()) Addressables.Release(kv.Value);
            }
            _handles.Clear();
        }
    }
}
#endif
