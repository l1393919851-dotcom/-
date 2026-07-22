# NeonTower 预制体说明

> **项目**：赛博不夜城（Cyber Never Sleeps）
> **资产类型**：7 层赛博朋克霓虹大楼预制体
> **创建者**：程基岩（技术/引擎工程）
> **依据规格**：`design/art/大厦霓虹楼_视觉规格.md` v1.0
> **Unity 管线**：URP 14.0.11 · URP 2D Renderer · PPU = 32
> **日期**：2025-07-21

---

## 1. 预制体层级结构

```
NeonTower (GameObject)
│
├── Transform                           ← Position: (0, 0, 0), Scale: (1, 1, 1)
│
├── SpriteRenderer [Base]               ← Order in Layer: 0
│   ├── Sprite: bldg_neon_tower_base    (楼体底图，仅废墟色系)
│   ├── Material: (默认 Sprite)
│   └── Color: White (r:1, g:1, b:1, a:1)
│
├── SpriteRenderer [Neon]               ← Order in Layer: 1
│   ├── Sprite: bldg_neon_tower_neon    (霓虹叠加层，仅发光元素)
│   ├── Material: mat_neon_additive     (Additive 混合模式)
│   └── Color: White (由动画控制 Alpha)
│
├── Animator
│   ├── Controller: neon_tower_controller
│   ├── 默认状态: Breathing (循环呼吸动画)
│   └── 参数: NeonEnabled (Bool), SwitchToGlitch (Trigger), SwitchToPulse (Trigger)
│
├── BoxCollider2D
│   └── Size: (6.5, 3) Units, Offset: (0, -4.75)
│
└── NeonTowerController (Script)
    ├── neonEnabled: true               ← 主开关
    ├── staticIntensity: 0.8            ← 关闭后的常亮强度
    └── neonSpriteRenderer: [Neon 层引用]
```

---

## 2. 文件清单

```
Assets/Art/Buildings/NeonTower/
├── Sprites/
│   ├── bldg_neon_tower_base.png        ← 楼体底图（320×400 px）
│   └── bldg_neon_tower_neon.png        ← 霓虹叠加层（320×400 px）
├── Materials/
│   └── mat_neon_additive.mat           ← Additive 混合材质
├── Shaders/
│   └── shd_neon_additive.shader        ← 自定义 Additive Sprite Shader
├── Animations/
│   ├── neon_breathing.anim             ← 呼吸动画（Sine 波，3s 周期）
│   ├── neon_pulse.anim                 ← 脉冲动画（快速 Attack + 缓降，1.5s 周期）
│   ├── neon_glitch.anim                ← 故障闪烁动画（随机二值跳变，8s 循环）
│   └── neon_tower_controller.controller ← Animator Controller
├── Prefabs/
│   └── NeonTower.prefab                ← 预制体
└── (各目录的 .meta 文件)

Assets/Scripts/Gameplay/Buildings/
└── NeonTowerController.cs              ← 霓虹闪烁控制脚本
```

---

## 3. 材质/Shader 说明

### mat_neon_additive

| 属性 | 值 |
|------|-----|
| Shader | `URP/2D/Sprite-Additive`（自定义） |
| 混合模式 | **`Blend One One`**（Additive 叠加） |
| 队列 | Transparent (3000) |
| ZWrite | Off |
| Cull | Off |

### 工作方式

- 霓虹叠加层的白色/彩色像素会以加色混合（Additive）方式叠加在楼体底图上
- 黑色区域（Alpha = 0）完全不发光，完全透出底图层
- 动画通过改变 SpriteRenderer 的 `Color.a` 控制整体发光强度：
  - `a = 1.0` → 全亮度发光
  - `a = 0.0` → 完全不发光
  - `a = 0.8` → 80% 强度常亮（关闭闪烁时）

### Shader 说明

`shd_neon_additive.shader` 基于 URP `Sprite-Unlit-Default` 修改，核心差异：
- 两个 Pass 的 Blend 模式从 `SrcAlpha OneMinusSrcAlpha` 改为 `One One`
- 保留完整的 URP 2D 管线兼容性（LightMode: Universal2D / UniversalForward）
- Fallback 到 `Sprites/Default`

---

## 4. 动画参数摘要

### 4.1 Animator Controller 状态机

```
                          ┌─────────────┐
           ┌──────────────│  Breathing  │◄────── Default Entry
           │              │ (默认循环)   │
           │              └──────┬──────┘
           │                     │
           │     SwitchToGlitch  │  SwitchToPulse
           │       (Trigger)     │   (Trigger)
           ▼                     ▼
    ┌──────────┐          ┌──────────┐
    │  Glitch  │          │  Pulse   │
    │ (故障闪烁)│          │ (脉冲)   │
    └────┬─────┘          └────┬─────┘
         │                     │
         └─────────┬───────────┘
                   │ NeonEnabled = false
                   ▼
            ┌──────────────┐
            │ Static_Off   │◄──── 80% 常亮，无动画
            │ (常亮关闭态) │
            └──────┬───────┘
                   │ NeonEnabled = true
                   ▼
            ┌──────────────┐
            │  Breathing   │
            └──────────────┘
```

### 4.2 Animation Clip 参数

| Clip | 类型 | 周期 | Alpha 范围 | 缓动 | 对应光源 |
|------|------|------|-----------|------|---------|
| `neon_breathing` | Sine 波呼吸 | 3.0s | 0.60 → 1.00 → 0.60 | 平滑 Sine | N2 灯带、N4 应急灯、N7 门头灯框 |
| `neon_pulse` | 快升缓降脉冲 | 1.5s | 0.20 → 1.00 → 0.20 | 快速 Attack + 缓 Decay | N1 天线顶光、N8 阳台灯条 |
| `neon_glitch` | 随机故障闪烁 | 8.0s（循环） | 0.00 ↔ 1.00 | 二值跳变（无插值） | N5 大招牌、N6 次招牌 |

### 4.3 Animator 参数

| 参数名 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `NeonEnabled` | Bool | true | 主开关：开→动画运行；关→切换到 Static_Off（80% 常亮） |
| `SwitchToGlitch` | Trigger | — | 触发切换到 Glitch 状态 |
| `SwitchToPulse` | Trigger | — | 触发切换到 Pulse 状态 |

---

## 5. 可访问性控制

### 主开关

预制体通过 `NeonTowerController` 脚本暴露 `neonEnabled` 参数：

```csharp
// C# 调用示例
NeonTowerController controller = GetComponent<NeonTowerController>();
controller.SetNeonEnabled(false);  // 关闭闪烁，80% 常亮
controller.SetNeonEnabled(true);   // 恢复闪烁动画
controller.ToggleNeon();           // 切换开关状态
```

- **开启**：所有霓虹灯按预设动画运行（呼吸/脉冲/故障）
- **关闭**：所有霓虹灯以 80% 强度常亮，取消所有闪烁动画
- 所有闪烁频率 ≤ 3Hz，符合 WCAG 可访问性标准

### Inspector 暴露参数（在预制体 Inspector 中）

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Neon Enabled` | bool | true | 主开关 |
| `Static Intensity` | float [0-1] | 0.8 | 关闭后的常亮强度 |
| `Neon Sprite Renderer` | SpriteRenderer | (auto) | 霓虹层引用 |

---

## 6. 如何使用

### 拖入场景
1. 在 Project 窗口导航到 `Assets/Art/Buildings/NeonTower/Prefabs/`
2. 将 `NeonTower.prefab` 拖入场景
3. 调整 Transform Position 到需要的位置

### 碰撞体调整
- BoxCollider2D 默认占地 6.5×3 Units，偏移 `(0, -4.75)` 对齐底部
- 可在预制体实例上调整 Size 或 Offset

### 自定义动画
- 可在 Animator 中添加更多动画状态切换规则
- 可通过 `neon_tower_controller.controller` 添加新的 Animation Clip

### 性能
- 总 Draw Calls: **2**（Base Sprite + Neon Additive Sprite）
- 不使用 URP Point Light 2D，纯 Sprite 自发光

---

## 7. 已知限制 / TODO

| 项目 | 说明 | 优先级 |
|------|------|--------|
| Shader 编译 | `shd_neon_additive.shader` 需在 Unity Editor 中编译验证 | 中 |
| Sprite 对齐 | 需在 Unity 中确认两张 Sprite 完全对齐（320×400, PPU=32） | 高 |
| 动画精细度 | 当前使用全局 Alpha 动画，后续可升级为 Shader 参数驱动（按像素颜色区分光源） | 低 |
| 地面阴影 | 可选 `bldg_neon_tower_shadow` Sprite 暂未实现 | 低 |
| 场景集成 | 需添加到场景 Tilemap 中测试 Y 轴排序 | 中 |

---

## 8. 版本记录

| 版本 | 日期 | 变更 | 作者 |
|------|------|------|------|
| v1.0 | 2025-07-21 | 初始创建：预制体、材质、动画、控制器脚本、文档 | 程基岩 |
