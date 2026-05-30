using UnityEngine;
using UnityEngine.UI;

namespace Base.UIFramework.Localization
{
    /// <summary>
    /// 本地化文本组件。挂在 UI 的 Text 节点上，自动根据 Key 显示当前语言对应文本。
    /// 切换语言时会自动刷新。
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour
    {
        /// <summary>本地化 key（如 "ui_main_title"）。</summary>
        public string Key;

        private Text _text;

        /// <summary>
        /// Awake：缓存 Text 引用并订阅语言切换事件。
        /// </summary>
        private void Awake()
        {
            _text = GetComponent<Text>();
            LocalizationManager.Instance.OnLanguageChanged += Refresh;
        }

        /// <summary>
        /// OnEnable：每次显示时刷新文本，确保最新。
        /// </summary>
        private void OnEnable()
        {
            Refresh();
        }

        /// <summary>
        /// OnDestroy：反注册事件，避免泄漏。
        /// </summary>
        private void OnDestroy()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= Refresh;
            }
        }

        /// <summary>
        /// 设置 key 并立即刷新。
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
            if (_text == null) _text = GetComponent<Text>();
            if (_text != null) _text.text = LocalizationManager.Instance.Get(Key);
        }
    }
}
