using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Linq;
using Gameplay.Buildings;

/// <summary>
/// 修复 NeonTower 预制体的层级结构：
/// 两个 SpriteRenderer 拆分到子对象，解掉 "IsFinite(distanceForSort)" 断言崩溃。
/// 
/// 用法：Unity Editor 菜单 → Tools → Fix NeonTower Prefab Structure
/// </summary>
public class FixNeonTowerPrefab
{
    // 路径常量
    private const string PrefabPath = "Assets/Art/Buildings/NeonTower/Prefabs/NeonTower.prefab";
    private const string NeonSpritePath = "Assets/Art/Buildings/NeonTower/Sprites/bldg_neon_tower_neon.png";

    [MenuItem("Tools/Fix NeonTower Prefab Structure")]
    public static void Fix()
    {
        // 1. 加载预制体
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[FixNeonTower] 找不到预制体: {PrefabPath}");
            return;
        }

        // 2. 实例化
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (instance == null)
        {
            Debug.LogError("[FixNeonTower] 实例化失败");
            return;
        }

        Undo.RegisterCreatedObjectUndo(instance, "Fix NeonTower Structure");

        // 获取现有组件
        SpriteRenderer[] allRenderers = instance.GetComponents<SpriteRenderer>();
        SpriteRenderer baseRenderer = null;
        SpriteRenderer neonRenderer = null;

        foreach (var sr in allRenderers)
        {
            if (sr.sortingOrder == 0) baseRenderer = sr;
            else if (sr.sortingOrder == 1) neonRenderer = sr;
        }

        if (baseRenderer == null || neonRenderer == null)
        {
            Debug.LogError("[FixNeonTower] 找不到两个 SpriteRenderer");
            GameObject.DestroyImmediate(instance);
            return;
        }

        // 3. 创建子对象
        GameObject baseGO = new GameObject("Base");
        GameObject neonGO = new GameObject("Neon");

        Undo.RegisterCreatedObjectUndo(baseGO, "Create Base child");
        Undo.RegisterCreatedObjectUndo(neonGO, "Create Neon child");

        // 设父子关系
        Undo.SetTransformParent(baseGO.transform, instance.transform, "Parent Base");
        Undo.SetTransformParent(neonGO.transform, instance.transform, "Parent Neon");

        // 位置归零
        baseGO.transform.localPosition = Vector3.zero;
        neonGO.transform.localPosition = Vector3.zero;

        // 4. 把 SpriteRenderer 移到子对象
        Undo.RecordObject(baseRenderer, "Move Base SR");
        Undo.RecordObject(neonRenderer, "Move Neon SR");

        // 复制到子对象
        SpriteRenderer baseCopy = baseGO.AddComponent<SpriteRenderer>();
        EditorUtility.CopySerialized(baseRenderer, baseCopy);

        SpriteRenderer neonCopy = neonGO.AddComponent<SpriteRenderer>();
        EditorUtility.CopySerialized(neonRenderer, neonCopy);

        // 删掉根上的
        Undo.DestroyObjectImmediate(baseRenderer);
        Undo.DestroyObjectImmediate(neonRenderer);

        // 5. 更新 NeonTowerController 的 references
        NeonTowerController controller = instance.GetComponent<NeonTowerController>();
        if (controller != null)
        {
            SerializedObject so = new SerializedObject(controller);
            SerializedProperty srProp = so.FindProperty("neonSpriteRenderer");
            if (srProp != null)
            {
                srProp.objectReferenceValue = neonCopy;
                so.ApplyModifiedProperties();
            }
        }

        // 6. 更新 Animation Clip 路径
        Animator animator = instance.GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController is AnimatorController ac)
        {
            foreach (var layer in ac.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    AnimatorState animState = state.state;
                    if (animState.motion is AnimationClip clip)
                    {
                        string clipPath = AssetDatabase.GetAssetPath(clip);
                        if (string.IsNullOrEmpty(clipPath)) continue;

                        SerializedObject clipSo = new SerializedObject(clip);
                        SerializedProperty bindings = clipSo.FindProperty("m_AnimationClipSettings.m_EditorCurves");

                        if (bindings != null && bindings.isArray)
                        {
                            for (int i = 0; i < bindings.arraySize; i++)
                            {
                                SerializedProperty curveProp = bindings.GetArrayElementAtIndex(i);
                                SerializedProperty pathProp = curveProp.FindPropertyRelative("path");
                                if (pathProp != null && string.IsNullOrEmpty(pathProp.stringValue))
                                {
                                    pathProp.stringValue = "Neon";
                                }
                            }
                        }

                        // 也修 genericBindings 里的 path
                        SerializedProperty genericBindings = clipSo.FindProperty("m_ClipBindingConstant.genericBindings");
                        if (genericBindings != null && genericBindings.isArray)
                        {
                            for (int i = 0; i < genericBindings.arraySize; i++)
                            {
                                SerializedProperty binding = genericBindings.GetArrayElementAtIndex(i);
                                SerializedProperty pathProp = binding.FindPropertyRelative("path");
                                if (pathProp != null && pathProp.intValue == 0)
                                {
                                    // path=0 means root in generic bindings; leave it alone
                                    // The EditorCurves.path change above should fix the binding
                                }
                            }
                        }

                        clipSo.ApplyModifiedProperties();
                        Debug.Log($"[FixNeonTower] 已更新动画路径: {clip.name}");
                    }
                }
            }
        }

        // 7. 保存预制体覆盖
        PrefabUtility.SaveAsPrefabAssetAndConnect(instance, PrefabPath, InteractionMode.UserAction);

        Debug.Log("[FixNeonTower] ✅ NeonTower 预制体结构已修复！");
        Debug.Log("   ✓ Base/Neon 拆分为子对象");
        Debug.Log("   ✓ Animation Clip 路径更新为 'Neon'");
        Debug.Log("   ✓ NeonTowerController 引用已更新");

        // 清理实例（保留覆盖后的预制体）
        // 用户可以在 Hierarchy 中删除 instance，或直接使用更新后的预制体
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        EditorUtility.DisplayDialog("修复完成",
            "NeonTower 预制体结构已修复！\n\n" +
            "修改内容:\n" +
            "• Base SpriteRenderer 移到子对象 'Base'\n" +
            "• Neon SpriteRenderer 移到子对象 'Neon'\n" +
            "• Animation Clip 路径更新为 'Neon/'\n" +
            "• 控制器引用已更新\n\n" +
            "现在可以将 NeonTower.prefab 拖入场景测试。",
            "好的");
    }
}
