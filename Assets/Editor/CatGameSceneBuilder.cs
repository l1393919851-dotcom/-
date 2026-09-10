#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class CatGameSceneBuilder
{
    [MenuItem("Tools/猫舍物语/一键创建游戏场景", false, 1)]
    public static void BuildScene()
    {
        // 创建新场景
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 创建摄像机
        GameObject camGo = new GameObject("Main Camera");
        Camera cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.backgroundColor = new Color(1f, 0.96f, 0.9f);
        camGo.transform.position = new Vector3(0, 0, -10);
        camGo.tag = "MainCamera";

        // 创建游戏控制器
        GameObject managerGo = new GameObject("CatGameManager");
        managerGo.AddComponent<CatGameMain>();

        // 保存场景
        string scenePath = "Assets/Scenes/CatGame.unity";
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
        EditorSceneManager.SaveScene(newScene, scenePath);

        Debug.Log("[猫舍物语] 场景创建完成！按 Play 即可运行游戏。");
        EditorUtility.DisplayDialog("猫舍物语", "场景创建完成！\n\n按 Play 按钮即可运行游戏。\n\n游戏会自动构建所有UI，无需手动设置。", "好的");
    }

    [MenuItem("Tools/猫舍物语/清空存档", false, 2)]
    public static void ClearSave()
    {
        if (EditorUtility.DisplayDialog("清空存档", "确定要清空所有游戏存档吗？\n这不可撤销！", "确定", "取消"))
        {
            PlayerPrefs.DeleteKey("CatGameSave");
            PlayerPrefs.Save();
            Debug.Log("[猫舍物语] 存档已清空");
        }
    }
}
#endif
