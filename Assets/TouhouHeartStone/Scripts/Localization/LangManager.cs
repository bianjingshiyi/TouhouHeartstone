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
        #region 公共方法

        #region 语言
        public string[] getLanguages()
        {
            //返回副本，避免被更改
            string[] languages = new string[langs.Length];
            Array.Copy(langs, 0, languages, 0, langs.Length);
            return languages;
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
        #endregion

        #region 翻译代理
        /// <summary>
        /// 获取当前语言对应字符串
        /// </summary>
        /// <param name="sourceString"></param>
        /// <returns></returns>
        [Obsolete("Use _() instead.")]
        public string getString(string sourceString)
        {
            return _(sourceString);
        }
        public string _(string text)
        {
            return ui._(text);
        }

        public string _(string text, params object[] args)
        {
            return ui._(text, args);
        }

        public string _n(string text, string pluralText, long n)
        {
            return ui._n(text, pluralText, n);
        }

        public string _n(string text, string pluralText, long n, params object[] args)
        {
            return ui._n(text, pluralText, n, args);
        }

        public string _p(string context, string text)
        {
            return ui._p(context, text);
        }

        public string _p(string context, string text, params object[] args)
        {
            return ui._p(context, text, args);
        }

        public string _pn(string context, string text, string pluralText, long n)
        {
            return ui._pn(context, text, pluralText, n);
        }

        public string _pn(string context, string text, string pluralText, long n, params object[] args)
        {
            return ui._pn(context, text, pluralText, n, args);
        }
        public event Action OnLanguageChange
        {
            add
            {
                ui.OnLanguageChange += value;
                nodeDoc.OnLanguageChange += value;
                
            }
            remove
            {
                ui.OnLanguageChange -= value;
                nodeDoc.OnLanguageChange -= value;
            }
        }
        #endregion

        #endregion

        #region 私有方法

        #region 生命周期
        protected override void onAwake()
        {
            base.onAwake();
            loadAll();
        }
        #endregion

        #region 加载

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
                    ui.AddEmpty(defaultCulture);
                    nodeDoc.AddEmpty(defaultCulture);
                    continue;
                }
                loadLang(lang);
            }
        }

        private void loadLang(string lang)
        {
            var culture = new CultureInfo(lang);
            cultures.Add(culture);
            UberDebug.Log($"加载语言{lang}的翻译");
            loadI18N(culture, ui, uiFilePath, "UI");
            loadI18N(culture, nodeDoc, nodeDocFilePath, "节点文档");
        }
        private static void loadI18N(CultureInfo culture, I18n i18n, string pathTemplate, string part)
        {
            var lang = culture.Name;
            UberDebug.Log($"加载语言{lang}的{part}翻译");
            var langName = lang.Replace("-", "_");
            try
            {
                var path = string.Format(pathTemplate, langName);
                i18n.LoadFile(path, culture);
            }
            catch(Exception e)
            {
                UberDebug.LogError($"无法载入语言{lang}的{part}翻译文件, {e.Message}");
                // 为没有翻译文件的加载一个默认空翻译
                i18n.AddEmpty(culture);
            }
        }
        #endregion

        #endregion

        #region 事件
        /// <summary>
        /// 当语言切换
        /// </summary>
        public event Func<CultureInfo, Task> OnCultureChange;
        #endregion

        #region 属性字段
        public const string CN = "zh-CN";
        public const string EN = "en-US";
        /// <summary>
        /// 可用的语言
        /// </summary>
        public IEnumerable<CultureInfo> Cultures => cultures;
        /// <summary>
        /// 当前语言
        /// </summary>
        public CultureInfo CurrentCulture
        {
            get => currentCulture;
            set
            {
                currentCulture = value;
                ui.SetLang(currentCulture);
                nodeDoc.SetLang(currentCulture);

                OnCultureChange?.Invoke(CurrentCulture);
            }
        }
        public I18n ui { get; } = new I18n();
        public I18n nodeDoc { get; } = new I18n();
        [Obsolete]
        I18n ITranslationProvider.i18n => ui;
        CultureInfo currentCulture;
        List<CultureInfo> cultures = new List<CultureInfo>();
        /// <summary>
        /// 默认载入路径
        /// </summary>
        [SerializeField]
        string uiFilePath = "i18n/ui/{0}.mo";
        /// <summary>
        /// 默认载入路径
        /// </summary>
        [SerializeField]
        string nodeDocFilePath = "i18n/docs/node.{0}.mo";
        /// <summary>
        /// 载入的语言
        /// </summary>
        [SerializeField]
        string[] langs = { CN, EN };
        /// <summary>
        /// 默认语言
        /// </summary>
        [SerializeField]
        string defaultLang = CN;
        /// <summary>
        /// 不要加载默认语言
        /// </summary>
        [SerializeField]
        bool dontLoadDefaultLang = true;
        #endregion
    }
}
