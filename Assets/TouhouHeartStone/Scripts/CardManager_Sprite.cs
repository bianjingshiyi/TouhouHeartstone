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
                    return null;
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
        public async Task saveExternalImages(int cardPoolId)
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
                //获取使用这张图片的第一个皮肤
                CardSkinData cardSkin = cardPool.cardSkinList.FirstOrDefault(s => s.imagePath == pair.Key);
                BuffSkin buffSkin = cardPool.buffSkinList.FirstOrDefault(s => s.imagePath == pair.Key);
                string path;
                if (buffSkin != null)
                {
                    //Buff图路径为卡池名称/buffId/皮肤id/Image.jpg
                    path = Path.Combine(cardPoolInfo.cardPoolName, "Buff" + buffSkin.buffId.ToString(), "Image_" + buffSkin.language + ".jpg");
                }
                else if (cardSkin != null)
                {
                    //卡图路径为卡池名称/卡牌id/皮肤id/Image.jpg
                    path = Path.Combine(cardPoolInfo.cardPoolName, "Card" + cardSkin.id.ToString(), cardSkin.skinId.ToString(), "Image_" + cardSkin.language + ".jpg");
                }
                else
                {
                    //没有皮肤使用，跳过
                    continue;
                }
                //另存为图片
                if (pair.Value is Sprite sprite)
                {
                    if (isStandardCardPool(cardPoolInfo))
                        await game.fileManager.saveFile(Path.Combine(editorTextureDirName, path), sprite.texture.EncodeToJPG());
                    else
                        await game.fileManager.savePersistentFile(Path.Combine(textureDirName, path), sprite.texture.EncodeToJPG());
                }
                else if (pair.Value is Task<Sprite> task)
                {
                    //保存还在加载的图片
                    //虽然这种情况不太可能，但是还是加上去吧
                    sprite = await task;
                    if (isStandardCardPool(cardPoolInfo))
                        await game.fileManager.saveFile(Path.Combine(editorTextureDirName, path), sprite.texture.EncodeToJPG());
                    else
                        await game.fileManager.savePersistentFile(Path.Combine(textureDirName, path), sprite.texture.EncodeToJPG());
                }
                else
                {
                    Debug.LogWarning("图片" + path + "为空");
                    continue;
                }
                removeSpritePathList.Add(path);
                savedPathSpritePairList.Add(new KeyValuePair<string, Sprite>(path, sprite));
                Debug.Log("另存为图片" + path + "成功");
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
                _externalSpriteCacheDict.Add(savedPathSpritePairList[i].Key, savedPathSpritePairList[i].Value);
            }
        }
        public async Task<Sprite> loadImage(string path, int cardPoolId)
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
        #endregion
        #region 属性字段
        Dictionary<string, object> _externalSpriteCacheDict = new Dictionary<string, object>();
        #endregion
    }
}