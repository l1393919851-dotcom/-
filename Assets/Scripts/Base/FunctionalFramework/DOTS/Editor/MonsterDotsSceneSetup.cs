#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Functional.DOTS.Editor
{
    /// <summary>
    /// 编辑器工具：创建 DOTS 测试用的 URP 主相机（Entities 1.2 无 GameObject/Entities 菜单）。
    /// </summary>
    public static class MonsterDotsSceneSetup
    {
        [MenuItem("Tools/Functional/创建 DOTS 测试相机")]
        public static void CreateDotsTestCamera()
        {
            var existing = GameObject.FindGameObjectWithTag("MainCamera");
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("创建 DOTS 测试相机",
                        "场景已有 MainCamera，是否仍新建一个 DOTS 测试相机？", "新建", "取消"))
                {
                    return;
                }
            }

            var camGo = new GameObject("Main Camera (DOTS)");
            camGo.tag = "MainCamera";

            var camera = camGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;

            camGo.AddComponent<UniversalAdditionalCameraData>();

            camGo.transform.position = new Vector3(0f, 80f, -120f);
            camGo.transform.rotation = Quaternion.Euler(35f, 0f, 0f);

            Selection.activeGameObject = camGo;
            Debug.Log("[DOTS] 已创建 URP 主相机。位置 (0,80,-120)，朝向场景中心。可按需调整。");
        }

        [MenuItem("Tools/Functional/创建 DOTS 测试相机", true)]
        public static bool ValidateCreateCamera() => !Application.isPlaying;
    }
}
#endif
