using System;
using System.Threading.Tasks;
using UI;
using Controllers;
using TouhouCardEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using MongoDB.Bson.Serialization;
using TouhouHeartstone;

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
            Debug.Log("开始读取卡池信息");
            _cardPoolDict.Clear();
            string[] fileList = getCardPoolsPath();
            for (int i = 0; i < fileList.Length; i++)
            {
                string path = fileList[i];
                Debug.Log("开始读取卡池信息" + path);
                string[] lines;
                if (path.StartsWith("sa:"))
                {
                    lines = await game.fileManager.readStreamingTextFile(path.Substring(3, path.Length - 3), lineCount: 1);
                }
                else
                {
                    lines = await game.fileManager.readTextFile(path, lineCount: 1);
                }
                if (lines == null || lines.Length < 1)
                {
                    Debug.LogWarning("读取卡池信息" + path + "为空");
                    continue;
                }
                CardPoolInfo cardPool = JsonHelper.fromJson<CardPoolInfo>(lines[0]);
                //去重，多份文件中可能包含相同的卡池
                string conflictCardPoolPath = null;
                foreach (var pair in _cardPoolDict)
                {
                    if (pair.Value.cardPoolId == cardPool.cardPoolId)
                    {
                        conflictCardPoolPath = pair.Key;
                        break;
                    }
                }
                if (conflictCardPoolPath != null)
                {
                    _cardPoolDict.Remove(conflictCardPoolPath);
                }
                _cardPoolDict.Add(path, cardPool);
                onCardPoolInfoRead?.Invoke(path, cardPool);
                Debug.Log("读取卡池信息" + path + "完毕");
            }
            return new Dictionary<string, CardPoolInfo>(_cardPoolDict);
        }
        /// <summary>
        /// 获取所有卡池的路径
        /// </summary>
        /// <returns></returns>
        public string[] getCardPoolsPath()
        {
            //自定义卡池，C盘的完整地址或者是Android的Data地址
            string searchPattern = string.Intern("*." + _cardPoolFilePostfix);
            List<string> fileList = new List<string>(game.fileManager.getPersistentFiles(_cardPoolDirName, searchPattern, true));
            if (Application.isEditor)
            {
                //编辑器下的官方卡池，位于项目文件夹下的ExternalData中
                fileList.AddRange(game.fileManager.getFiles(Path.GetFullPath(_editorCardPoolDirName), searchPattern));
            }
            if (!Application.isEditor || _enableStreamingAssetsInEditor)
            {
                //官方卡池，以sa:开头的StreamingAssets
                fileList.AddRange(game.fileManager.getStreamingFiles(_cardPoolDirName, searchPattern));
            }
            return fileList.ToArray();
        }
        public void addExternalCardPool(int cardPoolId)
        {
            _externalCardDefineDict.Add(cardPoolId, new List<CardDefine>());
            _externalCardSkinDict.Add(cardPoolId, new List<CardSkinData>());
            _externalCardActionGraphInfoDict.Add(cardPoolId, new List<ActionNodeGraphInfo>());
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
        public async Task<(CardDefine[] defines, CardSkinData[] skins, ActionNodeGraphInfo[] actionGraphInfos)> getExternalCardDefinesAsync(string path)
        {
            string[] lines;
            if (path.StartsWith("sa:"))
            {
                //读取StreamingAssets
                lines = await game.fileManager.readStreamingTextFile(path, 0, 5);
            }
            else if (Path.GetFullPath(path) == path)
            {
                //如果是绝对路径就从绝对路径上读取，这是因为在Editor模式下无法区分可持续化文件与工程文件所以使用绝对路径
                lines = await game.fileManager.readTextFile(path, 0, 5);
            }
            else
            {
                //相对路径，只能从可持续化文件中读取
                lines = await game.fileManager.readPersistentTextFile(path, 0, 5);
            }
            if (lines != null && lines.Length > 0)
            {
                //卡池
                string json = lines[0];
                CardPoolInfo cardPool = JsonHelper.fromJson<CardPoolInfo>(json);
                path = Path.GetFullPath(path);
                if (_cardPoolDict.ContainsKey(path))
                {
                    _cardPoolDict[path] = cardPool;
                }
                else
                {
                    _cardPoolDict.Add(path, cardPool);
                }
                if (_externalCardDefineDict.ContainsKey(cardPool.cardPoolId))
                {
                    CardDefine[] defines = _externalCardDefineDict[cardPool.cardPoolId].ToArray();
                    CardSkinData[] skins = _externalCardSkinDict[cardPool.cardPoolId].ToArray();
                    ActionNodeGraphInfo[] actionGraphInfos = _externalCardActionGraphInfoDict[cardPool.cardPoolId].ToArray();
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
                    json = lines[3];
                    actionGraphInfos = JsonHelper.fromJson<ActionNodeGraphInfo[]>(json);
                    _externalCardActionGraphInfoDict.Add(cardPool.cardPoolId, new List<ActionNodeGraphInfo>(actionGraphInfos));
                    //增益
                    GeneratedBuffDefine[] buffDefines = null;
                    if (lines.Length > 4)
                    {
                        json = lines[4];
                        if (!string.IsNullOrEmpty(json))
                        {
                            try
                            {
                                buffDefines = JsonHelper.fromJson<GeneratedBuffDefine[]>(json);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("加载" + path + "中的增益发生异常：" + e);
                            }
                        }
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
        public CardPoolInfo getCardPoolOfCard(CardDefine card)
        {
            foreach (var pair in _externalCardDefineDict)
            {
                if (pair.Value.Contains(card))
                {
                    foreach (var cardPool in _cardPoolDict.Values)
                    {
                        if (cardPool.cardPoolId == pair.Key)
                            return cardPool;
                    }
                }
            }
            return null;
        }
        public string getCardPoolPath(CardPoolInfo cardPool)
        {
            foreach (var pair in _cardPoolDict)
            {
                if (pair.Value.cardPoolId == cardPool.cardPoolId)
                    return pair.Key;
            }
            return null;
        }
        public CardPoolInfo getCardPool(int cardPoolId)
        {
            foreach (var cardPool in _cardPoolDict.Values)
            {
                if (cardPool.cardPoolId == cardPoolId)
                    return cardPool;
            }
            return null;
        }
        public async Task<Sprite> loadExternalImage(string path, string dirName)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            if (!string.IsNullOrEmpty(dirName))
            {
                path = Path.Combine(dirName, path);
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
                CardPoolInfo cardPool = getCardPoolOfCard(GetCardDefine(cardId));
                if (skin.imagePath.StartsWith("sa:") || skin.imagePath.StartsWith("res:"))
                    return loadExternalImage(skin.imagePath, null);
                else
                    return loadExternalImage(skin.imagePath,
                        Path.Combine(getCardPoolPathRootDir(getCardPoolPath(cardPool)), textureDirName));
            }
        }

        public string getCardPoolPathRootDir(string path)
        {
            DirectoryInfo dirInfo = Directory.GetParent(path);
            if (dirInfo.Name != cardPoolDirName)
            {
                dirInfo = dirInfo.Parent;
            }
            dirInfo = dirInfo.Parent;
            return dirInfo.FullName;
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
            return isStandardCardPool(cardPool.cardPoolId);
        }
        public bool isStandardCardPool(int cardPoolId)
        {
            return _standardCardPoolsId.Contains(cardPoolId);
        }
        #endregion
        #region 私有方法
        private void onAwake_CardPool()
        {
            //注册序列化类型
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if ((type.IsSubclassOf(typeof(GeneratedEffect)) ||
                        type.IsSubclassOf(typeof(PropModifier))) &&
                        !BsonClassMap.IsClassMapRegistered(type))
                    {
                        BsonClassMap classMap = new BsonClassMap(type);
                        classMap.AutoMap();
                        BsonClassMap.RegisterClassMap(classMap);
                    }
                }
            }
        }
        #endregion
        #region 属性字段
        public Dictionary<string, object> externalSpriteCacheDict { get; } = new Dictionary<string, object>();
        public string cardPoolDirName => _cardPoolDirName;
        public string textureDirName => _textureDirName;
        public string editorCardPoolDirName => _editorCardPoolDirName;
        public string editorTextureDirName => _editorTextureDirName;
        public string cardPoolFilePostfix => _cardPoolFilePostfix;
        /// <summary>
        /// 键为文件地址，值为卡池信息
        /// </summary>
        Dictionary<string, CardPoolInfo> _cardPoolDict = new Dictionary<string, CardPoolInfo>();
        Dictionary<int, List<CardDefine>> _externalCardDefineDict = new Dictionary<int, List<CardDefine>>();
        /// <summary>
        /// 用于存储外部皮肤信息的字典，Key为卡池Id，值为卡池包含皮肤列表
        /// </summary>
        Dictionary<int, List<CardSkinData>> _externalCardSkinDict = new Dictionary<int, List<CardSkinData>>();
        Dictionary<int, List<ActionNodeGraphInfo>> _externalCardActionGraphInfoDict = new Dictionary<int, List<ActionNodeGraphInfo>>();
        [Header("卡池")]
        [SerializeField]
        bool _enableStreamingAssetsInEditor = false;
        [SerializeField]
        string _cardPoolFilePostfix = "ddcp";
        [SerializeField]
        string _cardPoolDirName = "CardPools";
        [SerializeField]
        string _textureDirName = "Textures";
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
