#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Functional.ECS.Editor
{
    /// <summary>
    /// 编辑器菜单：检查 Burst 相关包是否已写入 manifest 并提示当前编译状态。
    /// </summary>
    public static class BurstPackageValidator
    {
        private static readonly string[] RequiredPackages =
        {
            "com.unity.burst",
            "com.unity.collections",
            "com.unity.mathematics"
        };

        [MenuItem("Tools/Functional/检查 Burst 安装状态")]
        public static void CheckBurstInstallation()
        {
            var manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            string manifest = File.Exists(manifestPath) ? File.ReadAllText(manifestPath) : "";

            var report = "[Burst 检查报告]\n";
            foreach (var pkg in RequiredPackages)
            {
                bool inManifest = manifest.Contains($"\"{pkg}\"");
                report += inManifest ? $"  ✓ {pkg} 已在 manifest.json\n" : $"  ✗ {pkg} 未在 manifest.json\n";
            }

#if UNITY_BURST
            report += "  ✓ UNITY_BURST 编译宏已定义（Burst 代码会参与编译）\n";
            report += "\n结论：Burst 可用。场景中使用 ECSWorldRunner 并勾选 Use Burst When Available 即可。";
#else
            report += "  ✗ UNITY_BURST 未定义（Burst 代码未参与编译）\n";
            report += "\n结论：请重新打开 Unity 工程，等待 Package Manager 下载完成后重试本菜单。";
#endif

            Debug.Log(report);
            EditorUtility.DisplayDialog("Burst 安装检查", report, "确定");
        }
    }
}
#endif
