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
            cardPoolInfo.cardPoolName = game.lang._(StringTable.EDITOR_NEW_CARDPOOL_DEFAULT_NAME);
            cardPoolInfo.imageResPath = null;
            cardPoolInfo.versionNumber = CURRENT_CARD_POOL_VERSION;
            cardPool = new CardPool();
            CardPoolReference cardPoolRef = new CardPoolReference(cardPoolId, 0);
            _localCardPoolDict[cardPoolRef] = cardPool;
            _creatingCardPoolInfo = cardPoolInfo;
        }
        /// <summary>
        /// 是否正在创建卡池。正在创建的卡池无法通过ID或引用获取CardPoolInfo。
        /// </summary>
        /// <param name="cardPoolRef"></param>
        /// <returns></returns>
        public bool isCreatingCardPool(ICardPoolReference cardPoolRef)
        {
            if (_creatingCardPoolInfo == null || cardPoolRef == null)
                return false;
            return _creatingCardPoolInfo.CompareCardPool(cardPoolRef);
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
            var cardPoolRefs = _localCardPoolDict.Keys.Where(p => p.CompareCardPool(_creatingCardPoolInfo)).ToArray();
            foreach (var cardPoolRef in cardPoolRefs)
            {
                _localCardPoolDict.Remove(cardPoolRef);
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
            foreach (var pair in _localCardPoolInfoDict)
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
            var cardPool = _localCardPoolDict[cardPoolRef];
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
                var title = game.lang._(StringTable.INVALID_OPERATION);
                var desc = game.lang._(StringTable.EDITOR_DIALOG_CANNOT_MODIFY_STANDARD_CARDPOOL);
                game.ui.dialog.ShowConfirm(title, desc, null);
                return false;
            }
            var cardPoolRef = cardPoolInfo.ToReference();
            // 如果该卡池正在被创建，将正在创建的卡池设为空。
            if (_creatingCardPoolInfo.CompareCardPool(cardPoolInfo))
                finishCreatingCardPool();


            if (isInnateCardPool(cardPoolInfo))
            {
                //官方卡池只能保存到指定路径下面，也就是工程文件
                var cardPoolName = cardPoolInfo.cardPoolName;
                path = Path.Combine(editorCardPoolDirName, $"{cardPoolName}.{cardPoolFilePostfix}");
                path = Path.GetFullPath(path);
            }
            else if (path.isStreamingAssetsPath(out var streamingPath))
            {
                //如果是StreamingAssets，那要转存成可持续化文件
                path = Path.Combine(Application.persistentDataPath, streamingPath);
            }
            // 修改卡池的版本，并使卡池编辑次数+1。
            cardPoolInfo.editTime++;
            cardPoolInfo.version = 0;
            cardPoolInfo.versionNumber = CURRENT_CARD_POOL_VERSION;

            // 更新字典中的卡池信息和卡池内容。
            removeConflictCardPoolInfo(_localCardPoolInfoDict, cardPoolInfo.cardPoolId);
            _localCardPoolInfoDict[path] = cardPoolInfo;
            _localCardPoolDict.Remove(cardPoolRef);
            _localCardPoolDict.Add(cardPoolInfo.ToReference(), cardPool);

            // 移除无效的皮肤
            removeInvalidSkins(cardPool);
            // 检查并修复路径不正确的卡图
            await fixIncorrectPathImages(cardPoolInfo, cardPool);
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

        #region 读取卡池信息
        /// <summary>
        /// 读取所有卡池信息。可能的位置有自定义卡池路径，编辑器下的ExternalData，非编辑器下的StreamingAssets以及缓存中的卡池。
        /// </summary>
        public async Task loadAllCardPoolInfosAsync()
        {
            await loadCardPoolInfosToDict(_localCardPoolInfoDict, getLocalCardPoolsPath(), true);
            await loadCardPoolInfosToDict(_cachedCardPoolInfoDict, getCachedCardPoolsPath(), false);
        }
        /// <summary>
        /// 读取某个路径上的卡池信息。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<CardPoolInfo> loadLocalCardPoolInfoAsync(string path)
        {
            return loadCardPoolInfoToDict(_localCardPoolInfoDict, path, true);
        }
        /// <summary>
        /// 从缓存文件夹中读取从网络上下载的卡池的卡池信息。
        /// </summary>
        /// <param name="poolRef">卡池引用。</param>
        /// <returns></returns>
        public async Task<CardPoolInfo> loadCachedCardPoolInfoAsync(ICardPoolReference poolRef)
        {
            if (poolRef == null)
                return null;
            return await loadCachedCardPoolInfoAsync(poolRef.CardPoolID, poolRef.CardPoolVersion);
        }
        /// <summary>
        /// 从缓存文件夹中读取从网络上下载的卡池的卡池信息。
        /// </summary>
        /// <param name="cardPoolId">卡池ID。</param>
        /// <param name="version">卡池版本。</param>
        /// <returns></returns>
        public async Task<CardPoolInfo> loadCachedCardPoolInfoAsync(long cardPoolId, int version)
        {
            string resID = CardPoolReference.GetResourceID(cardPoolId, version);
            if (game.cache.FileExists(ResourceType.CardDefine, resID))
            {
                string path = game.cache.GetCachePath(ResourceType.CardDefine, resID);
                return await loadCachedCardPoolInfoAsync(path);
            }
            return null;
        }
        /// <summary>
        /// 从缓存文件夹中读取从网络上下载的卡池的卡池信息。
        /// </summary>
        /// <param name="path">路径。</param>
        /// <returns></returns>
        public async Task<CardPoolInfo> loadCachedCardPoolInfoAsync(string path)
        {
            if (!game.fileManager.isFileExist(path))
                return null;
            return await loadCardPoolInfoToDict(_cachedCardPoolInfoDict, path, false);
        }
        #endregion

        #region 读取卡池内容
        /// <summary>
        /// 从目标路径上加载卡池
        /// </summary>
        /// <param name="path">流文件路径或者绝对路径</param>
        /// <param name="handleException">是否要自动处理加载卡池时的异常，不向上层抛出。</param>
        /// <param name="env">卡池环境，用于在修复卡池的步骤中读取其他卡池。</param>
        /// <exception cref="FileLoadException">卡池文件为空。</exception>
        /// <exception cref="FormatException">反序列化失败。</exception>
        /// <returns></returns>
        public Task<(CardPoolInfo cardPoolInfo, CardPool cardPool)> loadCardPool(string path, bool handleException = false)
        {
            var isLocal = isLocalCardPoolPath(path);
            var infoDict = isLocal ? _localCardPoolInfoDict : _cachedCardPoolInfoDict;
            var poolDict = isLocal ? _localCardPoolDict : _cachedCardPoolDict;
            return loadCardPoolToDict(infoDict, poolDict, path, isLocal, handleException);
        }
        /// <summary>
        /// 从目标路径上加载本地卡池
        /// </summary>
        /// <param name="path">流文件路径或者绝对路径</param>
        /// <param name="handleException">是否要自动处理加载卡池时的异常，不向上层抛出。</param>
        /// <param name="env">卡池环境，用于在修复卡池的步骤中读取其他卡池。</param>
        /// <exception cref="FileLoadException">卡池文件为空。</exception>
        /// <exception cref="FormatException">反序列化失败。</exception>
        /// <returns></returns>
        public Task<(CardPoolInfo cardPoolInfo, CardPool cardPool)> loadLocalCardPool(string path, bool handleException = false)
        {
            return loadCardPoolToDict(_localCardPoolInfoDict, _localCardPoolDict, path, true, handleException);
        }
        /// <summary>
        /// 从缓存文件夹中读取从网络上下载的卡池。
        /// </summary>
        /// <param name="poolRef">卡池引用。</param>
        /// <returns></returns>
        public async Task<(CardPoolInfo info, CardPool cardPool)> loadCachedCardPool(ICardPoolReference poolRef, bool handleException = true)
        {
            if (poolRef == null)
                return (null, null);
            return await loadCachedCardPool(poolRef.CardPoolID, poolRef.CardPoolVersion);
        }
        /// <summary>
        /// 从缓存文件夹中读取从网络上下载的卡池。
        /// </summary>
        /// <param name="cardPoolId">卡池ID。</param>
        /// <param name="version">卡池版本。</param>
        /// <returns></returns>
        public async Task<(CardPoolInfo info, CardPool cardPool)> loadCachedCardPool(long cardPoolId, int version, bool handleException = true)
        {
            string resID = CardPoolReference.GetResourceID(cardPoolId, version);
            if (game.cache.FileExists(ResourceType.CardDefine, resID))
            {
                string path = game.cache.GetCachePath(ResourceType.CardDefine, resID);
                var cardPoolData = await loadCachedCardPool(path, handleException);
                return cardPoolData;
            }
            return (null, null);
        }
        /// <summary>
        /// 从缓存文件夹中读取从网络上下载的卡池。
        /// </summary>
        /// <param name="path">卡池路径。</param>
        /// <returns></returns>
        public async Task<(CardPoolInfo info, CardPool cardPool)> loadCachedCardPool(string path, bool handleException = true)
        {
            if (!game.fileManager.isFileExist(path))
                return (null, null);
            return await loadCardPoolToDict(_cachedCardPoolInfoDict, _cachedCardPoolDict, path, false, handleException);
        }
        #endregion

        #region 移除卡池
        public void clearCachedCardPools()
        {
            _cachedCardPoolDict.Clear();
            _cachedCardPoolInfoDict.Clear();
        }
        public bool removeCardPool(ICardPoolReference cardPoolRef)
        {
            string path = _localCardPoolInfoDict.FirstOrDefault(p => p.Value.CompareCardPool(cardPoolRef)).Key;
            if (path != null)
            {
                _localCardPoolDict.Remove(cardPoolRef);
                _localCardPoolInfoDict.Remove(path);
                game.fileManager.deleteFile(path);
                return true;
            }
            else
                return false;
        }
        #endregion

        #region 获取卡池信息
        /// <summary>
        /// 获取所有卡池的路径。可能的位置有自定义卡池路径，编辑器下的ExternalData，非编辑器下的StreamingAssets。
        /// </summary>
        /// <returns></returns>
        public string[] getLocalCardPoolsPath()
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
        /// 获取所有缓存卡池的路径。
        /// </summary>
        /// <returns></returns>
        public string[] getCachedCardPoolsPath()
        {
            //缓存内的卡池
            string[] cacheFiles = game.cache.GetCachePaths(ResourceType.CardDefine);
            Debug.Log($"获取缓存卡池：\r\n{string.Join(",\r\n", cacheFiles)}");
            return cacheFiles;
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
            foreach (var pair in _localCardPoolInfoDict)
            {
                if (pair.Value.CompareCardPool(cardPoolRef))
                    return pair.Key;
            }
            foreach (var pair in _cachedCardPoolInfoDict)
            {
                if (pair.Value.CompareCardPool(cardPoolRef))
                    return pair.Key;
            }
            return null;
        }
        /// <summary>
        /// 检测是否加载了某个卡池及其版本。
        /// </summary>
        /// <param name="includingCache">是否包括缓存内的卡池。</param>
        /// <returns></returns>
        public bool isCardPoolInfoLoaded(ICardPoolReference poolRef, bool includingCache = false)
        {
            if (poolRef == null)
                return false;
            if (isBuiltinCardPool(poolRef) || isInnateCardPool(poolRef))
                return true;
            if (!includingCache && !isLocalCardPool(poolRef))
            {
                return false;
            }
            var cardPoolInfo = getCardPoolInfo(poolRef);
            return cardPoolInfo != null;
        }
        public CardPoolInfo getCardPoolInfo(string path)
        {
            if (isLocalCardPoolPath(path))
            {
                if (_localCardPoolInfoDict.TryGetValue(path, out var info))
                    return info;
            }
            else
            {
                if (_cachedCardPoolInfoDict.TryGetValue(path, out var info))
                    return info;
            }
            return null;
        }
        public CardPoolInfo getCardPoolInfo(ICardPoolReference cardPoolRef)
        {
            if (cardPoolRef == null)
                return null;
            if (isLocalCardPool(cardPoolRef))
            {
                var cardPool = _localCardPoolInfoDict.Values.FirstOrDefault(p => p.CompareCardPool(cardPoolRef));
                if (cardPool != null)
                    return cardPool;
            }
            else
            {
                var cardPool = _cachedCardPoolInfoDict.Values.FirstOrDefault(p => p.CompareCardPool(cardPoolRef));
                if (cardPool != null)
                    return cardPool;
            }
            if (_creatingCardPoolInfo != null && _creatingCardPoolInfo.CompareCardPool(cardPoolRef))
                return _creatingCardPoolInfo;
            return null;
        }
        public CardPoolInfo getLocalCardPoolInfo(long cardPoolId)
        {
            var poolRef = getLocalCardPoolReference(cardPoolId);
            return getCardPoolInfo(poolRef);
        }
        public CardPoolInfo[] getAllCardPoolInfos()
        {
            List<CardPoolInfo> set = new List<CardPoolInfo>();
            if (_creatingCardPoolInfo != null)
                set.Add(_creatingCardPoolInfo);
            set.AddRange(_localCardPoolInfoDict.Values);
            set.AddRange(_cachedCardPoolInfoDict.Values);
            return set.ToArray();
        }
        /// <summary>
        /// 获取所有本地卡池信息。
        /// </summary>
        /// <returns></returns>
        public CardPoolInfo[] getLocalCardPoolInfos()
        {
            List<CardPoolInfo> set = new List<CardPoolInfo>();
            if (_creatingCardPoolInfo != null)
                set.Add(_creatingCardPoolInfo);
            set.AddRange(_localCardPoolInfoDict.Values);
            return set.ToArray();
        }
        #endregion

        #region 获取卡池内容
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
            var cache = await loadCachedCardPool(poolRef);
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
            if (isLocalCardPool(cardPoolRef))
                cardPool = _localCardPoolDict.FirstOrDefault(p => cardPoolRef.CompareCardPool(p.Key)).Value;
            else
                cardPool = _cachedCardPoolDict.FirstOrDefault(p => cardPoolRef.CompareCardPool(p.Key)).Value;
            return cardPool != null;
        }
        public bool tryGetLocalCardPool(long cardPoolId, out CardPool cardPool)
        {
            var poolRef = getLocalCardPoolReference(cardPoolId);
            return tryGetCardPool(poolRef, out cardPool);
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
        public string getCardPoolName(CardPoolInfo info)
        {
            if (info == null)
                return null;
            if (info.cardPoolId == CardManager.STANDARD_POOL_ID)
            {
                return game.lang._(STANDARD_POOL_NAME);
            }
            return info.cardPoolName ?? game.lang._(StringTable.EDITOR_UNKNOWN_CARD_POOL);
        }
        public string getCardPoolName(CardPoolInfoPack info)
        {
            if (info == null)
                return null;
            if (info.cardPoolId == CardManager.STANDARD_POOL_ID)
            {
                return game.lang._(STANDARD_POOL_NAME);
            }
            return info.cardPoolName ?? game.lang._(StringTable.EDITOR_UNKNOWN_CARD_POOL);
        }
        #endregion

        #region 获取卡池引用
        public ICardPoolReference getCardPoolReferenceOfBuff(BuffDefine buff)
        {
            if (buff == null)
                return null;
            var poolRef = _localCardPoolDict.FirstOrDefault(p => p.Key.CardPoolID == buff.cardPoolId && p.Value.buffDefineList.Contains(buff)).Key;
            if (poolRef != null)
                return poolRef;
            poolRef = _cachedCardPoolDict.FirstOrDefault(p => p.Key.CardPoolID == buff.cardPoolId && p.Value.buffDefineList.Contains(buff)).Key;
            if (poolRef != null)
                return poolRef;
            return null;
        }
        public ICardPoolReference getCardPoolReferenceOfCard(CardDefine card)
        {
            if (card == null)
                return null;
            var poolRef = _localCardPoolDict.FirstOrDefault(p => p.Key.CardPoolID == card.cardPoolId && p.Value.cardDefineList.Contains(card)).Key;
            if (poolRef != null)
                return poolRef;
            poolRef = _cachedCardPoolDict.FirstOrDefault(p => p.Key.CardPoolID == card.cardPoolId && p.Value.cardDefineList.Contains(card)).Key;
            if (poolRef != null)
                return poolRef;
            return null;
        }

        /// <summary>
        /// 在一个卡池引用的集合中，根据卡池的ID获取指定的卡池引用。
        /// 多用于根据卡池ID获取某个房间内允许的卡池。
        /// </summary>
        /// <param name="cardPoolId">卡池ID。</param>
        /// <param name="set">卡池引用集合。</param>
        /// <returns>获取到的卡池引用。</returns>
        public ICardPoolReference getCardPoolReferenceInEnv(long cardPoolId, IEnumerable<ICardPoolReference> set)
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
            var info = _localCardPoolInfoDict.Values.FirstOrDefault(p => p.cardPoolId == cardPoolId);
            // 返回引用。
            return info?.ToReference();
        }
        /// <summary>
        /// 获取所有卡池引用（只限读取了CardPoolInfo的）。
        /// </summary>
        /// <returns></returns>
        public ICardPoolReference[] getAllCardPoolReferences()
        {
            return getAllCardPoolInfos().Select(i => i.ToReference()).ToArray();
        }
        /// <summary>
        /// 获取所有本地卡池。
        /// </summary>
        /// <returns></returns>
        public ICardPoolReference[] getLocalCardPoolReferences()
        {
            return getLocalCardPoolInfos().Select(i => i.ToReference()).ToArray();
        }
        #endregion

        #region 卡池解析
        /// <summary>
        /// 获取卡池内的所有包含过时节点的节点图。
        /// </summary>
        /// <returns>过时节点节点图列表。</returns>
        public ObsoleteNodeHintInfo[] getObsoleteNodeGraphs(CardPool cardPool)
        {
            HashSet<ObsoleteNodeHintInfo> list = new HashSet<ObsoleteNodeHintInfo>();
            void checkObsoleteNodes(Node node, ObsoleteNodeHintInfo graphObj)
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
                foreach (var node in effect.graph.nodes)
                {
                    checkObsoleteNodes(node, ObsoleteNodeHintInfo.card(card.id));
                }
            };
            Action<BuffDefine, GeneratedEffect> buffAction = (buff, effect) =>
            {
                foreach (var node in effect.graph.nodes)
                {
                    checkObsoleteNodes(node, ObsoleteNodeHintInfo.buff(buff.id));
                }
            };
            Action<GeneratedActionDefine> defineAction = (define) =>
            {
                foreach (var node in define.graph.nodes)
                {
                    checkObsoleteNodes(node, ObsoleteNodeHintInfo.action(define.defineId));
                }
            };
            Action<GeneratedRuleLogic> logicAction = (logic) =>
            {
                foreach (var node in logic.graph.nodes)
                {
                    checkObsoleteNodes(node, ObsoleteNodeHintInfo.rule(logic.id));
                }
            };
            iterateActionGraphs(cardPool, cardAction, buffAction, defineAction, logicAction);
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
            if (cardPoolRef == null)
                return false;
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
            if (cardPoolRef == null)
                return false;
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
            var localPoolRef = getLocalCardPoolReference(cardPoolId);
            if (localPoolRef != null)
            {
                var path = getCardPoolPath(localPoolRef)?.Replace("/", "\\");
                var dir = game.pve.getCardPoolFileDirectory()?.Replace("/", "\\");
                if (path.StartsWith(dir))
                {
                    return true;
                }
            }

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

            if (Application.isEditor)
            {
                //编辑器下的官方卡池，位于项目文件夹下的ExternalData中
                var startPath = Path.GetFullPath(_editorCardPoolDirName);
                if (Path.GetFullPath(path).StartsWith(startPath))
                    return true;
            }

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
            if (cardPoolRef == null)
                return false;
            // 内置和经典卡池肯定是本人拥有的。
            if (isBuiltinCardPool(cardPoolRef) || isInnateCardPool(cardPoolRef))
                return true;
            if (isCreatingCardPool(cardPoolRef))
                return true;
            string path = getCardPoolPath(cardPoolRef);
            return isLocalCardPoolPath(path);
        }
        public bool isLocalCardPoolRefExists(ICardPoolReference cardPoolRef)
        {
            var info = game.card.getLocalCardPoolInfo(cardPoolRef.CardPoolID);
            return info != null && info.editTime == cardPoolRef.CardPoolVersion;
        }
        #endregion
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

            TypeFinder typeFinder = GraphTypes.getTypeByName;

            CardPool cardPool = serializableCardPool.toCardPool(cardPoolID, game, typeFinder);
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

            TypeFinder typeFinder = GraphTypes.getTypeByName;

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
        private void removeInvalidSkins(CardPool cardPool)
        {
            cardPool.cardSkinList.RemoveAll(skin => !cardPool.cardDefineList.Exists(c => c.id == skin.id));
            cardPool.buffSkinList.RemoveAll(skin => !cardPool.buffDefineList.Exists(b => b.id == skin.buffId));
        }
        private async Task fixCardPool(CardPoolInfo cardPoolInfo, CardPool cardPool, IEnumerable<ICardPoolReference> env = null)
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

            var builtinActions = builtinActionDefines;

            List<ActionGraph> collectedGraphs = new List<ActionGraph>();
            // 遍历所有节点图并修复信息。
            iterateActionGraphs(cardPool,
                (b, e) => effectAction(e),
                (b, e) => effectAction(e),
                defineAction,
                l => collectGraph(l.graph));

            // 修复所有收集到的节点。
            foreach (var graph in collectedGraphs)
            {
                await fixGraph(graph);
            }

            foreach (var cardDefine in cardPool.cardDefineList)
            {
                fixCardDefine(cardDefine);
            }
            foreach (var buffDefine in cardPool.buffDefineList)
            {
                fixBuffDefine(buffDefine);
            }

            fixSkins();



            #region 内嵌方法
            void fixSkins()
            {
                foreach (var skin in cardPool.cardSkinList)
                {
                    if (string.IsNullOrEmpty(skin.language))
                    {
                        //皮肤不能没有对应语言信息，如果没有那么至少设置为默认语言
                        skin.language = game.langManager.getDefaultLanguage();
                    }
                }
                foreach (var skin in cardPool.buffSkinList)
                {
                    if (string.IsNullOrEmpty(skin.language))
                    {
                        //皮肤不能没有对应语言信息，如果没有那么至少设置为默认语言
                        skin.language = game.langManager.getDefaultLanguage();
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
            // 收集节点图，防止在遍历节点的时候因为操作节点图而报错。
            void collectGraph(ActionGraph graph)
            {
                collectedGraphs.Add(graph);
            }

            #region 修复节点信息
            async Task fixGraph(ActionGraph graph)
            {
                foreach (var node in graph.nodes.ToArray())
                {
                    await fixNode(node);
                }
                foreach (var connection in graph.connections.OfType<InvalidNodeConnection>().ToArray())
                {
                    graph.disconnect(connection);
                }
            }
            async Task fixNode(Node node)
            {
                if (node is ActionNode action)
                {
                    await fixActionNodeIncorrectData(action);
                }
            }
            // 修复动作节点的错误。
            async Task fixActionNodeIncorrectData(ActionNode actionNode)
            {
                await fixUnknownOrObsoleteNode(actionNode);
                fixMassObsoleteNode(actionNode);
                fixOldReferences(actionNode);
                fixConditionalNode(actionNode);
                fixBuiltinCardRefs(actionNode);
                fixConstsToDefaultValues(actionNode);
                fixRuleObsoleteNode(actionNode);
                fixCardTypeObsoleteNode(actionNode);
            }
            // 修复未知或过时的节点。
            async Task fixUnknownOrObsoleteNode(ActionNode actionNode)
            {
                if (string.IsNullOrEmpty(actionNode.defineName))
                    return;
                var defineRef = actionNode.defineRef;
                bool actionExists = false;
                if (defineRef != null)
                {
                    IEnumerable<ActionDefine> actionDefines = null;
                    // 检查动作是否存在。
                    if (defineRef.cardPoolId == BUILTIN_POOL_ID)
                    {
                        // 内置卡池。
                        actionDefines = builtinActions;
                    }
                    else
                    {
                        // 自定义卡池。
                        // 获取目标卡池的动作引用。
                        var poolRef = getCardPoolReferenceInEnv(defineRef.cardPoolId, getAllCardPoolReferences());
                        var (_, definePool) = await getOrLoadCardPool(poolRef, true);
                        if (definePool != null)
                            actionDefines = definePool.actionDefineList;
                    }
                    if (actionDefines != null)
                        actionExists = actionDefines.Any(a => a.defineId == defineRef.defineId);
                }
                if (actionExists)
                    return;
                //包含不存在的动作名称，可能是名称过期了
                ActionReference newDefineRef = null;
                // 内置动作。
                foreach (ActionDefine actionDefine in builtinActions)
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
                if (newDefineRef == null)
                {
                    Debug.LogError($"动作节点{actionNode.id}的名称未知：{actionNode.defineName}");
                    return;
                }

                Debug.LogWarning($"将动作节点{actionNode.id}过期的动作名称{actionNode.defineName}替换为{newDefineRef}");
                actionNode.defineRef = newDefineRef;
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
                            var defaultValue = action.getInputDefaultValue(constName);
                            if (defaultValue == null)
                            {
                                switch (constPair.Key)
                                {
                                    case BuiltinActionDefineID.BooleanConst: defaultValue = false; break;
                                    case BuiltinActionDefineID.IntegerConst: defaultValue = 0; break;
                                    case BuiltinActionDefineID.StringConst: defaultValue = string.Empty; break;
                                    case BuiltinActionDefineID.GetCardDefine: defaultValue = new DefineReference(0, 0); break;
                                    case BuiltinActionDefineID.GetBuffDefine: defaultValue = new BuffReference(0, 0); break;
                                }
                            }
                            else if (constPair.Key == BuiltinActionDefineID.GetBuffDefine && defaultValue is DefineReference buffDefRef)
                            {
                                defaultValue = new BuffReference(buffDefRef.cardPoolId, buffDefRef.defineId);
                            }
                            dest.node.setInputDefaultValue(vi.name, vi.paramIndex, defaultValue);
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
            // 修复因加入自定义规则而过时的节点。
            void fixRuleObsoleteNode(ActionNode actionNode)
            {
                if (actionNode.defineRef == null)
                    return;
                var graph = actionNode.graph;
                var posX = actionNode.posX;
                var posY = actionNode.posY;
                if (actionNode.defineRef.isBuiltinReference(BuiltinActionDefineID.SelectOneFromCardDefines_Obs_Rule))
                {
                    actionNode.defineRef = new ActionReference(0, BuiltinActionDefineID.SelectOneFromCardDefines);
                    actionNode.define = getActionDefine(builtinCardPoolReference, actionNode.defineRef.defineId);
                    actionNode.Define();

                    connectThisCardOwner("player");
                }
                else if (actionNode.defineRef.isBuiltinReference(BuiltinActionDefineID.SelectOneFromCards_Obs_Rule))
                {
                    actionNode.defineRef = new ActionReference(0, BuiltinActionDefineID.SelectOneFromCards);
                    actionNode.define = getActionDefine(builtinCardPoolReference, actionNode.defineRef.defineId);
                    actionNode.Define();

                    connectThisCardOwner("player");
                }
                else if (actionNode.defineRef.isBuiltinReference(BuiltinActionDefineID.HealTargets_Obs_Rule))
                {
                    actionNode.defineRef = new ActionReference(0, BuiltinActionDefineID.HealTargets);
                    actionNode.define = getActionDefine(builtinCardPoolReference, actionNode.defineRef.defineId);
                    actionNode.Define();

                    connectSource("source");
                }

                void connectThisCardOwner(string inputName)
                {
                    var thisCardDefine = getActionDefine(builtinCardPoolReference, BuiltinActionDefineID.ThisCard);
                    var getCardOwnerDefine = getActionDefine(builtinCardPoolReference, BuiltinActionDefineID.GetCardOwner);
                    var thisCardNode = graph.createActionNode(thisCardDefine, posX - 800, posY);
                    var getCardOwnerNode = graph.createActionNode(getCardOwnerDefine, posX - 500, posY);

                    var thisCardOutput = thisCardNode.getOutputPort(MethodActionDefine.returnValueName);
                    var getCardOwnerInput = getCardOwnerNode.getInputPort("card");
                    var getCardOwnerOutput = getCardOwnerNode.getOutputPort(MethodActionDefine.returnValueName);
                    var selectCardInput = actionNode.getInputPort(inputName);

                    graph.connect(thisCardOutput, getCardOwnerInput);
                    graph.connect(getCardOwnerOutput, selectCardInput);
                }
                void connectSource(string inputName)
                {
                    var thisCardDefine = getActionDefine(builtinCardPoolReference, BuiltinActionDefineID.ThisCard);
                    var thisCardNode = graph.createActionNode(thisCardDefine, posX - 500, posY);

                    var thisCardOutput = thisCardNode.getOutputPort(MethodActionDefine.returnValueName);
                    var selectCardInput = actionNode.getInputPort(inputName);

                    graph.connect(thisCardOutput, selectCardInput);
                }
            }
            // 修复因加入卡牌类型参数而过时的节点。
            void fixCardTypeObsoleteNode(ActionNode actionNode)
            {
                if (actionNode.defineRef == null)
                    return;
                var graph = actionNode.graph;
                if (actionNode.defineRef.isBuiltinReference(BuiltinActionDefineID.IsCardOfType_Obs_CardType))
                {
                    actionNode.defineRef = new ActionReference(0, BuiltinActionDefineID.IsCardOfType);
                    actionNode.define = getActionDefine(builtinCardPoolReference, actionNode.defineRef.defineId);
                    actionNode.Define();

                    updateDefaultValue();
                }
                else if (actionNode.defineRef.isBuiltinReference(BuiltinActionDefineID.IsCardDefineOfType_Obs_CardType))
                {
                    actionNode.defineRef = new ActionReference(0, BuiltinActionDefineID.IsCardDefineOfType);
                    actionNode.define = getActionDefine(builtinCardPoolReference, actionNode.defineRef.defineId);
                    actionNode.Define();

                    updateDefaultValue();
                }
                else if (actionNode.defineRef.isBuiltinReference(BuiltinActionDefineID.CheckCardSnapshotType_Obs_CardType))
                {
                    actionNode.defineRef = new ActionReference(0, BuiltinActionDefineID.CheckCardSnapshotType);
                    actionNode.define = getActionDefine(builtinCardPoolReference, actionNode.defineRef.defineId);
                    actionNode.Define();

                    updateDefaultValue();
                }
                void updateDefaultValue()
                {
                    CardDefineSelect value = (CardDefineSelect)actionNode.getInputDefaultValue<int>("select");
                    string newValue;
                    switch (value)
                    {
                        case CardDefineSelect.Servant:
                            newValue = DEFINE.SERVANT;
                            break;
                        case CardDefineSelect.Item:
                            newValue = DEFINE.ITEM;
                            break;
                        case CardDefineSelect.Master:
                            newValue = DEFINE.MASTER;
                            break;
                        case CardDefineSelect.Skill:
                            newValue = DEFINE.SKILL;
                            break;
                        default:
                            newValue = DEFINE.SPELL;
                            break;
                    }
                    actionNode.setInputDefaultValue("type", newValue);
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
                if (characterRef != null)
                {
                    if (isBuiltinCardPool(characterRef.cardPoolId) && ClassicCardID.tryGetClassicAlternateOfBuiltin(characterRef.defineId, out var charClassicId))
                    {
                        define.setCharacterRef(new DefineReference(STANDARD_POOL_ID, charClassicId));
                    }
                }
                var skillRef = define.getSkillRef();
                if (skillRef != null)
                {
                    if (isBuiltinCardPool(skillRef.cardPoolId) && ClassicCardID.tryGetClassicAlternateOfBuiltin(skillRef.defineId, out var skillClassicId))
                    {
                        define.setSkillRef(new DefineReference(STANDARD_POOL_ID, skillClassicId));
                    }
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
        private void iterateActionGraphs(CardPool cardPool, Action<CardDefine, GeneratedEffect> cardAction,
            Action<BuffDefine, GeneratedEffect> buffAction,
            Action<GeneratedActionDefine> defineAction,
            Action<GeneratedRuleLogic> ruleLogicAction)
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
            foreach (var ruleLogic in cardPool.ruleLogicList)
            {
                ruleLogicAction?.Invoke(ruleLogic);
            }
        }
        private void removeConflictCardPoolInfo(Dictionary<string, CardPoolInfo> targetDict, long cardPoolId)
        {
            //去重，多份文件中可能包含相同的卡池
            var conflictedKeys = targetDict.Where(i => i.Value.cardPoolId == cardPoolId).Select(i => i.Key).ToArray();
            foreach (var key in conflictedKeys)
            {
                targetDict.Remove(key);
            }
        }
        #endregion

        private string allocCardPoolPath(CardPoolInfo cardPool)
        {
            var cardPoolName = cardPool.cardPoolName;
            string path = Path.Combine(Application.persistentDataPath, cardPoolDirName, $"{cardPoolName}.{cardPoolFilePostfix}");
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
        /// <summary>
        /// 向一个字典内读取的所有卡池信息。
        /// </summary>
        private async Task loadCardPoolInfosToDict(Dictionary<string, CardPoolInfo> targetDict, string[] fileList, bool uniquePools)
        {
            Debug.Log("开始读取卡池信息");
            targetDict.Clear();

            // 开始读取。
            for (int i = 0; i < fileList.Length; i++)
            {
                string path = fileList[i];
                await loadCardPoolInfoToDict(targetDict, path, uniquePools);
            }


            // 旧版本转换
            // 将dependencies转为dependencyRefs;
            foreach (var pair in targetDict)
            {
                var cardPool = pair.Value;
                if (cardPool.dependencyRefs == null && cardPool.dependencies != null)
                {
                    var depInfos = cardPool.dependencies.SelectMany(depPoolId => targetDict.Values.Where(p => p.cardPoolId == depPoolId));
                    var depRefs = depInfos.Select(poolInfo => poolInfo.ToReference());
                    cardPool.dependencies = null;
                    cardPool.dependencyRefs = depRefs.ToArray();
                }
            }

            Debug.Log($"卡池信息读取完毕，共{fileList.Length}个卡池");
        }
        private async Task<CardPoolInfo> loadCardPoolInfoToDict(Dictionary<string, CardPoolInfo> targetDict, string path, bool uniquePools)
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
                if (uniquePools)
                {
                    removeConflictCardPoolInfo(targetDict, cardPool.cardPoolId);
                }
                targetDict.Add(path, cardPool);
                Debug.Log($"读取卡池信息{path}完毕");
                return cardPool;
            }
            catch (Exception e)
            {
                Debug.LogError($"读取卡池信息{path}失败：{e}");
            }
            return null;
        }
        private async Task<(CardPoolInfo cardPoolInfo, CardPool cardPool)> loadCardPoolToDict(
            Dictionary<string, CardPoolInfo> infoDict,
            Dictionary<ICardPoolReference, CardPool> poolDict,
            string path,
            bool uniquePool,
            bool handleException = false)
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
                if (uniquePool)
                {
                    removeConflictCardPoolInfo(_localCardPoolInfoDict, cardPoolInfo.cardPoolId);
                }
                infoDict[path] = cardPoolInfo;
                poolDict[cardPoolInfo.ToReference()] = cardPool;

                //错误信息修复
                await fixCardPool(cardPoolInfo, cardPool);

                importCardPoolI18n(cardPoolInfo, cardPool);
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
        #endregion

        #region 属性字段
        [TranslateMsg]
        public const string CARD_POOL_LOAD_ERROR_DESC = "卡池加载失败，卡池文件可能已损坏。";
        [TranslateMsg]
        public const string STANDARD_POOL_NAME = "经典卡池";
        public const long BUILTIN_POOL_ID = 0;
        public const long STANDARD_POOL_ID = 1;
        public const long PVE_POOL_ID = 2;
        public static readonly CardPoolReference builtinCardPoolReference = new CardPoolReference(BUILTIN_POOL_ID, 0);
        public string cardPoolDirName => _cardPoolDirName;
        public string editorCardPoolDirName => _editorCardPoolDirName;
        public string cardPoolFilePostfix => _cardPoolFilePostfix;
        public long[] standardCardPoolsId => _standardCardPoolsId;
        /// <summary>
        /// 键为文件地址，值为卡池信息
        /// </summary>
        private Dictionary<string, CardPoolInfo> _localCardPoolInfoDict = new Dictionary<string, CardPoolInfo>();
        /// <summary>
        /// 键为卡池Id，值为卡池
        /// </summary>
        private Dictionary<ICardPoolReference, CardPool> _localCardPoolDict = new Dictionary<ICardPoolReference, CardPool>();
        /// <summary>
        /// 键为文件地址，值为卡池信息
        /// </summary>
        private Dictionary<string, CardPoolInfo> _cachedCardPoolInfoDict = new Dictionary<string, CardPoolInfo>();
        /// <summary>
        /// 键为卡池Id，值为卡池
        /// </summary>
        private Dictionary<ICardPoolReference, CardPool> _cachedCardPoolDict = new Dictionary<ICardPoolReference, CardPool>();
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
    public struct ObsoleteNodeHintInfo
    {
        public int cardId;
        public int buffId;
        public int actionId;
        public int ruleId;
        public static ObsoleteNodeHintInfo card(int id) => new ObsoleteNodeHintInfo() { cardId = id };
        public static ObsoleteNodeHintInfo buff(int id) => new ObsoleteNodeHintInfo() { buffId = id };
        public static ObsoleteNodeHintInfo action(int id) => new ObsoleteNodeHintInfo() { actionId = id };
        public static ObsoleteNodeHintInfo rule(int id) => new ObsoleteNodeHintInfo() { ruleId = id };
    }
}
