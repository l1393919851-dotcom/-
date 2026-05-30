// =============================================================================
// 预制体 UI 自动绑定代码生成器（仅编辑器模式可用）。
//
// 功能：扫描选中的预制体，根据"前缀_名字"的命名约定生成 partial 类绑定字段
//      和 AutoBindWidgets 方法。业务类只要在 OnInit 里调用一次 AutoBindWidgets()
//      就能拿到所有 UI 组件引用，省去手写一堆 transform.Find。
//
// 命名约定：
//   _btn_xxx     → UnityEngine.UI.Button         字段名 BtnXxx
//   _txt_xxx     → UnityEngine.UI.Text           字段名 TxtXxx
//   _img_xxx     → UnityEngine.UI.Image          字段名 ImgXxx
//   _raw_xxx     → UnityEngine.UI.RawImage       字段名 RawXxx
//   _input_xxx   → UnityEngine.UI.InputField     字段名 InputXxx
//   _scroll_xxx  → UnityEngine.UI.ScrollRect     字段名 ScrollXxx
//   _slider_xxx  → UnityEngine.UI.Slider         字段名 SliderXxx
//   _toggle_xxx  → UnityEngine.UI.Toggle         字段名 ToggleXxx
//   _tmp_xxx     → TMPro.TMP_Text                字段名 TmpXxx       (需要 BASE_TMP_SUPPORT)
//   _tmpinput_xxx→ TMPro.TMP_InputField          字段名 TmpInputXxx  (需要 BASE_TMP_SUPPORT)
//   _tr_xxx      → UnityEngine.RectTransform     字段名 TrXxx
//   _go_xxx      → UnityEngine.GameObject        字段名 GoXxx
//
// 入口：
//   1) 菜单 "Tools/Base/UI Bindings Generator..." 打开窗口
//   2) 在 Project 面板右键预制体 → "Base/Generate UI Bindings"
// =============================================================================
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Base.UIFramework.EditorTools
{
    /// <summary>
    /// 编辑器窗口：UI 自动绑定代码生成器。
    /// 通过菜单 Tools/Base/UI Bindings Generator... 打开。
    /// </summary>
    public class UIBindingsGenerator : EditorWindow
    {
        private GameObject _prefab;
        private string _namespace = "Game.UI";
        private string _className = "";
        private DefaultAsset _outputFolder;
        private Vector2 _scroll;

        /// <summary>
        /// 菜单项：打开生成器窗口。
        /// </summary>
        [MenuItem("Tools/Base/UI Bindings Generator...")]
        public static void OpenWindow()
        {
            var win = GetWindow<UIBindingsGenerator>("UI Bindings Generator");
            win.minSize = new Vector2(420, 360);
        }

        /// <summary>
        /// 右键菜单：在 Project 面板选中预制体后，可直接生成。
        /// </summary>
        [MenuItem("Assets/Base/Generate UI Bindings", true)]
        private static bool ValidateGenerateFromAsset()
        {
            return Selection.activeObject is GameObject;
        }

        /// <summary>
        /// 右键菜单的真正入口：自动填充并弹窗。
        /// </summary>
        [MenuItem("Assets/Base/Generate UI Bindings", false, 2000)]
        private static void GenerateFromAsset()
        {
            var win = GetWindow<UIBindingsGenerator>("UI Bindings Generator");
            win._prefab = Selection.activeObject as GameObject;
            win._className = win._prefab != null ? win._prefab.name : "";
            win.Repaint();
        }

        /// <summary>
        /// 绘制编辑器界面。
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.LabelField("UI 绑定代码生成器", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "用法：把要生成绑定的预制体拖到 Prefab 槽里，设置好命名空间、类名、输出目录，点击 Generate。\n" +
                "命名约定：_btn_xxx → BtnXxx (Button)，_txt_xxx → TxtXxx (Text)，等等。", MessageType.Info);

            _prefab = EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), false) as GameObject;
            if (_prefab != null && string.IsNullOrEmpty(_className)) _className = _prefab.name;
            _namespace = EditorGUILayout.TextField("Namespace", _namespace);
            _className = EditorGUILayout.TextField("Class Name", _className);
            _outputFolder = EditorGUILayout.ObjectField("Output Folder", _outputFolder, typeof(DefaultAsset), false) as DefaultAsset;

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(_prefab == null || string.IsNullOrEmpty(_className)))
            {
                if (GUILayout.Button("Generate", GUILayout.Height(36))) Generate();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("识别到的字段预览：", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(120));
            if (_prefab != null)
            {
                foreach (var f in CollectFields(_prefab))
                {
                    EditorGUILayout.LabelField($"{f.TypeName}  {f.FieldName}", f.NodePath);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 触发生成：收集字段 → 拼接代码 → 写文件 → 刷新资源数据库。
        /// </summary>
        private void Generate()
        {
            var fields = CollectFields(_prefab);
            if (fields.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有找到任何符合命名约定的节点。", "OK");
                return;
            }
            var code = BuildCode(fields);
            var folder = GetOutputFolder();
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            var filePath = Path.Combine(folder, _className + ".Generated.cs");
            File.WriteAllText(filePath, code, new UTF8Encoding(false));
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("生成完成", $"已生成：\n{filePath}\n\n请在你的业务类（partial class {_className}）里调用 AutoBindWidgets()。", "OK");
        }

        /// <summary>
        /// 获取最终输出目录（绝对路径）。
        /// </summary>
        private string GetOutputFolder()
        {
            if (_outputFolder != null)
            {
                var path = AssetDatabase.GetAssetPath(_outputFolder);
                return Path.GetFullPath(path);
            }
            return Path.GetFullPath("Assets/Scripts/UI");
        }

        /// <summary>
        /// 遍历预制体，按命名约定收集需要绑定的节点。
        /// </summary>
        private static List<BindingField> CollectFields(GameObject prefab)
        {
            var list = new List<BindingField>();
            if (prefab == null) return list;
            var instance = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));
            try
            {
                CollectRecursive(instance.transform, instance.transform, list);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(instance);
            }
            return list;
        }

        /// <summary>
        /// 递归收集：对每一个节点判断是否匹配命名约定。
        /// </summary>
        private static void CollectRecursive(Transform root, Transform current, List<BindingField> list)
        {
            if (current != root && current.name.StartsWith("_"))
            {
                var field = ParseNode(current.name);
                if (field != null)
                {
                    field.NodePath = GetRelativePath(root, current);
                    list.Add(field);
                }
            }
            for (int i = 0; i < current.childCount; i++)
            {
                CollectRecursive(root, current.GetChild(i), list);
            }
        }

        /// <summary>
        /// 解析节点名："_btn_shop_buy" → 前缀 btn + 名字 ShopBuy。
        /// 未匹配的前缀返回 null。
        /// </summary>
        private static BindingField ParseNode(string nodeName)
        {
            var parts = nodeName.Split(new[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return null;
            var prefix = parts[0].ToLower();
            var nameParts = new List<string>();
            for (int i = 1; i < parts.Length; i++) nameParts.Add(CapitalizeFirst(parts[i]));
            var nameSuffix = string.Concat(nameParts);
            switch (prefix)
            {
                case "btn":      return new BindingField { TypeName = "UnityEngine.UI.Button",     FieldName = "Btn" + nameSuffix };
                case "txt":      return new BindingField { TypeName = "UnityEngine.UI.Text",       FieldName = "Txt" + nameSuffix };
                case "img":      return new BindingField { TypeName = "UnityEngine.UI.Image",      FieldName = "Img" + nameSuffix };
                case "raw":      return new BindingField { TypeName = "UnityEngine.UI.RawImage",   FieldName = "Raw" + nameSuffix };
                case "input":    return new BindingField { TypeName = "UnityEngine.UI.InputField", FieldName = "Input" + nameSuffix };
                case "scroll":   return new BindingField { TypeName = "UnityEngine.UI.ScrollRect", FieldName = "Scroll" + nameSuffix };
                case "slider":   return new BindingField { TypeName = "UnityEngine.UI.Slider",     FieldName = "Slider" + nameSuffix };
                case "toggle":   return new BindingField { TypeName = "UnityEngine.UI.Toggle",     FieldName = "Toggle" + nameSuffix };
                case "tmp":      return new BindingField { TypeName = "TMPro.TMP_Text",            FieldName = "Tmp" + nameSuffix,      RequiresTMP = true };
                case "tmpinput": return new BindingField { TypeName = "TMPro.TMP_InputField",      FieldName = "TmpInput" + nameSuffix, RequiresTMP = true };
                case "tr":       return new BindingField { TypeName = "UnityEngine.RectTransform", FieldName = "Tr" + nameSuffix };
                case "go":       return new BindingField { TypeName = "UnityEngine.GameObject",    FieldName = "Go" + nameSuffix, IsGameObject = true };
            }
            return null;
        }

        /// <summary>
        /// 首字母大写工具方法。
        /// </summary>
        private static string CapitalizeFirst(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        /// <summary>
        /// 计算节点相对于预制体根节点的层级路径，用于 Transform.Find。
        /// </summary>
        private static string GetRelativePath(Transform root, Transform t)
        {
            var stack = new Stack<string>();
            while (t != null && t != root)
            {
                stack.Push(t.name);
                t = t.parent;
            }
            return string.Join("/", stack);
        }

        /// <summary>
        /// 拼接最终生成代码。
        /// </summary>
        private string BuildCode(List<BindingField> fields)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// =============================================================================");
            sb.AppendLine("// 自动生成文件，请勿手动修改。");
            sb.AppendLine("// 由 Tools/Base/UI Bindings Generator 生成。");
            sb.AppendLine("// =============================================================================");
            sb.AppendLine();

            bool needTMP = false;
            foreach (var f in fields) if (f.RequiresTMP) { needTMP = true; break; }
            if (needTMP) sb.AppendLine("#if BASE_TMP_SUPPORT");

            sb.AppendLine($"namespace {_namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public partial class {_className}");
            sb.AppendLine("    {");
            foreach (var f in fields)
            {
                sb.AppendLine($"        /// <summary>节点路径: {f.NodePath}</summary>");
                sb.AppendLine($"        private {f.TypeName} {f.FieldName};");
            }
            sb.AppendLine();
            sb.AppendLine("        /// <summary>自动绑定所有 UI 组件。请在 OnInit 里调用一次。</summary>");
            sb.AppendLine("        protected void AutoBindWidgets()");
            sb.AppendLine("        {");
            foreach (var f in fields)
            {
                if (f.IsGameObject)
                {
                    sb.AppendLine($"            {f.FieldName} = transform.Find(\"{f.NodePath}\")?.gameObject;");
                }
                else if (f.TypeName == "UnityEngine.RectTransform")
                {
                    sb.AppendLine($"            {f.FieldName} = transform.Find(\"{f.NodePath}\") as UnityEngine.RectTransform;");
                }
                else
                {
                    sb.AppendLine($"            {f.FieldName} = transform.Find(\"{f.NodePath}\")?.GetComponent<{f.TypeName}>();");
                }
            }
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            if (needTMP) sb.AppendLine("#endif");
            return sb.ToString();
        }

        /// <summary>
        /// 内部数据结构：一个待生成的字段定义。
        /// </summary>
        private class BindingField
        {
            /// <summary>完整类型名（含命名空间）。</summary>
            public string TypeName;
            /// <summary>字段名（PascalCase）。</summary>
            public string FieldName;
            /// <summary>相对预制体根的节点路径。</summary>
            public string NodePath;
            /// <summary>是否依赖 TMP 包（影响生成 #if 块）。</summary>
            public bool RequiresTMP;
            /// <summary>是否是 GameObject 类型（生成的查找代码不同）。</summary>
            public bool IsGameObject;
        }
    }
}
