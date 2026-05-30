using System;
using System.Collections;
using UnityEngine;

namespace Base.ResourceFramework
{
    /// <summary>
    /// 异步加载请求封装，统一对外的等待方式。
    /// 既可以通过协程 yield return，也可以通过事件回调。
    /// </summary>
    /// <typeparam name="T">要加载的资源类型。</typeparam>
    public class LoadRequest<T> : IEnumerator where T : UnityEngine.Object
    {
        /// <summary>资源路径。</summary>
        public string Path { get; private set; }

        /// <summary>是否完成。</summary>
        public bool IsDone { get; private set; }

        /// <summary>加载到的资源。</summary>
        public T Asset { get; private set; }

        /// <summary>加载进度，0~1。</summary>
        public float Progress { get; private set; }

        /// <summary>加载完成回调。</summary>
        public event Action<T> OnComplete;

        /// <summary>
        /// 创建一个异步加载请求。
        /// </summary>
        public LoadRequest(string path)
        {
            Path = path;
            IsDone = false;
            Progress = 0f;
        }

        /// <summary>
        /// 由 ResourceManager 在内部调用，用来推进进度。
        /// </summary>
        public void SetProgress(float p)
        {
            Progress = Mathf.Clamp01(p);
        }

        /// <summary>
        /// 由 ResourceManager 在加载完成后调用，触发回调。
        /// </summary>
        public void Complete(T asset)
        {
            Asset = asset;
            Progress = 1f;
            IsDone = true;
            try { OnComplete?.Invoke(asset); }
            catch (Exception e) { Debug.LogException(e); }
        }

        // ----------- IEnumerator 实现：让协程可以 yield return 这个请求 -----------

        /// <summary>IEnumerator.Current：协程使用，无实际意义。</summary>
        public object Current => null;

        /// <summary>IEnumerator.MoveNext：未完成时返回 true，让协程继续等待。</summary>
        public bool MoveNext() => !IsDone;

        /// <summary>IEnumerator.Reset：不支持。</summary>
        public void Reset() { }
    }
}
