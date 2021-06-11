using UnityEngine;
using BJSYGameCore;
using MukioI18n;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace Game
{
    /// <summary>
    /// 语言管理器
    /// </summary>
    public partial class LangManager : Manager, ITranslationProvider, ILanguageManager
    {
        #region 公共成员
        public string[] getLanguages()
        {
            return langs;
        }

        public string getDefaultLanguage()
        {
            return defaultLang;
        }

        public string getLanguage()
        {
            return CurrentCulture.Name;
        }

        public void setLanguage(string cultureName)
        {
            CurrentCulture = new CultureInfo(cultureName);
        }
        /// <summary>
        /// 加载初始语言
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public Task initLanguage(CultureInfo cultureInfo)
        {
            return switchLanguage(cultureInfo);
        }

        /// <summary>
        /// 切换语言
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public async Task switchLanguage(CultureInfo cultureInfo)
        {
            var lastCulture = i18n.CurrentLang;
            if (lastCulture == cultureInfo) return;

            // 加载语言资源
            await i18n.LoadFileAsync(folderPath, cultureInfo);

            // 触发事件来通知UI进行更改。
            Task changeCultureTask = OnCultureChange?.Invoke(cultureInfo);
            if (changeCultureTask != null)
                await changeCultureTask;

            // 卸载已经加载的资源
            i18n.Unload(lastCulture);
        }

        /// <summary>
        /// 获取当前语言对应字符串
        /// </summary>
        /// <param name="sourceString"></param>
        /// <returns></returns>
        public string getString(string sourceString)
        {
            return _(sourceString);
        }

        /// <summary>
        /// 当语言切换
        /// </summary>
        public event Func<CultureInfo, Task> OnCultureChange;
        #endregion
        /// <summary>
        /// 默认载入路径
        /// </summary>
        [SerializeField]
        string folderPath = "i18n";
        /// <summary>
        /// 载入的语言
        /// </summary>
        [SerializeField]
        string[] langs = { "zh-CN", "en-US" };

        /// <summary>
        /// 默认语言
        /// </summary>
        [SerializeField]
        string defaultLang = "zh-CN";

        /// <summary>
        /// 不要加载默认语言
        /// </summary>
        [SerializeField]
        bool dontLoadDefaultLang = true;

        List<CultureInfo> cultures = new List<CultureInfo>();

        /// <summary>
        /// 可用的语言
        /// </summary>
        public IEnumerable<CultureInfo> Cultures => cultures;

        CultureInfo currentCulture;

        /// <summary>
        /// 当前语言
        /// </summary>
        public CultureInfo CurrentCulture
        {
            get => currentCulture;
            set
            {
                currentCulture = value;
                i18n.SetLang(currentCulture);

                OnCultureChange?.Invoke(CurrentCulture);
            }
        }

        public I18n i18n { get; } = new I18n();

        protected override void onAwake()
        {
            base.onAwake();
            loadAll();
        }

        private void loadAll()
        {
            var defaultCulture = new CultureInfo(defaultLang);
            // 设置当前的默认语言
            CurrentCulture = defaultCulture;

            foreach (var lang in langs)
            {
                // 在部分情况下，可能想要使用程序内的源文本作为翻译文本
                if (lang == defaultLang && dontLoadDefaultLang)
                {
                    cultures.Add(defaultCulture);
                    i18n.AddEmpty(defaultCulture);
                    continue;
                }
                loadLang(lang);
            }
        }

        private void loadLang(string lang)
        {
            try
            {
                UberDebug.Log($"加载语言{lang}的翻译");
                var culture = new CultureInfo(lang);
                cultures.Add(culture);
                i18n.Load(folderPath, culture);
            }
            catch (FileNotFoundException e)
            {
                UberDebug.LogError($"无法载入语言{lang}的翻译文件, {e.Message}");

                // 为没有翻译文件的加载一个默认空翻译
                if (lang == defaultLang)
                    i18n.AddEmpty(new CultureInfo(defaultLang));
            }
        }
        #region TranslationProxy
        public string _(string text)
        {
            return i18n._(text);
        }

        public string _(string text, params object[] args)
        {
            return i18n._(text, args);
        }

        public string _n(string text, string pluralText, long n)
        {
            return i18n._n(text, pluralText, n);
        }

        public string _n(string text, string pluralText, long n, params object[] args)
        {
            return i18n._n(text, pluralText, n, args);
        }

        public string _p(string context, string text)
        {
            return i18n._p(context, text);
        }

        public string _p(string context, string text, params object[] args)
        {
            return i18n._p(context, text, args);
        }

        public string _pn(string context, string text, string pluralText, long n)
        {
            return i18n._pn(context, text, pluralText, n);
        }

        public string _pn(string context, string text, string pluralText, long n, params object[] args)
        {
            return i18n._pn(context, text, pluralText, n, args);
        }
        public event Action OnLanguageChange
        {
            add
            {
                i18n.OnLanguageChange += value;
            }
            remove
            {
                i18n.OnLanguageChange -= value;
            }
        }
        #endregion
    }
}
