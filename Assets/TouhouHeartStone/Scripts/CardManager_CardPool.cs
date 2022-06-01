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
using System.Reflection;

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
        public async Task<Dictionary<string, CardPoolInfo>> loadCardPoolInfosAsync(Action<string, CardPoolInfo> onCardPoolInfoRead = null)
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
        public void createCardPool(out CardPoolInfo cardPoolInfo, out CardPool cardPool)
        {
            int cardPoolId = 0;
            for (int i = 1; i < 0x7FFF; i++)
            {
                if (game.card.isStandardCardPool(i))
                {
                    //不能是官方卡池ID
                    continue;
                }
                foreach (var info in _cardPoolInfoDict.Values)
                {
                    if (info.cardPoolId == i)
                    {
                        //不能是已有的卡池ID
                        continue;
                    }
                }
                cardPoolId = i;
                break;
            }
            cardPoolInfo = new CardPoolInfo();
            cardPoolInfo.cardPoolId = cardPoolId;
            cardPoolInfo.cardPoolName = StringTable.EDITOR_NEW_CARDPOOL_DEFAULT_NAME;
            cardPoolInfo.imageResPath = null;
            cardPoolInfo.version = CARD_POOL_VERSION_0_2;
            cardPool = new CardPool();
            _cardPoolDict[cardPoolId] = cardPool;
        }
        /// <summary>
        /// 保存卡池，如果卡池没有被保存过那么会被分配一个新的地址，否则会覆盖已经保存的文件。
        /// </summary>
        /// <param name="cardPoolInfo"></param>
        /// <returns></returns>
        public async Task saveCardPool(CardPoolInfo cardPoolInfo)
        {
            if (!Application.isEditor && isStandardCardPool(cardPoolInfo.cardPoolId))
            {
                //不能修改标准卡池，虽然其实玩家想改也没人能阻止就是了，最多是和别人玩会失去同步吧。
                game.ui.dialog.ShowConfirm(StringTable.INVALID_OPERATION, StringTable.EDITOR_DIALOG_CANNOT_MODIFY_STANDARD_CARDPOOL, null);
                return;
            }
            //如果当前的卡池没有被保存过，没有分配地址，则给它分配一个地址。
            string cardPoolPath = null;
            foreach (var pair in _cardPoolInfoDict)
            {
                if (pair.Value.cardPoolId == cardPoolInfo.cardPoolId)
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
            SerializableCardPool cardPool = new SerializableCardPool(_cardPoolDict[cardPoolInfo.cardPoolId]);
            //对缓存的动作定义进行保存
            //if (_runtimeGenActDefDict.TryGetValue(cardPoolInfo.cardPoolId, out var actionDefineList))
            //{
            //    cardPool.actionDefineList = actionDefineList.ConvertAll(g => new SerializableActionDefine(g));
            //}
            //对缓存的图片进行保存
            await saveExternalImages(cardPoolInfo.cardPoolId);
            //保存文件
            string path = cardPoolPath;
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
            cardPoolInfo.version = CARD_POOL_VERSION_0_3;
            await game.fileManager.saveFile(path, new string[]
            {
                cardPoolInfo.toJson(),//文件头信息
                cardPool.toJson(true)//内容对象
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
            //初始化JSON
            if (!JsonHelper.isInited)
            {
                JsonHelper.init();
            }
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
            if (cardPoolInfo.version >= CARD_POOL_VERSION_0_3)
            {
                if (path.StartsWith("sa:"))
                    json = await game.fileManager.readStreamingTextFileToEnd(path, 1);
                else
                    json = await game.fileManager.readTextFileToEnd(path, 1);
                try
                {
                    SerializableCardPool serializableCardPool = JsonHelper.fromJson<SerializableCardPool>(json);
                    cardPool = serializableCardPool.toCardPool(game.editor.getTypeByName);
                }
                catch (Exception e)
                {
                    throw new FormatException("反序列化卡池" + path + "失败：" + e, e);
                }
                _cardPoolInfoDict[path] = cardPoolInfo;
                _cardPoolDict[cardPoolInfo.cardPoolId] = cardPool;
                //if (!_runtimeGenActDefDict.TryGetValue(cardPoolInfo.cardPoolId, out var actionDefineList))
                //{
                //    actionDefineList = new List<GeneratedActionDefine>();
                //    _runtimeGenActDefDict.Add(cardPoolInfo.cardPoolId, actionDefineList);
                //}
                //actionDefineList.Clear();
                //actionDefineList.AddRange(cardPool.actionDefineList.ConvertAll(s => s.toGeneratedActionDefine(game.editor.getTypeByName)));
            }
            else if (cardPoolInfo.version >= CARD_POOL_VERSION_0_2)
            {
                if (path.StartsWith("sa:"))
                {
                    json = await game.fileManager.readStreamingTextFileToEnd(path, 1);
                }
                else
                {
                    json = await game.fileManager.readTextFileToEnd(path, 1);
                }
                try
                {
                    cardPool = JsonHelper.fromJson<CardPool>(json);
                }
                catch (Exception e)
                {
                    throw new FormatException("反序列化卡池" + path + "失败：" + e, e);
                }
                _cardPoolInfoDict[path] = cardPoolInfo;
                _cardPoolDict[cardPoolInfo.cardPoolId] = cardPool;
                if (!_runtimeGenActDefDict.TryGetValue(cardPoolInfo.cardPoolId, out var actionDefineList))
                {
                    actionDefineList = new List<GeneratedActionDefine>();
                    _runtimeGenActDefDict.Add(cardPoolInfo.cardPoolId, actionDefineList);
                }
                actionDefineList.Clear();
                actionDefineList.AddRange(cardPool.actionDefineList.ConvertAll(s => s.toGeneratedActionDefine(game.editor.getTypeByName)));
            }
            else if (cardPoolInfo.version == CARD_POOL_VERSION_0_1)
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
                try
                {
                    cardPool = JsonHelper.fromJson<CardPool>(json);
                }
                catch (Exception e)
                {
                    throw new FormatException("反序列化卡池" + path + "失败：" + e, e);
                }
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
                        try
                        {
                            cardPool.cardDefineList.AddRange(JsonHelper.fromJson<CardDefine[]>(json));
                        }
                        catch (Exception e)
                        {
                            throw new FormatException("反序列化卡池" + path + "的卡牌失败：" + e, e);
                        }
                    }
                }
                //卡牌皮肤
                if (lines.Length > 1)
                {
                    json = lines[1];
                    if (!string.IsNullOrEmpty(json))
                    {
                        try
                        {
                            cardPool.cardSkinList.AddRange(JsonHelper.fromJson<CardSkinData[]>(json));
                        }
                        catch (Exception e)
                        {
                            throw new FormatException("反序列化卡池" + path + "的卡牌皮肤失败：" + e, e);
                        }
                    }
                }
                //编辑器信息
                if (lines.Length > 2)
                {
                    json = lines[2];
                    if (!string.IsNullOrEmpty(json))
                    {
                        try
                        {
                            cardPool.actionNodeGraphInfoList.AddRange(JsonHelper.fromJson<ActionNodeGraphInfo[]>(json));
                        }
                        catch (Exception e)
                        {
                            throw new FormatException("反序列化卡池" + path + "的编辑器信息失败：" + e, e);
                        }
                    }
                }
                //增益
                if (lines.Length > 3)
                {
                    json = lines[3];
                    if (!string.IsNullOrEmpty(json))
                    {
                        try
                        {
                            cardPool.buffDefineList.AddRange(JsonHelper.fromJson<GeneratedBuffDefine[]>(json));
                        }
                        catch (Exception e)
                        {
                            throw new FormatException("反序列化卡池" + path + "的增益失败：" + e, e);
                        }
                    }
                }
                //增益皮肤
                if (lines.Length > 4)
                {
                    json = lines[4];
                    if (!string.IsNullOrEmpty(json))
                    {
                        try
                        {
                            cardPool.buffSkinList.AddRange(JsonHelper.fromJson<BuffSkin[]>(json));
                        }
                        catch (Exception e)
                        {
                            throw new FormatException("反序列化卡池" + path + "的增益皮肤失败：" + e, e);
                        }
                    }
                }
                _cardPoolInfoDict[path] = cardPoolInfo;
                _cardPoolDict[cardPoolInfo.cardPoolId] = cardPool;
            }
            //错误信息修复
            Dictionary<string, ActionDefine> actionDefineDict = ActionDefine.loadDefinesFromAssemblies(new Assembly[]
            {
                typeof(CardEngine).Assembly,
                typeof(HearthStoneRule).Assembly,
                typeof(EffectEditorLibrary).Assembly
            });
            foreach (var card in cardPool.cardDefineList)
            {
                foreach (GeneratedEffect effect in card.getGeneratedEffects())
                {
                    foreach (var trigger in effect.triggerList)
                    {
                        if (trigger == null)
                            continue;
                        fixTriggerIncorrectData(trigger);
                    }
                    effect.traverseActionNode(fixActionNodeIncorrectData);
                }
            }
            foreach (var skin in cardPool.cardSkinList)
            {
                if (string.IsNullOrEmpty(skin.language))
                {
                    //皮肤不能没有对应语言信息，如果没有那么至少设置为默认语言
                    skin.language = game.langManager.getDefaultLanguage();
                }
            }
            foreach (var graphInfo in cardPool.actionNodeGraphInfoList)
            {
                if (string.IsNullOrEmpty(graphInfo.entryType))
                {
                    //流程图信息不能没有类型，如果为空则默认为triggerList
                    graphInfo.entryType = nameof(GeneratedEffect.triggerList);
                }
                foreach (var actionNode in graphInfo.freeActionNodeList)
                {
                    actionNode.traverse(fixActionNodeIncorrectData);
                }
            }
            foreach (var buff in cardPool.buffDefineList)
            {
                foreach (var effect in buff.effectList)
                {
                    foreach (var trigger in effect.triggerList)
                    {
                        if (trigger == null)
                            continue;
                        fixTriggerIncorrectData(trigger);
                    }
                    effect.traverseActionNode(fixActionNodeIncorrectData);
                }
            }
            return (cardPoolInfo, cardPool);

            void fixActionNodeIncorrectData(ActionNode actionNode)
            {
                if (!actionDefineDict.ContainsKey(actionNode.defineName) && !cardPool.actionDefineList.Exists(a => a.name == actionNode.defineName))
                {
                    //包含不存在的动作名称，可能是名称过期了
                    string newDefineName = null;
                    foreach (ActionDefine actionDefine in actionDefineDict.Values)
                    {
                        if (actionDefine.obsoleteNames != null && actionDefine.obsoleteNames.Contains(actionNode.defineName))
                        {
                            newDefineName = actionDefine.defineName;
                            break;
                        }
                    }
                    if (newDefineName != null)
                    {
                        Debug.LogWarning("将动作节点" + actionNode.id + "过期的动作名称" + actionNode.defineName + "替换为" + newDefineName);
                        actionNode.defineName = newDefineName;
                    }
                    else
                    {
                        Debug.LogError("动作节点" + actionNode.id + "的名称未知：" + actionNode.defineName);
                    }
                }
            }
            void fixTriggerIncorrectData(TriggerGraph trigger)
            {
                //if (!string.IsNullOrEmpty(trigger.eventName) && !EventTypeInfo.isValidEventTypeName(trigger.eventName))
                //{
                //    //触发的事件名称可能已经过期
                //    EventTypeInfo eventTypeInfo = EventTypeInfo.findEventTypeInfoForObsolateName(trigger.eventName);
                //    if (eventTypeInfo != null)
                //    {
                //        Debug.LogWarning(trigger + "事件名称" + trigger.eventName + "已经过期，被替换为" + eventTypeInfo.eventName);
                //        trigger.eventName = eventTypeInfo.eventName;
                //    }
                //    else
                //    {
                //        Debug.LogError(trigger + "的事件名称" + trigger.eventName + "可能已经过期且找不到可替换名称");
                //    }
                //}
                foreach (var targetChecker in trigger.targetCheckerList)
                {
                    if (string.IsNullOrEmpty(targetChecker.targetType))
                    {
                        //目标的类型不能为空，如果为空则默认为仆从
                        targetChecker.targetType = DEFINE.TARGET_TYPE_SERVANT;
                    }
                }
            }
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
        public async Task<CardDefine[]> reloadCardPoolsAndGetCardDefinesAsync(IEnumerable<int> cardPoolsId)
        {
            List<CardDefine> cardDefineList = new List<CardDefine>();
            var cardPoolInfoDict = await loadCardPoolInfosAsync();
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
        public void createCard(CardPoolInfo cardPoolInfo, out CardDefine cardDefine, out CardSkinData cardSkin)
        {
            if (!tryGetCardPool(cardPoolInfo.cardPoolId, out CardPool cardPool))
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolInfo.cardPoolId);
            }
            int cardId = 0;
            for (int i = 1; i < 0xFFFF; i++)
            {
                cardId = 1 << 31 | cardPoolInfo.cardPoolId << 16 | i;
                if (cardPool.cardDefineList.Exists(c => c.id == cardId))
                    continue;
                break;
            }
            cardDefine = new CardDefine(cardId, DEFINE.SERVANT);
            cardPool.cardDefineList.Add(cardDefine);
            cardSkin = new CardSkinData();
            cardSkin.id = cardDefine.id;
            cardPool.cardSkinList.Add(cardSkin);
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
                    CardPoolInfo cardPoolInfo = getCardPoolInfo(getCardPoolId(cardPool));
                    return cardPoolInfo;
                }
            }
            return null;
        }
        public string getCardPoolPath(CardPoolInfo cardPoolInfo)
        {
            return getCardPoolPath(cardPoolInfo.cardPoolId);
        }
        public string getCardPoolPath(int cardPoolId)
        {
            foreach (var pair in _cardPoolInfoDict)
            {
                if (pair.Value.cardPoolId == cardPoolId)
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
        public CardSkinData createSkin(int cardPoolId, int cardDefineId)
        {
            if (tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                CardSkinData skin = new CardSkinData();
                skin.id = cardDefineId;
                skin.skinName = StringTable.EDITOR_NEW_SKIN_DEFAULT_NAME;
                skin.skinId = getAllSkins(cardDefineId).GroupBy(c => c.skinId).Select(g => g.Key).Max() + 1; ;
                cardPool.cardSkinList.Add(skin);
                return skin;
            }
            else
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
        }
        public CardSkinData createSkinForDiffLanguage(int cardPoolId, CardSkinData skin, string language)
        {
            if (tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                CardSkinData newSkin = new CardSkinData(skin);
                newSkin.language = language;
                cardPool.cardSkinList.Add(newSkin);
                return newSkin;
            }
            else
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
        }
        public void removeSkin(int cardPoolId, int cardDefineId, int skinId)
        {
            if (tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                cardPool.cardSkinList.RemoveAll(s => s.id == cardDefineId && s.skinId == skinId);
            }
            else
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
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
        public Task<Sprite> loadCardImage(int cardId)
        {
            CardSkinData skin = getCardSkin(cardId);
            if (skin.image != null)
                return Task.FromResult(skin.image);
            else
            {
                string path = skin.imagePath;
                if (string.IsNullOrEmpty(path))
                {
                    return Task.FromResult<Sprite>(null);
                }
                else if (getCardPoolOfCard(getCardDefine(cardId)) is CardPoolInfo cardPool &&
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
                //查找所有皮肤
                var skinGroups = cardPool.cardSkinList.Where(s => s.id == cardId).GroupBy(s => s.skinId).OrderBy(g => g.Key);
                if (skinGroups.Count() < 1)
                    continue;
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
            if (skinDic.TryGetValue(cardId, out CardSkinData builtinSkin))
            {
                skinList.Add(builtinSkin);
            }
            if (tempSkinDic.TryGetValue(cardId, out CardSkinData tempSkin))
            {
                skinList.Add(tempSkin);
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
        public string cardPoolDirName => _cardPoolDirName;
        public string textureDirName => _textureDirName;
        public string editorCardPoolDirName => _editorCardPoolDirName;
        public string editorTextureDirName => _editorTextureDirName;
        public string cardPoolFilePostfix => _cardPoolFilePostfix;
        public int[] standardCardPoolsId => _standardCardPoolsId;
        /// <summary>
        /// 键为文件地址，值为卡池信息
        /// </summary>
        Dictionary<string, CardPoolInfo> _cardPoolInfoDict = new Dictionary<string, CardPoolInfo>();
        /// <summary>
        /// 键为卡池Id，值为卡池
        /// </summary>
        Dictionary<int, CardPool> _cardPoolDict = new Dictionary<int, CardPool>();
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
        const float CARD_POOL_VERSION_0_3 = 0.3f;
        const float CARD_POOL_VERSION_0_2 = 0.2f;
        const float CARD_POOL_VERSION_0_1 = 0.1f;
        #endregion
    }
}
