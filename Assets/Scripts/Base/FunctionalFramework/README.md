# FunctionalFramework —— 功能扩展框架

与 `Base` 框架配合使用，提供场景、配置表、音效、状态机、行为树、ECS 等游戏常用能力。

---

## 目录结构

```
FunctionalFramework/
├── SceneManage/          # 场景加载与流程
├── ConfigTable/          # JSON/CSV 配置表
├── SoundManage/          # BGM / SFX / Voice
├── StateMachine/         # 有限状态机 FSM
├── BehaviorTree/         # 行为树 BT
├── ECS/                  # 轻量 ECS + 可选 Burst
│   ├── Components/
│   ├── Systems/
│   └── Burst/            # 安装 Burst 包后自动启用（UNITY_BURST）
└── Examples/             # 使用示例
```

---

## 快速上手

### 场景 SceneManage

```csharp
SceneFlowManager.Instance.LoadSceneAsync("Main", SceneLoadMode.Single,
    p => Debug.Log(p), () => Debug.Log("完成"));

var flow = new[] {
    new SceneFlowStep("Boot"),
    new SceneFlowStep("Lobby"),
};
SceneFlowManager.Instance.RunFlow(flow);
```

### 配置表 ConfigTable

JSON 格式（`Resources/Config/Hero.json`）：

```json
{ "items": [ { "Id": 1, "Name": "战士", "Hp": 100, "Speed": 5.0 } ] }
```

```csharp
[Config("Config/Hero", ConfigFormat.Json)]
public class HeroConfig : IConfigRow { public int Id; public string Name; ... }

ConfigManager.Instance.Load<HeroConfig>();
var row = ConfigManager.Instance.GetTable<HeroConfig>().Get(1);
```

### 音效 SoundManage

```csharp
SoundManager.Instance.PlayBGM("Audio/BGM_Main");
SoundManager.Instance.PlaySFX("Audio/SFX_Click");
SoundManager.Instance.SetVolume(SoundChannel.BGM, 0.5f);
```

### 状态机 StateMachine

```csharp
var fsm = new Fsm<PlayerContext>();
fsm.AddState(new IdleState());
fsm.AddTransition<IdleState, RunState>(c => c.IsMoving);
fsm.Start<IdleState>(context);
fsm.Update(context, Time.deltaTime);
```

### 行为树 BehaviorTree

```csharp
var root = new SelectorNode();
root.AddChild(new SequenceNode()
    .AddChild(new ConditionNode(ctx => HasTarget(ctx)))
    .AddChild(new ActionNode(ctx => Chase(ctx))));
```

### ECS

```csharp
var world = new World();
var e = world.CreateEntity();
world.AddComponent(e, new PositionComponent { Value = Vector3.zero });
world.AddComponent(e, new VelocityComponent { Value = Vector3.forward });
world.AddSystem(new MovementSystem(viewRegistry));
world.Update(Time.deltaTime);
```

场景中使用 `ECSWorldRunner` + `ExampleMonsterSpawner` 可快速验证大量实体。

---

## Burst 支持（大量怪物优化）

### 安装状态（本项目）

| 包 | manifest.json | 说明 |
|----|---------------|------|
| `com.unity.burst` 1.8.18 | ✓ 已添加 | 并行编译 |
| `com.unity.collections` 2.1.4 | ✓ 已添加 | NativeArray / Job |
| `com.unity.mathematics` 1.2.6 | ✓ 已添加 | float3 等数学类型 |

> 首次添加后需**重新打开 Unity**，等待 Package Manager 下载完成。  
> 菜单 **Tools → Functional → 检查 Burst 安装状态** 可验证是否就绪。

### 使用步骤

1. 确认上述三个包已在 `Packages/manifest.json`（本项目已配置）
2. 重新打开 Unity，等待包下载完成（无需手动添加 `FUNCTIONAL_BURST` 宏）
3. 场景挂 `ECSWorldRunner`，勾选 **Use Burst When Available**
4. 运行后 Console 应出现：`[ECSWorldRunner] Burst 已启用 → BurstMovementSystem`

启用后 `BurstMovementSystem` 使用 `IJobParallelFor` + `[BurstCompile]` 并行更新位置。

```csharp
// 启动时检查（ExampleFunctionalBootstrap 已调用）
BurstSupport.LogStatus();
Debug.Log(BurstSupport.IsPackageAvailable); // true 表示 Burst 可用
```

---

## 与 Base 框架联动

| 模块 | 联动 |
|------|------|
| SceneManage | 切场景时调用 `ResourceManager.ReleaseAll()` |
| ConfigTable | 优先通过 `ResourceManager` 加载 TextAsset |
| SoundManage | 优先通过 `ResourceManager` 加载 AudioClip |

---

## 示例脚本

| 脚本 | 说明 |
|------|------|
| `ExampleFunctionalBootstrap` | 加载配置、初始化音效 |
| `ExamplePlayerFsm` | 状态机 WASD 切换 Idle/Run |
| `ExampleMonsterAI` | 行为树巡逻/追击 |
| `ExampleMonsterSpawner` | ECS 批量生成 100 个实体 |
