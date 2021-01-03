using UnityEngine;
using BJSYGameCore;
using MukioI18n;
using System.IO;
using System.Globalization;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// 语言管理器
    /// </summary>
    public class LangManager : Manager, ITranslationProvider
    {
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

        List<CultureInfo> cultures = new List<CultureInfo>();

        public IEnumerable<CultureInfo> Cultures => cultures;

        public I18n i18n { get; } = new I18n();

        protected override void onAwake()
        {
            base.onAwake();

            foreach (var lang in langs)
            {
                try
                {
                    var culture = new CultureInfo(lang);
                    cultures.Add(culture);
                    i18n.Load(Path.Combine(Application.streamingAssetsPath, folderPath), culture);
                }
                catch (FileNotFoundException e)
                {
                    UberDebug.LogError($"无法载入语言{lang}的翻译文件, {e.Message}");

                    // 为没有翻译文件的加载一个默认空翻译
                    if (lang == defaultLang)
                        i18n.AddEmpty(new CultureInfo(defaultLang));
                }
            }
            i18n.SetDefaultLang(new CultureInfo(defaultLang));
        }
    }
}
