using System;
using System.Threading.Tasks;
using UI;
using Controllers;
using TouhouCardEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TouhouHeartstone;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法
        /// <summary>
        /// 获取所有效果类型的名称
        /// </summary>
        /// <returns></returns>
        public string[] getEffectNames()
        {
            return _effectTypes;
        }
        /// <summary>
        /// 获取所有给定效果类型的名称
        /// </summary>
        /// <param name="effects"></param>
        /// <returns></returns>
        public string[] getEffectNames(GeneratedEffect[] effects)
        {
            string[] effectNames = new string[effects.Length];
            for (int i = 0; i < effectNames.Length; i++)
            {
                string effectName;
                if (effects[i] is GeneratedHalo)
                    effectName = _effectTypes[1];
                else
                    effectName = _effectTypes[0];
                effectNames[i] = string.Format(game.langManager.getString(StringTable.EDITOR_EDIT_EFFECT), effectName);
            }
            return effectNames;
        }
        public GeneratedEffect createEffectByIndex(int index)
        {
            if (index == 1)
                return new GeneratedHalo();
            else
                return new GeneratedEffect();
        }
        public int getEffectTypeIndex(GeneratedEffect effect)
        {
            if (effect is GeneratedHalo)
                return 1;
            else
                return 0;
        }
        #endregion
        #region 属性字段
        readonly string[] _effectTypes = new string[]
        {
            StringTable.EDITOR_EFFECT_NAME_NORMAL_EFFECT,
            StringTable.EDITOR_EFFECT_NAME_HALO_EFFECT
        };
        #endregion
    }
    partial class CardManager
    {
        #region 公有方法
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onCardPoolInfoRead"></param>
        /// <returns>Key为路径，Value为信息</returns>
        public async Task<Dictionary<string, CardPoolInfo>> getCardPoolInfosAsync(Action<string, CardPoolInfo> onCardPoolInfoRead = null)
        {
            Dictionary<string, CardPoolInfo> dict = new Dictionary<string, CardPoolInfo>();
            var files = game.fileManager.getFiles(CARDPOOLS_DIR_NAME, "*.ddcp", true);
            for (int i = 0; i < files.Length; i++)
            {
                string path = files[i];
                string[] lines = await game.fileManager.readTextFile(path, lineCount: 1);
                if (lines == null || lines.Length < 1)
                    continue;
                CardPoolInfo cardPool = JsonHelper.fromJson<CardPoolInfo>(lines[0]);
                dict.Add(files[i], cardPool);
                onCardPoolInfoRead?.Invoke(files[i], cardPool);
            }
            return dict;
        }
        public void addExternalCardPool(int cardPoolId)
        {
            _externalCardDefineDict.Add(cardPoolId, new List<CardDefine>());
            _externalCardSkinDict.Add(cardPoolId, new List<CardSkinData>());
        }
        /// <summary>
        /// 从可持续文件中加载所有外部卡牌，包括数据和皮肤
        /// </summary>
        /// <returns></returns>
        public async Task loadAllExternalCards()
        {

        }
        public async Task<(CardDefine[] defines, CardSkinData[] skins)> getExternalCardDefinesAsync(string[] paths)
        {
            List<CardDefine> defineList = new List<CardDefine>();
            List<CardSkinData> skinList = new List<CardSkinData>();
            for (int i = 0; i < paths.Length; i++)
            {
                var cards = await getExternalCardDefinesAsync(paths[i]);
                defineList.AddRange(cards.defines);
                skinList.AddRange(cards.skins);
            }
            return (defineList.ToArray(), skinList.ToArray());
        }
        public async Task<(CardDefine[] defines, CardSkinData[] skins, ActionNodeGraphInfo[] actionGraphInfos)> getExternalCardDefinesAsync(string path, bool readEditorInfo = false)
        {
            var lines = await game.fileManager.readTextFile(path, 0, 5);
            if (lines != null && lines.Length > 0)
            {
                //卡池
                string json = lines[0];
                CardPoolInfo cardPool = JsonHelper.fromJson<CardPoolInfo>(json);
                if (_externalCardDefineDict.ContainsKey(cardPool.cardPoolId))
                {
                    CardDefine[] defines = _externalCardDefineDict[cardPool.cardPoolId].ToArray();
                    CardSkinData[] skins = _externalCardSkinDict[cardPool.cardPoolId].ToArray();
                    ActionNodeGraphInfo[] actionGraphInfos = readEditorInfo ? _externalCardActionGraphInfoDict[cardPool.cardPoolId].ToArray() : null;
                    return (defines, skins, actionGraphInfos);
                }
                else
                {
                    //卡牌
                    json = lines[1];
                    CardDefine[] defines = JsonHelper.fromJson<CardDefine[]>(json);
                    _externalCardDefineDict.Add(cardPool.cardPoolId, new List<CardDefine>(defines));
                    //皮肤
                    json = lines[2];
                    CardSkinData[] skins = JsonHelper.fromJson<CardSkinData[]>(json);
                    _externalCardSkinDict.Add(cardPool.cardPoolId, new List<CardSkinData>(skins));
                    //编辑器信息
                    ActionNodeGraphInfo[] actionGraphInfos = null;
                    if (readEditorInfo)
                    {
                        json = lines[3];
                        actionGraphInfos = JsonHelper.fromJson<ActionNodeGraphInfo[]>(json);
                        _externalCardActionGraphInfoDict.Add(cardPool.cardPoolId, new List<ActionNodeGraphInfo>(actionGraphInfos));
                    }
                    //增益
                    if (lines.Length > 4)
                    {
                        json = lines[4];
                        GeneratedBuffDefine[] buffDefines = null;
                        try
                        {
                            buffDefines = JsonHelper.fromJson<GeneratedBuffDefine[]>(json);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("加载" + path + "中的增益发生异常：" + e);
                        }
                        if (!_externalBuffDefineDict.TryGetValue(cardPool.cardPoolId, out var buffList) || buffList == null)
                        {
                            buffList = new List<GeneratedBuffDefine>();
                            _externalBuffDefineDict[cardPool.cardPoolId] = buffList;
                        }
                        if (buffDefines != null)
                        {
                            buffList.AddRange(buffDefines);
                        }
                    }
                    return (defines, skins, actionGraphInfos);
                }
            }
            else
                throw new IOException("无法读取文件" + path);
        }
        public async Task<CardDefine[]> getExternalCardDefinesAsync(IEnumerable<int> cardPoolsId)
        {
            List<CardDefine> cardDefineList = new List<CardDefine>();
            var cardPoolInfoDict = await getCardPoolInfosAsync();
            foreach (var cardPoolId in cardPoolsId)
            {
                foreach (var pair in cardPoolInfoDict)
                {
                    if (pair.Value.cardPoolId == cardPoolId)
                    {
                        var result = await getExternalCardDefinesAsync(pair.Key);
                        cardDefineList.AddRange(result.defines);
                    }
                }
            }
            return cardDefineList.ToArray();
        }
        public void addExternalCard(int cardPoolId, CardDefine cardDefine, CardSkinData cardSkin)
        {
            _externalCardDefineDict[cardPoolId].Add(cardDefine);
            _externalCardSkinDict[cardPoolId].Add(cardSkin);
        }
        public void removeExternalCard(CardDefine card)
        {
            //删除卡牌
            foreach (var cardList in _externalCardDefineDict.Values)
            {
                cardList.Remove(card);
            }
            //删除皮肤
            foreach (var skinList in _externalCardSkinDict.Values)
            {
                skinList.RemoveAll(s => s.id == card.id);
            }
        }
        public bool tryGetExternalCardDefine(int cardId, out CardDefine card)
        {
            card = null;
            foreach (var pair in _externalCardDefineDict)
            {
                card = pair.Value.Find(c => c.id == cardId);
                if (card != null)
                    return true;
            }
            return false;
        }
        public async Task<Sprite> loadExternalImage(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            if (await getSpriteFromCache(path) is Sprite sprite)
            {
                return sprite;
            }
            if (path.StartsWith("res:"))
            {
                //Unity资源
                sprite = Resources.Load<Sprite>(path.Substring(4, path.Length - 4));
                if (sprite == null)
                {
                    Debug.LogError("加载图片" + path + "失败");
                }
            }
            else if (path.StartsWith("sa:"))
            {
                byte[] bytes;
                try
                {
                    try
                    {
                        bytes = await game.fileManager.readStreamingBinaryFile(path.Substring(3, path.Length - 3));
                    }
                    catch (FileNotFoundException)
                    {
                        //可能是后缀不对
                        bytes = await game.fileManager.readStreamingBinaryFile(
                            path.EndsWith(".png") ?
                            path.Substring(3, path.Length - 3).Replace(".png", ".jpg") :
                            path.Substring(3, path.Length - 3).Replace(".jpg", ".png"));
                    }
                    Texture2D texture = new Texture2D(512, 512);
                    texture.LoadImage(bytes);
                    sprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.one * .5f);
                }
                catch (IOException e)
                {
                    Debug.LogError("加载图片" + path + "失败：" + e);
                    sprite = null;
                }
            }
            else
            {
                //外部贴图
                byte[] bytes = await game.fileManager.readBinaryFile(path);
                Texture2D texture = new Texture2D(512, 512);
                texture.LoadImage(bytes);
                sprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.one * .5f);
            }
            //添加到缓存
            addSpriteToCache(path, sprite);
            return sprite;
        }
        /// <summary>
        /// 将外部图片添加到缓存
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sprite"></param>
        public void addSpriteToCache(string path, Sprite sprite)
        {
            externalSpriteCacheDict[path] = sprite;
        }
        /// <summary>
        /// 获取缓存的外部图片
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<Sprite> getSpriteFromCache(string path)
        {
            if (externalSpriteCacheDict.TryGetValue(path, out object value))
            {
                if (value is Sprite)
                {
                    return Task.FromResult(value as Sprite);
                }
                else if (value is Task<Sprite> task)
                    return task;
            }
            return Task.FromResult<Sprite>(null);
        }
        public Task<Sprite> loadCardImage(int cardId)
        {
            CardSkinData skin = getSkin(cardId);
            if (skin.image != null)
                return Task.FromResult(skin.image);
            else
            {
                return loadExternalImage(skin.imagePath);
            }
        }
        public bool tryGetExternalSkin(int cardId, out CardSkinData skin)
        {
            skin = null;
            foreach (var pair in _externalCardSkinDict)
            {
                skin = pair.Value.Find(s => s.id == cardId);
                if (skin != null)
                    return true;
            }
            return false;
        }
        public CardSkinData[] getAllSkins(int cardId)
        {
            List<CardSkinData> skinList = new List<CardSkinData>();
            for (int i = 0; i < _skins.Length; i++)
            {
                if (_skins[i].id == cardId)
                {
                    skinList.Add(_skins[i]);
                }
            }
            foreach (var skins in _externalCardSkinDict.Values)
            {
                foreach (var skin in skins)
                {
                    if (skin.id == cardId)
                    {
                        skinList.Add(skin);
                    }
                }
            }
            return skinList.ToArray();
        }
        #endregion
        #region 私有方法
        #endregion
        #region 属性字段
        public Dictionary<string, object> externalSpriteCacheDict { get; } = new Dictionary<string, object>();
        Dictionary<int, List<CardDefine>> _externalCardDefineDict = new Dictionary<int, List<CardDefine>>();
        /// <summary>
        /// 用于存储外部皮肤信息的字典，Key为卡池Id，值为卡池包含皮肤列表
        /// </summary>
        Dictionary<int, List<CardSkinData>> _externalCardSkinDict = new Dictionary<int, List<CardSkinData>>();
        Dictionary<int, List<ActionNodeGraphInfo>> _externalCardActionGraphInfoDict = new Dictionary<int, List<ActionNodeGraphInfo>>();
        const string CARDPOOLS_DIR_NAME = "CardPools";
        #endregion
    }
}
