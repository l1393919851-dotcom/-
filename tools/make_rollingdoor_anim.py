"""
生成新的 rollingdoor_open.anim:
- Sprite Swap 动画 (m_PPtrCurves)
- 8 帧, 前慢后快 (ease-in, 总时长 0.65s)
- 开头 0s 触发 SFX 事件 (调 Controller.OnRollingDoorStart)
"""
import json

# 加载帧 guids
with open('tools/_frame_guids.json', 'r') as f:
    guids = json.load(f)

# 帧时间 (前慢后快, ease-in 曲线)
# t = (i/7)^1.5, total 0.65s
times = [0.0]
for i in range(1, 8):
    t_norm = (i / 7) ** 1.5
    times.append(round(t_norm * 0.65, 3))
# 修整: 最后一帧必须在 stopTime
times[-1] = 0.65

print('帧时间:')
for i, t in enumerate(times):
    print(f'  frame {i}: {t}s')

# .anim YAML
# 关键: m_PPtrCurves for sprite swap
# binding: typeID=212 (SpriteRenderer), attribute=3738242886 (m_Sprite), isPPtrCurve=1

pptr_entries = []
for i, t in enumerate(times):
    pptr_entries.append(f'''      - time: {t}
        value: {{fileID: 21300000, guid: {guids[str(i)]}, type: 3}}''')

anim_yaml = f'''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!74 &7400000
AnimationClip:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: rollingdoor_open
  serializedVersion: 7
  m_Legacy: 0
  m_Compressed: 0
  m_UseHighQualityCurve: 1
  m_RotationCurves: []
  m_CompressedRotationCurves: []
  m_EulerCurves: []
  m_PositionCurves: []
  m_ScaleCurves: []
  m_FloatCurves: []
  m_PPtrCurves:
  - curve:
{chr(10).join(pptr_entries)}
    m_PreInfinity: 2
    m_PostInfinity: 2
  m_SampleRate: 60
  m_WrapMode: 0
  m_Bounds:
    m_Center: {{x: 0, y: 0, z: 0}}
    m_Extent: {{x: 0, y: 0, z: 0}}
  m_ClipBindingConstant:
    genericBindings:
    - serializedVersion: 2
      path: 0
      attribute: 3738242886
      script: {{fileID: 0}}
      typeID: 212
      customType: 0
      isPPtrCurve: 1
      isIntCurve: 0
      isSerializeReferenceCurve: 0
    pptrCurveMapping: []
  m_AnimationClipSettings:
    serializedVersion: 2
    m_AdditiveReferencePoseClip: {{fileID: 0}}
    m_AdditiveReferencePoseTime: 0
    m_StartTime: 0
    m_StopTime: 0.65
    m_OrientationOffsetY: 0
    m_Level: 0
    m_CycleOffset: 0
    m_HasAdditiveReferencePose: 0
    m_LoopTime: 0
    m_LoopBlend: 0
    m_LoopBlendOrientation: 0
    m_LoopBlendPositionY: 0
    m_LoopBlendPositionXZ: 0
    m_KeepOriginalOrientation: 0
    m_KeepOriginalPositionY: 0
    m_KeepOriginalPositionXZ: 0
    m_HeightFromFeet: 0
    m_Mirror: 0
  m_EditorCurves: []
  m_EulerEditorCurves: []
  m_HasGenericRootQuaternion: 0
  m_HasMotionFloatCurves: 0
  m_Events:
  - time: 0
    functionName: OnRollingDoorStart
    data:
    objectReferenceParameter: {{fileID: 0}}
    floatParameter: 0
    intParameter: 0
    messageOptions: 0
'''

with open('Assets/Art/Buildings/NeonTower/Animations/rollingdoor_open.anim', 'w', encoding='utf-8') as f:
    f.write(anim_yaml)

print('\n写: Assets/Art/Buildings/NeonTower/Animations/rollingdoor_open.anim')
print('  - 8 帧 sprite swap')
print('  - ease-in timing (前慢后快), 总 0.65s')
print('  - 0s 触发 OnRollingDoorStart 事件 (SFX 占位)')
