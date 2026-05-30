using UnityEngine;

namespace Base.Common
{
    /// <summary>
    /// MonoBehaviour 单例基类。
    /// 适用于需要 Update / 协程 / 生命周期回调的管理器（如 UIManager、ResourceManager）。
    /// 实例会被挂在一个名为 "[MonoSingletons]" 的根 GameObject 上，并标记为 DontDestroyOnLoad。
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        private static bool _isQuitting;

        /// <summary>
        /// 获取单例实例。如果场景中不存在则自动创建。
        /// 注意：在 OnApplicationQuit 之后访问会返回 null，避免内存泄漏。
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_isQuitting) return null;
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        var root = GameObject.Find("[MonoSingletons]");
                        if (root == null)
                        {
                            root = new GameObject("[MonoSingletons]");
                            DontDestroyOnLoad(root);
                        }
                        var go = new GameObject(typeof(T).Name);
                        go.transform.SetParent(root.transform);
                        _instance = go.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 判断单例是否已经存在（用于避免在不该创建实例的时候触发创建）。
        /// </summary>
        public static bool HasInstance => _instance != null && !_isQuitting;

        /// <summary>
        /// Unity Awake 回调：绑定唯一实例并调用 OnInit。
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this as T;
            OnInit();
        }

        /// <summary>
        /// Unity OnApplicationQuit 回调：标记退出状态，避免再次创建实例。
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        /// <summary>
        /// Unity OnDestroy 回调：清空引用。
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        /// <summary>
        /// 子类重写此方法做初始化工作（Awake 时触发，只调用一次）。
        /// </summary>
        protected virtual void OnInit() { }
    }
}
