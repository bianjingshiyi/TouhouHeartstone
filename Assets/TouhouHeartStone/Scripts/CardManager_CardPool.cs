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
            _cardPoolInfoDict.Clear();
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
                foreach (var pair in _cardPoolInfoDict)
                {
                    if (pair.Value.cardPoolId == cardPool.cardPoolId)
                    {
                        conflictCardPoolPath = pair.Key;
                        break;
                    }
                }
                if (conflictCardPoolPath != null)
                {
                    _cardPoolInfoDict.Remove(conflictCardPoolPath);
                }
                _cardPoolInfoDict.Add(path, cardPool);
                onCardPoolInfoRead?.Invoke(path, cardPool);
                Debug.Log("读取卡池信息" + path + "完毕");
            }
            Debug.Log("卡池信息读取完毕，共" + fileList.Length + "个卡池");
            return new Dictionary<string, CardPoolInfo>(_cardPoolInfoDict);
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
                string[] files = game.fileManager.getFiles(Path.GetFullPath(_editorCardPoolDirName), searchPattern);
                fileList.AddRange(files);
                Debug.Log("获取编辑器卡池：\r\n" + string.Join(",\r\n", files));
            }
            if (!Application.isEditor || _enableStreamingAssetsInEditor)
            {
                //官方卡池，以sa:开头的StreamingAssets
                string[] files = game.fileManager.getStreamingFiles(_cardPoolDirName, searchPattern);
                fileList.AddRange(files);
                Debug.Log("获取官方卡池：\r\n" + string.Join(",\r\n", files));
            }
            return fileList.ToArray();
        }
        public void createCardPool(int cardPoolId, out CardPoolInfo cardPoolInfo, out CardPool cardPool)
        {
            cardPoolInfo = new CardPoolInfo();
            cardPoolInfo.cardPoolId = cardPoolId;
            cardPoolInfo.version = CARD_POOL_VERSION_0_1;
            cardPool = new CardPool();
            _cardPoolDict[cardPoolId] = cardPool;
        }
        public async Task saveCardPool(CardPoolInfo cardPoolInfo, CardDefine[] cardDefines, CardSkinData[] cardSkins, ActionNodeGraphInfo[] actionNodeGraphInfos)
        {
            if (!Application.isEditor && isStandardCardPool(cardPoolInfo))
            {
                //不能修改标准卡池，虽然其实玩家想改也没人能阻止就是了，最多是和别人玩会失去同步吧。
                game.ui.dialog.ShowConfirm(StringTable.INVALID_OPERATION, StringTable.EDITOR_DIALOG_CANNOT_MODIFY_STANDARD_CARDPOOL, null);
                return;
            }
            //如果当前的卡池没有被保存过，没有分配地址，则给它分配一个地址。
            string cardPoolPath = null;
            foreach (var pair in _cardPoolInfoDict)
            {
                if (pair.Value == cardPoolInfo)
                {
                    cardPoolPath = pair.Key;
                    break;
                }
            }
            if (cardPoolPath == null)
            {
                //没有指定地址
                cardPoolPath = allocCardPoolPath(cardPoolInfo);
                _cardPoolInfoDict.Add(cardPoolPath, cardPoolInfo);
            }
            string path;
            //对缓存的图片进行保存
            foreach (var pair in externalSpriteCacheDict)
            {
                //获取使用这张图片的第一个皮肤
                CardSkinData skin = cardSkins.FirstOrDefault(s => s.imagePath == pair.Key);
                if (skin == null)
                {
                    //没有皮肤使用，跳过
                    continue;
                }
                //另存为图片
                if (isStandardCardPool(cardPoolInfo))
                {
                    path = Path.Combine(cardPoolInfo.cardPoolName, skin.id.ToString(), skin.skinId.ToString(), "Image.jpg");
                    if (pair.Value is Sprite sprite)
                    {
                        await game.fileManager.saveFile(Path.Combine(editorTextureDirName, path), sprite.texture.EncodeToJPG());
                    }
                    else if (pair.Value is Task<Sprite> task)
                    {
                        sprite = await task;
                        await game.fileManager.saveFile(Path.Combine(editorTextureDirName, path), sprite.texture.EncodeToJPG());
                    }
                    else
                    {
                        Debug.LogWarning("图片" + path + "为空");
                        continue;
                    }
                }
                else
                {
                    path = Path.Combine(cardPoolInfo.cardPoolName, skin.id.ToString(), skin.skinId.ToString(), "Image.jpg");
                    if (pair.Value is Sprite sprite)
                    {
                        await game.fileManager.savePersistentFile(Path.Combine(textureDirName, path), sprite.texture.EncodeToJPG());
                    }
                    else if (pair.Value is Task<Sprite> task)
                    {
                        sprite = await task;
                        await game.fileManager.savePersistentFile(Path.Combine(textureDirName, path), sprite.texture.EncodeToJPG());
                    }
                    else
                    {
                        Debug.LogWarning("图片" + path + "为空");
                        continue;
                    }
                }
                Debug.Log("另存为图片" + path + "成功");
                //替换所有引用这个图片的皮肤的图片地址
                foreach (var otherSkin in cardSkins.Where(s => s.imagePath == pair.Key))
                {
                    otherSkin.imagePath = path;
                }
            }
            //保存文件
            path = cardPoolPath;
            if (isStandardCardPool(cardPoolInfo))
            {
                //官方卡池只能保存到指定路径下面，也就是工程文件
                path = Path.Combine(editorCardPoolDirName, cardPoolInfo.cardPoolName + '.' + cardPoolFilePostfix);
            }
            else if (path.StartsWith("sa:"))
            {
                //如果是StreamingAssets，那要转存成可持续化文件
                path = path.Substring(3, path.Length - 3);
            }
            //#if UNITY_EDITOR
            //            if (game.card.isStandardCardPool(_curCardPoolInfo) &&
            //                path.StartsWith(Path.Combine(Application.persistentDataPath, game.card.cardPoolDirName)))
            //            {
            //                path = path.Replace(Path.Combine(Application.persistentDataPath, game.card.cardPoolDirName), game.card.editorCardPoolDirName);
            //            }
            //#endif
            cardPoolInfo.version = CARD_POOL_VERSION_0_1;
            CardPool cardPool = new CardPool();
            cardPool.cardDefineList.AddRange(cardDefines);
            cardPool.cardSkinList.AddRange(cardSkins);
            cardPool.actionNodeGraphInfoList.AddRange(actionNodeGraphInfos);
            cardPool.buffDefineList.AddRange(getBuffDefines(cardPoolInfo.cardPoolId));
            cardPool.buffSkinList.AddRange(getBuffSkins(cardPoolInfo.cardPoolId));
            await game.fileManager.saveFile(path, new string[]
            {
                cardPoolInfo.toJson(),//文件头信息
                cardPool.toJson()//内容对象
            });
            Debug.Log("保存卡池" + path + "成功");
        }
        public async Task saveCardPool(int cardPoolId)
        {
            if (!Application.isEditor && isStandardCardPool(cardPoolId))
            {
                //不能修改标准卡池，虽然其实玩家想改也没人能阻止就是了，最多是和别人玩会失去同步吧。
                game.ui.dialog.ShowConfirm(StringTable.INVALID_OPERATION, StringTable.EDITOR_DIALOG_CANNOT_MODIFY_STANDARD_CARDPOOL, null);
                return;
            }
            CardPoolInfo cardPoolInfo = null;
            foreach (var pair in _cardPoolInfoDict)
            {
                if (pair.Value.cardPoolId == cardPoolId)
                {
                    cardPoolInfo = pair.Value;
                    break;
                }
            }
            if (cardPoolInfo == null)
            {
                Debug.LogError("不存在卡池" + cardPoolId);
                return;
            }
            CardPool cardPool = _cardPoolDict[cardPoolId];
            //如果当前的卡池没有被保存过，没有分配地址，则给它分配一个地址。
            string cardPoolPath = null;
            foreach (var pair in _cardPoolInfoDict)
            {
                if (pair.Value.cardPoolId == cardPoolId)
                {
                    cardPoolPath = pair.Key;
                    break;
                }
            }
            if (cardPoolPath == null)
            {
                //没有指定地址
                cardPoolPath = allocCardPoolPath(cardPoolInfo);
                _cardPoolInfoDict.Add(cardPoolPath, cardPoolInfo);
            }
            string path;
            //对缓存的图片进行保存
            foreach (var pair in externalSpriteCacheDict)
            {
                //获取使用这张图片的第一个皮肤
                CardSkinData skin = cardPool.cardSkinList.FirstOrDefault(s => s.imagePath == pair.Key);
                if (skin == null)
                {
                    //没有皮肤使用，跳过
                    continue;
                }
                //另存为图片
                if (isStandardCardPool(cardPoolInfo))
                {
                    path = Path.Combine(cardPoolInfo.cardPoolName, skin.id.ToString(), skin.skinId.ToString(), "Image.jpg");
                    if (pair.Value is Sprite sprite)
                    {
                        await game.fileManager.saveFile(Path.Combine(editorTextureDirName, path), sprite.texture.EncodeToJPG());
                    }
                    else if (pair.Value is Task<Sprite> task)
                    {
                        sprite = await task;
                        await game.fileManager.saveFile(Path.Combine(editorTextureDirName, path), sprite.texture.EncodeToJPG());
                    }
                    else
                    {
                        Debug.LogWarning("图片" + path + "为空");
                        continue;
                    }
                }
                else
                {
                    path = Path.Combine(cardPoolInfo.cardPoolName, skin.id.ToString(), skin.skinId.ToString(), "Image.jpg");
                    if (pair.Value is Sprite sprite)
                    {
                        await game.fileManager.savePersistentFile(Path.Combine(textureDirName, path), sprite.texture.EncodeToJPG());
                    }
                    else if (pair.Value is Task<Sprite> task)
                    {
                        sprite = await task;
                        await game.fileManager.savePersistentFile(Path.Combine(textureDirName, path), sprite.texture.EncodeToJPG());
                    }
                    else
                    {
                        Debug.LogWarning("图片" + path + "为空");
                        continue;
                    }
                }
                Debug.Log("另存为图片" + path + "成功");
                //替换所有引用这个图片的皮肤的图片地址
                foreach (var otherSkin in cardPool.cardSkinList.Where(s => s.imagePath == pair.Key))
                {
                    otherSkin.imagePath = path;
                }
            }
            //保存文件
            path = cardPoolPath;
            if (isStandardCardPool(cardPoolInfo))
            {
                //官方卡池只能保存到指定路径下面，也就是工程文件
                path = Path.Combine(editorCardPoolDirName, cardPoolInfo.cardPoolName + '.' + cardPoolFilePostfix);
            }
            else if (path.StartsWith("sa:"))
            {
                //如果是StreamingAssets，那要转存成可持续化文件
                path = path.Substring(3, path.Length - 3);
            }
            cardPoolInfo.version = CARD_POOL_VERSION_0_1;
            await game.fileManager.saveFile(path, new string[]
            {
                cardPoolInfo.toJson(),//文件头信息
                cardPool.toJson()//内容对象
            });
            Debug.Log("保存卡池" + path + "成功");
        }
        /// <summary>
        /// 从目标路径上加载卡池
        /// </summary>
        /// <param name="path">流文件路径或者绝对路径</param>
        /// <returns></returns>
        public async Task<(CardPoolInfo cardPoolInfo, CardPool cardPool)> loadCardPool(string path)
        {
            string[] lines;
            if (path.StartsWith("sa:"))
            {
                lines = await game.fileManager.readStreamingTextFile(path);
            }
            else
            {
                lines = await game.fileManager.readTextFile(path);
            }
            if (lines == null || lines.Length < 1)
            {
                throw new FileLoadException("卡池文件" + path + "为空");
            }
            string json = lines[0];
            CardPoolInfo cardPoolInfo = JsonHelper.fromJson<CardPoolInfo>(json);
            CardPool cardPool;
            if (cardPoolInfo.version >= CARD_POOL_VERSION_0_1)
            {
                if (path.StartsWith("sa:"))
                {
                    lines = await game.fileManager.readStreamingTextFile(path, 1, 1);
                }
                else
                {
                    lines = await game.fileManager.readTextFile(path, 1, 1);
                }
                json = lines[0];
                cardPool = JsonHelper.fromJson<CardPool>(json);
                _cardPoolInfoDict[path] = cardPoolInfo;
                _cardPoolDict[cardPoolInfo.cardPoolId] = cardPool;
            }
            else
            {
                cardPool = new CardPool();
                if (path.StartsWith("sa:"))
                {
                    lines = await game.fileManager.readStreamingTextFile(path, 1, 5);
                }
                else
                {
                    lines = await game.fileManager.readTextFile(path, 1, 5);
                }
                //卡牌
                if (lines.Length > 0)
                {
                    json = lines[0];
                    if (!string.IsNullOrEmpty(json))
                    {
                        cardPool.cardDefineList.AddRange(JsonHelper.fromJson<CardDefine[]>(json));
                    }
                }
                //卡牌皮肤
                if (lines.Length > 1)
                {
                    json = lines[1];
                    if (!string.IsNullOrEmpty(json))
                    {
                        cardPool.cardSkinList.AddRange(JsonHelper.fromJson<CardSkinData[]>(json));
                    }
                }
                //编辑器信息
                if (lines.Length > 2)
                {
                    json = lines[2];
                    if (!string.IsNullOrEmpty(json))
                    {
                        cardPool.actionNodeGraphInfoList.AddRange(JsonHelper.fromJson<ActionNodeGraphInfo[]>(json));
                    }
                }
                //增益
                if (lines.Length > 3)
                {
                    json = lines[3];
                    if (!string.IsNullOrEmpty(json))
                    {
                        cardPool.buffDefineList.AddRange(JsonHelper.fromJson<GeneratedBuffDefine[]>(json));
                    }
                }
                //增益皮肤
                if (lines.Length > 4)
                {
                    json = lines[4];
                    if (!string.IsNullOrEmpty(json))
                    {
                        cardPool.buffSkinList.AddRange(JsonHelper.fromJson<BuffSkin[]>(json));
                    }
                }
                _cardPoolInfoDict[path] = cardPoolInfo;
                _cardPoolDict[cardPoolInfo.cardPoolId] = cardPool;
            }
            return (cardPoolInfo, cardPool);
        }
        public bool tryGetCardPool(int cardPoolId, out CardPool cardPool)
        {
            return _cardPoolDict.TryGetValue(cardPoolId, out cardPool);
        }
        public bool tryGetCardPool(string path, out CardPool cardPool)
        {
            if (_cardPoolInfoDict.TryGetValue(path, out CardPoolInfo cardPoolInfo))
            {
                return tryGetCardPool(cardPoolInfo.cardPoolId, out cardPool);
            }
            else
            {
                cardPool = null;
                return false;
            }
        }
        public bool removeCardPool(int cardPoolId)
        {
            if (_cardPoolDict.ContainsKey(cardPoolId))
            {
                _cardPoolDict.Remove(cardPoolId);
            }
            string path = null;
            foreach (var pair in _cardPoolInfoDict)
            {
                if (pair.Value.cardPoolId == cardPoolId)
                {
                    path = pair.Key;
                    break;
                }
            }
            if (path != null)
            {
                game.fileManager.deleteFile(path);
                return true;
            }
            else
                return false;
        }
        public async Task<(CardDefine[] defines, CardSkinData[] skins)> getExternalCardDefinesAsync(string[] paths)
        {
            List<CardDefine> defineList = new List<CardDefine>();
            List<CardSkinData> skinList = new List<CardSkinData>();
            for (int i = 0; i < paths.Length; i++)
            {
                if (!tryGetCardPool(paths[i], out CardPool cardPool))
                {
                    cardPool = (await loadCardPool(paths[i])).cardPool;
                }
                defineList.AddRange(cardPool.cardDefineList);
                skinList.AddRange(cardPool.cardSkinList);
            }
            return (defineList.ToArray(), skinList.ToArray());
        }
        //public async Task<(CardDefine[] defines, CardSkinData[] skins, ActionNodeGraphInfo[] actionGraphInfos)> getOrLoadCardPoolContents(string path)
        //{
        //    string[] lines;
        //    if (path.StartsWith("sa:"))
        //    {
        //        //读取StreamingAssets
        //        lines = await game.fileManager.readStreamingTextFile(path, 0, 5);
        //    }
        //    else if (Path.GetFullPath(path) == path)
        //    {
        //        //如果是绝对路径就从绝对路径上读取，这是因为在Editor模式下无法区分可持续化文件与工程文件所以使用绝对路径
        //        lines = await game.fileManager.readTextFile(path, 0, 5);
        //    }
        //    else
        //    {
        //        //相对路径，只能从可持续化文件中读取
        //        lines = await game.fileManager.readPersistentTextFile(path, 0, 5);
        //    }
        //    if (lines != null && lines.Length > 0)
        //    {
        //        //卡池信息
        //        string json = lines[0];
        //        CardPoolInfo cardPoolInfo = JsonHelper.fromJson<CardPoolInfo>(json);
        //        path = Path.GetFullPath(path);
        //        _cardPoolInfoDict[path] = cardPoolInfo;
        //        if (_externalCardDefineDict.ContainsKey(cardPoolInfo.cardPoolId))
        //        {
        //            //如果已经加载就不重复加载
        //            CardDefine[] defines = _externalCardDefineDict[cardPoolInfo.cardPoolId].ToArray();
        //            CardSkinData[] skins = _externalCardSkinDict[cardPoolInfo.cardPoolId].ToArray();
        //            ActionNodeGraphInfo[] actionGraphInfos = _externalCardActionGraphInfoDict[cardPoolInfo.cardPoolId].ToArray();
        //            return (defines, skins, actionGraphInfos);
        //        }
        //        else
        //        {
        //            if (cardPoolInfo.version >= CARD_POOL_VERSION_0_1)
        //            {
        //                //内容对象
        //                json = lines[1];
        //                CardPool cardPool = JsonHelper.fromJson<CardPool>(json);
        //                _externalCardDefineDict.Add(cardPoolInfo.cardPoolId, new List<CardDefine>(cardPool.cardDefines));
        //                _externalCardSkinDict.Add(cardPoolInfo.cardPoolId, new List<CardSkinData>(cardPool.cardSkins));
        //                _externalCardActionGraphInfoDict.Add(cardPoolInfo.cardPoolId, new List<ActionNodeGraphInfo>(cardPool.actionNodeGraphInfos));
        //                _externalBuffDefineDict.Add(cardPoolInfo.cardPoolId, new List<GeneratedBuffDefine>(cardPool.buffDefines));
        //                _externalBuffSkinDict.Add(cardPoolInfo.cardPoolId, new List<BuffSkin>(cardPool.buffSkins));
        //                return (cardPool.cardDefines, cardPool.cardSkins, cardPool.actionNodeGraphInfos);
        //            }
        //            else
        //            {
        //                //卡牌
        //                json = lines[1];
        //                CardDefine[] defines = JsonHelper.fromJson<CardDefine[]>(json);
        //                _externalCardDefineDict.Add(cardPoolInfo.cardPoolId, new List<CardDefine>(defines));
        //                //皮肤
        //                json = lines[2];
        //                CardSkinData[] skins = JsonHelper.fromJson<CardSkinData[]>(json);
        //                _externalCardSkinDict.Add(cardPoolInfo.cardPoolId, new List<CardSkinData>(skins));
        //                //编辑器信息
        //                json = lines[3];
        //                ActionNodeGraphInfo[] actionGraphInfos = JsonHelper.fromJson<ActionNodeGraphInfo[]>(json);
        //                _externalCardActionGraphInfoDict.Add(cardPoolInfo.cardPoolId, new List<ActionNodeGraphInfo>(actionGraphInfos));
        //                //增益
        //                GeneratedBuffDefine[] buffDefines = null;
        //                if (lines.Length > 4)
        //                {
        //                    json = lines[4];
        //                    if (!string.IsNullOrEmpty(json))
        //                    {
        //                        try
        //                        {
        //                            buffDefines = JsonHelper.fromJson<GeneratedBuffDefine[]>(json);
        //                        }
        //                        catch (Exception e)
        //                        {
        //                            Debug.LogError("加载" + path + "中的增益发生异常：" + e);
        //                        }
        //                    }
        //                }
        //                if (!_externalBuffDefineDict.TryGetValue(cardPoolInfo.cardPoolId, out var buffList) || buffList == null)
        //                {
        //                    buffList = new List<GeneratedBuffDefine>();
        //                    _externalBuffDefineDict[cardPoolInfo.cardPoolId] = buffList;
        //                }
        //                if (buffDefines != null)
        //                {
        //                    buffList.AddRange(buffDefines);
        //                }
        //                return (defines, skins, actionGraphInfos);
        //            }
        //        }
        //    }
        //    else
        //        throw new IOException("无法读取文件" + path);
        //}
        public async Task<CardDefine[]> reloadCardPoolsAndGetCardDefinesAsync(IEnumerable<int> cardPoolsId)
        {
            List<CardDefine> cardDefineList = new List<CardDefine>();
            var cardPoolInfoDict = await getCardPoolInfosAsync();
            foreach (var cardPoolId in cardPoolsId)
            {
                foreach (var pair in cardPoolInfoDict)
                {
                    if (pair.Value.cardPoolId == cardPoolId)
                    {
                        var result = await loadCardPool(pair.Key);
                        cardDefineList.AddRange(result.cardPool.cardDefineList);
                    }
                }
            }
            return cardDefineList.ToArray();
        }
        public void createCard(int cardPoolId, CardDefine cardDefine, CardSkinData cardSkin)
        {
            _cardPoolDict[cardPoolId].cardDefineList.Add(cardDefine);
            _cardPoolDict[cardPoolId].cardSkinList.Add(cardSkin);
        }
        public void removeCard(CardDefine card)
        {
            foreach (var cardPool in _cardPoolDict.Values)
            {
                if (cardPool.cardDefineList.Remove(card))
                {
                    cardPool.cardSkinList.RemoveAll(s => s.id == card.id);
                }
            }
        }
        public bool tryGetCardDefine(int cardId, out CardDefine card)
        {
            card = null;
            foreach (var cardPool in _cardPoolDict.Values)
            {
                card = cardPool.cardDefineList.Find(c => c.id == cardId);
                if (card != null)
                    return true;
            }
            return false;
        }
        public CardPoolInfo getCardPoolOfCard(CardDefine card)
        {
            foreach (var cardPool in _cardPoolDict.Values)
            {
                if (cardPool.cardDefineList.Contains(card))
                {
                    return getCardPoolInfo(getCardPoolId(cardPool));
                }
            }
            return null;
        }
        public string getCardPoolPath(CardPoolInfo cardPool)
        {
            foreach (var pair in _cardPoolInfoDict)
            {
                if (pair.Value.cardPoolId == cardPool.cardPoolId)
                    return pair.Key;
            }
            return null;
        }
        public CardPoolInfo getCardPoolInfo(int cardPoolId)
        {
            foreach (var cardPool in _cardPoolInfoDict.Values)
            {
                if (cardPool.cardPoolId == cardPoolId)
                    return cardPool;
            }
            return null;
        }
        public int getCardPoolId(CardPool cardPool)
        {
            foreach (var pair in _cardPoolDict)
            {
                if (pair.Value == cardPool)
                {
                    return pair.Key;
                }
            }
            return 0;
        }
        public async Task<Sprite> loadImage(string path, int cardPoolId)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            Sprite sprite;
            if (path.StartsWith("sa:"))
            {
                //流文件
                sprite = await getSpriteFromCache(path);
                if (sprite != null)
                {
                    //存在缓存，直接返回缓存
                    return sprite;
                }
                else
                {
                    //读取文件
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
                        return sprite;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("加载图片" + path + "失败：" + e);
                    }
                }
            }
            else if (path.StartsWith("res:"))
            {
                //资源文件
                sprite = await getSpriteFromCache(path);
                if (sprite != null)
                {
                    //存在缓存，直接返回缓存
                    return sprite;
                }
                else
                {
                    //读取文件
                    sprite = Resources.Load<Sprite>(path.Substring(4, path.Length - 4));
                    if (sprite == null)
                    {
                        Debug.LogError("加载图片" + path + "为空");
                    }
                }
            }
            else
            {
                //外部文件
                if (cardPoolId > 0)
                {
                    //存在卡池，要考虑一下记载这个路径的卡池的路径，综合得到绝对路径
                    //因为无法区分可持续化文件和工程文件的相对路径，所以需要通过绝对路径来读取文件
                    //外部图片路径无法记载绝对路径，因为不确定安装平台和目录，所以一定是相对路径
                    CardPoolInfo cardPool = getCardPoolInfo(cardPoolId);
                    if (cardPool != null)
                    {
                        path = Path.Combine(getCardPoolPathRootDir(getCardPoolPath(cardPool)), textureDirName, path);
                    }
                }
                byte[] bytes = await game.fileManager.readBinaryFile(path);
                Texture2D texture = new Texture2D(512, 512);
                texture.LoadImage(bytes);
                sprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.one * .5f);
            }
            if (sprite != null)
            {
                addSpriteToCache(path, sprite);
            }
            return sprite;
        }
        public async Task<Sprite> loadExternalImage(string path, string dirName)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            Sprite sprite;
            if (path.StartsWith("res:"))
            {
                sprite = await getSpriteFromCache(path);
                if (sprite != null)
                {
                    return sprite;
                }
                //Unity资源
                sprite = Resources.Load<Sprite>(path.Substring(4, path.Length - 4));
                if (sprite == null)
                {
                    Debug.LogError("加载图片" + path + "为空");
                }
            }
            else if (path.StartsWith("sa:"))
            {
                sprite = await getSpriteFromCache(path);
                if (sprite != null)
                {
                    return sprite;
                }
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
                if (!string.IsNullOrEmpty(dirName))
                {
                    path = Path.Combine(dirName, path);
                }
                sprite = await getSpriteFromCache(path);
                if (sprite != null)
                {
                    return sprite;
                }
                //外部贴图
                byte[] bytes = Path.GetFullPath(path) == path ?//如果直接给出完整路径就直接从完整路径上读
                    await game.fileManager.readBinaryFile(path) :
                    await game.fileManager.readPersistentBinaryFile(path);
                Texture2D texture = new Texture2D(512, 512);
                texture.LoadImage(bytes);
                sprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.one * .5f);
            }
            //添加到缓存
            if (sprite != null)
            {
                addSpriteToCache(path, sprite);
            }
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
                string path = skin.imagePath;
                if (string.IsNullOrEmpty(path))
                {
                    return Task.FromResult<Sprite>(null);
                }
                else if (getCardPoolOfCard(GetCardDefine(cardId)) is CardPoolInfo cardPool &&
                    game.card.getCardPoolPath(cardPool) is string cardPoolPath)
                {
                    if (cardPoolPath.StartsWith("sa:") &&
                        !path.StartsWith("sa:") &&
                        !path.StartsWith("res:"))
                    {
                        //流文件
                        return loadExternalImage("sa:" + Path.Combine(textureDirName, path), null);
                    }
                    else
                    {
                        //绝对路径
                        return loadExternalImage(path, Path.Combine(getCardPoolPathRootDir(cardPoolPath), textureDirName));
                    }
                }
                else
                {
                    //没有卡池信息
                    return loadExternalImage(path, game.card.textureDirName);
                }
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

        public bool tryGetCardSkin(int cardId, out CardSkinData skin)
        {
            skin = null;
            foreach (var cardPool in _cardPoolDict.Values)
            {
                skin = cardPool.cardSkinList.Find(s => s.id == cardId);
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
            foreach (var cardPool in _cardPoolDict.Values)
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
        private string allocCardPoolPath(CardPoolInfo cardPool)
        {
            return Path.Combine(Application.persistentDataPath, cardPoolDirName, cardPool.cardPoolName + "." + cardPoolFilePostfix);
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
        Dictionary<string, CardPoolInfo> _cardPoolInfoDict = new Dictionary<string, CardPoolInfo>();
        /// <summary>
        /// 键为卡池Id，值为卡池
        /// </summary>
        Dictionary<int, CardPool> _cardPoolDict = new Dictionary<int, CardPool>();
        //Dictionary<int, List<CardDefine>> _externalCardDefineDict = new Dictionary<int, List<CardDefine>>();
        /// <summary>
        /// 用于存储外部皮肤信息的字典，Key为卡池Id，值为卡池包含皮肤列表
        /// </summary>
        //Dictionary<int, List<CardSkinData>> _externalCardSkinDict = new Dictionary<int, List<CardSkinData>>();
        //Dictionary<int, List<ActionNodeGraphInfo>> _externalCardActionGraphInfoDict = new Dictionary<int, List<ActionNodeGraphInfo>>();
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
        const float CARD_POOL_VERSION_0_1 = 0.1f;
        #endregion
    }
}
