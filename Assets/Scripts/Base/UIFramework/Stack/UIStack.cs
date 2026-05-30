using System.Collections.Generic;

namespace Base.UIFramework
{
    /// <summary>
    /// UI 栈。用于管理"后退"行为（例如按返回键、ESC 关闭最上层界面）。
    /// 不是所有 UI 都需要进栈：常驻界面（如 HUD）、Toast、引导遮罩通常不入栈。
    /// 是否入栈由 UIManager.Open 时的参数控制。
    /// </summary>
    public class UIStack
    {
        private readonly Stack<string> _stack = new Stack<string>();

        /// <summary>
        /// 栈内界面数量。
        /// </summary>
        public int Count => _stack.Count;

        /// <summary>
        /// 入栈一个界面。
        /// </summary>
        public void Push(string uiName)
        {
            _stack.Push(uiName);
        }

        /// <summary>
        /// 取出栈顶界面（同时移除）。
        /// </summary>
        public string Pop()
        {
            return _stack.Count > 0 ? _stack.Pop() : null;
        }

        /// <summary>
        /// 查看栈顶界面（不移除）。
        /// </summary>
        public string Peek()
        {
            return _stack.Count > 0 ? _stack.Peek() : null;
        }

        /// <summary>
        /// 从栈中移除某个界面（用于这个界面被外部主动关闭时，保持栈一致）。
        /// </summary>
        public bool Remove(string uiName)
        {
            if (_stack.Count == 0) return false;
            var list = new List<string>(_stack);
            int idx = list.IndexOf(uiName);
            if (idx < 0) return false;
            list.RemoveAt(idx);
            _stack.Clear();
            for (int i = list.Count - 1; i >= 0; i--) _stack.Push(list[i]);
            return true;
        }

        /// <summary>
        /// 清空栈。
        /// </summary>
        public void Clear()
        {
            _stack.Clear();
        }

        /// <summary>
        /// 判断某个 UI 是否在栈中。
        /// </summary>
        public bool Contains(string uiName)
        {
            return _stack.Contains(uiName);
        }
    }
}
