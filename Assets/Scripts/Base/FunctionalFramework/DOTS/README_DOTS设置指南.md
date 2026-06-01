# Unity DOTS + Entities Graphics 设置指南（SBPK）

适用于 **Unity 2022.3 LTS**。本目录提供大量怪物（10 万+）的推荐方案：**无 GameObject、GPU 实例化渲染**。

---

## 一、和当前轻量 ECS 的区别

| | 自研 `Functional.ECS` | Unity DOTS + Entities Graphics |
|--|----------------------|--------------------------------|
| 实体 | struct 组件 + 可选 GameObject | `Entity` + `IComponentData` |
| 渲染 | 每实体 `transform.position` | **Entities Graphics 自动合批** |
| 10 万同屏 | 不适合 | **适合** |
| 学习/迁移成本 | 低 | 中高 |
| 与 uGUI/Base UI | 完全兼容 | 兼容（需 URP） |

**结论：** 大量同屏怪物用本目录 DOTS 方案；UI、配置、音效等继续用 `Base` / `FunctionalFramework` 其他模块。

---

## 二、已添加的 Package（manifest.json）

| 包 | 版本 | 作用 |
|----|------|------|
| `com.unity.entities` | 1.2.4 | ECS 核心、默认 World、ISystem |
| `com.unity.entities.graphics` | 1.2.4 | 实体渲染（替代 10 万个 MeshRenderer） |
| `com.unity.render-pipelines.universal` | 14.0.11 | **URP（Entities Graphics 必需）** |
| `com.unity.burst` / `collections` / `mathematics` | 已有 | Job / 数学 |

> **重要：** Entities Graphics **不支持 Built-in 管线**，必须切换到 URP。

首次打开 Unity 后等待 Package Manager 下载完成。

---

## 三、必须完成的编辑器设置（一次性）

### 步骤 1：创建 URP 资源

1. 菜单 **Assets → Create → Rendering → URP Asset (with Universal Renderer)**
2. 命名为 `UniversalRP-HighFidelity`（或任意名）
3. 选中该 Asset，在 Inspector 中确认：
   - **Rendering Path** 为 **Forward+**（Entities Graphics 1.2 要求 Forward+）

### 步骤 2：指定项目使用 URP

1. **Edit → Project Settings → Graphics**
2. **Scriptable Render Pipeline Settings** 拖入上一步的 URP Asset
3. **Edit → Project Settings → Quality**（各档位）
4. 将 **Render Pipeline Asset** 同样设为该 URP Asset

### 步骤 3：创建 DOTS 用材质

1. **Assets → Create → Material**
2. Shader 选择 **Universal Render Pipeline / Lit**
3. 指定颜色（例如红色），保存为 `Assets/Materials/MonsterDots.mat`

### 步骤 4：搭建测试场景

1. 新建空场景（或清空测试场景）
2. **创建相机（任选一种）：**
   - **推荐：** 菜单 **Tools → Functional → 创建 DOTS 测试相机**（自动添加 Camera + URP 相机组件并摆好视角）
   - **或手动：** 场景里保留/新建 **Main Camera**，确保挂有 **Universal Additional Camera Data**（URP 项目一般会自动添加）
   - > **说明：** Entities 1.2 **没有** `GameObject → Entities` 菜单，普通 URP 主相机即可渲染 DOTS 实体。
3. 创建空物体 `MonsterDotsSpawner`，挂载脚本 `Functional.DOTS.MonsterDotsSpawner`
4. Inspector 配置：
   - **Mesh**：可留空（自动用 Cube）
   - **Material**：拖入上一步 URP Lit 材质（**必须**）
   - **Spawn Count**：先试 `10000`，再试 `100000`
   - **Spawn Per Frame**：`2000`（分帧生成，避免单帧卡死）
5. 运行 Play

Console 出现 `DOTS 实体生成完成` 且 Scene 视图能看到大量立方体即成功。

---

## 四、代码结构

```
DOTS/
├── Functional.DOTS.asmdef      # 独立程序集，引用 Entities / URP
├── Components/
│   └── MonsterDotsComponents.cs    # MonsterVelocity
├── Systems/
│   └── MonsterDotsMoveSystem.cs    # Burst ISystem，更新 LocalTransform
├── MonsterDotsSpawner.cs           # 运行时创建渲染原型 + 分帧 Instantiate
└── README_DOTS设置指南.md          # 本文件
```

### 数据流

```
MonsterDotsSpawner (主线程)
  → RenderMeshUtility.AddComponents 创建带渲染的 prototype
  → EntityManager.Instantiate × N（无 GameObject）

MonsterDotsMoveSystem (Burst, 每帧)
  → Query LocalTransform + MonsterVelocity
  → Position += Velocity * dt

Entities Graphics (自动)
  → 合批绘制相同 Mesh + Material 的实体
```

---

## 五、常用 API 速查

### 创建带渲染的实体原型

```csharp
var entity = em.CreateEntity();
var renderMeshArray = new RenderMeshArray(new[] { material }, new[] { mesh });
RenderMeshUtility.AddComponents(entity, em,
    new RenderMeshDescription(ShadowCastingMode.On, receiveShadows: true),
    renderMeshArray,
    MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
em.AddComponentData(entity, LocalTransform.FromPositionRotationScale(pos, rot, scale));
```

### 批量生成

```csharp
var instance = em.Instantiate(prototype);
em.SetComponentData(instance, LocalTransform.FromPosition(...));
```

### Burst 系统

```csharp
[BurstCompile]
public partial struct MySystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (t, v) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<MonsterVelocity>>())
            t.ValueRW.Position += v.ValueRO.Value * SystemAPI.Time.DeltaTime;
    }
}
```

### 与 Base UI 共存

- UI 仍用 `UIManager` / Canvas（Screen Space）
- 游戏逻辑实体用 DOTS World（`World.DefaultGameObjectInjectionWorld`）
- 不要在 DOTS 里为每个怪物 `new GameObject()`

---

## 六、性能与数量建议

| 数量 | 建议 |
|------|------|
| 1 万 | 默认配置即可 |
| 10 万 | 提高 `Spawn Per Frame`，关闭阴影或换简单 Mesh |
| 50 万+ | 考虑 LOD、剔除、简化材质、平台分档 |

仍卡顿时排查：

1. 材质是否为 **URP Lit**（Built-in Shader 会粉红或不显示）
2. Graphics 是否已指定 **URP Asset**
3. URP 是否为 **Forward+**
4. Profiler 中是否还有大量 `Instantiate`（应分帧）
5. 是否误开旧版 `ExampleMonsterSpawner`（会创建 10 万 GameObject）

---

## 七、推荐迁移路线（从现有项目）

```
阶段 1（当前）
  └─ 安装 Package + URP + MonsterDotsSpawner 验证 1 万～10 万

阶段 2
  └─ 用 Baker / SubScene 烘焙怪物预制体（替代纯代码 RenderMeshUtility）
  └─ 文档：Unity Manual → Baking entity prefabs

阶段 3
  └─ 接入 com.unity.physics（DOTS Physics）做碰撞
  └─ 接入 NetCode（若需要联机）

阶段 4
  └─ 与 Addressables + Entities 内容管道整合
```

### SubScene + Baker 示例（方向）

```csharp
// 挂在预制体上，Bake 时转成 Entity 预制体
public class MonsterAuthoring : MonoBehaviour
{
    public float Speed = 2f;
}

public class MonsterBaker : Baker<MonsterAuthoring>
{
    public override void Bake(MonsterAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Renderable);
        AddComponent(entity, new MonsterVelocity { Value = new float3(1, 0, 0) });
    }
}
```

将预制体放入 **SubScene**，运行时 `Instantiate` 实体预制体，适合正式项目资源管理。

---

## 八、常见问题

**Q: 画面全粉？**  
A: 材质 Shader 不是 URP Lit，或项目未切换到 URP。

**Q: 什么都看不见？**  
A: 主相机未对准场景中心；实体生成在 Y=0 平面。用 **Tools → Functional → 创建 DOTS 测试相机**，或把 Main Camera 移到 (0, 80, -120) 俯视。

**Q: 没有 GameObject → Entities 菜单？**  
A: **正常。** Entities 1.2 不提供该菜单，使用普通 URP Main Camera 即可。

**Q: Console 报 Entities World 未创建？**  
A: `com.unity.entities` 未下载完，重启 Unity。

**Q: 和旧 ECS 示例冲突吗？**  
A: 可同时存在；不要同场景同时跑 `ExampleMonsterSpawner`（GameObject 版）与 `MonsterDotsSpawner`。

**Q: WebGL 能用吗？**  
A: Entities Graphics 1.2 官方不支持 Web 平台。

---

## 九、参考链接

- [Entities Graphics 1.2 要求与兼容](https://docs.unity3d.com/Packages/com.unity.entities.graphics@1.2/manual/requirements-and-compatibility.html)
- [Entities 1.2 手册](https://docs.unity3d.com/Packages/com.unity.entities@1.2/manual/index.html)
- [Upgrade guide to Entities 1.0](https://docs.unity3d.com/Packages/com.unity.entities@1.2/manual/upgrade-guide.html)
