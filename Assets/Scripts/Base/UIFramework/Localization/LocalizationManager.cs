using System;
using System.Collections.Generic;
using UnityEngine;
using Base.Common;

namespace Base.UIFramework.Localization
{
    /// <summary>
    /// 支持的语言列表。可按需扩展。
    /// </summary>
    public enum Language
    {
        ChineseSimplified,
        ChineseTraditional,
        English,
        Japanese,
        Korean
    }

    /// <summary>
    /// 本地化管理器。
    /// - 通过 key 查找对应语言的字符串
    /// - 切换语言时通过事件通知所有 LocalizedText 自动刷新
    /// 
    /// 用法：
    ///   LocalizationManager.Instance.Load(Language.ChineseSimplified, dictionary);
    ///   string text = LocalizationManager.Instance.Get("ui_main_title");
    ///   LocalizationManager.Instance.ChangeLanguage(Language.English);
    /// </summary>
    public class LocalizationManager : Singleton<LocalizationManager>
    {
        /// <summary>当前语言。</summary>
        public Language CurrentLanguage { get; private set; } = Language.ChineseSimplified;

        /// <summary>语言切换事件。LocalizedText 监听该事件以自动刷新文本。</summary>
        public event Action OnLanguageChanged;

        private readonly Dictionary<string, string> _dict = new Dictionary<string, string>();

        /// <summary>
        /// 加载某种语言的翻译表（key → 翻译文本）。
        /// 一般在游戏启动时调用。
        /// </summary>
        public void Load(Language language, Dictionary<string, string> table)
        {
            CurrentLanguage = language;
            _dict.Clear();
            if (table != null)
            {
                foreach (var kv in table) _dict[kv.Key] = kv.Value;
            }
        }

        /// <summary>
        /// 切换语言。需要先 Load 好对应语言的翻译表。
        /// </summary>
        public void ChangeLanguage(Language language, Dictionary<string, string> table)
        {
            Load(language, table);
            try { OnLanguageChanged?.Invoke(); }
            catch (Exception e) { UnityEngine.Debug.LogException(e); }
        }

        /// <summary>
        /// 根据 key 获取翻译文本，不存在时返回 key 本身（便于发现缺失）。
        /// </summary>
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return "";
            return _dict.TryGetValue(key, out var v) ? v : key;
        }

        /// <summary>
        /// 带格式化参数的版本，例如 Get("welcome", playerName)。
        /// </summary>
        public string Get(string key, params object[] args)
        {
            var template = Get(key);
            try { return string.Format(template, args); }
            catch { return template; }
        }
    }
}
