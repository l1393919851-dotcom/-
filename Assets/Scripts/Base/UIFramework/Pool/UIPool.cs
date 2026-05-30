using System.Collections.Generic;
using UnityEngine;

namespace Base.UIFramework
{
    /// <summary>
    /// UI 隐藏池。关闭后标记为 cacheable 的面板会进入这里，下次打开复用，避免重新加载和实例化。
    /// 池内对象会被 SetActive(false) 挂在隐藏根节点下。
    /// </summary>
    public class UIPool
    {
        private readonly Dictionary<string, UIBase> _pool = new Dictionary<string, UIBase>();
        private readonly Transform _root;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="root">池中 UI 的挂载根节点。一般是一个 inactive 的空 GameObject。</param>
        public UIPool(Transform root)
        {
            _root = root;
        }

        /// <summary>
        /// 尝试取出一个已缓存的 UI。
        /// </summary>
        public UIBase TryGet(string uiName)
        {
            if (_pool.TryGetValue(uiName, out var ui))
            {
                _pool.Remove(uiName);
                return ui;
            }
            return null;
        }

        /// <summary>
        /// 缓存一个 UI 到隐藏池。
        /// </summary>
        public void Recycle(string uiName, UIBase ui)
        {
            if (ui == null) return;
            ui.gameObject.SetActive(false);
            ui.transform.SetParent(_root, false);
            _pool[uiName] = ui;
        }

        /// <summary>
        /// 清空整个隐藏池，销毁所有缓存的 UI。
        /// </summary>
        public void Clear()
        {
            foreach (var kv in _pool)
            {
                if (kv.Value != null) Object.Destroy(kv.Value.gameObject);
            }
            _pool.Clear();
        }
    }
}
