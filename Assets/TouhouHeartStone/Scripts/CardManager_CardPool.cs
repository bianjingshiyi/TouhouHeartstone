using System;
using System.Threading.Tasks;
using UI;
using Controllers;
using TouhouCardEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法
        /// <summary>
        /// 读取卡池信息
        /// </summary>
        /// <param name="onCardPoolInfoRead"></param>
        /// <returns>Key为路径，Value为信息</returns>
        public async Task<Dictionary<string, CardPoolInfo>> getCardPoolInfosAsync(Action<string, CardPoolInfo> onCardPoolInfoRead = null)
        {
            Dictionary<string, CardPoolInfo> dict = new Dictionary<string, CardPoolInfo>();
            string[] fileList = getCardPoolsPath();
            for (int i = 0; i < fileList.Length; i++)
            {
                string path = fileList[i];
                string[] lines = await game.fileManager.readTextFile(path, lineCount: 1);
                if (lines == null || lines.Length < 1)
                    continue;
                CardPoolInfo cardPool = JsonHelper.fromJson<CardPoolInfo>(lines[0]);
                //去重，多份文件中可能包含相同的卡池
                string conflictCardPoolPath = null;
                foreach (var pair in dict)
                {
                    if (pair.Value.cardPoolId == cardPool.cardPoolId)
                    {
                        conflictCardPoolPath = pair.Key;
                        break;
                    }
                }
                if (conflictCardPoolPath != null)
                {
                    dict.Remove(conflictCardPoolPath);
                }
                dict.Add(fileList[i], cardPool);
                onCardPoolInfoRead?.Invoke(fileList[i], cardPool);
            }
            return dict;
        }
        /// <summary>
        /// 获取所有卡池的路径
        /// </summary>
        /// <returns></returns>
        public string[] getCardPoolsPath()
        {
            List<string> fileList = new List<string>(game.fileManager.getPersistentFiles(_cardPoolDirName, "*." + _cardPoolFilePostfix, true));
#if UNITY_EDITOR
            fileList.AddRange(game.fileManager.getFiles(Path.GetFullPath(_editorCardPoolDirName), "*." + _cardPoolFilePostfix));
#endif
            return fileList.ToArray();
        }
        public void addExternalCardPool(int cardPoolId)
        {
            _externalCardDefineDict.Add(cardPoolId, new List<CardDefine>());
            _externalCardSkinDict.Add(cardPoolId, new List<CardSkinData>());
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
            var lines = Path.GetFullPath(path) == path ?
                await game.fileManager.readTextFile(path, 0, 5) :
                await game.fileManager.readPersistentTextFile(path, 0, 5);
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
                byte[] bytes = Path.GetFullPath(path) == path ?//如果直接给出完整路径就直接从完整路径上读
                    await game.fileManager.readBinaryFile(path) :
                    await game.fileManager.readPersistentBinaryFile(path);
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
        public bool isStandardCardPool(CardPoolInfo cardPool)
        {
            return _standardCardPoolsId.Contains(cardPool.cardPoolId);
        }
        #endregion
        #region 私有方法
        #endregion
        #region 属性字段
        public Dictionary<string, object> externalSpriteCacheDict { get; } = new Dictionary<string, object>();
        public string cardPoolDirName => _cardPoolDirName;
        public string editorCardPoolDirName => _editorCardPoolDirName;
        public string editorTextureDirName => _editorTextureDirName;
        Dictionary<int, List<CardDefine>> _externalCardDefineDict = new Dictionary<int, List<CardDefine>>();
        /// <summary>
        /// 用于存储外部皮肤信息的字典，Key为卡池Id，值为卡池包含皮肤列表
        /// </summary>
        Dictionary<int, List<CardSkinData>> _externalCardSkinDict = new Dictionary<int, List<CardSkinData>>();
        Dictionary<int, List<ActionNodeGraphInfo>> _externalCardActionGraphInfoDict = new Dictionary<int, List<ActionNodeGraphInfo>>();
        [Header("卡池")]
        [SerializeField]
        string _cardPoolFilePostfix = "ddcp";
        [SerializeField]
        string _cardPoolDirName = "CardPools";
        [SerializeField]
        string _editorCardPoolDirName = "ExternalData/CardPools";
        [SerializeField]
        string _editorTextureDirName = "ExternalData/Textures";
        [SerializeField]
        int[] _standardCardPoolsId = new int[]
{
            1
};
        #endregion
    }
}
