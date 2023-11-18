using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using Controllers;
using IGensoukyo.Utilities;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using UI;
using UnityEngine;

namespace Game
{
    public partial class CardManager
    {
        #region 公有方法

        #region 加载卡牌皮肤
        /// <summary>
        /// 加载xls文件中的皮肤
        /// </summary>
        /// <param name="path"></param>
        /// <param name="platform">当前平台</param>
        /// <param name="logger">提示用Logger</param>
        /// <returns></returns>
        public async Task<CardSkinData[]> loadXLSSkins(string path, PlatformCompability platform = null, StageLogger logger = null, bool loadResources = true)
        {
            platform = platform ?? PlatformCompability.Current;

            DataSet dataset = platform.SupportExcelReading ?
                await getManager<THHResourceManager>().loadExcelAsDataSet(path, platform) :
                await getManager<THHResourceManager>().loadDataSet(path, platform);

            if (dataset.Tables.Count < 1) throw new Exception("空数据集");
            var cardTable = dataset.Tables[0];
            var skinTable = dataset.Tables[1];
            List<CardSkinData> skins = new List<CardSkinData>();

            // Excel的第一行是注释，忽略
            for (int i = 1; i < cardTable.Rows.Count; i++)
            {
                try
                {
                    var datarow = cardTable.Rows[i];

                    if (datarow.ReadCol("Ignore", false))
                        continue;
                    CardSkinData skin = await loadSkinInCardTable(path, datarow, logger, loadResources);
                    int index = skins.FindIndex(s => s.id == skin.id);
                    if (index > 0)
                        skins[index] = skin;
                    else
                        skins.Add(skin);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error when loading {path} (row {i}): {e}");
                }
            }
            for (int i = 1; i < skinTable.Rows.Count; i++)
            {
                try
                {
                    var datarow = skinTable.Rows[i];

                    CardSkinData skin = await loadSkinInSkinTable(path, datarow, loadResources);
                    int index = skins.FindIndex(s => s.id == skin.id);
                    if (index > 0)
                        skins[index] = skin;
                    else
                        skins.Add(skin);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error when loading {path} (row {i}): {e}");
                }
            }
            return skins.ToArray();
        }
        #endregion

        #region 创建卡牌皮肤
        public CardSkinData createSkin(long cardPoolId, int cardDefineId)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            if (tryGetCardPool(cardPoolRef, out var cardPool))
            {
                CardSkinData skin = new CardSkinData();
                skin.id = cardDefineId;
                skin.skinName = StringTable.EDITOR_NEW_SKIN_DEFAULT_NAME;
                skin.language = game.lang.getLanguage();

                var skinIds = getAllCardSkins(cardPoolRef, cardDefineId).GroupBy(c => c.skinId).Select(g => g.Key);
                if (skinIds.Count() > 0)
                    skin.skinId = skinIds.Max() + 1;
                else
                    skin.skinId = 1;
                return skin;
            }
            else
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
        }
        public CardSkinData createSkinForDiffLanguage(CardSkinData skin, string language)
        {
            CardSkinData newSkin = new CardSkinData(skin);
            newSkin.language = language;
            return newSkin;
        }
        public CardSkinData[] cloneCardSkins(CardPoolInfo info, int cardId)
        {
            return cloneCardSkins(getAllCardSkins(info.ToReference(), cardId));
        }
        public CardSkinData[] cloneCardSkins(CardSkinData[] skins)
        {
            var json = skins.toJson();
            var clone = JsonHelper.fromJson<CardSkinData[]>(json);
            return clone;
        }
        #endregion

        #region 添加卡牌皮肤
        public void addCardSkin(long cardPoolId, CardSkinData cardSkin)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
            }
            cardPool.cardSkinList.Add(cardSkin);
        }
        #endregion

        #region 删除卡牌皮肤
        public void removeAllCardSkins(long cardPoolId, int cardDefineId)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
            }
            cardPool.cardSkinList.RemoveAll(s => s.id == cardDefineId);
        }
        public void removeSkin(long cardPoolId, int cardDefineId, int skinId)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool != null)
            {
                cardPool.cardSkinList.RemoveAll(s => s.id == cardDefineId && s.skinId == skinId);
            }
            else
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
        }
        #endregion

        #region 获取卡牌皮肤
        public CardSkinData getDefaultSkin(CardDefine cardDefine)
        {
            var poolRef = getCardPoolReferenceOfCard(cardDefine);
            return getDefaultSkin(poolRef, cardDefine.id);
        }
        public CardSkinData getDefaultSkin(ICardPoolReference cardPoolRef, int cardId)
        {
            if (tryGetDefaultCardSkin(cardPoolRef, cardId, out var skin))
            {
                return skin;
            }
            return null;
        }
        public bool tryGetDefaultCardSkin(ICardPoolReference cardPoolRef, int cardId, out CardSkinData skin)
        {
            if (cardPoolRef == null)
            {
                skin = null;
                return false;
            }
            if (isBuiltinCardPool(cardPoolRef.CardPoolID) && builtinSkinDic.ContainsKey(cardId))
            {
                skin = builtinSkinDic[cardId];
                return true;
            }
            var skins = getAllCardSkins(cardPoolRef, cardId);
            skin = skins.getDefaultSkin(game.langManager.getLanguage());
            return skin != null;
        }
        /// <summary>
        /// 根据id获取卡片皮肤
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CardSkinData getCardSkin(ICardPoolReference cardPoolRef, int cardId) => getDefaultSkin(cardPoolRef, cardId);
        public CardSkinData getCardSkin(CardDefine cardDefine) => getDefaultSkin(cardDefine);
        public bool tryGetCardSkin(ICardPoolReference cardPoolRef, int cardId, out CardSkinData skin) => tryGetDefaultCardSkin(cardPoolRef, cardId, out skin);
        public CardSkinData[] getAllCardSkins(ICardPoolReference cardPoolRef, int cardId)
        {
            List<CardSkinData> skinList = new List<CardSkinData>();
            skinList.AddRange(_skins.Where(s => s.id == cardId));
            skinList.AddRange(builtinSkinDic.Where(p => p.Key == cardId).Select(p => p.Value));
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                skinList.AddRange(cardPool.cardSkinList.Where(s => s.id == cardId));
            }
            return skinList.ToArray();
        }
        #endregion

        #endregion

        #region 私有方法
        private async Task<CardSkinData> loadSkinInCardTable(string path, DataRow datarow, StageLogger logger = null, bool loadResources = true)
        {
            //加载文字信息
            var skin = new CardSkinData()
            {
                id = datarow.ReadCol<int>("ID"),
                name = datarow.ReadCol<string>("Name"),
                desc = datarow.ReadCol("Desc", "")
            };
            logger?.NextSub($"加载卡片资源: {skin.name}");
            //加载图片
            string imagePath = datarow.ReadCol("Image", "");

            if (loadResources)
            {
                try
                {
                    if (imagePath.isResourcesPath(out var resourcesPath))
                    {
                        skin.image = Resources.Load<Sprite>(resourcesPath);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(imagePath))
                        {
                            skin.image = getDefaultSprite();
                        }
                        else
                        {
                            Texture2D texture = await getManager<THHResourceManager>().loadTexture(imagePath, Path.GetDirectoryName(path));
                            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100);
                            skin.image = sprite;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"加载贴图 {imagePath} 失败：{e}");
                    skin.image = getDefaultSprite();
                }
            }
            else
            {
                if (imagePath != null && !imagePath.isResourcesPath(out _))
                {
                    //标记贴图路径为流文件
                    imagePath = $"sa:{Path.GetDirectoryName(path)}/{imagePath}";
                }
                skin.imagePath = imagePath;
            }

            return skin;
        }
        private async Task<CardSkinData> loadSkinInSkinTable(string path, DataRow datarow, bool loadResources = true)
        {
            //加载文字信息
            var skin = new CardSkinData()
            {
                id = datarow.ReadCol<int>("ID"),
                name = datarow.ReadCol<string>("Name"),
                desc = datarow.ReadCol("Desc", "")
            };
            //加载图片
            string imagePath = datarow.ReadCol("Image", "");
            if (loadResources)
            {
                try
                {
                    if (imagePath.isResourcesPath(out string resourcesPath))
                    {
                        skin.image = Resources.Load<Sprite>(resourcesPath);
                    }
                    else
                    {
                        Texture2D texture = await getManager<THHResourceManager>().loadTexture(imagePath, Path.GetDirectoryName(path));
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100);
                        skin.image = sprite;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"加载贴图 {imagePath} 失败：{e}");
                    skin.image = getDefaultSprite();
                }
            }
            else
            {
                if (imagePath != null && !imagePath.isResourcesPath(out _))
                {
                    //标记为流文件
                    imagePath = $"sa:{Path.GetDirectoryName(path)}/{imagePath}";
                }
                skin.imagePath = imagePath;
            }
            //加载音效
            string soundPath = datarow.ReadCol("Sound_onUse", "");
            if (loadResources)
            {
                if (!string.IsNullOrEmpty(soundPath))
                    skin.sound_onUse = await getManager<THHResourceManager>().loadAudio(soundPath);
            }
            else
            {
                skin.soundPath_onUse = soundPath;
            }
            //加载投射物
            string projPath = datarow.ReadCol("Projectile", "");
            skin.projectilePath = projPath;
            return skin;
        }
        #endregion

        #region 属性字段
        Dictionary<int, CardSkinData> builtinSkinDic { get; } = new Dictionary<int, CardSkinData>();
        [SerializeField]
        CardSkinData[] _skins = new CardSkinData[0];
        #endregion
    }

    public static class CardSkinHelper
    {
        /// <summary>
        /// 获取卡牌皮肤列表中的所有指定语言的皮肤。每个皮肤ID组内如果没有指定语言的的皮肤，改为获取组内第一个皮肤。
        /// </summary>
        /// <param name="skins">卡牌皮肤列表。</param>
        /// <param name="language">指定语言。</param>
        /// <returns>指定语言的卡牌皮肤数组。</returns>
        public static CardSkinData[] getCardSkinsOfLanguage(this IEnumerable<CardSkinData> skins, string language)
        {
            return skins.GroupBy(s => s.skinId).Select(g => g.getCardSkinOfLanguageInGroup(language)).ToArray();
        }
        /// <summary>
        /// 获取卡牌皮肤列表中的所有指定皮肤ID的皮肤。每个语言组内如果没有指定皮肤ID的的皮肤，则不会加入到列表中。
        /// </summary>
        /// <param name="skins">卡牌皮肤列表。</param>
        /// <param name="skinId">指定皮肤ID。</param>
        /// <returns>指定皮肤ID的卡牌皮肤数组。</returns>
        public static CardSkinData[] getCardSkinsOfId(this IEnumerable<CardSkinData> skins, int skinId)
        {
            return skins.GroupBy(s => s.language).Select(g => g.getCardSkinOfIdInGroup(skinId)).Where(s => s != null).ToArray();
        }
        /// <summary>
        /// 获取卡牌皮肤语言组内的指定皮肤ID的卡牌皮肤。
        /// </summary>
        /// <param name="group">卡牌皮肤语言组。</param>
        /// <param name="skinId">指定皮肤ID。</param>
        /// <returns>指定皮肤ID的皮肤。</returns>
        public static CardSkinData getCardSkinOfIdInGroup(this IGrouping<string, CardSkinData> group, int skinId)
        {
            return group.FirstOrDefault(s => s.skinId == skinId);
        }
        /// <summary>
        /// 获取卡牌皮肤ID组内的指定语言的卡牌皮肤。如果不存在，改为获取第一个皮肤。
        /// </summary>
        /// <param name="group">卡牌皮肤语言组。</param>
        /// <param name="language">指定皮肤语言。</param>
        /// <returns>指定皮肤语言的皮肤。</returns>
        public static CardSkinData getCardSkinOfLanguageInGroup(this IGrouping<int, CardSkinData> group, string language)
        {
            return group.FirstOrDefault(s => s.language == language) ?? group.FirstOrDefault();
        }
        /// <summary>
        /// 获取一个卡牌皮肤列表中的，语言为指定语言的默认皮肤。
        /// </summary>
        /// <param name="skins">皮肤列表。</param>
        /// <param name="language">指定语言。</param>
        /// <returns>指定语言的默认皮肤。</returns>
        public static CardSkinData getDefaultSkin(this IEnumerable<CardSkinData> skins, string language)
        {
            return skins.getCardSkinsOfLanguage(language).FirstOrDefault();
        }
    }
}
