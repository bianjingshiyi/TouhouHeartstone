using System.Threading.Tasks;
using UnityEngine;
using SFB;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using System;
using Controllers;
using UI;
using System.Linq;
using System.Security.Cryptography;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法
        /// <summary>
        /// 让用户选择一张外部图片，然后读取并返回选择的贴图。如果图片已经读取过，则返回其在内存中的副本。
        /// </summary>
        public Task<Sprite> selectExternalImage()
        {
            TaskCompletionSource<Sprite> tcs = new TaskCompletionSource<Sprite>();
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                //手机端
                var permission = NativeGallery.GetImageFromGallery(async path =>
                {
                    if (path != null)
                    {
                        Sprite sprite = await getSpriteFromCache(path);
                        if (sprite == null)
                        {
                            Texture2D texture = NativeGallery.LoadImageAtPath(path, markTextureNonReadable: false);
                            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f);
                            //缓存外部图片
                            addSpriteToCache(path, sprite);
                        }
                        //更改贴图路径为外部文件路径
                        tcs.SetResult(sprite);
                    }
                    else
                    {
                        tcs.SetResult(null);
                    }
                });
                if (permission == NativeGallery.Permission.Denied)
                {
                    Debug.LogError("没有打开相册的权限");
                    // 拒绝权限后应当弹出框提示需要设置权限
                    NativeGallery.OpenSettings();
                }
            }
            else
            {
                //默认PC
                string[] paths = StandaloneFileBrowser.OpenFilePanel("", "", new ExtensionFilter[]
                {
                    new ExtensionFilter("Image Files", "png","jpg")
                }, false);
                if (paths == null || paths.Length < 1)
                {
                    return Task.FromResult<Sprite>(null);
                }
                string path = paths[0];
                UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path);
                uwr.SendWebRequest().completed += async op =>
                {
                    if (uwr.isNetworkError || uwr.isHttpError)
                    {
                        tcs.SetException(new IOException("读取图片发生异常：" + uwr.error));
                    }
                    else
                    {
                        Sprite sprite = await getSpriteFromCache(path);
                        if (sprite == null)
                        {
                            Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f);
                            //缓存外部图片
                            addSpriteToCache(path, sprite);
                        }
                        //更改贴图路径为外部文件路径
                        tcs.SetResult(sprite);
                    }
                    uwr.Dispose();
                };
            }
            return tcs.Task;
        }
        /// <summary>
        /// 保存外部图片，并将指定卡池中所有对外部图片的引用都替换为保存后的路径。
        /// </summary>
        /// <returns></returns>
        public async Task saveExternalImages(long cardPoolId)
        {
            CardPoolInfo cardPoolInfo = getCardPoolInfo(cardPoolId);
            if (!tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            //对缓存的图片进行保存
            List<string> removeSpritePathList = new List<string>();
            List<KeyValuePair<string, Sprite>> savedPathSpritePairList = new List<KeyValuePair<string, Sprite>>();
            foreach (var pair in _externalSpriteCacheDict)
            {
                //已经存在的资源图片不需要保存
                if (isHashNamedJPG(pair.Key))
                    continue;
                //获取使用这张图片的第一个皮肤
                CardSkinData cardSkin = cardPool.cardSkinList.FirstOrDefault(s => s.imagePath == pair.Key);
                BuffSkin buffSkin = cardPool.buffSkinList.FirstOrDefault(s => s.imagePath == pair.Key);
                string path;
                //没有皮肤使用，跳过
                if (buffSkin == null && cardSkin == null)
                    continue;
                //另存为图片
                if (pair.Value is Sprite sprite)
                {
                    //是Sprite，啥也不做
                }
                else if (pair.Value is Task<Sprite> task)
                {
                    //保存还在加载的图片
                    //虽然这种情况不太可能，但是还是加上去吧
                    sprite = await task;
                }
                else
                {
                    Debug.LogWarning("图片" + pair.Key + "为空");
                    continue;
                }
                path = await saveImage(cardPoolId, sprite.texture);
                removeSpritePathList.Add(pair.Key);
                savedPathSpritePairList.Add(new KeyValuePair<string, Sprite>(path, sprite));
                //替换所有引用这个图片的皮肤的图片地址
                foreach (var otherCardSkin in cardPool.cardSkinList)
                {
                    if (otherCardSkin.imagePath == pair.Key)
                    {
                        otherCardSkin.imagePath = path;
                    }
                }
                foreach (var otherBuffSkin in cardPool.buffSkinList)
                {
                    if (otherBuffSkin.imagePath == pair.Key)
                    {
                        otherBuffSkin.imagePath = path;
                    }
                }
            }
            for (int i = 0; i < removeSpritePathList.Count; i++)
            {
                _externalSpriteCacheDict.Remove(removeSpritePathList[i]);
            }
            for (int i = 0; i < savedPathSpritePairList.Count; i++)
            {
                string path = savedPathSpritePairList[i].Key;
                if (!_externalSpriteCacheDict.ContainsKey(path))
                {
                    _externalSpriteCacheDict.Add(path, savedPathSpritePairList[i].Value);
                }
            }
        }
        public async Task<string> saveImage(long cardPoolId, Texture2D texture)
        {
            byte[] bytes = texture.EncodeToJPG();
            string hash = calcHashString(bytes);
            string path = string.Format("{0}.jpg", hash);
            if (isStandardCardPool(cardPoolId))
            {
                string filePath = Path.Combine(editorTextureDirName, path);
                if (!game.fileManager.isFileExist(filePath))
                {
                    await game.fileManager.saveFile(filePath, bytes);
                    Debug.Log("另存为图片" + path + "成功");
                }
            }
            else
            {
                string filePath = Path.Combine(textureDirName, path);
                if (!game.fileManager.isPersistentFileExist(filePath))
                {
                    await game.fileManager.savePersistentFile(filePath, bytes);
                    Debug.Log("另存为图片" + path + "成功");
                }
            }
            return path;
        }
        public async Task<Sprite> loadImage(string path, long cardPoolId)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            Sprite sprite = await getSpriteFromCache(path);
            if (sprite != null)
            {
                //存在缓存，直接返回缓存
                return sprite;
            }
            if (path.StartsWith("sa:"))
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
                }
                catch (Exception e)
                {
                    Debug.LogError("加载图片" + path + "失败：" + e);
                }
            }
            else if (path.StartsWith("res:"))
            {
                //读取文件
                sprite = Resources.Load<Sprite>(path.Substring(4, path.Length - 4));
                if (sprite == null)
                {
                    Debug.LogError("加载图片" + path + "为空");
                }
            }
            else
            {
                //外部文件
                byte[] bytes;
                if (getCardPoolInfo(cardPoolId) is CardPoolInfo cardPoolInfo &&
                    getCardPoolPath(cardPoolInfo) is string cardPoolPath)
                {
                    if (cardPoolPath.StartsWith("sa:") &&
                        !path.StartsWith("sa:") &&
                        !path.StartsWith("res:"))
                    {
                        //卡池在StreamingAssets中，但是卡图是相对路径
                        //需要从StreamingAssets中读取相对路径上的图片
                        string saPath = Path.Combine(textureDirName, path);
                        bytes = await game.fileManager.readStreamingBinaryFile(saPath);
                    }
                    else
                    {
                        //存在卡池，要考虑一下记载这个路径的卡池的路径，综合得到绝对路径
                        //因为无法区分可持续化文件和工程文件的相对路径，所以需要通过绝对路径来读取文件
                        //外部图片路径无法记载绝对路径，因为不确定安装平台和目录，所以一定是相对路径
                        string absPath = Path.Combine(getCardPoolPathRootDir(getCardPoolPath(cardPoolInfo)), textureDirName, path);
                        bytes = await game.fileManager.readBinaryFile(absPath);
                    }
                }
                else
                {
                    throw new KeyNotFoundException("不存在卡池" + cardPoolId);
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
            _externalSpriteCacheDict[path] = sprite;
        }
        /// <summary>
        /// 获取缓存的外部图片
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<Sprite> getSpriteFromCache(string path)
        {
            if (_externalSpriteCacheDict.TryGetValue(path, out object value))
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
        /// <summary>
        /// 获取加载到内存的Sprite的地址
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        public string getSpritePath(Sprite sprite)
        {
            foreach (var pair in _externalSpriteCacheDict)
            {
                if ((object)pair.Value == sprite)
                {
                    return pair.Key;
                }
            }
            return null;
        }
        /// <summary>
        /// 获取外部图片的绝对路径。
        /// </summary>
        /// <param name="path">图片名</param>
        /// <returns>绝对路径。</returns>
        public string getExternalImagePath(string path)
        {
            return Path.Combine(externalImageDir, path);
        }
        /// <summary>
        /// 判断一张外部图片是否存在。
        /// </summary>
        /// <param name="path">图片名。</param>
        /// <returns>是否存在。</returns>
        public bool isExternalImageExists(string path)
        {
            return game.fileManager.isFileExist(getExternalImagePath(path));
        }
        public static string calcHashString(byte[] bytes)
        {
            byte[] hash = _hashProvider.ComputeHash(bytes);
            return string.Join(string.Empty, Array.ConvertAll(hash, b => b.ToString("x2")));
        }
        public static bool isHashNamedJPG(string imagePath)
        {
            return 
                imagePath.Length == 68 &&
                imagePath.EndsWith(".jpg") &&
                imagePath.Substring(0, 32).All(c => ('0' <= c && c <= '9') || ('a' <= c && c <= 'z'));
        }
        #endregion
        #region 私有方法
        private void onAwake_Sprite()
        {
            externalImageDir = Path.Combine(Application.persistentDataPath, textureDirName);
        }
        #endregion
        #region 属性字段
        public string externalImageDir { get; private set; }
        Dictionary<string, object> _externalSpriteCacheDict = new Dictionary<string, object>();

        private static SHA256 _hashProvider = SHA256.Create();
        #endregion
    }
}