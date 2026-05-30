using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Base.Common;
using Base.ResourceFramework;

namespace Base.UIFramework.Debug
{
    /// <summary>
    /// UI 框架运行时调试器。按 F1 切换显示一个简易调试面板。
    /// 显示：当前打开的 UI 列表、栈状态、资源缓存数量。
    /// 仅在编辑器和 Development Build 下生效。
    /// </summary>
    public class UIDebugger : MonoSingleton<UIDebugger>
    {
        /// <summary>是否启用调试器（运行时切换）。</summary>
        public bool Enable = false;

        /// <summary>调试热键。</summary>
        public KeyCode Hotkey = KeyCode.F1;

        private Vector2 _scrollPos;
        private GUIStyle _labelStyle;

        /// <summary>
        /// Update：监听热键开关调试面板。
        /// </summary>
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(Hotkey)) Enable = !Enable;
        }

        /// <summary>
        /// OnGUI：使用 IMGUI 绘制调试面板（不影响真实 UI）。
        /// </summary>
        private void OnGUI()
        {
            if (!Enable) return;
            EnsureStyle();
            GUILayout.BeginArea(new Rect(10, 10, 360, 600), GUI.skin.box);
            GUILayout.Label("<b><size=18>UI Debugger (F1)</size></b>", _labelStyle);
            GUILayout.Space(4);
            GUILayout.Label($"Opening UI: {GetOpeningCount()}", _labelStyle);
            GUILayout.Label($"Stack Count: {UIManager.Instance.Stack.Count}", _labelStyle);
            GUILayout.Label($"AssetCache: {ResourceManager.Instance.Cache.Count}", _labelStyle);
            GUILayout.Space(8);
            GUILayout.Label("<b>Opening UIs:</b>", _labelStyle);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            foreach (var name in GetOpeningNames())
            {
                GUILayout.Label(name, _labelStyle);
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>
        /// 通过反射拿到 UIManager 内部 _opening 字典的数量。
        /// </summary>
        private int GetOpeningCount()
        {
            var dict = GetOpeningDict();
            return dict?.Count ?? 0;
        }

        /// <summary>
        /// 通过反射拿到 UIManager 内部 _opening 字典的所有 key。
        /// </summary>
        private IEnumerable<string> GetOpeningNames()
        {
            var dict = GetOpeningDict();
            if (dict == null) yield break;
            foreach (var k in dict.Keys) yield return k;
        }

        /// <summary>
        /// 通过反射拿到 UIManager 内部 _opening 字典（仅调试用途）。
        /// </summary>
        private Dictionary<string, UIBase> GetOpeningDict()
        {
            var f = typeof(UIManager).GetField("_opening", BindingFlags.NonPublic | BindingFlags.Instance);
            return f?.GetValue(UIManager.Instance) as Dictionary<string, UIBase>;
        }

        /// <summary>
        /// 创建 GUIStyle（支持富文本）。
        /// </summary>
        private void EnsureStyle()
        {
            if (_labelStyle != null) return;
            _labelStyle = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 14 };
        }
    }
}
