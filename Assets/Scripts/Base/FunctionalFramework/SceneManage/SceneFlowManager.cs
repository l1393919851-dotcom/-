using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Base.Common;
using Base.ResourceFramework;

namespace Functional.SceneManage
{
    /// <summary>
    /// 场景流程管理器。负责同步/异步加载、卸载、流程跳转与进度回调。
    /// 
    /// 典型用法：
    ///   SceneFlowManager.Instance.LoadScene("Main");
    ///   SceneFlowManager.Instance.LoadSceneAsync("Battle", SceneLoadMode.Single, p => loadingBar.value = p);
    ///   SceneFlowManager.Instance.RunFlow(steps);
    /// </summary>
    public class SceneFlowManager : MonoSingleton<SceneFlowManager>
    {
        /// <summary>当前激活的主场景名。</summary>
        public string CurrentScene { get; private set; }

        /// <summary>是否正在加载场景。</summary>
        public bool IsLoading { get; private set; }

        /// <summary>加载进度 0~1。</summary>
        public float Progress { get; private set; }

        /// <summary>场景加载开始。</summary>
        public event Action<string> OnLoadStarted;

        /// <summary>场景加载进度更新。</summary>
        public event Action<string, float> OnLoadProgress;

        /// <summary>场景加载完成。</summary>
        public event Action<string> OnLoadCompleted;

        /// <summary>场景卸载完成。</summary>
        public event Action<string> OnUnloadCompleted;

        private readonly Queue<SceneFlowStep> _flowQueue = new Queue<SceneFlowStep>();
        private Coroutine _loadCoroutine;

        /// <summary>
        /// 同步加载场景（会卡顿，仅适合小场景或调试）。
        /// </summary>
        public void LoadScene(string sceneName, SceneLoadMode mode = SceneLoadMode.Single)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            var loadMode = mode == SceneLoadMode.Single ? LoadSceneMode.Single : LoadSceneMode.Additive;
            if (mode == SceneLoadMode.Single)
            {
                ReleaseSceneResources();
            }
            SceneManager.LoadScene(sceneName, loadMode);
            CurrentScene = sceneName;
            OnLoadCompleted?.Invoke(sceneName);
        }

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        public void LoadSceneAsync(string sceneName, SceneLoadMode mode = SceneLoadMode.Single, Action<float> onProgress = null, Action onComplete = null)
        {
            if (IsLoading)
            {
                Debug.LogWarning("[SceneFlowManager] 已有加载任务进行中，请等待完成或使用 RunFlow");
                return;
            }
            _loadCoroutine = StartCoroutine(LoadSceneCoroutine(sceneName, mode, onProgress, onComplete));
        }

        /// <summary>
        /// 异步卸载叠加场景。
        /// </summary>
        public void UnloadSceneAsync(string sceneName, Action onComplete = null)
        {
            StartCoroutine(UnloadSceneCoroutine(sceneName, onComplete));
        }

        /// <summary>
        /// 按顺序执行场景流程（如 启动 → 大厅 → 战斗）。
        /// </summary>
        public void RunFlow(IEnumerable<SceneFlowStep> steps, Action onAllComplete = null)
        {
            _flowQueue.Clear();
            foreach (var s in steps) _flowQueue.Enqueue(s);
            if (_flowQueue.Count == 0) return;
            StartCoroutine(RunFlowCoroutine(onAllComplete));
        }

        /// <summary>
        /// 重新加载当前场景。
        /// </summary>
        public void ReloadCurrent(Action onComplete = null)
        {
            if (string.IsNullOrEmpty(CurrentScene)) return;
            LoadSceneAsync(CurrentScene, SceneLoadMode.Single, null, onComplete);
        }

        private IEnumerator RunFlowCoroutine(Action onAllComplete)
        {
            while (_flowQueue.Count > 0)
            {
                var step = _flowQueue.Dequeue();
                bool done = false;
                LoadSceneAsync(step.SceneName, step.Mode, step.ShowLoading ? p => OnLoadProgress?.Invoke(step.SceneName, p) : null, () => done = true);
                while (!done) yield return null;
            }
            onAllComplete?.Invoke();
        }

        private IEnumerator LoadSceneCoroutine(string sceneName, SceneLoadMode mode, Action<float> onProgress, Action onComplete)
        {
            IsLoading = true;
            Progress = 0f;
            OnLoadStarted?.Invoke(sceneName);

            if (mode == SceneLoadMode.Single)
            {
                ReleaseSceneResources();
            }

            var loadMode = mode == SceneLoadMode.Single ? LoadSceneMode.Single : LoadSceneMode.Additive;
            var op = SceneManager.LoadSceneAsync(sceneName, loadMode);
            if (op == null)
            {
                Debug.LogError($"[SceneFlowManager] 加载失败，请确认场景已加入 Build Settings：{sceneName}");
                IsLoading = false;
                yield break;
            }

            op.allowSceneActivation = false;
            while (op.progress < 0.9f)
            {
                Progress = op.progress / 0.9f;
                onProgress?.Invoke(Progress);
                OnLoadProgress?.Invoke(sceneName, Progress);
                yield return null;
            }

            Progress = 1f;
            onProgress?.Invoke(1f);
            OnLoadProgress?.Invoke(sceneName, 1f);
            op.allowSceneActivation = true;
            while (!op.isDone) yield return null;

            CurrentScene = sceneName;
            IsLoading = false;
            OnLoadCompleted?.Invoke(sceneName);
            onComplete?.Invoke();
            _loadCoroutine = null;
        }

        private IEnumerator UnloadSceneCoroutine(string sceneName, Action onComplete)
        {
            var op = SceneManager.UnloadSceneAsync(sceneName);
            if (op == null)
            {
                onComplete?.Invoke();
                yield break;
            }
            while (!op.isDone) yield return null;
            OnUnloadCompleted?.Invoke(sceneName);
            onComplete?.Invoke();
        }

        /// <summary>
        /// 切场景时释放资源缓存（与 ResourceManager 联动）。
        /// </summary>
        private void ReleaseSceneResources()
        {
            if (ResourceManager.HasInstance)
            {
                ResourceManager.Instance.ReleaseAll();
            }
        }
    }
}
