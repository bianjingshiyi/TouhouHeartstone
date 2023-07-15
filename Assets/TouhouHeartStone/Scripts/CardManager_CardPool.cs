using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Controllers;
using ExcelDataReader.Log;
using MongoDB.Bson.Serialization;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
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
                try
                {
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
                    CardPoolInfo cardPool = parseCardPoolInfo(lines[0]);
                    _cardPoolInfoDict.Add(path, cardPool);
                    onCardPoolInfoRead?.Invoke(path, cardPool);
                    Debug.Log("读取卡池信息" + path + "完毕");
                }
                catch (Exception e)
                {
                    Debug.LogError("读取卡池信息" + path + "失败：" + e);
                }
            }


            // 旧版本转换
            // 将dependencies转为dependencyRefs;
            foreach (var pair in _cardPoolInfoDict)
            {
                var cardPool = pair.Value;
                if (cardPool.dependencyRefs == null && cardPool.dependencies != null)
                {
                    var depInfos = cardPool.dependencies.SelectMany(depPoolId => _cardPoolInfoDict.Values.Where(p => p.cardPoolId == depPoolId));
                    var depRefs = depInfos.Select(poolInfo => poolInfo.ToReference());
                    cardPool.dependencies = null;
                    cardPool.dependencyRefs = depRefs.ToArray();
                }
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
            cardPoolInfo.versionNumber = CURRENT_CARD_POOL_VERSION;
            cardPool = new CardPool();
            CardPoolReference cardPoolRef = new CardPoolReference(cardPoolId, 0);
            _cardPoolDict[cardPoolRef] = cardPool;
            _creatingCardPoolId = cardPoolId;
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
            }
            var cardPoolRef = cardPoolInfo.ToReference();
            var cardPool = _cardPoolDict[cardPoolRef];
            // 获取卡池中对其他卡池的卡牌的引用。
            // TODO: 在被引用的卡池有新版本后，卡池保存时会提示玩家需要将卡池引用修改到新版本。
            cardPoolInfo.dependencyRefs = evaluateCardPoolDependencies(cardPoolRef);
            // 如果该卡池正在被创建，将正在创建的卡池设为空。
            if (_creatingCardPoolId == cardPoolInfo.cardPoolId)
                _creatingCardPoolId = -1;

            SerializableCardPool seriCardPool = new SerializableCardPool(cardPool);
            // 检查并修复老旧大卡图
            await fixLargeImage(cardPoolInfo.cardPoolId, cardPool);
            // 对缓存的图片进行保存
            await saveExternalImages(cardPoolInfo.cardPoolId, cardPool);
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
            cardPoolInfo.versionNumber = CURRENT_CARD_POOL_VERSION;
            //初始化JSON
            if (!JsonHelper.isInited)
            {
                JsonHelper.init();
            }

            // 卡池版本+1。
            cardPoolInfo.editTime++;
            _cardPoolInfoDict[cardPoolPath] = cardPoolInfo;
            _cardPoolDict.Remove(cardPoolRef);
            _cardPoolDict.Add(cardPoolInfo.ToReference(), cardPool);

            bool readFriendly;
#if UNITY_EDITOR
            readFriendly = true;
#else
            readFriendly = false;
#endif
            await game.fileManager.saveFile(path, new string[]
            {
                cardPoolInfo.toJson(),//文件头信息
                seriCardPool.toJson(readFriendly)//内容对象
            });
            Debug.Log("保存卡池" + path + "成功");
            return true;
        }

        /// <summary>
        /// 获取卡池内的所有包含过时节点的节点图。
        /// </summary>
        /// <returns>过时节点节点图列表。</returns>
        public (int cardId, int buffId, int actionId)[] getObsoleteNodeGraphs(CardPool cardPool)
        {
            HashSet<(int cardId, int buffId, int actionId)> list = new HashSet<(int cardId, int buffId, int actionId)>(); 
            Dictionary<string, ActionDefine> actionDefineDict = ActionDefine.loadDefinesFromAssemblies(new Assembly[]
            {
                typeof(CardEngine).Assembly,
                typeof(HearthStoneRule).Assembly,
                typeof(EffectEditorLibrary).Assembly
            });
            void checkObsoleteNodes(Node node, (int cardId, int buffId, int actionId) graphObj)
            {
                if (!(node is ActionNode actionNode))
                {
                    return;
                }
                ActionDefine define = null;
                if (!actionDefineDict.TryGetValue(actionNode.defineName, out define))
                {
                    define = cardPool.actionDefineList.Find(a => a.defineName == actionNode.defineName);
                }
                if (define == null || define.obsoleteMsg != null)
                {
                    list.Add(graphObj);
                }
            }


            Action<CardDefine, GeneratedEffect> cardAction = (card, effect) =>
            {
                effect.traverseActionNode(node => checkObsoleteNodes(node, (card.id, 0, 0)));
            };
            Action<BuffDefine, GeneratedEffect> buffAction = (buff, effect) =>
            {
                effect.traverseActionNode(node => checkObsoleteNodes(node, (0, buff.id, 0)));
            };
            Action<GeneratedActionDefine> defineAction = (define) =>
            {
                define.traverse(node => checkObsoleteNodes(node, (0, 0, define.id)));
            };
            traverseActionGraphs(cardPool, cardAction, buffAction, defineAction);
            return list.ToArray();
        }

        /// <summary>
        /// 解析卡池文件的头文件信息，并返回卡池信息。
        /// </summary>
        /// <param name="line">文件信息</param>
        /// <returns>卡池信息。</returns>
        /// <exception cref="FormatException"></exception>
        public CardPoolInfo parseCardPoolInfo(string line)
        {
            CardPoolInfo cardPool = JsonHelper.fromJson<CardPoolInfo>(line);

            //去重，多份文件中可能包含相同的卡池
            string conflictCardPoolPath = null;
            foreach (var pair in _cardPoolInfoDict)
            {
                if (pair.Value.CompareCardPool(cardPool))
                {
                    conflictCardPoolPath = pair.Key;
                    break;
                }
            }
            if (conflictCardPoolPath != null)
            {
                _cardPoolInfoDict.Remove(conflictCardPoolPath);
            }
            return cardPool;
        }

        /// <summary>
        /// 解析卡池文件
        /// </summary>
        /// <param name="lines">文件文本行</param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public (CardPoolInfo cardPoolInfo, CardPool cardPool) parseCardPool(string[] lines)
        {
            string infoJson = lines[0];
            CardPoolInfo cardPoolInfo = parseCardPoolInfo(infoJson);

            var cardPoolId = cardPoolInfo.cardPoolId;
            if (cardPoolInfo.versionNumber >= CARD_POOL_VERSION_2)
            {
                var remain = string.Join("\n", lines.Skip(1));
                var cardPool = parseCardPool_v2(remain, cardPoolId);
                return (cardPoolInfo, cardPool);
            }
            else if (cardPoolInfo.versionNumber >= CARD_POOL_VERSION_1 || cardPoolInfo.version >= CARD_POOL_VERSION_0_3)
            {
                var remain = string.Join("\n", lines.Skip(1));
                var cardPool = parseCardPool_v1(remain, cardPoolId);
                return (cardPoolInfo, cardPool);
            }
            else if (cardPoolInfo.version >= CARD_POOL_VERSION_0_2)
            {
                var remain = string.Join("\n", lines.Skip(1));
                var cardPool = parseCardPool_v0_12(remain);
                return (cardPoolInfo, cardPool);
            }
            else if (cardPoolInfo.version == CARD_POOL_VERSION_0_1)
            {
                var cardPool = parseCardPool_v0_12(lines[1]);
                return (cardPoolInfo, cardPool);
            }
            else
            {
                var cardPool = parseCardPool_v0_0(lines.Skip(1).ToArray());
                return (cardPoolInfo, cardPool);
            }
        }

        /// <summary>
        /// 从目标路径上加载卡池
        /// </summary>
        /// <param name="path">流文件路径或者绝对路径</param>
        /// <param name="handleException">是否在方法内就捕获异常。</param>
        /// <exception cref="FileLoadException">卡池文件为空。</exception>
        /// <exception cref="FormatException">反序列化失败。</exception>
        /// <returns></returns>
        public async Task<(CardPoolInfo cardPoolInfo, CardPool cardPool)> loadCardPool(string path, bool handleException = false)
        {
            //初始化JSON
            if (!JsonHelper.isInited)
            {
                JsonHelper.init();
            }
            try
            {
                string[] lines;
                if (path.StartsWith("sa:"))
                {
                    lines = await game.fileManager.readStreamingTextFile(path, 0, 0);
                }
                else
                {
                    lines = await game.fileManager.readTextFile(path, 0, 0);
                }
                if (lines == null || lines.Length < 1)
                {
                    throw new FileLoadException("卡池文件" + path + "为空");
                }
                CardPoolInfo cardPoolInfo;
                CardPool cardPool;
                try
                {
                    (cardPoolInfo, cardPool) = parseCardPool(lines);
                }
                catch (Exception e)
                {
                    throw new FormatException("反序列化卡池" + path + "失败：" + e, e);
                }

                addCardPool(cardPool, cardPoolInfo, path);

                //错误信息修复
                await fixCardPool(cardPoolInfo, cardPool);
                return (cardPoolInfo, cardPool);
            }
            catch (Exception e)
            {
                if (!handleException)
                {
                    throw;
                }
                Debug.LogError($"卡池{path}加载失败：{e}");
                return (null, null);
            }
        }

        /// <summary>
        /// 在一个卡池引用的集合中，根据卡池的ID获取指定的卡池引用。
        /// 多用于根据卡池ID获取某个房间内允许的卡池。
        /// </summary>
        /// <param name="cardPoolId">卡池ID。</param>
        /// <param name="set">卡池引用集合。</param>
        /// <param name="fallbackLocal">如果没有找到卡池引用，是否获取本地的卡池引用。</param>
        /// <returns>获取到的卡池引用。</returns>
        public ICardPoolReference getCardPoolReferenceInSet(long cardPoolId, IEnumerable<ICardPoolReference> set, bool fallbackLocal = true)
        {
            // 集合不存在，返回null。
            if (set == null)
            {
                return null;
            }

            // 内置卡池，返回内置卡尺引用。
            if (isBuiltinCardPool(cardPoolId))
            {
                return builtinCardPoolReference;
            }

            // 查找集合内的卡池引用。
            var pool = set.FirstOrDefault(p => p.CardPoolID == cardPoolId);
            if (pool != null)
            {
                return pool.ToReference();
            }

            // 没有找到，获取本地卡池。
            if (fallbackLocal)
            {
                return getLocalCardPoolReference(cardPoolId);
            }
            return null;
        }
        /// <summary>
        /// 根据卡池ID获取本地的卡池引用（优先获取不是缓存中的，并且可以在编辑器内看到的卡池）。
        /// </summary>
        /// <param name="cardPoolId">卡池ID。</param>
        /// <param name="fallbackCached">如果没有非缓存卡池，是否获取缓存中的卡池？</param>
        /// <returns></returns>
        public ICardPoolReference getLocalCardPoolReference(long cardPoolId, bool fallbackCached = false)
        {
            if (isCreatingCardPool(cardPoolId))
            {
                return new CardPoolReference(cardPoolId, 0);
            }
            if (isBuiltinCardPool(cardPoolId))
            {
                return builtinCardPoolReference;
            }
            var info = _cardPoolInfoDict.Values.FirstOrDefault(p => p.cardPoolId == cardPoolId && isLocalCardPool(p));
            if (info == null && fallbackCached)
            {
                info = _cardPoolInfoDict.Values.FirstOrDefault(p => p.cardPoolId == cardPoolId);
            }
            if (info != null)
            {
                return info.ToReference();
            }
            return null;
        }
        /// <summary>
        /// 根据卡池ID获取本地的卡池（优先获取不是缓存中的，并且可以在编辑器内看到的卡池）。
        /// </summary>
        /// <param name="cardPoolId">卡池ID。</param>
        /// <param name="fallbackCached">如果没有非缓存卡池，是否获取缓存中的卡池？</param>
        /// <param name="cardPool">获取到的卡池。</param>
        /// <returns></returns>
        public CardPool getLocalCardPool(long cardPoolId, bool fallbackCached = false)
        {
            var cardPoolRef = getLocalCardPoolReference(cardPoolId, fallbackCached);
            if (cardPoolRef == null)
            {
                return null;
            }
            return _cardPoolDict[cardPoolRef];
        }
        /// <summary>
        /// 根据卡池ID获取本地的卡池（包括缓存内的卡池）。
        /// </summary>
        /// <param name="cardPoolId"></param>
        /// <param name="cardPool"></param>
        /// <returns></returns>
        public bool tryGetCardPool(long cardPoolId, out CardPool cardPool)
        {
            cardPool = _cardPoolDict.FirstOrDefault(p => p.Key.CardPoolID == cardPoolId).Value;
            return cardPool != null;
        }
        public bool tryGetCardPool(ICardPoolReference cardPoolRef, out CardPool cardPool)
        {
            if (cardPoolRef == null)
            {
                cardPool = null;
                return false;
            }
            return _cardPoolDict.TryGetValue(cardPoolRef, out cardPool);
        }
        public bool tryGetCardPool(string path, out CardPool cardPool)
        {
            if (_cardPoolInfoDict.TryGetValue(path, out CardPoolInfo cardPoolInfo))
            {
                return tryGetCardPool(cardPoolInfo.ToReference(), out cardPool);
            }
            else
            {
                cardPool = null;
                return false;
            }
        }
        public bool removeCardPool(CardPoolReference cardPoolRef)
        {
            if (_cardPoolDict.ContainsKey(cardPoolRef))
            {
                _cardPoolDict.Remove(cardPoolRef);
            }
            string path = null;
            foreach (var pair in _cardPoolInfoDict)
            {
                if (pair.Value.CompareCardPool(cardPoolRef))
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

        public async Task<(CardPoolInfo info, CardPool cardPool)> loadCachedCardPoolAsync(long cardPoolId, int version)
        {
            try
            {
                string resID = CardPoolReference.GetResourceID(cardPoolId, version);
                if (game.cache.FileExists(game.fileManager, ResourceType.CardDefine, resID))
                {
                    string path = game.cache.GetCachePath(ResourceType.CardDefine, resID);
                    var cardPoolData = await loadCardPool(path);
                    return cardPoolData;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
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
        public async Task<Dictionary<ICardPoolReference, CardDefine[]>> reloadCardPoolsAndGetCardDefinesAsync(IEnumerable<ICardPoolReference> cardPoolsRef)
        {
            Dictionary<ICardPoolReference, CardDefine[]> cardPoolDict = new Dictionary<ICardPoolReference, CardDefine[]>();
            var cardPoolInfoDict = await loadCardPoolInfosAsync();
            foreach (var cardPoolRef in cardPoolsRef)
            {
                if (isBuiltinCardPool(cardPoolRef.CardPoolID))
                {
                    //内置卡牌
                    cardPoolDict.Add(builtinCardPoolReference, defineDic.Values.ToArray());
                    continue;
                }
                foreach (var pair in cardPoolInfoDict)
                {
                    if (pair.Value.CompareCardPool(cardPoolRef))
                    {
                        var result = await loadCardPool(pair.Key);
                        cardPoolDict.Add(result.cardPoolInfo.ToReference(), result.cardPool.cardDefineList.ToArray());
                    }
                }
            }
            return cardPoolDict;
        }
        #region 卡牌
        public void createCard(CardPoolInfo cardPoolInfo, out CardDefine cardDefine, out CardSkinData cardSkin)
        {
            if (!tryGetCardPool(cardPoolInfo.ToReference(), out CardPool cardPool))
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
            cardSkin = new CardSkinData();
            cardSkin.id = cardDefine.id;
        }
        public void addCard(CardPoolInfo cardPoolInfo, CardDefine cardDefine)
        {
            addCard(cardPoolInfo.cardPoolId, cardDefine);
        }
        public void addCard(long cardPoolId, CardDefine cardDefine)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            cardPool.cardDefineList.Add(cardDefine);
        }
        public void removeCard(CardDefine card)
        {
            var cardPool = getCardPoolOfCard(card);
            if (cardPool != null)
            {
                if (cardPool.cardDefineList.Remove(card))
                {
                    cardPool.cardSkinList.RemoveAll(s => s.id == card.id);
                }
            }
        }
        public void removeCard(long cardPoolId, int cardId)
        {
            removeCard(getCardDefine(cardPoolId, cardId));
        }
        public CardDefine cloneCardDefine(CardDefine define)
        {
            var json = new SerializableCardDefine(define).toJson();
            var cloneSeri = JsonHelper.fromJson<SerializableCardDefine>(json);

            EventTypeInfoFinder eventFinder = EventTypeInfoLoader.FindEventTypeInfo;

            var nodeDefiner = new THHNodeDefiner(name => game.getActionDefine(name, define.cardPoolId), eventFinder);
            var clone = cloneSeri.toCardDefine(nodeDefiner);
            clone.cardPoolId = define.cardPoolId;
            return clone;
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
        public CardPoolInfo getCardPoolInfoOfCard(CardDefine card)
        {
            return getCardPoolInfo(getCardPoolReferenceOfCard(card));
        }
        public ICardPoolReference getCardPoolReferenceOfCard(CardDefine card)
        {
            if (isBuiltinCard(card))
                return builtinCardPoolReference;
            foreach (var pair in _cardPoolDict)
            {
                if (pair.Key.CardPoolID == card.cardPoolId && pair.Value.cardDefineList.Contains(card))
                {
                    return pair.Key;
                }
            }
            return null;
        }
        public CardPool getCardPoolOfCard(CardDefine card)
        {
            foreach (var pair in _cardPoolDict)
            {
                if (pair.Key.CardPoolID == card.cardPoolId && pair.Value.cardDefineList.Contains(card))
                {
                    return pair.Value;
                }
            }
            return null;
        }
        public string getCardPoolPath(CardPoolInfo cardPoolInfo)
        {
            return getCardPoolPath(cardPoolInfo.ToReference());
        }
        /// <summary>
        /// 根据卡池引用获取卡池路径（结果为第一个符合ID的卡池）。
        /// </summary>
        /// <param name="cardPoolRef">卡池引用。</param>
        /// <returns>卡池路径。</returns>
        public string getCardPoolPath(ICardPoolReference cardPoolRef)
        {
            foreach (var pair in _cardPoolInfoDict)
            {
                if (pair.Value.CompareCardPool(cardPoolRef))
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
        public CardPoolInfo getCardPoolInfo(ICardPoolReference cardPoolRef)
        {
            foreach (var cardPool in _cardPoolInfoDict.Values)
            {
                if (cardPool.CompareCardPool(cardPoolRef))
                    return cardPool;
            }
            return null;
        }
        #region 卡牌皮肤
        public CardSkinData createSkin(long cardPoolId, int cardDefineId)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool != null)
            {
                CardSkinData skin = new CardSkinData();
                skin.id = cardDefineId;
                skin.skinName = StringTable.EDITOR_NEW_SKIN_DEFAULT_NAME;
                skin.skinId = getAllLocalSkins(cardPoolId, cardDefineId).GroupBy(c => c.skinId).Select(g => g.Key).Max() + 1;
                return skin;
            }
            else
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
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
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
        }
        public void addCardSkin(long cardPoolId, CardSkinData cardSkin)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            cardPool.cardSkinList.Add(cardSkin);
        }
        public void removeAllCardSkins(long cardPoolId, int cardDefineId)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
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
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
        }
        #endregion
        public async Task<Sprite> loadExternalImage(string path, string dirName)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            Sprite sprite;
            if (path.StartsWith("res:"))
            {
                sprite = getSpriteFromCache(path);
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
                sprite = getSpriteFromCache(path);
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
                sprite = getSpriteFromCache(path);
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
                    return getDefaultSprite();
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
        public Task<Sprite> loadCardImage(ICardPoolReference cardPoolRef, int cardId)
        {
            CardSkinData skin = getCardSkin(cardPoolRef, cardId);
            if (skin == null)
            {
                return Task.FromResult(getDefaultSprite());
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
                else if (getCardPoolInfo(cardPoolRef) is CardPoolInfo cardPool &&
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
        public bool isStandardCardPool(CardPoolInfo cardPool)
        {
            return isStandardCardPool(cardPool.cardPoolId);
        }
        public bool isStandardCardPool(long cardPoolId)
        {
            return _standardCardPoolsId.Contains(cardPoolId);
        }

        /// <summary>
        /// 判断卡池l路径是否是本地卡池（即内置、经典卡池，或卡池文件位于普通卡池目录内，而不属于缓存的卡池）。
        /// </summary>
        /// <param name="path">卡池路径。</param>
        /// <returns>是否是本地卡池？</returns>
        public bool isLocalCardPoolPath(string path)
        {
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
        /// 判断卡池是否是本地卡池（即内置、经典卡池，或卡池文件位于普通卡池目录内，而不属于缓存的卡池）。
        /// </summary>
        /// <param name="cardPoolRef">卡池引用。</param>
        /// <returns>是否是本地卡池？</returns>
        public bool isLocalCardPool(ICardPoolReference cardPoolRef)
        {
            // 内置和经典卡池肯定是本人拥有的。
            if (isBuiltinCardPool(cardPoolRef.CardPoolID) || isStandardCardPool(cardPoolRef.CardPoolID))
                return true;
            if (isCreatingCardPool(cardPoolRef.CardPoolID))
                return true;
            string path = getCardPoolPath(cardPoolRef);
            return isLocalCardPoolPath(path);
        }
        /// <summary>
        /// 重新获取一个卡池内对其他卡池的引用。
        /// </summary>
        /// <param name="cardPoolRef">要检查的卡池ID。</param>
        /// <returns>引用的其他卡池列表。</returns>
        public CardPoolReference[] evaluateCardPoolDependencies(ICardPoolReference cardPoolRef)
        {
            HashSet<CardPoolReference> poolList = new HashSet<CardPoolReference>();
            if (!tryGetCardPool(cardPoolRef, out var cardPool))
            {
                return new CardPoolReference[0];
            }
            traverseActionNodes(cardPool, addCardPoolByNode);
            foreach (var card in cardPool.cardDefineList)
            {
                // 获取卡牌属性中所有引用其他卡池的属性。
                var propNames = card.getPropNames();
                foreach (string propName in propNames)
                {
                    var prop = card.getProp<object>(propName);
                    addCardPoolOfObject(prop);
                }
                // 获取卡牌效果中所有引用其他卡池的属性。
                var genEffects = card.getGeneratedEffects();
                foreach (var effect in genEffects)
                {
                    addCardPoolByEffect(effect);
                }
            }
            foreach (var buffDef in cardPool.buffDefineList)
            {
                // 获取增益效果中所有引用其他卡池的属性。
                foreach (var effect in buffDef.effectList)
                {
                    addCardPoolByEffect(effect);
                }
            }
            return poolList.ToArray();

            // 使用的内嵌方法。

            void addCardPoolOfDefRef(DefineReference defRef)
            {
                if (defRef.cardPoolId != cardPoolRef.CardPoolID)
                {
                    var depPoolInfo = getCardPoolInfo(defRef.cardPoolId);
                    if (depPoolInfo != null)
                    {
                        CardPoolReference depRef = new CardPoolReference(defRef.cardPoolId, depPoolInfo.editTime);
                        poolList.Add(depRef);
                    }
                }
            }
            void addCardPoolOfObject(object obj)
            {
                if (obj is DefineReference defRef)
                {
                    addCardPoolOfDefRef(defRef);
                }
                else if (obj is IEnumerable<DefineReference> defRefCol)
                {
                    foreach (var dRef in defRefCol)
                    {
                        addCardPoolOfDefRef(dRef);
                    }
                }
            }
            void addCardPoolByNode(Node node)
            {
                foreach (var valueConst in node.consts)
                {
                    addCardPoolOfObject(valueConst.Value);
                }
            }
            void addCardPoolByEffect(GeneratedEffect effect)
            {
                foreach (var propName in effect.getPropNames())
                {
                    var prop = effect.getProp<object>(propName);
                    addCardPoolOfObject(prop);
                }
            }
        }
        public bool isCreatingCardPool(long cardPoolId)
        {
            return _creatingCardPoolId == cardPoolId;
        }

        public void showCardPoolLoadErrorDialog()
        {
            var title = game.lang._(StringTable.ERROR);
            var content = game.lang._(CARD_POOL_LOAD_ERROR_DESC);
            game.ui.dialog.ShowConfirm(title, content);
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

        /// <summary>
        /// 向当前卡池信息添加一个卡池
        /// </summary>
        /// <param name="cardPool"></param>
        /// <param name="info"></param>
        /// <param name="filePath"></param>
        void addCardPool(CardPool cardPool, CardPoolInfo info, string filePath)
        {
            _cardPoolInfoDict[filePath] = info;
            _cardPoolDict[info.ToReference()] = cardPool;
        }

        /// <summary>
        /// 解析卡池文件，版本1.0-2.0
        /// </summary>
        /// <param name="json"></param>
        /// <param name="cardPoolID"></param>
        /// <returns></returns>
        CardPool parseCardPool_v2(string json, long cardPoolID)
        {
            SerializableCardPool serializableCardPool = JsonHelper.fromJson<SerializableCardPool>(json);

            ActionDefineFinder defineFinder = name => game.getActionDefine(name, cardPoolID);
            EventTypeInfoFinder eventFinder = EventTypeInfoLoader.FindEventTypeInfo;
            TypeFinder typeFinder = game.editor.getTypeByName;

            CardPool cardPool = serializableCardPool.toCardPool(defineFinder, eventFinder, typeFinder);
            for (int i = 0; i < cardPool.cardDefineList.Count; i++)
            {
                cardPool.cardDefineList[i].cardPoolId = cardPoolID;
            }
            for (int i = 0; i < cardPool.buffDefineList.Count; i++)
            {
                cardPool.buffDefineList[i].cardPoolId = cardPoolID;
            }
            return cardPool;
        }

        /// <summary>
        /// 解析卡池文件，版本0.3-1.0
        /// </summary>
        /// <param name="json"></param>
        /// <param name="cardPoolID"></param>
        /// <returns></returns>
        CardPool parseCardPool_v1(string json, long cardPoolID)
        {
            SerializableCardPool serializableCardPool = JsonHelper.fromJson<SerializableCardPool>(json);

            ActionDefineFinder defineFinder = name => game.getActionDefine(name, cardPoolID);
            EventTypeInfoFinder eventFinder = EventTypeInfoLoader.FindEventTypeInfo;
            TypeFinder typeFinder = game.editor.getTypeByName;

            var convertor = new CardPoolConvertor(cardPoolID, serializableCardPool, defineFinder, eventFinder, typeFinder);
            return convertor.Convert();
        }


        /// <summary>
        /// 解析卡池文件，版本0.1-0.2
        /// </summary>
        /// <param name="json"></param>
        /// <param name="cardPoolID"></param>
        /// <returns></returns>
        CardPool parseCardPool_v0_12(string json)
        {
            var cardPool = JsonHelper.fromJson<CardPool>(json);
            return cardPool;
        }

        /// <summary>
        /// 解析卡池文件，版本0.0
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        CardPool parseCardPool_v0_0(string[] lines)
        {
            var cardPool = new CardPool();

            //卡牌
            if (lines.Length > 0)
            {
                var json = lines[0];
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        cardPool.cardDefineList.AddRange(JsonHelper.fromJson<CardDefine[]>(json));
                    }
                    catch (Exception e)
                    {
                        throw new FormatException("反序列化卡池卡牌失败：" + e, e);
                    }
                }
            }
            //卡牌皮肤
            if (lines.Length > 1)
            {
                var json = lines[1];
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        cardPool.cardSkinList.AddRange(JsonHelper.fromJson<CardSkinData[]>(json));
                    }
                    catch (Exception e)
                    {
                        throw new FormatException("反序列化卡池卡牌皮肤失败：" + e, e);
                    }
                }
            }
            //增益
            if (lines.Length > 3)
            {
                var json = lines[3];
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        cardPool.buffDefineList.AddRange(JsonHelper.fromJson<GeneratedBuffDefine[]>(json));
                    }
                    catch (Exception e)
                    {
                        throw new FormatException("反序列化卡池增益失败：" + e, e);
                    }
                }
            }
            //增益皮肤
            if (lines.Length > 4)
            {
                var json = lines[4];
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        cardPool.buffSkinList.AddRange(JsonHelper.fromJson<BuffSkin[]>(json));
                    }
                    catch (Exception e)
                    {
                        throw new FormatException("反序列化卡池增益皮肤失败：" + e, e);
                    }
                }
            }
            return cardPool;
        }
        [Obsolete("Use parseCardPool_v1 and addCardPool instead.")]
        private async Task<CardPool> loadCardPool_v1(string path, CardPoolInfo cardPoolInfo)
        {
            string json = path.StartsWith("sa:") ?
                await game.fileManager.readStreamingTextFileToEnd(path, 1) :
                await game.fileManager.readTextFileToEnd(path, 1);
            CardPool cardPool;
            try
            {
                cardPool = parseCardPool_v1(json, cardPoolInfo.cardPoolId);
            }
            catch (Exception e)
            {
                throw new FormatException("反序列化卡池" + path + "失败", e);
            }
            addCardPool(cardPool, cardPoolInfo, path);
            return cardPool;
        }
        private async Task fixCardPool(CardPoolInfo cardPoolInfo, CardPool cardPool)
        {

            bool hasUnsavedFix = false;
            void fixData(Node node)
            {
                if (node is ActionNode action)
                {
                    fixActionNodeIncorrectData(action);
                }
            }
            Dictionary<string, ActionDefine> actionDefineDict = game.actionDefineDict;

            // 遍历所有节点图并修复信息。
            traverseActionGraphs(cardPool,
                (c, e) => effectAction(e),
                (b, e) => effectAction(e),
                defineAction);


            foreach (var skin in cardPool.cardSkinList)
            {
                if (string.IsNullOrEmpty(skin.language))
                {
                    //皮肤不能没有对应语言信息，如果没有那么至少设置为默认语言
                    skin.language = game.langManager.getDefaultLanguage();
                }
                if (!string.IsNullOrEmpty(skin.imagePath) && !isHashNamedImage(skin.imagePath))
                {
                    //图片地址必须是32位哈希.jpg
                    Sprite sprite = await loadImage(skin.imagePath, cardPoolInfo.cardPoolId, false);
                    // 不保存丢失的贴图。
                    if (sprite != null)
                    {
                        skin.imagePath = await saveImage(cardPoolInfo.cardPoolId, sprite.texture);
                        hasUnsavedFix = true;
                    }
                }
            }
            foreach (var buffSkin in cardPool.buffSkinList)
            {
                if (string.IsNullOrEmpty(buffSkin.imagePath))
                    continue;
                if (!isHashNamedImage(buffSkin.imagePath))
                {
                    //图片地址必须是32位哈希.jpg
                    Sprite sprite = await loadImage(buffSkin.imagePath, cardPoolInfo.cardPoolId, false);
                    // 不保存丢失的贴图。
                    if (sprite != null)
                    {
                        buffSkin.imagePath = await saveImage(cardPoolInfo.cardPoolId, sprite.texture);
                        hasUnsavedFix = true;
                    }
                }
            }
            if (hasUnsavedFix)
                await saveCardPool(cardPoolInfo);



            #region 内嵌方法
            // 用于遍历卡牌/增益效果的方法。
            void effectAction(GeneratedEffect effect)
            {
                if (effect is GeneratedActiveEffect active)
                {
                    fixActiveEntryIncorrectData(active.entryNode);
                }
                effect.traverseActionNode(fixData);
            }
            // 用于遍历自定义动作的方法。
            void defineAction(GeneratedActionDefine define)
            {
                define.traverse(fixData);
            }

            void fixActionNodeIncorrectData(ActionNode actionNode)
            {
                if (!actionDefineDict.ContainsKey(actionNode.defineName) && !cardPool.actionDefineList.Exists(a => a.defineName == actionNode.defineName))
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
                foreach (var constPair in actionNode.consts)
                {
                    if (constPair.Value is CardReference cardRef)
                    {
                        //CardReference过期，使用DefineReference替代
                        actionNode.setConst(constPair.Key, new DefineReference(cardRef.cardPoolId, cardRef.cardId));
                    }
                }
                // 将旧版的“获取增益定义”的ID从int变为DefineReference。
                if (actionNode.defineName == GetBuffDefineAction.defName)
                {
                    var constValue = actionNode.getConst(GetBuffDefineAction.constName);
                    if (constValue is int buffId)
                    {
                        actionNode.setConst(GetBuffDefineAction.constName, new DefineReference(cardPoolInfo.cardPoolId, buffId));
                    }
                }
            }
            void fixActiveEntryIncorrectData(ActiveEntryNode activeEntry)
            {
                foreach (var targetChecker in activeEntry.targetCheckerList)
                {
                    if (string.IsNullOrEmpty(targetChecker.targetType))
                    {
                        //目标的类型不能为空，如果为空则默认为仆从
                        targetChecker.targetType = DEFINE.TARGET_TYPE_SERVANT;
                    }
                }
            }
            #endregion
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

        /// <summary>
        /// 建立需要的卡池文件夹
        /// </summary>
        void prepareCardpoolDir()
        {
            var cardPoolDir = Path.Combine(Application.persistentDataPath, cardPoolDirName);
            if (!Directory.Exists(cardPoolDir))
                Directory.CreateDirectory(cardPoolDir);

            var picDir = Path.Combine(Application.persistentDataPath, textureDirName);
            if (!Directory.Exists(picDir))
                Directory.CreateDirectory(picDir);
        }

        private void traverseActionGraphs(CardPool cardPool, Action<CardDefine, GeneratedEffect> cardAction,
            Action<BuffDefine, GeneratedEffect> buffAction, 
            Action<GeneratedActionDefine> defineAction)
        {
            foreach (var card in cardPool.cardDefineList)
            {
                foreach (GeneratedEffect effect in card.getGeneratedEffects())
                {
                    cardAction?.Invoke(card, effect);
                }
            }
            foreach (var buff in cardPool.buffDefineList)
            {
                foreach (var effect in buff.effectList)
                {
                    buffAction?.Invoke(buff, effect);
                }
            }
            foreach (var actionDefine in cardPool.actionDefineList)
            {
                defineAction?.Invoke(actionDefine);
            }
        }
        private void traverseActionNodes(CardPool cardPool, Action<Node> action)
        {
            Action<CardDefine, GeneratedEffect> cardAction = (card, effect) =>
            {
                effect.traverseActionNode(action);
            };
            Action<BuffDefine, GeneratedEffect> buffAction = (buff, effect) =>
            {
                effect.traverseActionNode(action);
            };
            Action<GeneratedActionDefine> defineAction = (define) =>
            {
                define.traverse(action);
            };
            traverseActionGraphs(cardPool, cardAction, buffAction, defineAction);
        }
        #endregion
        #region 属性字段
        [TranslateMsg]
        public const string CARD_POOL_LOAD_ERROR_DESC = "卡池加载失败，卡池文件可能已损坏。";
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
        Dictionary<ICardPoolReference, CardPool> _cardPoolDict = new Dictionary<ICardPoolReference, CardPool>();
        private long _creatingCardPoolId = -1;
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
        const int CARD_POOL_VERSION_2 = 2;
        const int CURRENT_CARD_POOL_VERSION = CARD_POOL_VERSION_2;
        public static readonly CardPoolReference builtinCardPoolReference = new CardPoolReference(0, 0);
        #endregion
    }
}
