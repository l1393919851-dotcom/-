# Base —— UI 与资源加载通用框架

完全独立的 Unity 通用框架，无任何项目业务耦合。
所有模块都放在 `Assets/Base/` 下，按文件夹分类，所有方法均有中文注释。

---

## 目录结构

```
Assets/Base/
├── Common/                       # 通用基础工具
│   ├── Singleton/
│   │   ├── Singleton.cs          # 纯 C# 单例基类
│   │   └── MonoSingleton.cs      # MonoBehaviour 单例基类
│   └── Extensions/
│       └── TransformExtensions.cs # Transform/RectTransform 扩展
│
├── ResourceFramework/            # 资源加载框架
│   ├── Core/
│   │   ├── IResourceLoader.cs    # 加载器接口（可替换底层实现）
│   │   └── ResourceManager.cs    # 资源管理总入口
│   ├── Loader/
│   │   ├── ResourcesLoader.cs    # 基于 Resources 的实现
│   │   └── AddressablesLoader.cs # 基于 Addressables 的实现（BASE_ADDRESSABLES）
│   ├── Reference/
│   │   └── AssetRef.cs           # 引用计数
│   ├── Cache/
│   │   └── AssetCache.cs         # 缓存 + 延迟卸载
│   ├── Pool/
│   │   └── GameObjectPool.cs     # GameObject 对象池
│   └── Async/
│       └── LoadRequest.cs        # 异步加载请求（支持 yield / 回调）
│
├── UIFramework/                  # UI 框架
│   ├── Core/                     # 核心：管理器、基类、根节点、层级
│   ├── Stack/                    # 栈管理（返回栈）
│   ├── Animation/                # 入场/出场动画（Fade、Scale，含 DOTween 扩展）
│   ├── Event/                    # 全局事件中心
│   ├── Binding/                  # 数据绑定 BindableProperty
│   ├── Widgets/                  # 通用控件（含 TMP 版本）
│   │   ├── Toast/                # ToastManager + ToastManagerTMP
│   │   ├── Dialog/               # DialogManager + DialogManagerTMP
│   │   ├── LoopScrollView/       # 循环列表
│   │   └── RedDot/               # 红点系统
│   ├── Localization/             # LocalizedText + LocalizedTextTMP
│   ├── Input/                    # 输入屏蔽
│   ├── Adapter/                  # 安全区适配
│   ├── Pool/                     # UI 隐藏池
│   ├── Guide/                    # 引导系统（镂空遮罩）
│   ├── Debug/                    # 运行时调试面板（F1）
│   └── Editor/                   # 预制体绑定代码生成器（仅编辑器）
│
└── Examples/                     # 使用示例
    ├── ExampleBootstrap.cs       # 启动入口
    └── ExampleMainPanel.cs       # 一个完整的主界面示例
```

---

## 快速上手

### 1) 资源加载

```csharp
// 同步加载
var prefab = ResourceManager.Instance.Load<GameObject>("UI/MainPanel");

// 异步加载
ResourceManager.Instance.LoadAsync<Sprite>("Icons/Hero", sprite =>
{
    image.sprite = sprite;
});

// 协程方式等待
yield return ResourceManager.Instance.LoadAsync<GameObject>("Effects/Boom");

// 对象池
var bullet = ResourceManager.Instance.Spawn("Prefabs/Bullet", transform);
ResourceManager.Instance.Despawn("Prefabs/Bullet", bullet);

// 释放
ResourceManager.Instance.Release("UI/MainPanel");
```

### 2) 定义并打开一个面板

```csharp
[UI("UI/ShopPanel", UILayer.Popup, fullScreen: false, cacheable: true)]
public class ShopPanel : UIBase
{
    protected override void OnInit()    { /* 绑定组件，订阅事件 */ }
    protected override void OnOpen(object args) { /* 刷新数据 */ }
    protected override void OnClose()   { /* 反注册 */ }
}

UIManager.Instance.Open<ShopPanel>(args: null, pushStack: true);
UIManager.Instance.Close<ShopPanel>();
UIManager.Instance.CloseTop();
```

### 3) 事件中心

```csharp
EventCenter.Instance.Subscribe<int>("OnGoldChange", v => Debug.Log(v));
EventCenter.Instance.Publish("OnGoldChange", 100);
```

### 4) 数据绑定

```csharp
public BindableProperty<int> Gold = new(0);
Gold.OnValueChanged += (oldV, newV) => goldText.text = newV.ToString();
Gold.Value = 200; // 自动刷新
```

### 5) 通用控件

```csharp
ToastManager.Instance.Show("保存成功");

DialogManager.Instance.Show("提示", "确定要退出吗？",
    onConfirm: Application.Quit,
    onCancel: () => ToastManager.Instance.Show("取消"));

RedDotManager.Instance.AddListener("Mail/Reward", count => redDotImage.SetActive(count > 0));
RedDotManager.Instance.SetCount("Mail/Reward", 3);
```

### 6) 本地化

```csharp
var zh = new Dictionary<string, string> { { "ui_title", "主界面" } };
LocalizationManager.Instance.Load(Language.ChineseSimplified, zh);
string text = LocalizationManager.Instance.Get("ui_title");
```

### 7) 引导系统

```csharp
var steps = new List<GuideStep>
{
    new GuideStep { Id = 1, Target = shopBtnRect, Tip = "点击商店", OnTargetClick = () => Debug.Log("Step1 done") },
};
GuideManager.Instance.StartGuide(steps);

// 在被引导按钮的 OnClick 里调用
GuideManager.Instance.OnTargetClicked();
```

### 8) 运行时调试

按 F1 弹出 UIDebugger 面板，查看当前打开的 UI、栈和缓存数量。
需要场景里有任意一处第一次调用过 `UIDebugger.Instance` 即可激活。

---

## 设计要点

1. **单一入口**：UI 全部经过 `UIManager`，资源全部经过 `ResourceManager`。
2. **可替换加载器**：`ResourceManager.SetLoader(new XXXLoader())` 即可切到 Addressables 等。
3. **生命周期清晰**：UI 基类的 `OnInit/OnOpen/OnShow/OnHide/OnClose/OnDestroyUI` 全有注释。
4. **自动池化**：标记 `cacheable=true` 的面板关闭后进入隐藏池，下次打开复用。
5. **层级隔离**：每个 UILayer 单独 Canvas + sortingOrder，不打断合批。
6. **零编辑器依赖**：完全代码搭建，Toast、Dialog、SafeArea 都不依赖预制体。

---

## 可选扩展模块

为了不让框架的最小可用版本耦合第三方包，下面 4 个扩展默认是关闭的，
需要在 **Project Settings → Player → Other Settings → Scripting Define Symbols** 添加对应的宏，
没装包/没加宏的情况下扩展文件会被编译器完全跳过，不会报错。

| 扩展 | 宏 | 依赖包 | 启用后获得 |
|---|---|---|---|
| Addressables 加载器 | `BASE_ADDRESSABLES` | `com.unity.addressables` | `AddressablesLoader`，可替换默认 `ResourcesLoader` |
| TextMeshPro 支持 | `BASE_TMP_SUPPORT` | `com.unity.textmeshpro` | `LocalizedTextTMP`、`ToastManagerTMP`、`DialogManagerTMP`，与原始 uGUI 版本**并行存在**，互不冲突 |
| DOTween 动画 | `BASE_DOTWEEN_SUPPORT` | DOTween (Asset Store) | `UIDotweenAnimation`，替换默认的协程动画 |

> 多个宏用分号分隔，例如：`BASE_ADDRESSABLES;BASE_TMP_SUPPORT;BASE_DOTWEEN_SUPPORT`

### 切换到 Addressables

```csharp
// 启动入口（开启 BASE_ADDRESSABLES 宏后）
ResourceManager.Instance.SetLoader(new AddressablesLoader());
// 业务调用完全不变：
var prefab = ResourceManager.Instance.Load<GameObject>("UI/MainPanel");
```

### 使用 TextMeshPro 版本控件

原版的 `LocalizedText` / `ToastManager` / `DialogManager` 都保留，TMP 版本以 `XxxTMP` 命名：

```csharp
// 挂在 TMP_Text 节点上
gameObject.AddComponent<LocalizedTextTMP>().Key = "ui_title";

// 调用 TMP 版本的 Toast / Dialog
ToastManagerTMP.Instance.Show("保存成功");
DialogManagerTMP.Instance.Show("提示", "退出游戏？", onConfirm: Application.Quit);

// 可全局指定字体资源
ToastManagerTMP.Instance.FontAsset = myFontAsset;
```

### 启用 DOTween 动画

```csharp
// 在面板里返回 DOTween 动画即可（开启 BASE_DOTWEEN_SUPPORT 宏后）
protected override IUIAnimation CreateAnimation()
{
    return new UIDotweenAnimation
    {
        InDuration = 0.3f,
        InEase = DG.Tweening.Ease.OutBack
    };
}
```

---

## 预制体自动绑定代码生成器（Editor 工具）

省去手写大量 `transform.Find().GetComponent<>()` 的工作。

### 命名约定

| 前缀 | 类型 | 字段名 |
|---|---|---|
| `_btn_xxx` | `Button` | `BtnXxx` |
| `_txt_xxx` | `Text` | `TxtXxx` |
| `_img_xxx` | `Image` | `ImgXxx` |
| `_raw_xxx` | `RawImage` | `RawXxx` |
| `_input_xxx` | `InputField` | `InputXxx` |
| `_scroll_xxx` | `ScrollRect` | `ScrollXxx` |
| `_slider_xxx` | `Slider` | `SliderXxx` |
| `_toggle_xxx` | `Toggle` | `ToggleXxx` |
| `_tmp_xxx` | `TMP_Text` | `TmpXxx` *(需要 BASE_TMP_SUPPORT)* |
| `_tmpinput_xxx` | `TMP_InputField` | `TmpInputXxx` *(需要 BASE_TMP_SUPPORT)* |
| `_tr_xxx` | `RectTransform` | `TrXxx` |
| `_go_xxx` | `GameObject` | `GoXxx` |

### 使用流程

1. 制作预制体时按命名约定命名节点，例如 `_btn_shop`、`_txt_gold`
2. 菜单 **Tools → Base → UI Bindings Generator...**
   或在 Project 面板右键预制体 → **Base/Generate UI Bindings**
3. 填好命名空间、类名、输出目录，点击 Generate
4. 生成 `XxxPanel.Generated.cs`（partial 类，含字段 + `AutoBindWidgets()` 方法）

### 业务代码这样写

```csharp
[UI("UI/MainPanel", UILayer.Normal)]
public partial class MainPanel : UIBase
{
    protected override void OnInit()
    {
        AutoBindWidgets();          // 调一次即可
        BtnShop.onClick.AddListener(() => UIManager.Instance.Open<ShopPanel>());
        TxtGold.text = "0";
    }
}
```

---

## 默认依赖

- Unity 2020.3 +（用了 SetSiblingIndex、Screen.safeArea 等 API）
- UnityEngine.UI（uGUI）
- 仅使用 Unity 内置 API，**未引入任何第三方包**

TextMeshPro / Addressables / DOTween 都以"插件模式"提供，按需启用。
