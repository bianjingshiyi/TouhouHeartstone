using UnityEngine;
using BJSYGameCore;
using MukioI18n;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;

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
        public string getSystemLanguage()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.English:
                    return EN;
                default:
                    return CN;
            }
        }
        public void setLanguage(string cultureName)
        {
            CurrentCulture = new CultureInfo(cultureName);
        }
        public void loadI18n(I18n i18n, string pathTemplate, string part)
        {
            foreach (var culture in cultures)
            {
                var lang = culture.Name;
                // 在部分情况下，可能想要使用程序内的源文本作为翻译文本
                if (lang == getDefaultLanguage() && dontLoadDefaultLang)
                {
                    i18n.AddEmpty(culture);
                }
                else
                {
                    UberDebug.Log($"加载语言{culture.Name}的{part}翻译文件");
                    var langName = lang.Replace("-", "_");
                    try
                    {
                        var path = string.Format(pathTemplate, langName);
                        i18n.LoadFile(path, culture);
                    }
                    catch (Exception e)
                    {
                        UberDebug.LogError($"无法载入语言{lang}的{part}翻译文件, {e.Message}");
                        // 为没有翻译文件的加载一个默认空翻译
                        i18n.AddEmpty(culture);
                    }
                }
            }
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
        #endregion

        #endregion

        #region 私有方法

        #region 生命周期
        protected override void onAwake()
        {
            base.onAwake();
            i18n.OnLanguageChange += OnLanguageChangeCallback;
            loadAll();
        }
        #endregion

        #region 事件回调
        public void OnLanguageChangeCallback()
        {
            OnLanguageChange?.Invoke();
        }
        #endregion

        #region 加载

        private void loadAll()
        {
            // 设置当前的默认语言
            var systemCulture = new CultureInfo(getSystemLanguage());
            CurrentCulture = systemCulture;
            cultures.AddRange(langs.Select(l => new CultureInfo(l)));

            loadI18n(i18n, uiFilePath, "UI");
        }
        #endregion

        #endregion

        #region 事件
        /// <summary>
        /// 当语言切换
        /// </summary>
        public event Action<CultureInfo> OnCultureChange;
        public event Action OnLanguageChange;
        #endregion

        #region 属性字段
        public const string CN = "zh-Hans";
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
                i18n.SetLang(currentCulture);
                OnCultureChange?.Invoke(CurrentCulture);
            }
        }
        public I18n i18n { get; } = new I18n();
        [Obsolete]
        I18n ITranslationProvider.i18n => i18n;
        CultureInfo currentCulture;
        List<CultureInfo> cultures = new List<CultureInfo>();
        /// <summary>
        /// 默认载入路径
        /// </summary>
        [SerializeField]
        string uiFilePath = "i18n/ui/{0}.mo";
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
