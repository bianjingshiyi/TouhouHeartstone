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
        /// 加载文件中的皮肤
        /// </summary>
        /// <param name="path"></param>
        /// <param name="platform">当前平台</param>
        /// <param name="logger">提示用Logger</param>
        /// <returns></returns>
        public async Task<CardSkinData[]> loadSkins(string path, PlatformCompability platform = null, StageLogger logger = null, bool loadResources = true)
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
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool != null)
            {
                CardSkinData skin = new CardSkinData();
                skin.id = cardDefineId;
                skin.skinName = StringTable.EDITOR_NEW_SKIN_DEFAULT_NAME;
                var skinIds = getAllLocalSkins(cardPoolId, cardDefineId).GroupBy(c => c.skinId).Select(g => g.Key);
                if (skinIds.Count() > 0)
                    skin.skinId = skinIds.Max() + 1;
                else
                    skin.skinId = 1;
                return skin;
            }
            else
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
        }
        public CardSkinData createSkinForDiffLanguage(long cardPoolId, CardSkinData skin, string language)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool != null)
            {
                CardSkinData newSkin = new CardSkinData(skin);
                newSkin.language = language;
                return newSkin;
            }
            else
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
        }
        public CardSkinData[] cloneCardSkins(CardPoolInfo info, int cardId)
        {
            return cloneCardSkins(getAllSkins(info.ToReference(), cardId));
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
        /// <summary>
        /// 添加临时的卡片皮肤
        /// </summary>
        /// <param name="skin"></param>
        public void AddCardSkinTemp(CardSkinData skin)
        {
            if (skinDic.ContainsKey(skin.id))
            {
                Debug.LogError($"已存在相同的皮肤{skinDic[skin.id]}");
                return;
            }
            if (tempSkinDic.ContainsKey(skin.id))
            {
                Debug.LogError($"已存在相同的皮肤{tempSkinDic[skin.id]}");
                return;
            }
            tempSkinDic.Add(skin.id, skin);
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
            return getDefaultSkin(getCardPoolReferenceOfCard(cardDefine), cardDefine.id);
        }
        public CardSkinData getDefaultSkin(ICardPoolReference cardPoolRef, int id)
        {
            var skinGroup = getAllSkins(cardPoolRef, id).GroupBy(s => s.skinId).FirstOrDefault();
            if (skinGroup == null)
            {
                //卡牌没有任何皮肤
                throw new InvalidOperationException($"卡牌{id}没有任何皮肤");
            }
            //获取对应语言的皮肤
            CardSkinData skin = skinGroup.FirstOrDefault(s => s.language == game.langManager.getLanguage());
            //如果没有对应语言，就第一个
            if (skin == null)
                skin = skinGroup.First();
            return skin;
        }
        /// <summary>
        /// 根据id获取卡片皮肤
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CardSkinData getCardSkin(ICardPoolReference cardPoolRef, int cardId)
        {
            if (cardPoolRef.CompareCardPool(builtinCardPoolReference))
            {
                if (skinDic.TryGetValue(cardId, out CardSkinData builtInSkin))
                    return builtInSkin;
            }
            if (tryGetCardSkin(cardPoolRef, cardId, out CardSkinData skin))
                return skin;
            return null;
        }
        public CardSkinData getCardSkin(CardDefine cardDefine)
        {
            if (cardDefine == null)
                return null;
            return getCardSkin(getCardPoolReferenceOfCard(cardDefine), cardDefine.id);
        }
        public bool tryGetCardSkin(ICardPoolReference cardPoolRef, int cardId, out CardSkinData skin)
        {
            skin = null;
            if (tryGetCardPool(cardPoolRef, out var cardPool))
            {
                //查找所有皮肤
                var skinGroups = cardPool.cardSkinList.Where(s => s.id == cardId).GroupBy(s => s.skinId).OrderBy(g => g.Key);
                if (skinGroups.Count() > 0)
                {
                    var firstSkinGroup = skinGroups.First();
                    //如果存在语言相同的皮肤，则返回
                    skin = firstSkinGroup.FirstOrDefault(s => s.language == game.langManager.getLanguage());
                    if (skin != null)
                        return true;
                    //如果存在默认语言的皮肤，则返回
                    skin = firstSkinGroup.FirstOrDefault(s => s.language == game.langManager.getDefaultLanguage());
                    if (skin != null)
                        return true;
                    //都没有，随便返回第一个
                    skin = firstSkinGroup.First();
                    return true;
                }
            }
            if (tempSkinDic.ContainsKey(cardId))
            {
                skin = tempSkinDic[cardId];
                return true;
            }
            if (skinDic.ContainsKey(cardId))
            {
                skin = skinDic[cardId];
                return true;
            }
            return false;
        }
        public CardSkinData[] getAllLocalSkins(long cardPoolId, int cardId)
        {
            var localRef = getLocalCardPoolReference(cardPoolId);
            if (localRef != null)
            {
                return getAllSkins(localRef, cardId);
            }
            return Array.Empty<CardSkinData>();
        }

        public CardSkinData[] getAllSkins(ICardPoolReference cardPoolRef, int cardId)
        {
            List<CardSkinData> skinList = new List<CardSkinData>();
            for (int i = 0; i < _skins.Length; i++)
            {
                if (_skins[i].id == cardId)
                {
                    skinList.Add(_skins[i]);
                }
            }
            if (skinDic.TryGetValue(cardId, out CardSkinData builtinSkin))
            {
                skinList.Add(builtinSkin);
            }
            if (tempSkinDic.TryGetValue(cardId, out CardSkinData tempSkin))
            {
                skinList.Add(tempSkin);
            }
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                foreach (var skin in cardPool.cardSkinList)
                {
                    if (skin.id == cardId)
                    {
                        skinList.Add(skin);
                    }
                }
            }
            return skinList.ToArray();
        }
        public CardSkinData[] getAllSkins()
        {
            List<CardSkinData> skinList = new List<CardSkinData>();
            skinList.AddRange(_skins);
            foreach (var cardPool in _cardPoolDict.Values)
            {
                skinList.AddRange(cardPool.cardSkinList);
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
                    if (imagePath.StartsWith("res:"))
                    {
                        skin.image = Resources.Load<Sprite>(imagePath.Replace("res:", string.Empty));
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
                if (imagePath == null || imagePath.StartsWith("res:"))
                {
                    //什么也不做
                }
                else
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
                    if (imagePath.StartsWith("res:"))
                    {
                        skin.image = Resources.Load<Sprite>(imagePath.Replace("res:", string.Empty));
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
                if (imagePath == null || imagePath.StartsWith("res:"))
                {
                    //什么也不做
                }
                else
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
        Dictionary<int, CardSkinData> skinDic { get; } = new Dictionary<int, CardSkinData>();
        Dictionary<int, CardSkinData> tempSkinDic { get; } = new Dictionary<int, CardSkinData>();
        [SerializeField]
        CardSkinData[] _skins = new CardSkinData[0];
        #endregion
    }
}
