---
name: sbpk-resource-storage
description: >-
  SBPK Unity 项目资源存储与查找规则。处理预制体(prefab)、Art/Art_Replace 资源、对话产出脚本、
  Shader 放置路径时必须遵循。Use when creating/moving/finding prefabs, materials, meshes,
  textures, C# scripts, shaders, or any Assets under Art, Art_Replace, Scripts, or Shader.
---

# SBPK 资源存储管理

对话中涉及资源查找、创建、移动、引用时，必须按下列规则执行。不得擅自改放到正式业务目录。

## 1. 预制体及相关资源查找优先级

每次对预制体相关进行处理时（查找、引用、替换、实例化、改材质/网格等）：

1. **优先**在 `Assets/Art_Replace/3D_Prefab_replace/` 及其子文件夹中查找
2. **找不到**再在 `Assets/Art/` 及其子文件夹中查找

不要跳过 `Art_Replace` 直接用 `Art` 下的同名或同类资源。

## 2. 对话产出脚本存放

对话过程中新产出的脚本（`.cs` 等，**不包括** Shader）：

- 一律存放在 `Assets/Scripts/临时分配/`
- 不要自行归类到 `Gameplay`、`Base` 等正式目录；用户后续再划分文件夹

## 3. Shader 存放（必须先问）

对话过程中产出 Shader 文件（`.shader`、`.hlsl`、`.cginc` 等）时：

- **禁止**默认路径直接写入
- **必须先询问用户**放在以下哪一处，得到明确答复后再创建/保存：
  - `Assets/Shader/2D_shader/`
  - `Assets/Shader/3D_shader/`
  - `Assets/Shader/3D_shader_replace/`

## 快速对照

| 产出/操作 | 规则 |
|-----------|------|
| 找/用预制体及相关 | `Art_Replace/3D_Prefab_replace` → 找不到再用 `Art` |
| 新脚本（非 Shader） | `Assets/Scripts/临时分配/` |
| 新 Shader | 先问：`2D_shader` / `3D_shader` / `3D_shader_replace` |
