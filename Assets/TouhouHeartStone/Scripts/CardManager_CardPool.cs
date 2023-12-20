using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Controllers;
using MongoDB.Bson;
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

        #region 创建卡池
        /// <summary>
        /// 创建一个卡池。被创建的卡池不会立即被加入到CardPoolInfo的字典中，需要保存才能获取这个Info。不影响通过ID或引用获取卡池内容。
        /// </summary>
        /// <param name="cardPoolInfo">生成的卡池信息。</param>
        /// <param name="cardPool">卡池内容。</param>
        public void createCardPool(out CardPoolInfo cardPoolInfo, out CardPool cardPool)
        {
            long cardPoolId = game.NewSnowflakeId();
            cardPoolInfo = new CardPoolInfo();
            cardPoolInfo.cardPoolId = cardPoolId;
            cardPoolInfo.cardPoolName = StringTable.EDITOR_NEW_CARDPOOL_DEFAULT_NAME;
            cardPoolInfo.imageResPath = null;
            cardPoolInfo.versionNumber = CURRENT_CARD_POOL_VERSION;
            cardPool = new CardPool();
            CardPoolReference cardPoolRef = new CardPoolReference(cardPoolId, 0);
            _cardPoolDict[cardPoolRef] = cardPool;
            _creatingCardPoolInfo = cardPoolInfo;
        }
        /// <summary>
        /// 是否正在创建卡池。正在创建的卡池无法通过ID或引用获取CardPoolInfo。
        /// </summary>
        /// <param name="cardPoolId"></param>
        /// <returns></returns>
        public bool isCreatingCardPool(long cardPoolId)
        {
            if (_creatingCardPoolInfo == null)
                return false;
            return _creatingCardPoolInfo.cardPoolId == cardPoolId;
        }
        /// <summary>
        /// 完成创建卡池。只会删掉目前创建的卡池Info引用，需要保存卡池才能将Info加入字典。
        /// </summary>
        public void finishCreatingCardPool()
        {
            if (_creatingCardPoolInfo == null)
                return;
            _creatingCardPoolInfo = null;
        }
        /// <summary>
        /// 删除正在创建的卡池及其卡池内容。
        /// </summary>
        public void deleteCreatingCardPool()
        {
            if (_creatingCardPoolInfo == null)
                return;
            var cardPoolRefs = _cardPoolDict.Keys.Where(p => p.CompareCardPool(_creatingCardPoolInfo)).ToArray();
            foreach (var cardPoolRef in cardPoolRefs)
            {
                _cardPoolDict.Remove(cardPoolRef);
            }
            _creatingCardPoolInfo = null;
        }
        #endregion

        #region 保存卡池
        /// <summary>
        /// 保存卡池，如果卡池没有被保存过那么会被分配一个新的地址，否则会覆盖已经保存的文件。
        /// </summary>
        /// <param name="cardPoolInfo"></param>
        /// <returns>是否成功保存卡池。</returns>
        public Task<bool> saveCardPool(CardPoolInfo cardPoolInfo)
        {
            //如果当前的卡池没有被保存过，没有分配地址，则给它分配一个地址。
            string cardPoolPath = null;
            if (cardPoolInfo == null) { return Task.FromResult(false); }//如果没有保存需求就跳过这个流程
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
            return saveCardPool(cardPoolInfo, cardPoolPath);
        }
        /// <summary>
        /// 按指定的路径保存卡池。
        /// </summary>
        /// <param name="cardPoolInfo">卡池信息。</param>
        /// <param name="path">路径。</param>
        /// <returns>是否成功保存卡池。</returns>
        public Task<bool> saveCardPool(CardPoolInfo cardPoolInfo, string path)
        {
            var cardPoolRef = cardPoolInfo.ToReference();
            var cardPool = _cardPoolDict[cardPoolRef];
            return saveCardPool(cardPoolInfo, cardPool, path);
        }
        /// <summary>
        /// 按指定的路径和内容保存卡池。
        /// </summary>
        /// <param name="cardPoolInfo">卡池信息。</param>
        /// <param name="cardPool">卡池内容。</param>
        /// <param name="path">路径。</param>
        /// <returns>是否成功保存卡池。</returns>
        public async Task<bool> saveCardPool(CardPoolInfo cardPoolInfo, CardPool cardPool, string path)
        {
            if (!Application.isEditor && isInnateCardPool(cardPoolInfo.cardPoolId))
            {
                //不能修改标准卡池，虽然其实玩家想改也没人能阻止就是了，最多是和别人玩会失去同步吧。
                game.ui.dialog.ShowConfirm(StringTable.INVALID_OPERATION, StringTable.EDITOR_DIALOG_CANNOT_MODIFY_STANDARD_CARDPOOL, null);
                return false;
            }
            var cardPoolRef = cardPoolInfo.ToReference();
            // 获取卡池中对其他卡池的卡牌的引用。
            // TODO: 在被引用的卡池有新版本后，卡池保存时会提示玩家需要将卡池引用修改到新版本。
            cardPoolInfo.dependencyRefs = evaluateLocalCardPoolDependencies(cardPoolInfo.cardPoolId);
            // 如果该卡池正在被创建，将正在创建的卡池设为空。
            if (_creatingCardPoolInfo == cardPoolInfo)
                finishCreatingCardPool();


            if (isInnateCardPool(cardPoolInfo))
            {
                //官方卡池只能保存到指定路径下面，也就是工程文件
                path = Path.Combine(editorCardPoolDirName, $"{cardPoolInfo.cardPoolName}.{cardPoolFilePostfix}");
            }
            else if (path.isStreamingAssetsPath(out var streamingPath))
            {
                //如果是StreamingAssets，那要转存成可持续化文件
                path = streamingPath;
            }
            // 修改卡池的版本，并使卡池编辑次数+1。
            cardPoolInfo.editTime++;
            cardPoolInfo.version = 0;
            cardPoolInfo.versionNumber = CURRENT_CARD_POOL_VERSION;

            // 更新字典中的卡池信息和卡池内容。
            _cardPoolInfoDict[path] = cardPoolInfo;
            _cardPoolDict.Remove(cardPoolRef);
            _cardPoolDict.Add(cardPoolInfo.ToReference(), cardPool);

            // 检查并修复老旧大卡图
            await fixLargeImage(cardPoolInfo, cardPool);
            // 对缓存的图片进行保存
            await saveExternalImages(cardPoolInfo, cardPool);

            //初始化JSON
            if (!JsonHelper.isInited)
            {
                JsonHelper.init();
            }
            bool readFriendly;
#if UNITY_EDITOR
            readFriendly = true;
#else
            readFriendly = false;
#endif
            // 打成序列化对象。
            SerializableCardPoolInfo seriCardPoolInfo = new SerializableCardPoolInfo(cardPoolInfo);
            SerializableCardPool seriCardPool = new SerializableCardPool(cardPool);
            //保存文件
            await game.fileManager.saveFile(path, new string[]
            {
                seriCardPoolInfo.toJson(),//文件头信息
                seriCardPool.toJson(readFriendly)//内容对象
            });
            Debug.Log($"保存卡池{path}成功");
            return true;
        }
        #endregion

        #region 读取卡池
        /// <summary>
        /// 读取所有卡池信息。可能的位置有自定义卡池路径，编辑器下的ExternalData，以及非编辑器下的StreamingAssets。
        /// </summary>
        /// <param name="onCardPoolInfoRead"></param>
        /// <returns>Key为路径，Value为信息</returns>
        public async Task<Dictionary<string, CardPoolInfo>> loadCardPoolInfosAsync()
        {
            Debug.Log("开始读取卡池信息");
            _cardPoolInfoDict.Clear();

            // 获取所有路径。
            string[] fileList = getCardPoolsPath();

            // 开始读取。
            for (int i = 0; i < fileList.Length; i++)
            {
                string path = fileList[i];
                await loadCardPoolInfoAsync(path);
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

            Debug.Log($"卡池信息读取完毕，共{fileList.Length}个卡池");
            return new Dictionary<string, CardPoolInfo>(_cardPoolInfoDict);
        }
        /// <summary>
        /// 读取某个路径上的卡池信息。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="onCardPoolInfoRead"></param>
        /// <returns></returns>
        public async Task<CardPoolInfo> loadCardPoolInfoAsync(string path)
        {
            try
            {
                // 读取文件头作为卡池信息。
                Debug.Log($"开始读取卡池信息{path}");
                string[] lines;
                if (path.isStreamingAssetsPath(out var relativePath))
                    lines = await game.fileManager.readStreamingTextFile(relativePath, lineCount: 1);
                else
                    lines = await game.fileManager.readTextFile(path, lineCount: 1);

                // 卡池信息为空。
                if (lines == null || lines.Length < 1)
                {
                    Debug.LogWarning($"读取卡池信息{path}为空");
                    return null;
                }

                // 解析卡池信息。
                CardPoolInfo cardPool = parseCardPoolInfo(lines[0]);

                // 移除冲突的卡池信息，并添加卡池信息。
                removeConflictCardPoolInfo(cardPool);
                _cardPoolInfoDict.Add(path, cardPool);
                Debug.Log($"读取卡池信息{path}完毕");
                return cardPool;
            }
            catch (Exception e)
            {
                Debug.LogError($"读取卡池信息{path}失败：{e}");
            }
            return null;
        }
        /// <summary>
        /// 从目标路径上加载卡池
        /// </summary>
        /// <param name="path">流文件路径或者绝对路径</param>
        /// <param name="handleException">是否要自动处理加载卡池时的异常，不向上层抛出。</param>
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
                // 读取文件。
                string[] lines;
                if (path.isStreamingAssetsPath(out _))
                    lines = await game.fileManager.readStreamingTextFile(path, 0, 0);
                else
                    lines = await game.fileManager.readTextFile(path, 0, 0);

                // 读取文件失败。
                if (lines == null || lines.Length < 1)
                    throw new FileLoadException($"卡池文件{path}为空");

                // 解析卡池。
                CardPoolInfo cardPoolInfo;
                CardPool cardPool;
                try
                {
                    (cardPoolInfo, cardPool) = parseCardPool(lines);
                }
                catch (Exception e)
                {
                    throw new FormatException($"反序列化卡池{path}失败：{e}", e);
                }

                // 移除冲突的卡池，并添加该卡池。
                removeConflictCardPoolInfo(cardPoolInfo);
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
        /// 从缓存文件夹中读取从网络上下载的卡池。
        /// </summary>
        /// <param name="poolRef">卡池引用。</param>
        /// <returns></returns>
        public Task<(CardPoolInfo info, CardPool cardPool)> loadCachedCardPoolAsync(ICardPoolReference poolRef)
        {
            return loadCachedCardPoolAsync(poolRef.CardPoolID, poolRef.CardPoolVersion);
        }
        /// <summary>
        /// 从缓存文件夹中读取从网络上下载的卡池。
        /// </summary>
        /// <param name="cardPoolId">卡池ID。</param>
        /// <param name="version">卡池版本。</param>
        /// <returns></returns>
        public async Task<(CardPoolInfo info, CardPool cardPool)> loadCachedCardPoolAsync(long cardPoolId, int version)
        {
            string resID = CardPoolReference.GetResourceID(cardPoolId, version);
            if (game.cache.FileExists(game.fileManager, ResourceType.CardDefine, resID))
            {
                string path = game.cache.GetCachePath(ResourceType.CardDefine, resID);
                var cardPoolData = await loadCardPool(path, true);
                return cardPoolData;
            }
            return (null, null);
        }
        #endregion

        #region 移除卡池
        public bool removeCardPool(ICardPoolReference cardPoolRef)
        {
            string path = _cardPoolInfoDict.FirstOrDefault(p => p.Value.CompareCardPool(cardPoolRef)).Key;
            if (path != null)
            {
                _cardPoolDict.Remove(cardPoolRef);
                _cardPoolInfoDict.Remove(path);
                game.fileManager.deleteFile(path);
                return true;
            }
            else
                return false;
        }
        #endregion

        #region 获取卡池
        /// <summary>
        /// 获取所有卡池的路径。可能的位置有自定义卡池路径，编辑器下的ExternalData，以及非编辑器下的StreamingAssets。
        /// </summary>
        /// <returns></returns>
        public string[] getCardPoolsPath()
        {
            //自定义卡池，C盘的完整地址或者是Android的Data地址
            string searchPattern = string.Intern($"*.{_cardPoolFilePostfix}");
            List<string> fileList = new List<string>(game.fileManager.getPersistentFiles(_cardPoolDirName, searchPattern, true));
            if (Application.isEditor)
            {
                //编辑器下的官方卡池，位于项目文件夹下的ExternalData中
                string[] files = game.fileManager.getFiles(Path.GetFullPath(_editorCardPoolDirName), searchPattern);
                fileList.AddRange(files);
                Debug.Log($"获取编辑器卡池：\r\n{string.Join(",\r\n", files)}");
            }
            if (!Application.isEditor || _enableStreamingAssetsInEditor)
            {
                //官方卡池，以sa:开头的StreamingAssets
                string[] files = game.fileManager.getStreamingFiles(_cardPoolDirName, searchPattern);
                fileList.AddRange(files);
                Debug.Log($"获取官方卡池：\r\n{string.Join(",\r\n", files)}");
            }
            return fileList.ToArray();
        }
        /// <summary>
        /// 根据卡池路径获取卡池。如果卡池没有被加载，加载卡池。
        /// </summary>
        /// <param name="path">卡池路径。</param>
        /// <param name="handleException">是否在方法内就捕获异常。</param>
        /// <returns></returns>
        public async Task<(CardPoolInfo cardPoolInfo, CardPool cardPool)> getOrLoadCardPool(string path, bool handleException = false)
        {
            var poolRef = getCardPoolInfo(path);
            if (tryGetCardPool(poolRef, out var cardPool) && _cardPoolInfoDict.TryGetValue(path, out CardPoolInfo cardPoolInfo))
            {
                return (cardPoolInfo, cardPool);
            }

            if (path is string)
            {
                return await loadCardPool(path, handleException);
            }
            return (null, null);
        }
        /// <summary>
        /// 根据卡池引用获取卡池。如果卡池没有被加载，加载卡池。
        /// </summary>
        /// <param name="path">卡池引用。</param>
        /// <param name="handleException">是否在方法内就捕获异常。</param>
        /// <returns></returns>
        public async Task<(CardPoolInfo cardPoolInfo, CardPool cardPool)> getOrLoadCardPool(ICardPoolReference poolRef, bool handleException = false)
        {
            if (tryGetCardPool(poolRef, out var cardPool))
            {
                return (getCardPoolInfo(poolRef), cardPool);
            }

            var path = getCardPoolPath(poolRef);
            if (path is string)
            {
                return await loadCardPool(path, handleException);
            }
            var cache = await loadCachedCardPoolAsync(poolRef);
            if (cache.info != null && cache.cardPool != null)
            {
                return cache;
            }
            return (null, null);
        }
        public CardPool getLocalCardPool(long cardPoolId)
        {
            var poolRef = getLocalCardPoolReference(cardPoolId);
            return getCardPool(poolRef);
        }
        public CardPool getCardPool(ICardPoolReference cardPoolRef)
        {
            if (cardPoolRef == null)
                return null;
            if (tryGetCardPool(cardPoolRef, out var cardPool))
            {
                return cardPool;
            }
            return null;
        }
        public bool tryGetCardPool(ICardPoolReference cardPoolRef, out CardPool cardPool)
        {
            if (cardPoolRef == null)
            {
                cardPool = null;
                return false;
            }
            cardPool = _cardPoolDict.FirstOrDefault(p => cardPoolRef.CompareCardPool(p.Key)).Value;
            return cardPool != null;
        }
        public bool tryGetLocalCardPool(long cardPoolId, out CardPool cardPool)
        {
            var poolRef = getLocalCardPoolReference(cardPoolId);
            return tryGetCardPool(poolRef, out cardPool);
        }
        /// <summary>
        /// 根据卡池引用获取卡池路径（结果为第一个符合ID的卡池）。
        /// </summary>
        /// <param name="cardPoolRef">卡池引用。</param>
        /// <returns>卡池路径。</returns>
        public string getCardPoolPath(ICardPoolReference cardPoolRef)
        {
            if (cardPoolRef == null)
                return null;
            return _cardPoolInfoDict.FirstOrDefault(p => p.Value.CompareCardPool(cardPoolRef)).Key;
        }
        public CardPoolInfo getCardPoolInfo(string path)
        {
            if (_cardPoolInfoDict.TryGetValue(path, out var info))
            {
                return info;
            }
            return null;
        }
        public CardPoolInfo getCardPoolInfo(ICardPoolReference cardPoolRef)
        {
            if (cardPoolRef == null)
                return null;
            var cardPool = _cardPoolInfoDict.Values.FirstOrDefault(p => p.CompareCardPool(cardPoolRef));
            if (cardPool != null)
                return cardPool;
            if (_creatingCardPoolInfo != null && _creatingCardPoolInfo.CompareCardPool(cardPoolRef))
                return _creatingCardPoolInfo;
            return null;
        }
        public CardPoolInfo getLocalCardPoolInfo(long cardPoolId)
        {
            var poolRef = getLocalCardPoolReference(cardPoolId);
            return getCardPoolInfo(poolRef);
        }
        public static string[] getCardPoolImages(CardPoolInfo info, CardPool cardPool)
        {
            HashSet<string> imageFiles = new HashSet<string>();
            var resPath = info.imageResPath;
            if (!string.IsNullOrEmpty(resPath) && !resPath.isStreamingAssetsPath(out _) && !resPath.isResourcesPath(out _))
            {
                imageFiles.Add(info.imageResPath);
            }
            foreach (var item in cardPool.cardSkinList)
            {
                // 不导出内置资源
                var path = item.imagePath;
                if (string.IsNullOrEmpty(path) || path.isStreamingAssetsPath(out _) || path.isResourcesPath(out _))
                    continue;
                imageFiles.Add(path);
            }
            foreach (var item in cardPool.buffSkinList)
            {
                // 不导出内置资源
                var path = item.imagePath;
                if (string.IsNullOrEmpty(path) || path.isStreamingAssetsPath(out _) || path.isResourcesPath(out _))
                    continue;
                imageFiles.Add(path);
            }

            return imageFiles.ToArray();
        }
        /// <summary>
        /// 检测是否加载了某个卡池及其版本。
        /// </summary>
        /// <param name="includingCache">是否包括缓存内的卡池。</param>
        /// <returns></returns>
        public bool isCardPoolLoaded(ICardPoolReference poolRef, bool includingCache = false)
        {
            if (isBuiltinCardPool(poolRef.CardPoolID) || isInnateCardPool(poolRef.CardPoolID))
                return true;
            if (!includingCache && !isLocalCardPool(poolRef))
            {
                return false;
            }
            var cardPoolInfo = getCardPoolInfo(poolRef);
            return cardPoolInfo != null;
        }
        #endregion

        #region 获取卡池引用
        public ICardPoolReference getCardPoolReferenceOfBuff(BuffDefine buff)
        {
            if (buff == null)
                return null;
            return _cardPoolDict.FirstOrDefault(p => p.Key.CardPoolID == buff.cardPoolId && p.Value.buffDefineList.Contains(buff)).Key;
        }
        public ICardPoolReference getCardPoolReferenceOfCard(CardDefine card)
        {
            if (card == null)
                return null;
            return _cardPoolDict.FirstOrDefault(p => p.Key.CardPoolID == card.cardPoolId && p.Value.cardDefineList.Contains(card)).Key;
        }

        /// <summary>
        /// 在一个卡池引用的集合中，根据卡池的ID获取指定的卡池引用。
        /// 多用于根据卡池ID获取某个房间内允许的卡池。
        /// </summary>
        /// <param name="cardPoolId">卡池ID。</param>
        /// <param name="set">卡池引用集合。</param>
        /// <returns>获取到的卡池引用。</returns>
        public ICardPoolReference getCardPoolReferenceInSet(long cardPoolId, IEnumerable<ICardPoolReference> set)
        {
            // 集合不存在，返回null。
            if (set == null)
                return null;
            // 内置卡池，返回内置卡池引用。
            if (isBuiltinCardPool(cardPoolId))
                return builtinCardPoolReference;

            // 查找集合内的卡池引用。
            var pool = set.FirstOrDefault(p => p.CardPoolID == cardPoolId);
            if (pool != null)
                return pool.ToReference();

            // 没有找到。
            return null;
        }
        /// <summary>
        /// 根据卡池ID获取本地的卡池引用（不会获取缓存中的的卡池）。
        /// </summary>
        /// <param name="cardPoolId">卡池ID。</param>
        /// <returns></returns>
        public ICardPoolReference getLocalCardPoolReference(long cardPoolId)
        {
            // 正在创建的卡池，返回一个新的引用。
            if (isCreatingCardPool(cardPoolId))
                return new CardPoolReference(cardPoolId, 0);

            // 内置卡池，返回内置卡池引用。
            if (isBuiltinCardPool(cardPoolId))
                return builtinCardPoolReference;

            // 寻找本地的卡池。
            var info = _cardPoolInfoDict.Values.FirstOrDefault(p => p.cardPoolId == cardPoolId && isLocalCardPool(p));
            // 返回引用。
            return info?.ToReference();
        }
        /// <summary>
        /// 获取所有本地卡池。
        /// </summary>
        /// <returns></returns>
        public ICardPoolReference[] getLocalCardPoolReferences()
        {
            List<ICardPoolReference> set = new List<ICardPoolReference>();
            if (_creatingCardPoolInfo != null)
                set.Add(new CardPoolReference(_creatingCardPoolInfo.cardPoolId, 0));
            set.AddRange(_cardPoolInfoDict.Values.Where(p => isLocalCardPool(p)).Select(p => p.ToReference()));
            return set.ToArray();
        }
        #endregion

        #region 卡池解析
        /// <summary>
        /// 获取卡池内的所有包含过时节点的节点图。
        /// </summary>
        /// <returns>过时节点节点图列表。</returns>
        public (int cardId, int buffId, int actionId)[] getObsoleteNodeGraphs(CardPool cardPool)
        {
            HashSet<(int cardId, int buffId, int actionId)> list = new HashSet<(int cardId, int buffId, int actionId)>();
            void checkObsoleteNodes(Node node, (int cardId, int buffId, int actionId) graphObj)
            {
                if (node is not ActionNode actionNode)
                    return;
                ActionDefine define = getLocalActionDefine(actionNode.defineRef);
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
            try
            {
                var seri = JsonHelper.fromJson<SerializableCardPoolInfo>(line);
                return seri.toInfo();
            }
            catch (BsonSerializationException)
            {
                return JsonHelper.fromJson<CardPoolInfo>(line);
            }
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
        #endregion

        #region 卡池属性检测
        public bool isBuiltinCardPool(ICardPoolReference cardPoolRef)
        {
            return isBuiltinCardPool(cardPoolRef.CardPoolID);
        }
        public bool isBuiltinCardPool(long cardPoolId)
        {
            return cardPoolId == 0;
        }
        /// <summary>
        /// 是否是游戏自带的卡池。
        /// </summary>
        /// <param name="cardPoolRef"></param>
        /// <returns></returns>
        public bool isInnateCardPool(ICardPoolReference cardPoolRef)
        {
            return isInnateCardPool(cardPoolRef.CardPoolID);
        }
        /// <summary>
        /// 是否是游戏自带的卡池。
        /// </summary>
        /// <param name="cardPoolId"></param>
        /// <returns></returns>
        public bool isInnateCardPool(long cardPoolId)
        {
            return _standardCardPoolsId.Contains(cardPoolId);
        }
        public bool isCardPoolHidden(long cardPoolId)
        {
#if UNITY_EDITOR
            return false;
#else
            return _hiddenCardPoolsId.Contains(cardPoolId);
#endif
        }
        /// <summary>
        /// 判断卡池路径是否是本地卡池（即内置、经典卡池，或卡池文件位于普通卡池目录内，而不属于缓存的卡池）。
        /// </summary>
        /// <param name="path">卡池路径。</param>
        /// <returns>是否是本地卡池？</returns>
        public bool isLocalCardPoolPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            // StreamingAssets或Resources。
            path = path.Replace("/", "\\");
            if (path.isStreamingAssetsPath(out _) || path.isResourcesPath(out _))
                return true;

            // 如果路径以本地卡池的路径开头，说明是本身拥有的卡池。
            string cardPoolDir = Path.Combine(Application.persistentDataPath, cardPoolDirName).Replace("/", "\\");
            if (path.StartsWith(cardPoolDirName) || path.StartsWith(cardPoolDir))
                return true;

            // PVE
            string pveDir = Path.Combine(Application.persistentDataPath, game.pve.directory).Replace("/", "\\");
            if (path.StartsWith(game.pve.directory) || path.StartsWith(pveDir))
                return true;
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
            if (isBuiltinCardPool(cardPoolRef.CardPoolID) || isInnateCardPool(cardPoolRef.CardPoolID))
                return true;
            if (isCreatingCardPool(cardPoolRef.CardPoolID))
                return true;
            string path = getCardPoolPath(cardPoolRef);
            return isLocalCardPoolPath(path);
        }
        #endregion

        /// <summary>
        /// 重新获取一个本地卡池内对其他卡池的引用。
        /// </summary>
        /// <param name="cardPoolRef">要检查的卡池ID。</param>
        /// <returns>引用的其他卡池列表。</returns>
        public CardPoolReference[] evaluateLocalCardPoolDependencies(long cardPoolId)
        {
            HashSet<CardPoolReference> poolList = new HashSet<CardPoolReference>();
            if (!tryGetLocalCardPool(cardPoolId, out var cardPool))
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
                if (defRef == null)
                    return;
                if (defRef.cardPoolId != cardPoolId)
                {
                    var poolRef = getLocalCardPoolReference(defRef.cardPoolId);
                    var info = getCardPoolInfo(poolRef);
                    if (info != null)
                        poolList.Add(info.ToReference());
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
                foreach (var defVal in node.inputDefaultValues)
                {
                    addCardPoolOfObject(defVal.value);
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
        public void showCardPoolLoadErrorDialog()
        {
            var title = game.lang._(StringTable.ERROR);
            var content = game.lang._(CARD_POOL_LOAD_ERROR_DESC);
            game.ui.dialog.ShowConfirm(title, content);
        }
        #endregion

        #region 私有方法

        #region 生命周期
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

        #region 添加卡池
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
        #endregion

        #region 卡池解析

        /// <summary>
        /// 解析卡池文件，版本1.0-2.0
        /// </summary>
        /// <param name="json"></param>
        /// <param name="cardPoolID"></param>
        /// <returns></returns>
        CardPool parseCardPool_v2(string json, long cardPoolID)
        {
            SerializableCardPool serializableCardPool = JsonHelper.fromJson<SerializableCardPool>(json);

            TypeFinder typeFinder = game.editor.getTypeByName;

            CardPool cardPool = serializableCardPool.toCardPool(cardPoolID, game, typeFinder);
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

            TypeFinder typeFinder = game.editor.getTypeByName;

            var convertor = new CardPoolConvertor(cardPoolID, serializableCardPool, game, typeFinder);
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
                        throw new FormatException($"反序列化卡池卡牌失败：{e}", e);
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
                        throw new FormatException($"反序列化卡池卡牌皮肤失败：{e}", e);
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
                        throw new FormatException($"反序列化卡池增益失败：{e}", e);
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
                        throw new FormatException($"反序列化卡池增益皮肤失败：{e}", e);
                    }
                }
            }
            return cardPool;
        }
        #endregion

        #region 卡池修复
        private async Task fixCardPool(CardPoolInfo cardPoolInfo, CardPool cardPool)
        {
            Dictionary<int, int> massEventNodeMap = new Dictionary<int, int>()
            {
                { BuiltinActionDefineID.Discard_Obs_Mass, BuiltinActionDefineID.Discard },
                { BuiltinActionDefineID.HealTargets_Obs_Mass, BuiltinActionDefineID.HealTargets },
                { BuiltinActionDefineID.Seal_Obs_Mass, BuiltinActionDefineID.Seal },
                { BuiltinActionDefineID.DealDamageOrSpellDamage_Obs_Mass, BuiltinActionDefineID.DealDamageOrSpellDamage },
                { BuiltinActionDefineID.DealDamageSplitAmong_Obs_Mass, BuiltinActionDefineID.DealDamageSplitAmong },
                { BuiltinActionDefineID.DealDamageToRandomTargets_Obs_Mass, BuiltinActionDefineID.DealDamageToRandomTargets },
            };

            bool hasUnsavedFix = false;
            var builtinActions = builtinActionDefines;

            List<ActionGraph> collectedGraphs = new List<ActionGraph>();
            // 遍历所有节点图并修复信息。
            traverseActionGraphs(cardPool,
                (b, e) => effectAction(e),
                (b, e) => effectAction(e),
                defineAction);

            // 修复所有收集到的节点。
            foreach (var graph in collectedGraphs)
            {
                fixGraph(graph);
            }

            foreach (var cardDefine in cardPool.cardDefineList)
            {
                fixCardDefine(cardDefine);
            }
            foreach (var buffDefine in cardPool.buffDefineList)
            {
                fixBuffDefine(buffDefine);
            }

            await fixImages();
            if (hasUnsavedFix)
                await saveCardPool(cardPoolInfo);



            #region 内嵌方法
            async Task fixImages()
            {
                if (!string.IsNullOrEmpty(cardPoolInfo.imageResPath))
                {
                    if (!isHashNamedImage(cardPoolInfo.imageResPath))
                    {
                        //图片地址必须是32位哈希.jpg
                        Sprite sprite = await loadCardPoolImage(cardPoolInfo);
                        // 不保存丢失的贴图。
                        if (sprite != null)
                        {
                            cardPoolInfo.imageResPath = await saveImage(cardPoolInfo.cardPoolId, sprite.texture);
                            hasUnsavedFix = true;
                        }
                    }
                }
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
                        Sprite sprite = await loadImage(cardPoolInfo, skin.imagePath, false);
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
                        Sprite sprite = await loadBuffImage(cardPoolInfo, buffSkin, false);
                        // 不保存丢失的贴图。
                        if (sprite != null)
                        {
                            buffSkin.imagePath = await saveImage(cardPoolInfo.cardPoolId, sprite.texture);
                            hasUnsavedFix = true;
                        }
                    }
                }
            }
            // 用于遍历卡牌/增益效果的方法。
            void effectAction(GeneratedEffect effect)
            {
                if (effect is GeneratedActiveEffect active)
                {
                    fixActiveEntryIncorrectData(active.entryNode);
                }
                collectGraph(effect.graph);
            }
            // 用于遍历自定义动作的方法。
            void defineAction(GeneratedActionDefine define)
            {
                collectGraph(define.graph);
            }
            // 收集节点图，防止在traverse节点的时候因为操作节点图而报错。
            void collectGraph(ActionGraph graph)
            {
                collectedGraphs.Add(graph);
            }

            #region 修复节点信息
            void fixGraph(ActionGraph graph)
            {
                foreach (var node in graph.nodes.ToArray())
                {
                    fixNode(node);
                }
                foreach (var connection in graph.connections.OfType<InvalidNodeConnection>().ToArray())
                {
                    graph.disconnect(connection);
                }
            }
            void fixNode(Node node)
            {
                if (node is ActionNode action)
                {
                    fixActionNodeIncorrectData(action);
                }
            }
            // 修复动作节点的错误。
            async void fixActionNodeIncorrectData(ActionNode actionNode)
            {
                await fixUnknownOrObsoleteNode(actionNode);
                fixMassObsoleteNode(actionNode);
                fixOldReferences(actionNode);
                fixConditionalNode(actionNode);
                fixBuiltinCardRefs(actionNode);
                fixConstsToDefaultValues(actionNode);
            }
            // 修复未知或过时的节点。
            async Task fixUnknownOrObsoleteNode(ActionNode actionNode)
            {
                var defineRef = actionNode.defineRef;
                bool actionExists = false;
                if (defineRef != null)
                {
                    CardPool definePool = null;
                    // 获取目标卡池的动作引用。
                    var localPoolRef = getLocalCardPoolReference(defineRef.cardPoolId);
                    if (!tryGetCardPool(localPoolRef, out definePool))
                    {
                        var path = getCardPoolPath(localPoolRef);
                        if (!string.IsNullOrEmpty(path))
                        {
                            var result = await loadCardPool(path, true);
                            definePool = result.cardPool;
                        }
                    }
                    // 检查动作是否存在。
                    if (defineRef.cardPoolId == 0)
                    {
                        actionExists = builtinActions.ContainsKey(defineRef.defineId);
                    }
                    else if (definePool != null)
                    {
                        actionExists = definePool.actionDefineList.Exists(a => a.defineId == defineRef.defineId);
                    }
                }
                if (!actionExists)
                {
                    //包含不存在的动作名称，可能是名称过期了
                    ActionReference newDefineRef = null;
                    // 内置动作。
                    foreach (ActionDefine actionDefine in builtinActions.Values)
                    {
                        if (actionDefine.obsoleteNames?.Contains(actionNode.defineName) == true)
                        {
                            newDefineRef = new ActionReference(0, actionDefine.defineId);
                            break;
                        }
                    }
                    // 卡池内的自定义动作。
                    if (newDefineRef == null)
                    {
                        foreach (GeneratedActionDefine actionDefine in cardPool.actionDefineList)
                        {
                            if (actionDefine.obsoleteNames?.Contains(actionNode.defineName) == true)
                            {
                                newDefineRef = new ActionReference(actionDefine.cardPoolId, actionDefine.defineId);
                                break;
                            }
                        }
                    }
                    if (newDefineRef != null)
                    {
                        Debug.LogWarning($"将动作节点{actionNode.id}过期的动作名称{actionNode.defineName}替换为{newDefineRef}");
                        actionNode.defineRef = newDefineRef;
                    }
                    else
                    {
                        Debug.LogError($"动作节点{actionNode.id}的名称未知：{actionNode.defineName}");
                    }
                }
            }
            // 修复因加入群体事件而过时的节点。
            void fixMassObsoleteNode(ActionNode actionNode)
            {
                if (actionNode.defineRef == null)
                    return;
                foreach (var pair in massEventNodeMap)
                {
                    // 不是指定的过时动作定义。
                    if (!actionNode.defineRef.isBuiltinReference(pair.Key))
                        continue;
                    // 如果事件数组输出端口有连接，不自动将其修改为新的节点。
                    var outputs = actionNode.getOutputPorts<ValueOutput>();
                    if (outputs.Any(o =>
                    {
                        var type = o?.define?.type;
                        if (type == null) return false; // 不存在类型
                        if (!typeof(IEventArg[]).IsAssignableFrom(o?.define?.type)) return false; // 输出值不是事件数组
                        if (o.connections.Count() <= 0) return false; // 没有任何连接
                        return true;
                    }))
                        continue;
                    // 改为新的节点。
                    actionNode.defineRef = new ActionReference(0, pair.Value);
                }
            }
            // 修复过时的CardReference或者int。
            void fixOldReferences(ActionNode actionNode)
            {
                foreach (var defVal in actionNode.inputDefaultValues)
                {
                    if (defVal.value is CardReference cardRef)
                    {
                        //CardReference过期，使用DefineReference替代
                        actionNode.setInputDefaultValue(defVal.name, defVal.paramIndex, new DefineReference(cardRef.cardPoolId, cardRef.cardId));
                    }
                }
                // 将旧版的“获取增益定义”的ID从int变为DefineReference。
                if (actionNode.defineRef.isBuiltinReference(BuiltinActionDefineID.GetBuffDefine))
                {
                    var constValue = actionNode.getInputDefaultValue(GetBuffDefineAction.constName);
                    if (constValue is int buffId)
                    {
                        actionNode.setInputDefaultValue(GetBuffDefineAction.constName, new DefineReference(cardPoolInfo.cardPoolId, buffId));
                    }
                }
            }
            // 修复主动效果入口节点的错误。
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
            // 将旧版的“筛选”、“排序”、“重复动作直到”和“信仰对决”换为新的节点。
            void fixConditionalNode(ActionNode actionNode)
            {
                if (actionNode.defineRef == null)
                    return;
                // 内置动作
                if (actionNode.defineRef.cardPoolId == 0)
                {
                    int newId = 0;
                    string oldConditionName = null;
                    string newConditionName = null;
                    switch (actionNode.defineRef.defineId)
                    {
                        case BuiltinActionDefineID.Where_Obsolete:
                            newId = BuiltinActionDefineID.Where;
                            oldConditionName = "condition";
                            newConditionName = "condition";
                            break;
                        case BuiltinActionDefineID.OrderBy_Obsolete:
                            newId = BuiltinActionDefineID.OrderBy;
                            oldConditionName = "condition";
                            newConditionName = "condition";
                            break;
                        case BuiltinActionDefineID.DoUntilAction_Obsolete:
                            newId = BuiltinActionDefineID.DoUntilAction;
                            oldConditionName = "condition";
                            newConditionName = "condition";
                            break;
                        case BuiltinActionDefineID.FaithDuel_Obsolete:
                            newId = BuiltinActionDefineID.FaithDuel;
                            oldConditionName = "filter";
                            newConditionName = "filter";
                            break;
                    }

                    if (newId != 0)
                    {
                        // 断开原先的连接。
                        var connectedConditionOutput = actionNode.getOutputPort<ControlOutput>(oldConditionName);
                        var connection = actionNode.graph.getNodeConnections(actionNode).FirstOrDefault(c => c.source.name == oldConditionName);

                        IPort connectedConditionInput = connection?.destination;
                        Node conditionNode = null;
                        if (connectedConditionInput != null)
                        {
                            conditionNode = connectedConditionInput.node;
                        }
                        else if (connection is InvalidNodeConnection invalid)
                        {
                            conditionNode = actionNode.graph.findNodeById(invalid.destNodeId);
                        }
                        IPort connectedConditionFirstOutput = conditionNode?.getOutputPorts<ValueOutput>()?.FirstOrDefault();

                        if (connectedConditionOutput != null && connectedConditionInput != null)
                        {
                            actionNode.graph.disconnect(connectedConditionOutput, connectedConditionInput);
                        }

                        // 更新节点的信息和端口。
                        actionNode.defineRef = new ActionReference(0, newId);
                        var definer = getLocalNodeDefiner();
                        definer.Define(actionNode);

                        // 连接之前的条件节点的第一个输出端口和该节点的条件端口。
                        var newConditionPort = actionNode.getInputPort<ValueInput>(newConditionName);
                        if (newConditionPort != null && connectedConditionFirstOutput != null)
                        {
                            actionNode.graph.connect(connectedConditionFirstOutput, newConditionPort);
                        }
                    }
                }
            }
            // 将内置卡池的卡牌引用改为经典卡池的引用。
            void fixBuiltinCardRefs(ActionNode actionNode)
            {
                var defRef = actionNode.defineRef;
                if (defRef == null)
                    return;
                if (!defRef.isBuiltinReference(BuiltinActionDefineID.GetCardDefine) && !defRef.isBuiltinReference(BuiltinActionDefineID.GetCardDefineArray))
                    return;

                var defVals = actionNode.inputDefaultValues.ToArray();
                foreach (var defVal in defVals)
                {
                    var newValue = convertBuiltinRefToClassic(defVal.value);
                    if (newValue != null)
                        actionNode.setInputDefaultValue(defVal.name, defVal.paramIndex, newValue);
                }
            }
            void fixConstsToDefaultValues(ActionNode action)
            {
                if (action.defineRef == null)
                    return;
                var constValues = EffectEditorLibrary.obsoleteConstNodeDict;
                foreach (var constPair in constValues)
                {
                    if (action.defineRef.isBuiltinReference(constPair.Key))
                    {
                        var output = action.getOutputPorts<ValueOutput>().FirstOrDefault();
                        var constName = constPair.Value;
                        foreach (var connection in output.connections)
                        {
                            var dest = connection.destination;
                            if (dest is not ValueInput vi)
                                continue;
                            dest.node.setInputDefaultValue(vi.name, vi.paramIndex, action.getInputDefaultValue(constName));
                        }
                        action.graph.removeNode(action);
                        break;
                    }
                }
                // 将特效的InputDefaultValue替换为Prop。
                var animRefName = action.getInputDefaultValue<string>(SkinAnimationRef.NODE_CONST_KEY);
                if (animRefName != null)
                {
                    action.removeInputDefaultValue(SkinAnimationRef.NODE_CONST_KEY);
                    action.setProp(SkinAnimationRef.NODE_CONST_KEY, animRefName);
                }
            }
            #endregion

            #region 修复卡牌定义
            void fixCardDefine(CardDefine define)
            {
                fixSecretKeyword(define);
                fixCardDefineBuiltinCardRefs(define);
                foreach (var effect in define.getGeneratedEffects())
                {
                    fixEffect(effect);
                }
            }
            void fixSecretKeyword(CardDefine define)
            {
                // 移除陷阱关键词，并给第一个事件效果加入陷阱标签。
                if (define.hasKeyword(KEYWORD.SECRET))
                {
                    var keywords = define.getKeywords();
                    define.setKeywords(keywords.Where(k => k != KEYWORD.SECRET).ToArray());
                    var firstEvent = define.getGeneratedEffects().OfType<GeneratedEventEffect>().FirstOrDefault();
                    if (firstEvent != null)
                    {
                        var tags = firstEvent.getTags();
                        if (!tags.Contains(EFFECTTAG.EVENT_TRAP))
                        {
                            firstEvent.setTags(tags.Append(EFFECTTAG.EVENT_TRAP).ToArray());
                        }
                    }
                }
            }
            void fixCardDefineBuiltinCardRefs(CardDefine define)
            {
                // 将内置卡池的卡牌引用改为经典卡池的引用。
                var characterRef = define.getCharacterRef();
                if (isBuiltinCardPool(characterRef.cardPoolId) && ClassicCardID.tryGetClassicAlternateOfBuiltin(characterRef.defineId, out var charClassicId))
                {
                    define.setCharacterRef(new DefineReference(STANDARD_POOL_ID, charClassicId));
                }
                var skillRef = define.getSkillRef();
                if (isBuiltinCardPool(skillRef.cardPoolId) && ClassicCardID.tryGetClassicAlternateOfBuiltin(skillRef.defineId, out var skillClassicId))
                {
                    define.setSkillRef(new DefineReference(STANDARD_POOL_ID, skillClassicId));
                }
                foreach (var propName in define.getPropNames())
                {
                    var prop = define.getProp<object>(propName);
                    var newValue = convertBuiltinRefToClassic(prop);
                    if (newValue != null)
                        define.setProp(propName, newValue);
                }
            }
            #endregion

            #region 修复增益定义
            void fixBuffDefine(GeneratedBuffDefine define)
            {
                foreach (var effect in define.effectList)
                {
                    fixEffect(effect);
                }
            }
            #endregion

            #region 修复效果
            void fixEffect(GeneratedEffect effect)
            {
                fixEffectBuiltinCardRefs(effect);
            }
            void fixEffectBuiltinCardRefs(GeneratedEffect effect)
            {
                // 将内置卡池的卡牌引用改为经典卡池的引用。
                foreach (var propName in effect.getPropNames())
                {
                    var prop = effect.getProp<object>(propName);
                    var newValue = convertBuiltinRefToClassic(prop);
                    if (newValue != null)
                        effect.setProp(propName, newValue);
                }
            }
            #endregion

            object convertBuiltinRefToClassic(object value)
            {
                if (value is DefineReference cardRef)
                {
                    if (isBuiltinCardPool(cardRef.cardPoolId) && ClassicCardID.tryGetClassicAlternateOfBuiltin(cardRef.defineId, out var classicId))
                    {
                        return new DefineReference(STANDARD_POOL_ID, classicId);
                    }
                }
                else if (value is IEnumerable<DefineReference> cardRefs)
                {
                    var array = cardRefs.ToArray();
                    bool changed = false;
                    var newArray = new DefineReference[array.Length];
                    for (int i = 0; i < newArray.Length; i++)
                    {
                        var arrayElement = array[i];
                        if (arrayElement == null)
                            continue;
                        if (isBuiltinCardPool(arrayElement.cardPoolId) && ClassicCardID.tryGetClassicAlternateOfBuiltin(arrayElement.defineId, out var classicId))
                        {
                            newArray[i] = new DefineReference(STANDARD_POOL_ID, classicId);
                            changed = true;
                        }
                    }
                    if (changed)
                        return newArray;
                }
                return null;
            }
            #endregion
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
        private void removeConflictCardPoolInfo(CardPoolInfo cardPoolInfo)
        {
            //去重，多份文件中可能包含相同的卡池
            var conflictedKeys = _cardPoolInfoDict.Where(i => i.Value.CompareCardPool(cardPoolInfo)).Select(i => i.Key).ToArray();
            foreach (var key in conflictedKeys)
            {
                _cardPoolInfoDict.Remove(key);
            }
        }
        #endregion

        private string allocCardPoolPath(CardPoolInfo cardPool)
        {
            string path = Path.Combine(Application.persistentDataPath, cardPoolDirName, $"{cardPool.cardPoolName}.{cardPoolFilePostfix}");
            for (int i = 0; i < 100 && File.Exists(path); i++)
            {
                if (i == 99)
                {
                    throw new IOException("达到最大文件存储上限");
                }
                Match match = Regex.Match(path, $@"(?<path>.+)\s\((?<num>\d+)\)\.{cardPoolFilePostfix}");
                if (match.Success)
                {
                    int num = int.Parse(match.Groups["num"].Value);
                    path = path.Replace($"({num})", $"({num + 1})");
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
        private void prepareCardpoolDir()
        {
            var cardPoolDir = Path.Combine(Application.persistentDataPath, cardPoolDirName);
            if (!Directory.Exists(cardPoolDir))
                Directory.CreateDirectory(cardPoolDir);

            var picDir = Path.Combine(Application.persistentDataPath, textureDirName);
            if (!Directory.Exists(picDir))
                Directory.CreateDirectory(picDir);
        }
        #endregion

        #region 属性字段
        [TranslateMsg]
        public const string CARD_POOL_LOAD_ERROR_DESC = "卡池加载失败，卡池文件可能已损坏。";
        public const long STANDARD_POOL_ID = 1;
        public const long PVE_POOL_ID = 2;
        public static readonly CardPoolReference builtinCardPoolReference = new CardPoolReference(0, 0);
        public string cardPoolDirName => _cardPoolDirName;
        public string editorCardPoolDirName => _editorCardPoolDirName;
        public string cardPoolFilePostfix => _cardPoolFilePostfix;
        public long[] standardCardPoolsId => _standardCardPoolsId;
        /// <summary>
        /// 键为文件地址，值为卡池信息
        /// </summary>
        private Dictionary<string, CardPoolInfo> _cardPoolInfoDict = new Dictionary<string, CardPoolInfo>();
        /// <summary>
        /// 键为卡池Id，值为卡池
        /// </summary>
        private Dictionary<ICardPoolReference, CardPool> _cardPoolDict = new Dictionary<ICardPoolReference, CardPool>();
        private CardPoolInfo _creatingCardPoolInfo = null;
        [Header("卡池")]
        [SerializeField]
        private bool _enableStreamingAssetsInEditor = false;
        [SerializeField]
        private string _cardPoolFilePostfix = "ddcp";
        [SerializeField]
        private string _cardPoolDirName = "CardPools";
        [SerializeField]
        private string _editorCardPoolDirName = "ExternalData/CardPools";
        [SerializeField]
        private long[] _standardCardPoolsId = new long[]
        {
            1
        };
        [SerializeField]
        private long[] _hiddenCardPoolsId = new long[]
        {
            PVE_POOL_ID
        };
        private const float CARD_POOL_VERSION_0_3 = 0.3f;
        private const float CARD_POOL_VERSION_0_2 = 0.2f;
        private const float CARD_POOL_VERSION_0_1 = 0.1f;
        private const int CARD_POOL_VERSION_1 = 1;
        private const int CARD_POOL_VERSION_2 = 2;
        private const int CURRENT_CARD_POOL_VERSION = CARD_POOL_VERSION_2;
        #endregion
    }

    public static class PathExtensions
    {
        public static bool isStreamingAssetsPath(this string path, out string relativePath)
        {
            if (path.StartsWith("sa:"))
            {
                relativePath = path.Substring(3, path.Length - 3);
                return true;
            }
            relativePath = null;
            return false;
        }
        public static bool isResourcesPath(this string path, out string relativePath)
        {
            if (path.StartsWith("res:"))
            {
                relativePath = path.Substring(4, path.Length - 4);
                return true;
            }
            relativePath = null;
            return false;
        }
    }
}
