using System;
using System.Collections.Generic;
using UnityEngine;
using Base.Common;
using Base.ResourceFramework;

namespace Base.UIFramework
{
    /// <summary>
    /// UI 管理总入口。框架的"大脑"。
    /// 
    /// 核心职责：
    /// - 加载、缓存、销毁 UI 面板
    /// - 维护打开中的面板集合
    /// - 维护后退栈
    /// - 调度生命周期（Open / Show / Hide / Close）
    /// - 处理层级（Layer）和焦点
    /// 
    /// 典型用法：
    ///   UIManager.Instance.Open<MainPanel>();
    ///   UIManager.Instance.Open<ShopPanel>(args, pushStack: true);
    ///   UIManager.Instance.Close<ShopPanel>();
    ///   UIManager.Instance.CloseTop();
    /// </summary>
    public class UIManager : MonoSingleton<UIManager>
    {
        private UIRoot _root;
        private UIStack _stack;
        private UIPool _pool;
        private readonly Dictionary<string, UIBase> _opening = new Dictionary<string, UIBase>();

        /// <summary>UI 根节点。</summary>
        public UIRoot Root => _root;

        /// <summary>UI 栈。</summary>
        public UIStack Stack => _stack;

        /// <summary>
        /// 初始化：创建 UIRoot、UIPool。
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();
            var rootGo = new GameObject("[UIRoot]");
            rootGo.transform.SetParent(transform);
            rootGo.layer = LayerMask.NameToLayer("UI");
            var rt = rootGo.AddComponent<RectTransform>();
            _root = rootGo.AddComponent<UIRoot>();
            _root.Init();

            var poolGo = new GameObject("[UIPool]");
            poolGo.transform.SetParent(transform);
            poolGo.SetActive(false);
            _pool = new UIPool(poolGo.transform);

            _stack = new UIStack();
        }

        // ----------------- 打开 / 关闭 API -----------------

        /// <summary>
        /// 同步打开一个面板。如果资源未加载，会同步加载（可能卡顿）。
        /// </summary>
        /// <typeparam name="T">面板类型，需继承 UIBase 并标注 [UI] 特性。</typeparam>
        /// <param name="args">打开参数，会传给 OnOpen。</param>
        /// <param name="pushStack">是否压入返回栈。</param>
        public T Open<T>(object args = null, bool pushStack = false) where T : UIBase
        {
            return Open(typeof(T), args, pushStack) as T;
        }

        /// <summary>
        /// 同步打开一个面板（非泛型版本，方便反射使用）。
        /// </summary>
        public UIBase Open(Type type, object args = null, bool pushStack = false)
        {
            var attr = GetUIAttribute(type);
            if (attr == null)
            {
                UnityEngine.Debug.LogError($"[UIManager] {type.Name} 缺少 [UI] 特性");
                return null;
            }
            string uiName = type.FullName;
            if (_opening.TryGetValue(uiName, out var exist))
            {
                exist.InternalOpen(args);
                return exist;
            }
            var ui = GetOrCreateUI(type, attr);
            if (ui == null) return null;
            ShowUI(ui, args, pushStack);
            return ui;
        }

        /// <summary>
        /// 异步打开一个面板。资源未加载时会异步加载，加载期间通过回调返回。
        /// </summary>
        public void OpenAsync<T>(object args = null, bool pushStack = false, Action<T> onComplete = null) where T : UIBase
        {
            var type = typeof(T);
            var attr = GetUIAttribute(type);
            if (attr == null)
            {
                UnityEngine.Debug.LogError($"[UIManager] {type.Name} 缺少 [UI] 特性");
                onComplete?.Invoke(null);
                return;
            }
            string uiName = type.FullName;
            if (_opening.TryGetValue(uiName, out var exist))
            {
                exist.InternalOpen(args);
                onComplete?.Invoke(exist as T);
                return;
            }
            var cached = _pool.TryGet(uiName);
            if (cached != null)
            {
                AttachToLayer(cached, attr);
                ShowUI(cached, args, pushStack);
                onComplete?.Invoke(cached as T);
                return;
            }
            ResourceManager.Instance.LoadAsync<GameObject>(attr.PrefabPath, prefab =>
            {
                if (prefab == null) { onComplete?.Invoke(null); return; }
                var ui = InstantiateUI(prefab, type, attr);
                ShowUI(ui, args, pushStack);
                onComplete?.Invoke(ui as T);
            });
        }

        /// <summary>
        /// 关闭一个面板（通过类型）。
        /// </summary>
        public void Close<T>() where T : UIBase
        {
            Close(typeof(T).FullName);
        }

        /// <summary>
        /// 关闭一个面板（通过名字）。
        /// </summary>
        public void Close(string uiName)
        {
            if (!_opening.TryGetValue(uiName, out var ui)) return;
            _opening.Remove(uiName);
            _stack.Remove(uiName);
            ui.OnBlur();
            ui.InternalClose(() => HandleAfterClose(uiName, ui));
            NotifyTopFocus();
        }

        /// <summary>
        /// 关闭栈顶的面板（按返回键时调用）。
        /// </summary>
        public void CloseTop()
        {
            var top = _stack.Pop();
            if (!string.IsNullOrEmpty(top)) Close(top);
        }

        /// <summary>
        /// 关闭所有打开中的面板。
        /// </summary>
        public void CloseAll()
        {
            var names = new List<string>(_opening.Keys);
            foreach (var n in names) Close(n);
            _stack.Clear();
        }

        // ----------------- 查询 / 刷新 API -----------------

        /// <summary>
        /// 获取一个已经打开的面板，没打开返回 null。
        /// </summary>
        public T Get<T>() where T : UIBase
        {
            return _opening.TryGetValue(typeof(T).FullName, out var ui) ? ui as T : null;
        }

        /// <summary>
        /// 判断面板是否已打开。
        /// </summary>
        public bool IsOpen<T>() where T : UIBase
        {
            return _opening.ContainsKey(typeof(T).FullName);
        }

        /// <summary>
        /// 通知已打开的面板刷新数据。
        /// </summary>
        public void Refresh<T>(object args = null) where T : UIBase
        {
            if (_opening.TryGetValue(typeof(T).FullName, out var ui))
            {
                ui.OnRefresh(args);
            }
        }

        // ----------------- 内部实现 -----------------

        /// <summary>
        /// 读取类型上的 [UI] 特性。
        /// </summary>
        private static UIAttribute GetUIAttribute(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(UIAttribute), false);
            return attrs.Length > 0 ? attrs[0] as UIAttribute : null;
        }

        /// <summary>
        /// 从隐藏池取，没有就同步加载并实例化。
        /// </summary>
        private UIBase GetOrCreateUI(Type type, UIAttribute attr)
        {
            string uiName = type.FullName;
            var cached = _pool.TryGet(uiName);
            if (cached != null)
            {
                AttachToLayer(cached, attr);
                return cached;
            }
            var prefab = ResourceManager.Instance.Load<GameObject>(attr.PrefabPath);
            if (prefab == null)
            {
                UnityEngine.Debug.LogError($"[UIManager] 加载预制体失败：{attr.PrefabPath}");
                return null;
            }
            return InstantiateUI(prefab, type, attr);
        }

        /// <summary>
        /// 实例化预制体并附加 UIBase 组件。
        /// </summary>
        private UIBase InstantiateUI(GameObject prefab, Type type, UIAttribute attr)
        {
            var go = UnityEngine.Object.Instantiate(prefab);
            go.name = type.Name;
            var ui = go.GetComponent(type) as UIBase;
            if (ui == null)
            {
                ui = go.AddComponent(type) as UIBase;
            }
            ui.UIName = type.FullName;
            ui.Attribute = attr;
            AttachToLayer(ui, attr);
            ui.InternalInit();
            return ui;
        }

        /// <summary>
        /// 把面板放到对应的层级下。
        /// </summary>
        private void AttachToLayer(UIBase ui, UIAttribute attr)
        {
            var layer = _root.GetLayer(attr.Layer);
            ui.transform.SetParent(layer, false);
            var rt = ui.transform as RectTransform;
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.localScale = Vector3.one;
            }
            ui.transform.SetAsLastSibling();
        }

        /// <summary>
        /// 真正触发打开流程：失焦原栈顶 → 入栈 → 标记打开中 → 调用 InternalOpen。
        /// </summary>
        private void ShowUI(UIBase ui, object args, bool pushStack)
        {
            var top = _stack.Peek();
            if (!string.IsNullOrEmpty(top) && _opening.TryGetValue(top, out var topUI))
            {
                topUI.OnBlur();
            }
            if (pushStack) _stack.Push(ui.UIName);
            _opening[ui.UIName] = ui;
            ui.InternalOpen(args);
            ui.OnFocus();
        }

        /// <summary>
        /// 关闭流程的善后：进池或销毁。
        /// </summary>
        private void HandleAfterClose(string uiName, UIBase ui)
        {
            if (ui == null) return;
            if (ui.Attribute != null && ui.Attribute.Cacheable)
            {
                _pool.Recycle(uiName, ui);
            }
            else
            {
                ui.InternalDestroy();
                if (ui.Attribute != null)
                {
                    ResourceManager.Instance.Release(ui.Attribute.PrefabPath);
                }
                Destroy(ui.gameObject);
            }
        }

        /// <summary>
        /// 关闭后通知新的栈顶界面获得焦点。
        /// </summary>
        private void NotifyTopFocus()
        {
            var top = _stack.Peek();
            if (!string.IsNullOrEmpty(top) && _opening.TryGetValue(top, out var topUI))
            {
                topUI.OnFocus();
            }
        }
    }
}
