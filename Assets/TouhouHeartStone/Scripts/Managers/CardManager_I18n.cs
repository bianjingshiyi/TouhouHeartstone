using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BJSYGameCore;
using Controllers;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using UI;
using UnityEngine;

namespace Game
{
    public partial class CardManager : Manager
    {
        #region 公有方法

        #region 增益名称
        public void setBuffName(ICardPoolReference poolRef, BuffSkin skin, string language, string translation)
        {
            if (skin == null)
                return;
            skin.nameKey = setCardPoolLocalizedString(poolRef, skin.nameKey, language, translation);
        }
        public string getBuffName(ICardPoolReference poolRef, int id)
        {
            return getBuffName(poolRef, getBuffSkin(poolRef, id));
        }
        public string getBuffName(ICardPoolReference poolRef, BuffSkin skin)
        {
            return getLocalizedBuffName(poolRef, skin, game.lang.getLanguage());
        }
        /// <summary>
        /// 获取增益名称，如果没有指定语言，则使用当前语言。
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public string getBuffName(BuffDefine buff, string language = null)
        {
            if (tryGetLocalCardPool(buff.cardPoolId, out CardPool cardPool))
            {
                BuffSkin buffSkin = cardPool.buffSkinList.FirstOrDefault(s => s.buffId == buff.id);
                if (buffSkin != null)
                {
                    return cardPool.localizedStringList.localize(buffSkin.nameKey, language ?? game.lang.getLanguage());
                }
            }
            return game.lang._(StringTable.BUFF_NAME_UNKNOWN);
        }
        public string getLocalizedBuffName(ICardPoolReference poolRef, BuffSkin skin, string language)
        {
            if (skin == null)
                return game.lang._(StringTable.BUFF_NAME_UNKNOWN);
            if (!tryGetCardPool(poolRef, out CardPool cardPool))
                return game.lang._(StringTable.BUFF_NAME_UNKNOWN);
            return cardPool.localizedStringList.localize(skin.nameKey, language);
        }
        #endregion

        #region 增益描述
        public void setBuffDesc(ICardPoolReference poolRef, BuffSkin skin, string language, string translation)
        {
            if (skin == null)
                return;
            skin.descKey = setCardPoolLocalizedString(poolRef, skin.descKey, language, translation);
        }
        public string getBuffDesc(ICardPoolReference poolRef, int id)
        {
            return getBuffDesc(poolRef, getBuffSkin(poolRef, id));
        }
        public string getBuffDesc(ICardPoolReference poolRef, BuffSkin skin)
        {
            return getLocalizedBuffDesc(poolRef, skin, game.lang.getLanguage());
        }
        public string getLocalizedBuffDesc(ICardPoolReference poolRef, BuffSkin skin, string language)
        {
            if (skin == null)
                return game.lang._(StringTable.BUFF_DESC_UNKNOWN);
            if (!tryGetCardPool(poolRef, out CardPool cardPool))
                return game.lang._(StringTable.BUFF_DESC_UNKNOWN);
            return cardPool.localizedStringList.localize(skin.descKey, language);
        }
        #endregion

        #region 增益贴图路径
        public void setBuffImagePath(ICardPoolReference poolRef, BuffSkin skin, string language, string translation)
        {
            if (skin == null)
                return;
            skin.imagePathKey = setCardPoolLocalizedResource(poolRef, skin.imagePathKey, language, translation);
        }
        public string getBuffImagePath(ICardPoolReference poolRef, int id)
        {
            return getBuffImagePath(poolRef, getBuffSkin(poolRef, id));
        }
        public string getBuffImagePath(ICardPoolReference poolRef, BuffSkin skin)
        {
            return getLocalizedImagePath(poolRef, skin, game.lang.getLanguage());
        }
        public string getLocalizedImagePath(ICardPoolReference poolRef, BuffSkin skin, string language)
        {
            if (skin == null)
                return null;
            if (!tryGetCardPool(poolRef, out CardPool cardPool))
                return null;
            return cardPool.localizedResourceList.localize(skin.imagePathKey, language);
        }
        #endregion

        #region 卡牌名称
        public void setCardName(ICardPoolReference poolRef, CardSkinData skin, string language, string translation)
        {
            if (skin == null)
                return;
            skin.nameKey = setCardPoolLocalizedString(poolRef, skin.nameKey, language, translation);
        }
        public string getCardName(ICardPoolReference poolRef, int id)
        {
            return getCardName(poolRef, getCardSkin(poolRef, id));
        }
        public string getCardName(ICardPoolReference poolRef, CardSkinData skin)
        {
            return getLocalizedCardName(poolRef, skin, game.lang.getLanguage());
        }
        public string getLocalizedCardName(ICardPoolReference poolRef, CardSkinData skin, string language)
        {
            if (skin == null)
                return game.lang._(StringTable.BUILD_UNKNOWN_CARD_NAME);
            if (!tryGetCardPool(poolRef, out CardPool cardPool))
                return game.lang._(StringTable.BUILD_UNKNOWN_CARD_NAME);
            return cardPool.localizedStringList.localize(skin.nameKey, language);
        }
        public string getCardName(CardDefine card, string language = null)
        {
            if (tryGetLocalCardPool(card.cardPoolId, out CardPool cardPool))
            {
                CardSkinData skin = cardPool.cardSkinList.FirstOrDefault(s => s.id == card.id);
                if (skin != null)
                {
                    return cardPool.localizedStringList.localize(skin.nameKey, language ?? game.lang.getLanguage());
                }
            }
            return game.lang._(StringTable.BUILD_UNKNOWN_CARD_NAME);
        }
        #endregion

        #region 卡牌副名称
        public void setCardSubname(ICardPoolReference poolRef, CardSkinData skin, string language, string translation)
        {
            if (skin == null)
                return;
            skin.subnameKey = setCardPoolLocalizedString(poolRef, skin.subnameKey, language, translation);
        }
        public string getCardSubname(ICardPoolReference poolRef, int cardId)
        {
            return getCardSubname(poolRef, getCardSkin(poolRef, cardId));
        }
        public string getCardSubname(ICardPoolReference poolRef, CardSkinData skin)
        {
            return getLocalizedCardSubname(poolRef, skin, game.lang.getLanguage());
        }
        public string getLocalizedCardSubname(ICardPoolReference poolRef, CardSkinData skin, string language)
        {
            if (skin == null)
                return string.Empty;
            if (!tryGetCardPool(poolRef, out CardPool cardPool))
                return string.Empty;
            return cardPool.localizedStringList.localize(skin.subnameKey, language);
        }
        #endregion

        #region 卡牌描述
        public void setCardDesc(ICardPoolReference poolRef, CardSkinData skin, string language, string translation)
        {
            if (skin == null)
                return;
            skin.descKey = setCardPoolLocalizedString(poolRef, skin.descKey, language, translation);
        }
        public string getCardDesc(ICardPoolReference poolRef, int id)
        {
            return getCardDesc(poolRef, getCardSkin(poolRef, id));
        }
        public string getCardDesc(ICardPoolReference poolRef, CardSkinData skin)
        {
            return getLocalizedCardDesc(poolRef, skin, game.lang.getLanguage());
        }
        public string getLocalizedCardDesc(ICardPoolReference poolRef, CardSkinData skin, string language)
        {
            if (skin == null)
                return string.Empty;
            if (!tryGetCardPool(poolRef, out CardPool cardPool))
                return string.Empty;
            return cardPool.localizedStringList.localize(skin.descKey, language);
        }
        #endregion

        #region 卡牌贴图路径
        public void setCardImagePath(ICardPoolReference poolRef, CardSkinData skin, string language, string translation)
        {
            if (skin == null)
                return;
            skin.imagePathKey = setCardPoolLocalizedResource(poolRef, skin.imagePathKey, language, translation);
        }
        public string getCardImagePath(ICardPoolReference poolRef, int id)
        {
            return getCardImagePath(poolRef, getCardSkin(poolRef, id));
        }
        public string getCardImagePath(ICardPoolReference poolRef, CardSkinData skin)
        {
            return getLocalizedImagePath(poolRef, skin, game.lang.getLanguage());
        }
        public string getLocalizedImagePath(ICardPoolReference poolRef, CardSkinData skin, string language)
        {
            if (skin == null)
                return null;
            if (!tryGetCardPool(poolRef, out CardPool cardPool))
                return null;
            return cardPool.localizedResourceList.localize(skin.imagePathKey, language);
        }
        #endregion

        #region 卡池名称
        public void setCardPoolName(CardPoolInfo info, string language, string translation)
        {
            if (info == null)
                return;
            info.cardPoolNameKey = setCardPoolInfoLocalizedString(info, info.cardPoolNameKey, language, translation);
        }
        public string getCardPoolName(CardPoolInfo info)
        {
            return getLocalizedCardPoolName(info, game.lang.getLanguage());
        }
        public string getCardPoolName(ICardPoolReference poolRef)
        {
            if (poolRef == null)
                return game.lang._(StringTable.EDITOR_UNKNOWN_CARD_POOL);
            return getLocalizedCardPoolName(getCardPoolInfo(poolRef), game.lang.getLanguage());
        }
        public string getLocalizedCardPoolName(CardPoolInfo info, string language)
        {
            if (info == null)
                return game.lang._(StringTable.EDITOR_UNKNOWN_CARD_POOL);
            return info.getCardPoolName(language);
        }
        #endregion


        #region 卡池名称
        public void setCardPoolImagePath(CardPoolInfo info, string language, string translation)
        {
            if (info == null)
                return;
            info.imageResPathKey = setCardPoolInfoLocalizedResource(info, info.imageResPathKey, language, translation);
        }
        public string getCardPoolImagePath(CardPoolInfo info)
        {
            return getLocalizedCardPoolImagePath(info, game.lang.getLanguage());
        }
        public string getLocalizedCardPoolImagePath(CardPoolInfo info, string language)
        {
            if (info == null)
                return null;
            return info.getCardPoolImagePath(language);
        }
        #endregion

        public LocalizedResource getLocalizedResourceFromPool(ICardPoolReference poolRef, string key)
        {
            if (!tryGetCardPool(poolRef, out CardPool cardPool))
                return null;
            return cardPool.localizedResourceList.FirstOrDefault(res => res.value == key);
        }

        #endregion

        #region 私有方法
        private string setLocalizedString(List<LocalizedString> set, string key, string language, string translation)
        {
            if (language == LangManager.SOURCE)
            {
                return translation;
            }
            var obj = set.getLocalizedObject<string, LocalizedString>(key);
            if (obj == null)
            {
                obj = new LocalizedString(key);
                set.Add(obj);
            }
            obj.setTranslation(language, translation);
            return key;
        }
        private string setLocalizedResource(List<LocalizedResource> set, string key, string language, string translation)
        {
            if (language == LangManager.SOURCE)
            {
                return translation;
            }
            var obj = set.getLocalizedObject<string, LocalizedResource>(key);
            if (obj == null)
            {
                obj = new LocalizedResource(key);
                set.Add(obj);
            }
            obj.setTranslation(language, translation);
            return key;
        }
        private string setCardPoolLocalizedString(ICardPoolReference poolRef, string key, string language, string translation)
        {
            if (!tryGetCardPool(poolRef, out CardPool cardPool))
                return key;
            return setLocalizedString(cardPool.localizedStringList, key, language, translation);
        }
        private string setCardPoolLocalizedResource(ICardPoolReference poolRef, string key, string language, string translation)
        {
            if (!tryGetCardPool(poolRef, out CardPool cardPool))
                return key;
            return setLocalizedResource(cardPool.localizedResourceList, key, language, translation);
        }
        private string setCardPoolInfoLocalizedString(CardPoolInfo cardPoolInfo, string key, string language, string translation)
        {
            if (cardPoolInfo == null)
                return key;
            return setLocalizedString(cardPoolInfo.localizedStrings, key, language, translation);
        }
        private string setCardPoolInfoLocalizedResource(CardPoolInfo cardPoolInfo, string key, string language, string translation)
        {
            if (cardPoolInfo == null)
                return key;
            return setLocalizedResource(cardPoolInfo.localizedResources, key, language, translation);
        }

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
                Debug.LogError($"加载卡池{getCardPoolName(poolInfo)}翻译文件失败：{e.Message}");
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
                    var cardSkin = cardPool.cardSkinList.FirstOrDefault(s => s.id == card.id);
                    if (cardSkin == null)
                        continue;
                    var name = catalog.GetParticularString(cardNameContext, cardSkin.nameKey);
                    var subname = catalog.GetParticularString(cardSubnameContext, cardSkin.subnameKey);
                    var desc = catalog.GetParticularString(cardDescContext, cardSkin.descKey);
                    updateLocalizedString(culture.Name, cardSkin.nameKey, name);
                    updateLocalizedString(culture.Name, cardSkin.subnameKey, subname);
                    updateLocalizedString(culture.Name, cardSkin.descKey, desc);
                }
                foreach (var buff in cardPool.buffDefineList)
                {
                    var defaultSkin = cardPool.buffSkinList.FirstOrDefault(s => s.buffId == buff.id);
                    if (defaultSkin == null)
                        continue;
                    updateLocalizedString(culture.Name, defaultSkin.nameKey, catalog.GetParticularString(buffNameContext, defaultSkin.nameKey));
                    updateLocalizedString(culture.Name, defaultSkin.descKey, catalog.GetParticularString(buffDescContext, defaultSkin.descKey));
                }
            }

            void updateLocalizedString(string language, string key, string value)
            {
                if (string.IsNullOrEmpty(key))
                    return;
                var str = cardPool.getLocalizedString(key);
                if (str == null)
                {
                    str = new LocalizedString(key);
                    cardPool.localizedStringList.Add(str);
                }
                str.setTranslation(language, value);
            }
        }
        #endregion

        #region 属性字段
        public const string cardNameContext = "卡牌名称";
        public const string cardSubnameContext = "卡牌副标题";
        public const string cardDescContext = "卡牌描述";
        public const string buffNameContext = "增益名称";
        public const string buffDescContext = "增益描述";
        private Dictionary<long, (I18n i18n, CultureInfo[] languages)> cardPoolI18nDict = new Dictionary<long, (I18n i18n, CultureInfo[] languages)>();
        [SerializeField]
        private string editorCardPoolMoDirectory = "ExternalData/i18n/cards";
        [SerializeField]
        private string cardPoolMoDirectory = "i18n/cards";
        [SerializeField]
        private string cardPoolMoPrefixClassic = "classic";
        #endregion
    }
}
