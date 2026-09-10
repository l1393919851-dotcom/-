#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Gameplay.Buildings;

/// <summary>
/// Creates an Octopath-style isometric scene to preview NeonTower as a fake-3D box.
/// Menu: Tools → NeonTower → 创建八方旅人俯视角预览场景
/// </summary>
public static class NeonTowerIsoSceneBuilder
{
    const string ScenePath = "Assets/Scenes/NeonTowerIsoPreview.unity";
    const string PrefabPath = "Assets/Art/Buildings/NeonTower/Prefabs/NeonTower.prefab";
    const string FrontSpritePath = "Assets/Art/Buildings/NeonTower/Sprites/bldg_neon_tower_base.png";
    const string NeonSpritePath = "Assets/Art/Buildings/NeonTower/Sprites/bldg_neon_tower_neon.png";
    const string FrontMatPath = "Assets/Art/Buildings/NeonTower/Materials/mat_neon_base.mat";
    const string NeonMatPath = "Assets/Art/Buildings/NeonTower/Materials/mat_neon_additive.mat";

    [MenuItem("Tools/NeonTower/创建八方旅人俯视角预览场景", false, 10)]
    public static void BuildScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Directional light for slight depth shading on mesh ground/roof
        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.92f, 0.85f);
        light.intensity = 1.1f;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Fake 3D building
        var buildingGo = new GameObject("Fake3D_NeonTower");
        var box = buildingGo.AddComponent<Fake3DBuildingBox>();

        var front = AssetDatabase.LoadAssetAtPath<Sprite>(FrontSpritePath);
        var neon = AssetDatabase.LoadAssetAtPath<Sprite>(NeonSpritePath);
        var frontMat = AssetDatabase.LoadAssetAtPath<Material>(FrontMatPath);
        var neonMat = AssetDatabase.LoadAssetAtPath<Material>(NeonMatPath);

        if (front == null)
        {
            EditorUtility.DisplayDialog("NeonTower Iso", $"找不到正面贴图:\n{FrontSpritePath}", "OK");
            return;
        }

        // Use SerializedObject so private fields persist in the scene
        var so = new SerializedObject(box);
        so.FindProperty("frontSprite").objectReferenceValue = front;
        so.FindProperty("neonSprite").objectReferenceValue = neon;
        so.FindProperty("frontMaterial").objectReferenceValue = frontMat;
        so.FindProperty("neonMaterial").objectReferenceValue = neonMat;
        float autoDepth = Mathf.Max(6f, front.bounds.size.x * 0.2f);
        // Content is ~484px wide inside 1280 — prefer visual mesh width after rebuild.
        so.FindProperty("depth").floatValue = autoDepth;
        so.ApplyModifiedPropertiesWithoutUndo();
        box.Rebuild(); // preview in Scene view; Start() rebuilds again in Play

        // Flat prefab reference off to the side (far enough for wide sprites)
        float refX = Mathf.Max(30f, front.bounds.size.x * 0.7f + 10f);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab != null)
        {
            var flat = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            flat.name = "NeonTower_Flat_Reference";
            flat.transform.position = new Vector3(refX, 0f, 0f);
        }

        // Camera
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 12f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.05f, 0.1f, 1f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 200f;
        camGo.AddComponent<AudioListener>();

        // URP camera data if available
        var urpType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
        if (urpType != null)
            camGo.AddComponent(urpType);

        var orbit = camGo.AddComponent<IsoOrbitCamera>();
        var orbitSo = new SerializedObject(orbit);
        orbitSo.FindProperty("target").objectReferenceValue = buildingGo.transform;
        float focusY = front.bounds.size.y * 0.45f;
        orbitSo.FindProperty("targetOffset").vector3Value = new Vector3(0f, focusY, 0f);
        orbitSo.FindProperty("yaw").floatValue = 45f;
        orbitSo.FindProperty("pitch").floatValue = 35f;
        orbitSo.FindProperty("distance").floatValue = Mathf.Max(28f, front.bounds.size.x * 1.2f);
        orbitSo.FindProperty("orthoSize").floatValue = Mathf.Max(10f, front.bounds.size.y * 0.55f);
        orbitSo.ApplyModifiedPropertiesWithoutUndo();

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.Refresh();

        // Ensure playable from Build Settings (optional add)
        var scenes = EditorBuildSettings.scenes;
        bool found = false;
        foreach (var s in scenes)
        {
            if (s.path == ScenePath) { found = true; break; }
        }
        if (!found)
        {
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes)
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            EditorBuildSettings.scenes = list.ToArray();
        }

        Debug.Log($"[NeonTower Iso] 场景已创建: {ScenePath}");
        EditorUtility.DisplayDialog(
            "八方旅人俯视角预览",
            "场景已创建并打开。\n\n按 Play 后操作：\n" +
            "Q / E — 旋转 45°\n" +
            "右键拖拽 — 自由环绕\n" +
            "滚轮 — 缩放\n" +
            "WASD / 中键 — 平移\n" +
            "F — 焦点回到大楼\n\n" +
            "左侧是伪 3D 盒子大楼，右侧远处有原平面预制体对照。",
            "好的");
    }
}
#endif
