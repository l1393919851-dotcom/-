using System.Collections.Generic;
using UnityEngine;
using Base.UIFramework;
using Base.UIFramework.Localization;
using Base.UIFramework.Widgets;

namespace Base.Examples
{
    /// <summary>
    /// 框架启动示例。挂在场景中任意 GameObject 上（一般是 [Bootstrap] 空对象）。
    /// 负责：
    /// 1) 初始化资源系统
    /// 2) 初始化本地化字典
    /// 3) 注册红点
    /// 4) 打开第一个界面
    /// </summary>
    public class ExampleBootstrap : MonoBehaviour
    {
        /// <summary>
        /// Start：游戏启动入口。
        /// </summary>
        private void Start()
        {
            InitLocalization();
            InitRedDot();
            OpenMainPanel();
        }

        /// <summary>
        /// 初始化本地化：用代码硬编码一份简体中文翻译表作为示例。
        /// 实际项目里这部分应从 CSV/JSON/配置表读取。
        /// </summary>
        private void InitLocalization()
        {
            var zh = new Dictionary<string, string>
            {
                { "ui_main_title", "主界面" },
                { "ui_shop", "商店" },
                { "ui_setting", "设置" },
            };
            LocalizationManager.Instance.Load(Language.ChineseSimplified, zh);
        }

        /// <summary>
        /// 注册一些示例红点：邮件、邮件子节点。
        /// </summary>
        private void InitRedDot()
        {
            RedDotManager.Instance.Register("Mail/Reward");
            RedDotManager.Instance.Register("Mail/System");
            RedDotManager.Instance.SetCount("Mail/Reward", 3);
        }

        /// <summary>
        /// 打开主界面。
        /// </summary>
        private void OpenMainPanel()
        {
            UIManager.Instance.OpenAsync<ExampleMainPanel>(onComplete: ui =>
            {
                if (ui == null) Debug.LogWarning("[Bootstrap] 没有找到 UI/MainPanel 预制体，请创建后再试");
            });
        }
    }
}
