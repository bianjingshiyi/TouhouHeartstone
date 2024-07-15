using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using Controllers;
using TouhouCardEngine.Shared;
using UnityEngine;
using Object = UnityEngine.Object;
using ResourceType = TouhouCardEngine.ResourceType;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法

        /// <summary>
        /// 将外部资源添加到缓存
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sprite"></param>
        public void addResourceToCache<T>(string path, T t) where T : UnityEngine.Object
        {
            _externalResourcesCacheDict[path] = t;
        }
        /// <summary>
        /// 获取缓存的外部资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public T getResourceFromCache<T>(string path) where T : UnityEngine.Object
        {
            if (_externalResourcesCacheDict.TryGetValue(path, out var value) && value is T t)
            {
                return t;
            }
            return null;
        }
        /// <summary>
        /// 获取加载到内存的Sprite的地址
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public string getResourcePath<T>(T res) where T : UnityEngine.Object
        {
            foreach (var pair in _externalResourcesCacheDict)
            {
                if ((object)pair.Value == res)
                {
                    return pair.Key;
                }
            }
            return null;
        }
        /// <summary>
        /// 判断文件名是否为32bytes的小写hex哈希值
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public static bool isHashNamedResource(string imagePath)
        {
            var nameWithoutExt = Path.GetFileNameWithoutExtension(imagePath);
            return nameWithoutExt.Length == 64 && imagePath.Substring(0, 32).All(c => ('0' <= c && c <= '9') || ('a' <= c && c <= 'z'));
        }

        /// <summary>
        /// 检测卡池资源是否存在。（不包括缓存文件）
        /// </summary>
        /// <param name="poolRef">卡池引用。</param>
        /// <param name="type">资源类型。</param>
        /// <param name="relativePath">相对路径。</param>
        /// <returns></returns>
        public bool isResourceExists(ICardPoolReference poolRef, ResourceType type, string relativePath)
        {
            // 路径为空
            if (string.IsNullOrEmpty(relativePath))
                return false;

            var fullPath = getFullResourcePath(poolRef, type, relativePath);

            // 流式资源
            if (fullPath.isStreamingAssetsPath(out var streamingPath))
                return game.fileManager.isStreamingBinaryFileExist(streamingPath);

            // 外部资源
            return game.fileManager.isFileExist(fullPath);
        }
        #region 加载
        public T parseResource<T>(ResourceType resourceType, byte[] data) where T : Object
        {
            // 如果是空数组，返回null.
            if (data == null || data.Length <= 0)
                return null;
            switch (resourceType)
            {
                case ResourceType.Picture:
                    return createSprite(data) as T;
                case ResourceType.Sound:
                    return game.seManager.Decompress(data) as T;
            }
            return null;
        }
        public async Task<T> loadResource<T>(ICardPoolReference cardPoolRef, ResourceType resourceType, string relativePath, T defaultValue = null) where T : Object
        {
            // 不存在卡池引用参数
            if (cardPoolRef == null)
                return defaultValue;

            // 路径为空
            if (string.IsNullOrEmpty(relativePath))
                return defaultValue;

            T result = getResourceFromCache<T>(relativePath);
            if (result != null)
                return result;

            var fullPath = getFullResourcePath(cardPoolRef, resourceType, relativePath);
            if (fullPath == null)
                return defaultValue;

            if (fullPath.isResourcesPath(out var resourcesPath))
            {
                result = loadResourceAsset<T>(resourcesPath);
            }
            else
            {
                var bytes = await loadResourceBytes(resourceType, fullPath);
                if (bytes != null)
                    result = parseResource<T>(resourceType, bytes);
            }
            addResourceToCache(relativePath, result);
            return result;
        }
        public async Task<byte[]> loadResourceBytes(ResourceType resourceType, string fullPath)
        {
            // 路径为空
            if (string.IsNullOrEmpty(fullPath))
                return null;

            // 缓存资源
            if (!isLocalCardPoolPath(fullPath))
            {
                var id = Path.GetFileNameWithoutExtension(fullPath);
                if (game.cache.FileExists(resourceType, id))
                {
                    return await loadCacheBytes(resourceType, id);
                }
            }

            // 流式资源
            if (fullPath.isStreamingAssetsPath(out var streamingPath))
                return await loadStreamingAssetBytes(streamingPath);

            // 外部资源
            return await loadExternalBytes(fullPath);
        }
        #endregion

        #region 保存
        /// <summary>
        /// 保存外部资源，并将指定卡池中所有对外部资源的引用都替换为保存后的路径。
        /// </summary>
        /// <returns></returns>
        public async Task saveExternalResources(CardPoolInfo cardPoolInfo, CardPool cardPool)
        {
            long cardPoolId = cardPoolInfo.cardPoolId;
            //对缓存的资源进行保存
            List<string> removeResourcesPathList = new List<string>();
            List<KeyValuePair<string, UnityEngine.Object>> savedPathSpritePairList = new List<KeyValuePair<string, UnityEngine.Object>>();
            foreach (var pair in _externalResourcesCacheDict)
            {
                var cachePath = pair.Key;
                var cacheResource = pair.Value;
                // 已经存在的资源不需要保存
                if (isHashNamedResource(cachePath))
                    continue;
                // 资源为空。
                if (cacheResource is not UnityEngine.Object res)
                {
                    Debug.LogWarning($"资源{cachePath}为空");
                    continue;
                }

                if (res is Sprite sprite)
                {
                    //另存为资源

                    // 获取使用这张资源的所有皮肤
                    var infoResourcesUsedThis = cardPoolInfo.localizedResources.Where(res => res.getAllTranslations(true).Contains(cachePath));
                    var resourcesUsedThis = cardPool.localizedResourceList.Where(res => res.getAllTranslations(true).Contains(cachePath));

                    // 没有皮肤使用，跳过
                    if (infoResourcesUsedThis.Count() + resourcesUsedThis.Count() <= 0)
                        continue;

                    string path = await saveImage(cardPoolId, sprite.texture);
                    removeResourcesPathList.Add(cachePath);
                    savedPathSpritePairList.Add(new KeyValuePair<string, UnityEngine.Object>(path, sprite));
                    //替换所有引用这个资源的皮肤的资源地址
                    // 卡池文件头。
                    foreach (var resource in infoResourcesUsedThis)
                    {
                        if (resource.value == cachePath)
                        {
                            resource.value = path;
                            if (cardPoolInfo.imageResPathKey == cachePath)
                            {
                                cardPoolInfo.imageResPathKey = path;
                            }
                        }
                        foreach (var lang in resource.getAllLanguages())
                        {
                            if (resource.getTranslation(lang) == cachePath)
                            {
                                resource.setTranslation(lang, path);
                            }
                        }
                    }
                    // 卡尺文件内部。
                    foreach (var resource in resourcesUsedThis)
                    {
                        if (resource.value == cachePath)
                        {
                            resource.value = path;

                            // 替换卡牌和增益中所有引用该贴图的地址。
                            var cardSkins = cardPool.cardSkinList.Where(s => s.imagePathKey == cachePath);
                            var buffSkins = cardPool.buffSkinList.Where(s => s.imagePathKey == cachePath);
                            foreach (var otherCardSkin in cardSkins)
                            {
                                otherCardSkin.imagePathKey = path;
                            }
                            foreach (var otherBuffSkin in buffSkins)
                            {
                                otherBuffSkin.imagePathKey = path;
                            }
                        }
                        foreach (var lang in resource.getAllLanguages())
                        {
                            if (resource.getTranslation(lang) == cachePath)
                            {
                                resource.setTranslation(lang, path);
                            }
                        }
                    }
                }
                else if (res is AudioClip audio)
                {
                    // 另存为音频

                    // 获取使用这段音频的所有皮肤
                    var cardSkins = cardPool.cardSkinList.Where(s => Array.Exists(s.getAllSoundRefs(), r => r.path == cachePath));
                    // 没有皮肤使用，跳过
                    if (cardSkins.Count() <= 0)
                        continue;

                    string path = await saveSound(cardPoolId, audio);
                    removeResourcesPathList.Add(cachePath);
                    savedPathSpritePairList.Add(new KeyValuePair<string, UnityEngine.Object>(path, audio));
                    //替换所有引用这个资源的皮肤的资源地址
                    foreach (var otherCardSkin in cardSkins)
                    {
                        foreach (var otherSoundRef in otherCardSkin.getAllSoundRefs())
                        {
                            if (otherSoundRef.path == cachePath)
                                otherSoundRef.path = path;
                        }
                    }
                }
            }
            foreach (var path in removeResourcesPathList)
            {
                _externalResourcesCacheDict.Remove(path);
            }
            foreach (var pair in savedPathSpritePairList)
            {
                var path = pair.Key;
                var res = pair.Value;
                if (!_externalResourcesCacheDict.ContainsKey(path))
                {
                    _externalResourcesCacheDict.Add(path, res);
                }
            }
        }
        public Task<string> saveResourceBytes(long cardPoolId, byte[] bytes, ResourceType type, string extension)
        {
            string hash = calcHashString(bytes);
            return saveResourceBytes(cardPoolId, bytes, type, hash, extension);
        }
        public async Task<string> saveResourceBytes(long cardPoolId, byte[] bytes, ResourceType type, string hash, string extension)
        {
            string fileName = $"{hash}.{extension}";

            var poolRef = getLocalCardPoolReference(cardPoolId);
            var poolInfo = getCardPoolInfo(poolRef);
            var soundDir = getCardPoolResourceDirectory(poolInfo, type);
            string filePath = Path.Combine(soundDir, fileName);
            if (!game.fileManager.isFileExist(filePath))
            {
                await game.fileManager.saveFile(filePath, bytes);
                Debug.Log($"另存为资源{fileName}成功");
            }
            return fileName;
        }
        #endregion

        #region 路径
        public string getExternalResourceDirectory(ResourceType resourceType)
        {
            return Path.Combine(_persistentDataPath, getResourceDirectoryName(resourceType));
        }
        public string getExternalResourcePath(ResourceType resourceType, string path)
        {
            return Path.Combine(_persistentDataPath, getResourceDirectoryName(resourceType), path);
        }
        public string getFullResourcePath(ICardPoolReference cardPoolRef, ResourceType resourceType, string relativePath)
        {
            CardPoolInfo cardPool;
            if (isCreatingCardPool(cardPoolRef))
                cardPool = _creatingCardPoolInfo;
            else
                cardPool = getCardPoolInfo(cardPoolRef);
            // 卡池信息不存在。
            if (cardPool == null)
                return null;

            // 卡池路径是流文件路径。
            if (game.card.getCardPoolPath(cardPool) is string cardPoolPath)
            {
                if (cardPoolPath.isStreamingAssetsPath(out _) && !relativePath.isStreamingAssetsPath(out _) && !relativePath.isResourcesPath(out _))
                    return $"sa:{Path.Combine(getResourceDirectoryName(resourceType), relativePath)}";
            }
            //绝对路径
            return Path.Combine(getCardPoolResourceDirectory(cardPool, resourceType), relativePath);
        }
        public string getCardPoolResourceDirectory(CardPoolInfo cardPoolInfo, ResourceType resourceType)
        {
            string cardPoolDir = getCardPoolDirectory(cardPoolInfo);
            string specifiedDir = getCardPoolSpecifiedResourceDir(cardPoolInfo, resourceType);
            if (!string.IsNullOrEmpty(specifiedDir))
            {
                return Path.Combine(cardPoolDir, specifiedDir);
            }
            else
            {
                var lastDir = Path.GetDirectoryName(cardPoolDir);
                return Path.Combine(lastDir, getResourceDirectoryName(resourceType));
            }
        }
        #endregion

        #endregion

        #region 私有方法

        #region 加载
        private T loadResourceAsset<T>(string path) where T : UnityEngine.Object
        {
            return Resources.Load<T>(path);
        }
        private async Task<byte[]> loadStreamingAssetBytes(string path)
        {
            try
            {
                byte[] bytes;
                try
                {
                    bytes = await game.fileManager.readStreamingBinaryFile(path);
                }
                catch (FileNotFoundException)
                {
                    //可能是后缀不对
                    if (path.EndsWith(".png"))
                    {
                        bytes = await game.fileManager.readStreamingBinaryFile(path.Replace(".png", ".jpg"));
                    }
                    else if (path.EndsWith(".jpg"))
                    {
                        bytes = await game.fileManager.readStreamingBinaryFile(path.Replace(".jpg", ".png"));
                    }
                    else
                    {
                        throw;
                    }
                }
                return bytes;
            }
            catch (Exception e)
            {
                Debug.LogError($"加载流文件中的资源{path}失败：{e}");
            }
            return null;
        }
        private async Task<byte[]> loadExternalBytes(string path)
        {
            var fileManager = game.fileManager;
            bool isFullPath = Path.GetFullPath(path) == path;
            // 绝对路径
            if (isFullPath && fileManager.isFileExist(path))
            {
                //如果直接给出完整路径就直接从完整路径上读
                return await fileManager.readBinaryFile(path);
            }
            // 相对路径
            else if (fileManager.isPersistentFileExist(path))
            {
                // 从外部图片路径读取图片。
                return await fileManager.readPersistentBinaryFile(path);
            }
            // 实在找不到了
            Debug.LogWarning($"未找到路径为{path}的资源。");
            return null;
        }
        private async Task<byte[]> loadCacheBytes(ResourceType type, string picId)
        {
            return await game.cache.ReadCache(type, picId);
        }
        #endregion

        #region 路径
        private string getResourceDirectoryName(ResourceType resourceType)
        {
            switch (resourceType)
            {
                case ResourceType.CardDefine:
                    return cardPoolDirName;
                case ResourceType.Sound:
                    return soundDirName;
                case ResourceType.Picture:
                    return textureDirName;
            }
            return null;
        }
        private string getCardPoolSpecifiedResourceDir(CardPoolInfo cardPoolInfo, ResourceType resourceType)
        {
            switch (resourceType)
            {
                case ResourceType.Sound:
                    return cardPoolInfo.soundsDir;
                case ResourceType.Picture:
                    return cardPoolInfo.texturesDir;
            }
            return null;
        }
        #endregion

        #endregion

        #region 属性字段
        private Dictionary<string, Object> _externalResourcesCacheDict = new Dictionary<string, Object>();
        #endregion
    }
}
