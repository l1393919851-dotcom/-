"""
为 8 个序列帧生成 .meta 文件
- 唯一 guid
- Sprite, PPU=32, pivot 底中 (0.5, 0)
- maxTextureSize=1024 (够用)
"""
import os
import hashlib

frames_dir = 'Assets/Art/Buildings/NeonTower/Sprites/Frames'

# 预生成的稳定 guids (基于文件名 hash, 保证每次运行都一样)
def make_guid(name):
    h = hashlib.md5(name.encode('utf-8')).hexdigest()[:32]
    return h

frame_guids = {
    i: make_guid(f'rollingdoor_{i:02d}_neon_tower')
    for i in range(8)
}

print('帧 guids:')
for i, g in frame_guids.items():
    print(f'  rollingdoor_{i:02d}.png: {g}')

# .meta 模板
def make_meta(guid, sprite_id_suffix):
    return f'''fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 12
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 1024
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 0
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 7
  spritePivot: {{x: 0.5, y: 0}}
  spritePixelsToUnits: 32
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 3
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 1024
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: {sprite_id_suffix}
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    nameFileIdTable: {{}}
  mipmapEnable: 0
  mipmapLimitGroupName:
  spritePixelsToUnitsMode: 0
  mipmapBias: 0
  pSDRemoveMatte: 0
  pSRGBRemoveMatteEnable: 0
  overridePlatformSettings: 0
  spriteConfigData:
    generateShadowCaster: 0
  assetBundleName:
  assetBundleVariant:
'''

# 写 .meta
for i in range(8):
    guid = frame_guids[i]
    sprite_id = f'doorframe{i:02d}' + hashlib.md5(str(i).encode()).hexdigest()[:24]
    meta_content = make_meta(guid, sprite_id)
    meta_path = f'{frames_dir}/rollingdoor_{i:02d}.png.meta'
    with open(meta_path, 'w', encoding='utf-8') as f:
        f.write(meta_content)
    print(f'  写: rollingdoor_{i:02d}.png.meta (guid: {guid})')

print('\n完成!')
print('\n保存 guids 供 .anim 使用:')
import json
with open('tools/_frame_guids.json', 'w') as f:
    json.dump(frame_guids, f, indent=2)
print('  tools/_frame_guids.json')
