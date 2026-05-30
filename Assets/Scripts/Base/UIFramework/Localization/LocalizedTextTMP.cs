// =============================================================================
// 启用条件：
//   1) 通过 Package Manager 安装 "TextMeshPro" 包；
//   2) 在 Project Settings → Player → Scripting Define Symbols 添加 BASE_TMP_SUPPORT。
// 用法与 LocalizedText 一致，把脚本挂到 TMP_Text / TextMeshProUGUI 节点即可。
// =============================================================================
#if BASE_TMP_SUPPORT
using UnityEngine;
using TMPro;

namespace Base.UIFramework.Localization
{
    /// <summary>
    /// TextMeshPro 版本的本地化文本组件。
    /// 挂在 TMP_Text 节点上，根据 Key 自动取当前语言文本；切换语言时自动刷新。
    /// 与原始 LocalizedText 并行存在，二者可同时使用。
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTextTMP : MonoBehaviour
    {
        /// <summary>本地化 key（如 "ui_main_title"）。</summary>
        public string Key;

        private TMP_Text _text;

        /// <summary>
        /// Awake：缓存 TMP_Text 引用并订阅语言切换事件。
        /// </summary>
        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            LocalizationManager.Instance.OnLanguageChanged += Refresh;
        }

        /// <summary>
        /// OnEnable：每次显示时刷新文本，确保展示最新内容。
        /// </summary>
        private void OnEnable()
        {
            Refresh();
        }

        /// <summary>
        /// OnDestroy：反注册事件，避免内存泄漏。
        /// </summary>
        private void OnDestroy()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= Refresh;
            }
        }

        /// <summary>
        /// 修改 key 并立即刷新。
        /// </summary>
        public void SetKey(string key)
        {
            Key = key;
            Refresh();
        }

        /// <summary>
        /// 根据当前语言重新加载文本。
        /// </summary>
        public void Refresh()
        {
            if (_text == null) _text = GetComponent<TMP_Text>();
            if (_text != null) _text.text = LocalizationManager.Instance.Get(Key);
        }
    }
}
#endif
