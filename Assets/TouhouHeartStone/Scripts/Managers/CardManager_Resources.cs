using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Controllers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法
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
                    var resourcesUsedThis = cardPool.localizedResourceList.Where(res => res.getAllTranslations(true).Contains(cachePath));

                    // 没有皮肤使用，跳过
                    if (resourcesUsedThis.Count() <= 0)
                        continue;

                    string path = await saveImage(cardPoolId, sprite.texture);
                    removeResourcesPathList.Add(cachePath);
                    savedPathSpritePairList.Add(new KeyValuePair<string, UnityEngine.Object>(path, sprite));
                    //替换所有引用这个资源的皮肤的资源地址
                    if (cardPoolInfo.imageResPath == cachePath)
                    {
                        cardPoolInfo.imageResPath = path;
                    }
                    foreach (var resource in resourcesUsedThis)
                    {
                        if (resource.value == cachePath)
                        {
                            resource.value = path;

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
        #endregion

        #region 私有方法
        private T loadResourceAsset<T>(string path) where T : UnityEngine.Object
        {
            return Resources.Load<T>(path);
        }
        #endregion

        #region 属性字段
        private Dictionary<string, Object> _externalResourcesCacheDict = new Dictionary<string, Object>();
        #endregion
    }
}
