# LiteEcs

从新乐园（PetPark）Lua 客户端里摘出来的 ECS 框架，翻译成 C#。只保留框架本身，不含任何乐园玩法、配表、协议、UI、资源加载或引擎相关代码。

- 纯 C#，只依赖 `System` / `System.Collections.Generic`，不引用 UnityEngine。
- 已用 Unity 2022.3 自带的 Roslyn 按 netstandard 2.1 编译通过（C# 9 和 C# 7.3 都能编）。
- 命名空间统一为 `LiteEcs`。

## 放进项目

- **Unity**：整个文件夹拖进 `Assets/` 下任意位置。自带 `LiteEcs.asmdef`，会编成独立程序集，并自动被 `Assembly-CSharp` 引用。不想要独立程序集就删掉 asmdef。
- **非 Unity 的 .NET 项目**：把 `.cs` 文件加进工程即可，asmdef 可删。
- 接日志（可选）：默认写到 `Console`。Unity 里在启动时加一句：

```csharp
EcsLog.s_OnWarn = UnityEngine.Debug.LogWarning;
EcsLog.s_OnError = UnityEngine.Debug.LogError;
EcsLog.s_OnException = UnityEngine.Debug.LogException;
```

## 目录

| 文件夹 | 内容 |
|---|---|
| `Core/` | `LWorld` 世界、`LEntity` 实体、`LComponent` 组件、`LSystem` 系统、`LEntityManager` 实体管理、`EcsLog` 日志出口 |
| `Pool/` | `IPoolable`、`LObjectPool`、`LObjectPoolManager`：实体、组件、事件都走这里复用 |
| `Event/` | `LEventManager` 世界内同步事件，`LEntityEvent` 池化的实体事件基类 |
| `Timer/` | `LTimerManager` 随世界 Tick 的计时器，`TimerHandle` 带版本号的句柄 |
| `Tag/` | `LTagContainer` 引用计数标签，`TagsComponent` 标签组件，`TagIndexedEntityManager` 按标签建索引的实体管理 |
| `Blackboard/` | `Blackboard` 字符串键 KV 表，`BlackboardComponent` 实体黑板 |
| `StateGraph/` | `StateGraph` 状态图、`StateGraphNode` / `TaggedStateNode` 节点、`StateGraphEvent<T>` 转移规则、`StateGraphSystem` 图注册表、`StateGraphComponent` 实体当前状态 |

## 每帧顺序

外部每帧调用一次 `world.Tick(dt)`：

1. **实体管理 Tick**
   1. 上一帧以来排队加入的实体全部进表并 `Awake`，然后全部 `Start`（先全员 Awake 再全员 Start，Start 里可以放心读别的新实体）。
   2. 所有在表实体按加入顺序 `Tick`，实体内按组件添加顺序 `Tick`。
   3. 排队移除的实体全部 `End`，然后逐个 `Destroy` → `OnRemoveEntityInternal` → 离表 → 回池。
2. **计时器 Tick**
3. **各系统 `OnSystemTick`**（按 `AddSystem` 顺序）
4. **`LWorld.OnTick`**

其他生命周期：

- `world.Start()`：系统全部 `OnSystemAwake`，再全部 `OnSystemStart`，再 `OnStart`。
- `world.Dispose()`：`Alive` 先置 false → 系统全部 `End`、全部 `Destroy`、全部 `Dispose` → 实体全部 End / Destroy 并回池 → 清对象池 → 清计时器 → 清事件监听 → `OnDispose`。
- 组件：`Init(创建参数)` → `Awake` → `Start` → 每帧 `Tick` → `End` → `Destroy` → `OnRecycle`。
- 每个实体、系统、监听、计时器的回调都单独 try/catch，一个出错只打日志，不打断其他对象。

## 快速上手

```csharp
using System.Collections.Generic;
using LiteEcs;

// 1. 创建参数：一个对象实现多个组件关心的接口，所有组件拿到的是同一个对象
class UnitParam : ITagsCreateParam, IStateGraphCreateParam
{
    public IReadOnlyList<string> InitialTags { get; set; }
    public string StateGraphKey { get; set; }   // 为空时按标签自动找图
    public string InitialState { get; set; }
}

// 2. 自定义组件
class MoveComponent : LComponent
{
    public float Speed;
    public override void Tick(float dt) { /* 移动 */ }
    public override void OnRecycle() { Speed = 0; base.OnRecycle(); }   // 必须调 base
}

// 3. 实体事件（池化）
class ClickEvent : LEntityEvent
{
    public bool Leave;
    public override void OnRecycle() { Leave = false; base.OnRecycle(); }
}

// 4. 状态图：节点 + 转移规则
class ClickRule : StateGraphEvent<ClickEvent>
{
    protected override void OnEvent(ClickEvent evt)
    {
        evt.Entity.GetComponent<StateGraphComponent>().Goto(evt.Leave ? "idle" : "focused");
    }
}

class BuildingGraph : StateGraph
{
    public BuildingGraph(LWorld world) : base(world)
    {
        AddNode(new TaggedStateNode("idle"));      // 进入时实体加 "idle" 标签，退出时去掉
        AddNode(new TaggedStateNode("focused"));
        AddEvent(new ClickRule());
    }
}

// 5. 系统
class MoveSystem : LSystem
{
    protected override void OnTick(float dt) { /* 每帧逻辑 */ }
}

// 6. 世界：想按类型标签快速取实体时换成 TagIndexedEntityManager
class GameWorld : LWorld
{
    static readonly string[] s_IndexTags = { "building", "pet" };
    protected override LEntityManager CreateEntityManager() => new TagIndexedEntityManager(this, s_IndexTags);
}

// 7. 组装
var world = new GameWorld();
var graphs = new StateGraphSystem();
world.AddSystem(graphs);
world.AddSystem(new MoveSystem());
graphs.Register("building", w => new BuildingGraph(w));
world.Start();

var param = new UnitParam { InitialTags = new[] { "building" }, InitialState = "idle" };
var entity = world.CreateEntity();            // 从池里取，Id 从 1 递增
entity.AddComponent<TagsComponent>(param);
entity.AddComponent<StateGraphComponent>(param);
entity.AddComponent<MoveComponent>(param);
entity.AddComponent<BlackboardComponent>(param).Set("ConfigId", 1001);
world.AddEntity(entity);                      // 下一次 Tick 才 Awake / Start

// 每帧
world.Tick(deltaTime);

// 发事件
var click = world.TakeEvent<ClickEvent>(entity);
world.TriggerAndRecycle(click);               // 同步派发后立刻回池

// 计时器
TimerHandle handle = world.TimerManager.AddTimer(1f, 0.5f, 0, () => { /* 1 秒后开始每 0.5 秒一次 */ });
handle.Stop();

// 按标签取实体
var buildings = ((TagIndexedEntityManager)world.EntityManager).GetEntitiesByTag("building");

// 移除 / 销毁
world.RemoveEntity(entity.Id);                // 本帧 Tick 末尾 End / Destroy 并回池
world.Dispose();
```

## 使用约束

- **增删实体都是排队的**：`AddEntity` 之后到下一次 `Tick` 之前，`GetEntity` 取不到它，需要时用 `EntityManager.GetEntityIncludingPending`。
- **回池会清 Id**：实体回池后 `Id = 0`、`World = null`，再取出来会分配新 Id。不要在实体移除后继续持有它的引用，存 Id 再用 `GetEntity` 取。
- **重写 `OnRecycle` 必须调 `base.OnRecycle()`**，并把自己加的字段清回默认值，否则复用时会带着上一次的数据。
- **不要在组件生命周期回调里增删本实体的组件**。
- **组件槽位**：默认按具体类型存，一个实体每种组件只有一个；重复 Add 同类型会把旧的回池并报错。想让子类占用基类槽位，重写 `ComponentKey` 返回基类类型。
- **系统按类型唯一**：`GetSystem<T>()` 要用注册时的具体类型去取。
- **`CreateEntityManager` 在基类构造函数里调用**：重写时只能用静态数据，不能用子类自己的实例字段。
- **池化事件只在回调里有效**：`TriggerAndRecycle` 派发完立刻回池，监听方不能保存事件对象。
- **事件派发是快照语义**：派发过程中移除的监听本轮仍会被调用；新加的监听本轮不调用。同一个方法重复注册会被忽略。
- **标签索引只在进表时建一次**：`TagIndexedEntityManager` 只适合放实体类型这类不会变的标签；运行时状态标签请直接 `HasTag`。
- **状态图按世界缓存**：同一个世界里同 key 的实体共享一张图，节点不要存实体相关的字段；当前状态在 `StateGraphComponent` 上。`StateGraphComponent` 要求世界里先 `AddSystem(StateGraphSystem)`，并在实体 Start 前 `Register` 好。
- 黑板值是 `object`，值类型会装箱，适合放低频读写的 id、标志位。

## 和 Lua 原版的差异

翻译时保留了原版的结构和执行顺序，下面这些是有意改动的：

| 点 | Lua 原版 | C# 版 |
|---|---|---|
| 组件 / 系统的键 | 整数类型 id（`GetComponentType()` / `GetType()` 返回枚举值） | 按 C# 类型：`GetComponent<T>()`、`GetSystem<T>()`；可用 `ComponentKey` / `SystemKey` 重写 |
| 遍历顺序 | `pairs` 遍历，顺序不确定 | 实体、组件、系统都按加入顺序 |
| 加入 / 移除过程中再排队 | 加入阶段新排队的实体可能被跳过或只 Start 不 Awake；移除阶段新排队的会丢 | 一律留到下一帧处理 |
| 实体回池 | 基类不回池，由乐园的实体管理子类在移除时回池 | 基类移除和清空时都会把实体回池 |
| 计时器句柄 | 返回计时器对象本身；回池复用后旧引用 Stop 会停掉新计时器；回调里 Stop 自己会被重复回收 | 返回带版本号的 `TimerHandle`；停掉的计时器在 Tick 末尾才回池，回调里 Stop / Add 都安全 |
| `Interval <= 0` 且重复次数不为 1 | 第一次执行后永远不结束 | 第一次执行后结束 |
| 事件键 | 任意 Lua 值（乐园用事件类表） | 事件的 C# 类型，`AddListener<T>(Action<T>)` |
| 监听去重 | 需要 `Delegate.GetOrCreate` 拿到同一个函数对象 | C# 委托按目标对象 + 方法判等，直接传方法组即可 |
| 状态图缓存 | 全局静态单例，第一次绑定的世界决定事件挂在哪 | 挂在每个世界的 `StateGraphSystem` 上，世界销毁时一起清掉 |
| 状态图选图 | 写死按 Pet / Building / Scene 三个标签判断 | 注册时的 key 即标签，按注册顺序匹配；也可以在创建参数里直接指定 key |
| 状态图同 key 重复 AddEvent | 直接覆盖，旧监听不反注册 | 先反注册旧的 |
| 拼写 | `Destory` / `Recyle` | `Destroy` / `Recycle` |

## 原版对照

| Lua（ssr-logic） | C# |
|---|---|
| `Lua/Framework/ECS/LWorld.lua` | `Core/LWorld.cs` |
| `Lua/Framework/ECS/LEntity.lua` | `Core/LEntity.cs` |
| `Lua/Framework/ECS/LComponent.lua` + 乐园 `PComponentBase.lua`（带 Init 参数） | `Core/LComponent.cs` |
| `Lua/Framework/ECS/LSystem.lua` | `Core/LSystem.cs` |
| `Lua/Framework/ECS/LEntityManager.lua` | `Core/LEntityManager.cs` |
| `Lua/Framework/ECS/LObjectPool.lua` / `LObjectPoolManager.lua` | `Pool/` |
| `Lua/Framework/ECS/LEventManager.lua` + 乐园 `PUnitBaseEvent.lua` | `Event/` |
| `Lua/Framework/ECS/LTimer.lua` / `LTimerManager.lua` | `Timer/` |
| `Lua/Framework/ECS/LTagContainer.lua` + 乐园 `PTagsComponent.lua` | `Tag/LTagContainer.cs`、`Tag/TagsComponent.cs` |
| 乐园 `PEntityManager.lua` 的按实体类型索引部分 | `Tag/TagIndexedEntityManager.cs` |
| `Lua/Framework/StateMachine/Blackboard.lua` + 乐园 `PBlackboardComponent.lua` | `Blackboard/` |
| `Lua/Framework/StateGraph/*` + 乐园 `PStateGraph` / `PStateNode` / `PStateEvent` / `PStateGraphBuilder` / `PStateGraphComponent` | `StateGraph/` |

## 没有带走的部分

这些在乐园里和项目强绑定，或者本身是另一套框架：

- **全部玩法**：乐园的各个 System（交互、相机、摆放、环形菜单、云雾、气泡、特效等）、建筑 / 宠物 / 场景的具体状态节点、实体工厂 `PEntityCreater`、`PWorld` 的进场流程。
- **行为树**：乐园的 `PAIComponent` 用的是项目里另一套行为树框架（`BTUtils` / `BTLeafNode`），不属于 ECS 框架。移植后如需 AI，写一个持有行为树的组件，在 `OnComponentStart` 建树、`Tick` 里驱动、`OnRecycle` 归还即可。
- **视图与资源**：模型加载、特效、HUD、碰撞拾取这些依赖 Unity 和项目资源管理的组件。
- **项目绑定**：服务器实例 id 映射、配表、协议、UI 打开关闭等。
