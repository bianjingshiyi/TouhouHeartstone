using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Controllers;
using MongoDB.Bson.Serialization;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using UI;
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
            //int cardPoolId = 0;
            //for (int i = 1; i < 0x7FFF; i++)
            //{
            //    if (game.card.isStandardCardPool(i))//不能是官方卡池ID
            //    {
            //        continue;
            //    }
            //    if (_cardPoolInfoDict.Any(p => p.Value.cardPoolId == i))//不能是已有的卡池ID
            //    {
            //        continue;
            //    }
            //    cardPoolId = i;
            //    break;
            //}
            long cardPoolId = game.NewSnowflakeId();
            cardPoolInfo = new CardPoolInfo();
            cardPoolInfo.cardPoolId = cardPoolId;
            cardPoolInfo.cardPoolName = StringTable.EDITOR_NEW_CARDPOOL_DEFAULT_NAME;
            cardPoolInfo.imageResPath = null;
            cardPoolInfo.versionNumber = CARD_POOL_VERSION_1;
            cardPool = new CardPool();
            _cardPoolDict[cardPoolId] = cardPool;
        }
        /// <summary>
        /// 保存卡池，如果卡池没有被保存过那么会被分配一个新的地址，否则会覆盖已经保存的文件。
        /// </summary>
        /// <param name="cardPoolInfo"></param>
        /// <returns>是否成功保存卡池。</returns>
        public async Task<bool> saveCardPool(CardPoolInfo cardPoolInfo)
        {
            if (!Application.isEditor && isStandardCardPool(cardPoolInfo.cardPoolId))
            {
                //不能修改标准卡池，虽然其实玩家想改也没人能阻止就是了，最多是和别人玩会失去同步吧。
                game.ui.dialog.ShowConfirm(StringTable.INVALID_OPERATION, StringTable.EDITOR_DIALOG_CANNOT_MODIFY_STANDARD_CARDPOOL, null);
                return false;
            }
            //如果当前的卡池没有被保存过，没有分配地址，则给它分配一个地址。
            string cardPoolPath = null;
            if (cardPoolInfo == null) { return false; }//如果没有保存需求就跳过这个流程
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
            var cardPool = _cardPoolDict[cardPoolInfo.cardPoolId];
            // 获取卡池中对其他卡池的卡牌的引用。
            cardPoolInfo.dependencies = getCardPoolReferences(cardPoolInfo.cardPoolId);

            SerializableCardPool seriCardPool = new SerializableCardPool(cardPool);
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
            cardPoolInfo.version = 0;
            cardPoolInfo.versionNumber = CARD_POOL_VERSION_1;
            //初始化JSON
            if (!JsonHelper.isInited)
            {
                JsonHelper.init();
            }
            cardPoolInfo.editTime++;
            await game.fileManager.saveFile(path, new string[]
            {
                cardPoolInfo.toJson(),//文件头信息
                seriCardPool.toJson(true)//内容对象
            });
            Debug.Log("保存卡池" + path + "成功");
            return true;
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
            if (cardPoolInfo.versionNumber >= CARD_POOL_VERSION_1)
            {
                cardPool = await loadCardPool_v1(path, cardPoolInfo);
            }
            else if (cardPoolInfo.version >= CARD_POOL_VERSION_0_3)
            {
                cardPool = await loadCardPool_v1(path, cardPoolInfo);
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
            bool hasUnsavedFix = false;
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
                if (!string.IsNullOrEmpty(skin.imagePath) && !isHashNamedJPG(skin.imagePath))
                {
                    //图片地址必须是32位哈希.jpg
                    Sprite sprite = await loadImage(skin.imagePath, cardPoolInfo.cardPoolId);
                    skin.imagePath = await saveImage(cardPoolInfo.cardPoolId, sprite.texture);
                    hasUnsavedFix = true;
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
            foreach (var buffSkin in cardPool.buffSkinList)
            {
                if (string.IsNullOrEmpty(buffSkin.imagePath))
                    continue;
                if (!isHashNamedJPG(buffSkin.imagePath))
                {
                    //图片地址必须是32位哈希.jpg
                    Sprite sprite = await loadImage(buffSkin.imagePath, cardPoolInfo.cardPoolId);
                    buffSkin.imagePath = await saveImage(cardPoolInfo.cardPoolId, sprite.texture);
                    hasUnsavedFix = true;
                }
            }
            if (hasUnsavedFix)
                await saveCardPool(cardPoolInfo);
            return (cardPoolInfo, cardPool);

            void fixActionNodeIncorrectData(ActionNode actionNode)
            {
                if (!actionDefineDict.ContainsKey(actionNode.defineName) && !cardPool.generatedActionDefineList.Exists(a => a.defineName == actionNode.defineName))
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
                for (int i = 0; i < actionNode.consts.Length; i++)
                {
                    if (actionNode.consts[i] is CardReference cardRef)
                    {
                        //CardReference过期，使用DefineReference替代
                        actionNode.consts[i] = new DefineReference(cardRef.cardPoolId, cardRef.cardId);
                    }
                }
                // 将旧版的“获取增益定义”的ID从int变为DefineReference。
                if (actionNode.defineName == StringTable.EDITOR_NODE_NAME_GET_BUFF_DEFINE)
                {
                    if (actionNode.consts != null && actionNode.consts.Length > 0 && actionNode.consts[0] is int buffId)
                    {
                        actionNode.consts[0] = new DefineReference(cardPoolInfo.cardPoolId, buffId);
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
        public bool tryGetCardPool(long cardPoolId, out CardPool cardPool)
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
        public Dictionary<long, CardPool> getCardPools()
        {
            Dictionary<long, CardPool> cardPoolDict = new Dictionary<long, CardPool>(_cardPoolDict);
            cardPoolDict.Add(0, new CardPool()
            {
                cardDefineList = defineDic.Values.ToList(),
                cardSkinList = skinDic.Values.ToList()
            });
            return cardPoolDict;
        }
        public Dictionary<long, CardPool> getCardPools(IEnumerable<long> cardPoolsId)
        {
            Dictionary<long, CardPool> cardPoolDict = new Dictionary<long, CardPool>();
            foreach (long cardPoolId in cardPoolsId)
            {
                if (_cardPoolDict.TryGetValue(cardPoolId, out var cardPool))
                    cardPoolDict.Add(cardPoolId, cardPool);
            }
            cardPoolDict.Add(0, new CardPool()
            {
                cardDefineList = defineDic.Values.ToList(),
                cardSkinList = skinDic.Values.ToList()
            });
            return cardPoolDict;
        }

        public bool removeCardPool(long cardPoolId)
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
                _cardPoolInfoDict.Remove(path);
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

        public async Task<(CardPoolInfo info, CardPool cardPool)> loadCachedCardPoolAsync(long cardPoolId, int version)
        {
            string resID = CardPoolInfoPack.GetResourceID(cardPoolId, version);
            if (game.cache.FileExists(game.fileManager, ResourceType.CardDefine, resID))
            {
                string path = game.cache.GetCachePath(ResourceType.CardDefine, resID);
                var cardPoolData = await loadCardPool(path);
                return cardPoolData;
            }
            return (null, null);
        }
        public async Task<(CardDefine[] defines, CardSkinData[] skins)> loadCachedCardDefinesAsync(long cardPoolId, int version)
        {
            List<CardDefine> defineList = new List<CardDefine>();
            List<CardSkinData> skinList = new List<CardSkinData>();

            var cardPoolData = await loadCachedCardPoolAsync(cardPoolId, version);
            var cardPool = cardPoolData.cardPool;
            if (cardPool != null)
            {
                defineList.AddRange(cardPool.cardDefineList);
                skinList.AddRange(cardPool.cardSkinList);
            }
            return (defineList.ToArray(), skinList.ToArray());
        }
        public async Task<Dictionary<long, Dictionary<int, CardDefine>>> reloadCardPoolsAndGetCardDefinesAsync(IEnumerable<long> cardPoolsId)
        {
            Dictionary<long, Dictionary<int, CardDefine>> cardPoolDict = new Dictionary<long, Dictionary<int, CardDefine>>();
            var cardPoolInfoDict = await loadCardPoolInfosAsync();
            foreach (var cardPoolId in cardPoolsId)
            {
                if (cardPoolId == 0)
                {
                    //内置卡牌
                    cardPoolDict.Add(0, new Dictionary<int, CardDefine>(defineDic));
                    continue;
                }
                foreach (var pair in cardPoolInfoDict)
                {
                    if (pair.Value.cardPoolId == cardPoolId)
                    {
                        var result = await loadCardPool(pair.Key);
                        cardPoolDict.Add(result.cardPoolInfo.cardPoolId, result.cardPool.cardDefineList.ToDictionary(c => c.id));
                    }
                }
            }
            return cardPoolDict;
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
                cardId = i;
                if (cardPool.cardDefineList.Exists(c => c.id == cardId))
                    continue;
                break;
            }
            cardDefine = new CardDefine(cardId, DEFINE.SERVANT);
            cardDefine.cardPoolId = cardPoolInfo.cardPoolId;
            cardPool.cardDefineList.Add(cardDefine);
            cardSkin = new CardSkinData();
            cardSkin.id = cardDefine.id;
            cardPool.cardSkinList.Add(cardSkin);
        }
        public void removeCard(CardDefine card)
        {
            if (_cardPoolDict.TryGetValue(card.cardPoolId, out CardPool cardPool))
            {
                if (cardPool.cardDefineList.Remove(card))
                {
                    cardPool.cardSkinList.RemoveAll(s => s.id == card.id);
                }
            }
        }
        public bool tryGetCardDefine(DefineReference defRef, out CardDefine card)
        {
            return tryGetCardDefine(defRef.cardPoolId, defRef.defineId, out card);
        }
        public bool tryGetCardDefine(long cardPoolId, int cardId, out CardDefine card)
        {
            card = null;
            if (cardPoolId == 0 && cardId < 0)
                cardPoolId = cardId << 1 >> 17;
            if (tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                card = cardPool.cardDefineList.Find(d => d.id == cardId);
            }
            else if (defineDic.ContainsKey(cardId))
            {
                card = defineDic[cardId];
            }
            if (card != null)
            {
                return true;
            }
            return false;
        }
        public CardPoolInfo getCardPoolOfCard(CardDefine card)
        {
            return getCardPoolInfo(card.cardPoolId);
        }
        public string getCardPoolPath(CardPoolInfo cardPoolInfo)
        {
            return getCardPoolPath(cardPoolInfo.cardPoolId);
        }
        public string getCardPoolPath(long cardPoolId)
        {
            foreach (var pair in _cardPoolInfoDict)
            {
                if (pair.Value.cardPoolId == cardPoolId)
                    return pair.Key;
            }
            return null;
        }
        public CardPoolInfo getCardPoolInfo(long cardPoolId)
        {
            foreach (var cardPool in _cardPoolInfoDict.Values)
            {
                if (cardPool.cardPoolId == cardPoolId)
                    return cardPool;
            }
            return null;
        }
        public long getCardPoolId(CardPool cardPool)
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
        public CardSkinData createSkin(long cardPoolId, int cardDefineId)
        {
            if (tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                CardSkinData skin = new CardSkinData();
                skin.id = cardDefineId;
                skin.skinName = StringTable.EDITOR_NEW_SKIN_DEFAULT_NAME;
                skin.skinId = getAllSkins(cardPoolId, cardDefineId).GroupBy(c => c.skinId).Select(g => g.Key).Max() + 1; ;
                cardPool.cardSkinList.Add(skin);
                return skin;
            }
            else
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
        }
        public CardSkinData createSkinForDiffLanguage(long cardPoolId, CardSkinData skin, string language)
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
        public void removeSkin(long cardPoolId, int cardDefineId, int skinId)
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
                var fileManager = game.fileManager;
                bool fullPath = Path.GetFullPath(path) == path;
                byte[] bytes;
                if (fullPath && fileManager.isFileExist(path)) 
                {
                    //如果直接给出完整路径就直接从完整路径上读
                    bytes = await fileManager.readBinaryFile(path);
                }
                else if (fileManager.isPersistentFileExist(path))
                {
                    // 从外部图片路径读取图片。
                    bytes = await fileManager.readPersistentBinaryFile(path);
                }
                else
                {
                    //如果图片不存在，尝试从缓存资源中读取。
                    string picId = Path.GetFileNameWithoutExtension(path);
                    if (game.cache.FileExists(game.fileManager, ResourceType.Picture, picId))
                    {
                        bytes = await game.cache.ReadCache(fileManager, ResourceType.Picture, picId);
                    }
                    else
                    {
                        // 实在找不到了，返回一个空数组
                        Debug.LogWarning($"未找到路径为{path}的图片。");
                        bytes = Array.Empty<byte>();
                    }
                }
                // 如果是空数组，返回默认Sprite.
                if (bytes.Length <= 0)
                {
                    return getDefaultSprite().Result;
                }
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
        public Task<Sprite> loadCardImage(long cardPoolId, int cardId)
        {
            CardSkinData skin = getCardSkin(cardPoolId, cardId);
            if (skin == null)
            {
                return getDefaultSprite();
            }
            if (skin.image != null)
                return Task.FromResult(skin.image);
            else
            {
                string path = skin.imagePath;
                if (string.IsNullOrEmpty(path))
                {
                    return Task.FromResult<Sprite>(null);
                }
                else if (getCardPoolInfo(cardPoolId) is CardPoolInfo cardPool &&
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
        public Task<Sprite> loadCardImage(DefineReference defRef)
        {
            return loadCardImage(defRef.cardPoolId, defRef.defineId);
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
        public bool tryGetCardSkin(long cardPoolId, int cardId, out CardSkinData skin)
        {
            skin = null;
            if (_cardPoolDict.TryGetValue(cardPoolId, out var cardPool))
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
        public CardSkinData[] getAllSkins(long cardPoolId, int cardId)
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
            if (_cardPoolDict.TryGetValue(cardPoolId, out var cardPool))
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
        public bool isStandardCardPool(long cardPoolId)
        {
            return _standardCardPoolsId.Contains(cardPoolId);
        }
        /// <summary>
        /// 判断卡池是否是本人拥有的卡池。
        /// </summary>
        /// <param name="cardPoolId">卡池ID。</param>
        /// <returns>是否是本人拥有的卡池？</returns>
        public bool isOwnedCardPool(long cardPoolId)
        {
            // 内置和经典卡池肯定是本人拥有的。
            if (isBuiltinCardPool(cardPoolId) || isStandardCardPool(cardPoolId))
            {
                return true;
            }
            string path = getCardPoolPath(cardPoolId);
            if (string.IsNullOrEmpty(path))
                return false;

            // 如果路径以本地卡池的路径开头，说明是本身拥有的卡池。
            string startPath = Path.Combine(Application.persistentDataPath, cardPoolDirName);
            path = path.Replace("/", "\\");
            startPath = startPath.Replace("/", "\\");
            if (path.StartsWith(startPath) || path.StartsWith("sa:") || path.StartsWith("res:") || path.StartsWith(cardPoolDirName))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取一个卡池内对其他卡池的引用。
        /// </summary>
        /// <param name="cardPoolId">要检查的卡池ID。</param>
        /// <returns>引用的其他卡池列表。</returns>
        public long[] getCardPoolReferences(long cardPoolId)
        {
            HashSet<long> poolList = new HashSet<long>();
            if (!tryGetCardPool(cardPoolId, out var cardPool))
            {
                return new long[0];
            }
            foreach (var card in cardPool.cardDefineList)
            {
                foreach (GeneratedEffect effect in card.getGeneratedEffects())
                {
                    effect.traverseActionNode(addCardPool);
                }
            }
            foreach (var graphInfo in cardPool.actionNodeGraphInfoList)
            {
                foreach (var actionNode in graphInfo.freeActionNodeList)
                {
                    actionNode.traverse(addCardPool);
                }
            }
            foreach (var buff in cardPool.buffDefineList)
            {
                foreach (var effect in buff.effectList)
                {
                    effect.traverseActionNode(addCardPool);
                }
            }
            return poolList.ToArray();

            void addCardPool(ActionNode node)
            {
                foreach (var valueConst in node.consts)
                {
                    if (valueConst is DefineReference defRef && defRef.cardPoolId != cardPoolId)
                    {
                        poolList.Add(defRef.cardPoolId);
                    }
                }
            }
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
        private async Task<CardPool> loadCardPool_v1(string path, CardPoolInfo cardPoolInfo)
        {
            string json = path.StartsWith("sa:") ?
                await game.fileManager.readStreamingTextFileToEnd(path, 1) :
                await game.fileManager.readTextFileToEnd(path, 1);
            CardPool cardPool;
            try
            {
                //json = json.Replace("CardReference", "DefineReference");
                //json = Regex.Replace(json,
                //    "\"cardPoolId\"\\s*:\\s*(?<cardPoolId>-?\\d+)\\s*,\\s*\"(cardId|id)\"\\s*:\\s*(?<defineId>-?\\d+)",
                //    "\"cardPoolId\" : ${cardPoolId}, \"defineId\" : ${id}");
                SerializableCardPool serializableCardPool = JsonHelper.fromJson<SerializableCardPool>(json);
                cardPool = serializableCardPool.toCardPool(game.editor.getTypeByName);
                for (int i = 0; i < cardPool.cardDefineList.Count; i++)
                {
                    cardPool.cardDefineList[i].cardPoolId = cardPoolInfo.cardPoolId;
                }
                for (int i = 0; i < cardPool.buffDefineList.Count; i++)
                {
                    cardPool.buffDefineList[i].cardPoolId = cardPoolInfo.cardPoolId;
                }
            }
            catch (Exception e)
            {
                throw new FormatException("反序列化卡池" + path + "失败", e);
            }
            _cardPoolInfoDict[path] = cardPoolInfo;
            _cardPoolDict[cardPoolInfo.cardPoolId] = cardPool;
            return cardPool;
        }
        private string allocCardPoolPath(CardPoolInfo cardPool)
        {
            string path = Path.Combine(Application.persistentDataPath, cardPoolDirName, cardPool.cardPoolName + "." + cardPoolFilePostfix);
            for (int i = 0; i < 100 && File.Exists(path); i++)
            {
                if (i == 99)
                {
                    throw new IOException("达到最大文件存储上限");
                }
                Match match = Regex.Match(path, @"(?<path>.+)\s\((?<num>\d+)\)\." + cardPoolFilePostfix);
                if (match.Success)
                {
                    int num = int.Parse(match.Groups["num"].Value);
                    path = path.Replace("(" + num + ")", "(" + (num + 1) + ")");
                }
                else
                {
                    path = path.Insert(path.LastIndexOf('.'), " (0)");
                }
            }
            return path;
        }
        #endregion
        #region 属性字段
        public string cardPoolDirName => _cardPoolDirName;
        public string textureDirName => _textureDirName;
        public string editorCardPoolDirName => _editorCardPoolDirName;
        public string editorTextureDirName => _editorTextureDirName;
        public string cardPoolFilePostfix => _cardPoolFilePostfix;
        public long[] standardCardPoolsId => _standardCardPoolsId;
        /// <summary>
        /// 键为文件地址，值为卡池信息
        /// </summary>
        Dictionary<string, CardPoolInfo> _cardPoolInfoDict = new Dictionary<string, CardPoolInfo>();
        /// <summary>
        /// 键为卡池Id，值为卡池
        /// </summary>
        Dictionary<long, CardPool> _cardPoolDict = new Dictionary<long, CardPool>();
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
        long[] _standardCardPoolsId = new long[]
        {
            1
        };
        const float CARD_POOL_VERSION_0_3 = 0.3f;
        const float CARD_POOL_VERSION_0_2 = 0.2f;
        const float CARD_POOL_VERSION_0_1 = 0.1f;
        const int CARD_POOL_VERSION_1 = 1;
        #endregion
    }
}
