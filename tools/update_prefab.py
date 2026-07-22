"""
Surgically edit NeonTower.prefab to add Interior layer between Base and Neon.
"""
import re

PREFAB = "E:/测试1号/SBPK/Assets/Art/Buildings/NeonTower/Prefabs/NeonTower.prefab"

with open(PREFAB, 'r', encoding='utf-8') as f:
    text = f.read()

# === 1. Update root Transform m_Children to include Interior ===
# Find: "  m_Children:\n  - {fileID: 400001}\n  - {fileID: 400002}\n"
# Replace with: include 400003
old_children = "  m_Children:\n  - {fileID: 400001}\n  - {fileID: 400002}\n  m_Father:"
new_children = "  m_Children:\n  - {fileID: 400001}\n  - {fileID: 400002}\n  - {fileID: 400003}\n  m_Father:"
assert old_children in text, "root Transform children block not found"
text = text.replace(old_children, new_children)

# === 2. Update Neon SpriteRenderer m_SortingOrder from 1 to 2 ===
# Find the Neon SpriteRenderer block, change its sortingOrder
# The Neon's sortingOrder is in a block that also has m_Color before
# Use a precise context-aware replacement
old_neon_sorting = """  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 1
  m_Sprite: {fileID: 21300000, guid: be89f4b12c3d4e5f6a7b8c9d0e1f2a3c, type: 3}"""
new_neon_sorting = """  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 2
  m_Sprite: {fileID: 21300000, guid: be89f4b12c3d4e5f6a7b8c9d0e1f2a3c, type: 3}"""
assert old_neon_sorting in text, "Neon sortingOrder block not found"
text = text.replace(old_neon_sorting, new_neon_sorting)

# === 3. Append Interior blocks (GameObject + Transform + SpriteRenderer) at end of file ===

# New Interior blocks - matching style of existing Base/Neon
interior_blocks = """--- !u!1 &400000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 400003}
  - component: {fileID: 212004}
  m_Layer: 0
  m_Name: Interior
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &400003
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 400000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 400000}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!212 &212004
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 400000}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 0
  m_ReflectionProbeUsage: 0
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {fileID: 2100001, guid: af5b6375d1677e941bb572a47bf0108e, type: 2}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {fileID: 0}
  m_ProbeAnchor: {fileID: 0}
  m_LightProbeVolumeOverride: {fileID: 0}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {fileID: 0}
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 1
  m_Sprite: {fileID: 21300000, guid: a0e2c6c0d6464a43857722ad90fbf877, type: 3}
  m_Color: {r: 1, g: 1, b: 1, a: 1}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {x: 10, y: 12.5}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0
"""

# Make sure the Interior block isn't already there
if "--- !u!1 &400000" in text and "m_Name: Interior" in text:
    print("Interior block already exists - skipping append")
else:
    text = text.rstrip() + "\n" + interior_blocks
    print("Appended Interior blocks")

# Save
with open(PREFAB, 'w', encoding='utf-8') as f:
    f.write(text)

print(f"\nPrefab updated: {PREFAB}")
print("Changes:")
print("  - Root Transform m_Children: added Interior (400003)")
print("  - Neon SpriteRenderer sortingOrder: 1 -> 2")
print("  - Added Interior GameObject (400000), Transform (400003), SpriteRenderer (212004)")
print("  - Interior references sprite: bldg_neon_tower_interior.png")
print("  - Interior references material: mat_neon_base (reused)")
