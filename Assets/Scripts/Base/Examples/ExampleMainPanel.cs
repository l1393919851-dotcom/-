using UnityEngine;
using UnityEngine.UI;
using Base.UIFramework;
using Base.UIFramework.Animation;
using Base.UIFramework.Event;
using Base.UIFramework.Binding;
using Base.UIFramework.Widgets;

namespace Base.Examples
{
    /// <summary>
    /// 示例：主界面。演示如何使用 UI 框架的各项功能。
    /// 使用前提：在 Resources/UI 目录下放一个 MainPanel.prefab，根节点挂这个脚本。
    /// </summary>
    [UI("UI/MainPanel", UILayer.Normal, fullScreen: true, cacheable: true)]
    public class ExampleMainPanel : UIBase
    {
        /// <summary>主界面金币数据（数据绑定示例）。</summary>
        public BindableProperty<int> Gold = new BindableProperty<int>(0);

        private Text _goldText;
        private Button _shopBtn;
        private Button _settingBtn;

        /// <summary>
        /// 入场动画用缩放（弹窗风格）。也可以返回 null 表示无动画。
        /// </summary>
        protected override IUIAnimation CreateAnimation()
        {
            return new UIFadeAnimation { InDuration = 0.2f, OutDuration = 0.15f };
        }

        /// <summary>
        /// 第一次创建时调用：绑定组件引用、订阅事件、绑定数据。
        /// </summary>
        protected override void OnInit()
        {
            _goldText = transform.Find("GoldText").GetComponent<Text>();
            _shopBtn = transform.Find("ShopBtn").GetComponent<Button>();
            _settingBtn = transform.Find("SettingBtn").GetComponent<Button>();

            _shopBtn.onClick.AddListener(OnShopClick);
            _settingBtn.onClick.AddListener(OnSettingClick);

            Gold.OnValueChanged += (_, v) => _goldText.text = $"金币: {v}";
            EventCenter.Instance.Subscribe<int>("OnGoldChange", OnGoldChange);
        }

        /// <summary>
        /// 每次打开时调用：可接收外部传入的数据并刷新界面。
        /// </summary>
        protected override void OnOpen(object args)
        {
            Gold.ForceSet(Gold.Value);
        }

        /// <summary>
        /// 关闭时反注册事件，防止内存泄漏。
        /// </summary>
        protected override void OnClose()
        {
        }

        /// <summary>
        /// 销毁时彻底反注册。
        /// </summary>
        protected override void OnDestroyUI()
        {
            EventCenter.Instance.Unsubscribe<int>("OnGoldChange", OnGoldChange);
        }

        /// <summary>
        /// 商店按钮点击：打开商店弹窗，并压栈以支持返回。
        /// </summary>
        private void OnShopClick()
        {
            ToastManager.Instance.Show("打开商店");
        }

        /// <summary>
        /// 设置按钮点击：弹出确认对话框示例。
        /// </summary>
        private void OnSettingClick()
        {
            DialogManager.Instance.Show(
                "提示",
                "确定要退出游戏吗？",
                onConfirm: Application.Quit,
                onCancel: () => ToastManager.Instance.Show("已取消"));
        }

        /// <summary>
        /// 全局事件回调：金币变化。
        /// </summary>
        private void OnGoldChange(int newGold)
        {
            Gold.Value = newGold;
        }
    }
}
