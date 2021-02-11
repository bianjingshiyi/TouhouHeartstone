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
    public partial class LangManager : Manager, ITranslationProvider
    {
        #region 公共成员
        /// <summary>
        /// 加载初始语言
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public Task initLanguage(CultureInfo cultureInfo)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 切换语言
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public async Task switchLanguage(CultureInfo cultureInfo)
        {
            //加载语言资源
            //触发事件来通知UI进行更改。
            Task changeCultureTask = OnCultureChange?.Invoke(cultureInfo);
            if (changeCultureTask != null)
                await changeCultureTask;
            //卸载已经加载的资源
            throw new NotImplementedException();
        }
        /// <summary>
        /// 获取当前语言对应字符串
        /// </summary>
        /// <param name="sourceString"></param>
        /// <returns></returns>
        public string getString(string sourceString)
        {
            throw new NotImplementedException();
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
                i18n.SetDefaultLang(currentCulture);

                OnCultureChange?.Invoke(CurrentCulture);
            }
        }

        public I18n i18n { get; } = new I18n();

        protected override void onAwake()
        {
            base.onAwake();

            var defaultCulture = new CultureInfo(defaultLang);

            foreach (var lang in langs)
            {
                // 在部分情况下，可能想要使用程序内的源文本作为翻译文本
                if (lang == defaultLang && dontLoadDefaultLang)
                {
                    cultures.Add(defaultCulture);
                    i18n.AddEmpty(defaultCulture);
                    continue;
                }

                try
                {
                    UberDebug.Log($"加载语言{lang}的翻译");
                    var culture = new CultureInfo(lang);
                    cultures.Add(culture);
                    i18n.Load(Path.Combine(Application.streamingAssetsPath, folderPath), culture);
                }
                catch (FileNotFoundException e)
                {
                    UberDebug.LogError($"无法载入语言{lang}的翻译文件, {e.Message}");

                    // 为没有翻译文件的加载一个默认空翻译
                    if (lang == defaultLang)
                        i18n.AddEmpty(defaultCulture);
                }
            }
            // 设置当前的默认语言
            CurrentCulture = defaultCulture;
        }
    }
}
