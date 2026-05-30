using System;

namespace Base.Common
{
    /// <summary>
    /// 纯 C# 单例基类（非 MonoBehaviour）。
    /// 适用于不依赖 Unity 生命周期的纯逻辑模块（如数据中心、事件中心）。
    /// 使用方式：public class XXX : Singleton<XXX> { }
    /// </summary>
    public abstract class Singleton<T> where T : class, new()
    {
        private static T _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// 获取单例实例（线程安全，懒加载）。
        /// 第一次访问时会自动创建实例并调用 OnInit。
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                            (_instance as Singleton<T>)?.OnInit();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 销毁单例实例并调用 OnDispose，再次访问 Instance 时会重新创建。
        /// 一般在游戏退出或切换场景时调用。
        /// </summary>
        public static void Dispose()
        {
            if (_instance != null)
            {
                (_instance as Singleton<T>)?.OnDispose();
                _instance = null;
            }
        }

        /// <summary>
        /// 子类重写此方法做初始化工作（在第一次 Instance 时触发）。
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// 子类重写此方法做清理工作（在 Dispose 时触发）。
        /// </summary>
        protected virtual void OnDispose() { }
    }
}
