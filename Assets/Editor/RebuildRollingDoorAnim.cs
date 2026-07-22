using UnityEditor;
using UnityEngine;

/// <summary>
/// 用 Unity 官方 API 重建 rollingdoor_open.anim 的 PPtr (sprite swap) 曲线。
/// 菜单: Tools → Rebuild Rolling Door Animation
/// 执行前请确保 8 帧 sprite 已导入且 guid 有效。
/// </summary>
public static class RebuildRollingDoorAnim
{
    const string FRAME_DIR = "Assets/Art/Buildings/NeonTower/Sprites/Frames";
    const string CLIP_PATH = "Assets/Art/Buildings/NeonTower/Animations/rollingdoor_open.anim";

    // 前慢后快 ease-in (i/7)^1.5 * 0.65
    static readonly float[] Times =
    {
        0f,      // frame 0
        0.035f,  // frame 1
        0.099f,  // frame 2
        0.182f,  // frame 3
        0.281f,  // frame 4
        0.392f,  // frame 5
        0.516f,  // frame 6
        0.65f    // frame 7
    };

    [MenuItem("Tools/Rebuild Rolling Door Animation")]
    static void Rebuild()
    {
        // 1. 加载 8 帧 sprite
        Sprite[] frames = new Sprite[8];
        for (int i = 0; i < 8; i++)
        {
            string name = string.Format("rollingdoor_{0:D2}", i);
            string path = string.Format("{0}/{1}.png", FRAME_DIR, name);
            frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (frames[i] == null)
            {
                Debug.LogError("[RebuildRollingDoorAnim] Failed to load: " + path);
                EditorUtility.DisplayDialog("Error",
                    "Failed to load sprite:\n" + path +
                    "\n\nMake sure the 8 frame PNGs exist and are imported as Sprite (2D and UI).",
                    "OK");
                return;
            }
        }
        Debug.Log("[RebuildRollingDoorAnim] Loaded 8 frames OK.");

        // 2. 加载或创建 AnimationClip
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(CLIP_PATH);
        if (clip == null)
        {
            clip = new AnimationClip { name = "rollingdoor_open" };
            AssetDatabase.CreateAsset(clip, CLIP_PATH);
            Debug.Log("[RebuildRollingDoorAnim] Created new clip.");
        }
        else
        {
            Debug.Log("[RebuildRollingDoorAnim] Found existing clip, rebuilding curves.");
        }

        // 3. 清除所有现有曲线
        clip.ClearCurves();
        // 清除现有 events
        AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);

        // 4. 设置 PPtr 曲线 (sprite swap)
        //    path = ""  表示绑定到 Animator 所在的 GameObject 自身
        //    type = SpriteRenderer
        //    propertyName = "m_Sprite"
        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            path = "",
            type = typeof(SpriteRenderer),
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[8];
        for (int i = 0; i < 8; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = Times[i],
                value = frames[i]
            };
            Debug.Log(string.Format("  frame {0}: time={1:F3} sprite={2}", i, Times[i], frames[i].name));
        }

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);
        Debug.Log("[RebuildRollingDoorAnim] PPtr curve set OK.");

        // 5. 设置 AnimationEvent (SFX 占位)
        AnimationEvent animEvent = new AnimationEvent
        {
            time = 0f,
            functionName = "OnRollingDoorStart",
            floatParameter = 0f,
            intParameter = 0,
            objectReferenceParameter = null,
            messageOptions = SendMessageOptions.DontRequireReceiver
        };
        AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[] { animEvent });
        Debug.Log("[RebuildRollingDoorAnim] AnimationEvent set OK.");

        // 6. 设置 clip settings
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        settings.startTime = 0f;
        settings.stopTime = 0.65f;
        settings.hasAdditiveReferencePose = false;
        settings.level = 0f;
        settings.cycleOffset = 0f;
        settings.orientationOffsetY = 0f;
        settings.loopBlend = false;
        settings.loopBlendOrientation = false;
        settings.loopBlendPositionY = false;
        settings.loopBlendPositionXZ = false;
        settings.keepOriginalOrientation = false;
        settings.keepOriginalPositionY = false;
        settings.keepOriginalPositionXZ = false;
        settings.heightFromFeet = false;
        settings.mirror = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        Debug.Log("[RebuildRollingDoorAnim] Clip settings set OK. stopTime=" + settings.stopTime);

        // 7. 保存
        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[RebuildRollingDoorAnim] DONE! Animation rebuilt successfully.");

        EditorUtility.DisplayDialog("Success",
            "rollingdoor_open.anim has been rebuilt with 8 sprite-swap keyframes.\n\n" +
            "Binding: SpriteRenderer.m_Sprite (path=\"\")\n" +
            "Duration: 0.65s (ease-in)\n" +
            "Event: OnRollingDoorStart @ t=0\n\n" +
            "Now select RollingDoor in the prefab and open the Animation window.",
            "OK");
    }
}
