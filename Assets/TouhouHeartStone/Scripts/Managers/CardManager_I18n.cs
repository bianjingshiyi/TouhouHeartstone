using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BJSYGameCore;
using Controllers;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Game
{
    public partial class CardManager : Manager
    {
        private (I18n i18n, CultureInfo[] cultures) getOrLoadCardPoolI18n(CardPoolInfo poolInfo, CardPool pool)
        {
            if (cardPoolI18nDict.TryGetValue(poolInfo.cardPoolId, out var existing))
                return existing;
            string moFilePrefix = null;
            switch (poolInfo.cardPoolId)
            {
                case STANDARD_POOL_ID:
                    moFilePrefix = cardPoolMoPrefixClassic;
                    break;
                default:
                    return (null, null);
            }

            try
            {
                IEnumerable<string> filePaths;
                string searchPattern = "*.mo";
                if (!Application.isEditor || _enableStreamingAssetsInEditor)
                {
                    filePaths = game.fileManager.getStreamingFiles(cardPoolMoDirectory, searchPattern);
                }
                else
                {
                    filePaths = game.fileManager.getFiles(Path.GetFullPath(editorCardPoolMoDirectory), searchPattern);
                }
                I18n i18n = new I18n();
                List<CultureInfo> cultures = new List<CultureInfo>();
                foreach (var moPath in filePaths)
                {
                    var match = Regex.Match(moPath, $"{moFilePrefix}\\.([a-zA-Z\\-_]+)\\.mo");
                    if (!match.Success)
                        continue;
                    var language = match.Groups[1].Value;
                    var path = Path.Combine(cardPoolMoDirectory, $"{moFilePrefix}.{language}.mo");
                    var culture = new CultureInfo(language.Replace('_', '-'));
                    // 加载语言资源
                    i18n.LoadFile(path, culture);
                    cultures.Add(culture);
                }
                var tuple = (i18n, cultures.ToArray());
                cardPoolI18nDict.Add(poolInfo.cardPoolId, tuple);
                return tuple;
            }
            catch (Exception e)
            {
                Debug.LogError($"加载卡池{poolInfo.cardPoolName}翻译文件失败：{e.Message}");
            }
            return (null, null);
        }
        private void importCardPoolI18n(CardPoolInfo cardPoolInfo, CardPool cardPool)
        {
            var (i18n, cultures) = getOrLoadCardPoolI18n(cardPoolInfo, cardPool);
            if (i18n == null)
                return;
            foreach (var culture in cultures)
            {
                var catalog = i18n.GetCatalog(culture);
                foreach (var card in cardPool.cardDefineList)
                {
                    var defaultSkin = cardPool.cardSkinList.FirstOrDefault(s => s.language == sourceLanaguage && s.id == card.id);
                    if (defaultSkin == null)
                        continue;
                    var name = catalog.GetParticularString(cardNameContext, defaultSkin.name);
                    var subname = catalog.GetParticularString(cardSubnameContext, defaultSkin.subname);
                    var desc = catalog.GetParticularString(cardDescContext, defaultSkin.desc);

                    if (name == defaultSkin.name && subname == defaultSkin.subname && desc == defaultSkin.desc)
                        continue;

                    var translatedSkin = cardPool.cardSkinList.FirstOrDefault(s => s.language == culture.Name && s.id == card.id);
                    if (translatedSkin == null)
                    {
                        translatedSkin = new CardSkinData(defaultSkin);
                        translatedSkin.skinName = name;
                        translatedSkin.language = culture.Name;
                        cardPool.cardSkinList.Add(translatedSkin);
                    }
                    translatedSkin.name = name;
                    translatedSkin.subname = subname;
                    translatedSkin.desc = desc;
                }
                foreach (var buff in cardPool.buffDefineList)
                {
                    var defaultSkin = cardPool.buffSkinList.FirstOrDefault(s => s.language == sourceLanaguage && s.buffId == buff.id);
                    if (defaultSkin == null)
                        continue;
                    var name = catalog.GetParticularString(buffNameContext, defaultSkin.name);
                    var desc = catalog.GetParticularString(buffDescContext, defaultSkin.desc);

                    if (name == defaultSkin.name && desc == defaultSkin.desc)
                        continue;

                    var translatedSkin = cardPool.buffSkinList.FirstOrDefault(s => s.language == culture.Name && s.buffId == buff.id);
                    if (translatedSkin == null)
                    {
                        translatedSkin = new BuffSkin(defaultSkin);
                        translatedSkin.language = culture.Name;
                        cardPool.buffSkinList.Add(translatedSkin);
                    }
                    translatedSkin.name = name;
                    translatedSkin.desc = desc;
                }
            }
        }
        public const string cardNameContext = "卡牌名称";
        public const string cardSubnameContext = "卡牌副标题";
        public const string cardDescContext = "卡牌描述";
        public const string buffNameContext = "增益名称";
        public const string buffDescContext = "增益描述";
        public const string sourceLanaguage = "zh-Hans";
        private Dictionary<long, (I18n i18n, CultureInfo[] languages)> cardPoolI18nDict = new Dictionary<long, (I18n i18n, CultureInfo[] languages)>();
        [SerializeField]
        private string editorCardPoolMoDirectory = "ExternalData/i18n/cards";
        [SerializeField]
        private string cardPoolMoDirectory = "i18n/cards";
        [SerializeField]
        private string cardPoolMoPrefixClassic = "classic";
    }
}
