using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BJSYGameCore;
using Controllers;
using SFB;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using UI;
using UnityEngine;
using ResourceType = TouhouCardEngine.ResourceType;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法
        /// <summary>
        /// 让用户选择一张外部图片，然后读取并返回选择的贴图。如果图片已经读取过，则返回其在内存中的副本。
        /// </summary>
        public Task<Sprite> selectExternalImage(int width = 512, int height = 512)
        {
            TaskCompletionSource<Sprite> tcs = new TaskCompletionSource<Sprite>();
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                //手机端
                var permission = NativeGallery.GetImageFromGallery(async path =>
                {
                    if (path != null)
                    {
                        Sprite sprite = getSpriteFromCache(path);
                        if (sprite == null)
                        {
                            Texture2D orig = NativeGallery.LoadImageAtPath(path, markTextureNonReadable: false);
                            var texture = ResizeAndCropImage(orig, width, height);
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
                    Debug.Log($"没有选择文件");
                    return Task.FromResult<Sprite>(null);
                }
                string path = paths[0];

                if (!File.Exists(path))
                {
                    Debug.Log($"选择的文件 {path} 不存在");
                    return Task.FromResult<Sprite>(null);
                }

                Sprite sprite = getSpriteFromCache(path);
                if (sprite == null)
                {
                    try
                    {
                        Texture2D orig = new Texture2D(2, 2);
                        bool success = orig.LoadImage(File.ReadAllBytes(path));
                        if (!success)
                        {
                            Debug.Log($"文件 {path} 读取出错");
                            return Task.FromResult<Sprite>(null);
                        }
                        var texture = ResizeAndCropImage(orig, width, height);
                        sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f);
                        //缓存外部图片
                        addSpriteToCache(path, sprite);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        return Task.FromException<Sprite>(e);
                    }
                }
                //更改贴图路径为外部文件路径
                tcs.SetResult(sprite);
            }
            return tcs.Task;
        }
        /// <summary>
        /// 保存外部图片，并将指定卡池中所有对外部图片的引用都替换为保存后的路径。
        /// </summary>
        /// <returns></returns>
        public async Task saveExternalImages(long cardPoolId, CardPool cardPool)
        {
            //对缓存的图片进行保存
            List<string> removeSpritePathList = new List<string>();
            List<KeyValuePair<string, Sprite>> savedPathSpritePairList = new List<KeyValuePair<string, Sprite>>();
            foreach (var pair in _externalSpriteCacheDict)
            {
                //已经存在的资源图片不需要保存
                if (isHashNamedImage(pair.Key))
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
                else
                {
                    Debug.LogWarning($"图片{pair.Key}为空");
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

        /// <summary>
        /// 保存时修正过大的图片
        /// </summary>
        /// <param name="cardPoolId"></param>
        /// <param name="cardPool"></param>
        /// <returns></returns>
        public async Task fixLargeImage(long cardPoolId, CardPool cardPool)
        {
            // 已处理的图片列表，若为null则不需要替换
            Dictionary<string, string> replacedImages = new Dictionary<string, string>();

            Func<string, Task<string>> replaceImage = async (string imagePath) =>
            {
                if (string.IsNullOrEmpty(imagePath))
                    return imagePath;

                if (replacedImages.ContainsKey(imagePath))
                    return replacedImages[imagePath];

                string newPath = ""; // 加载失败或图像尺寸正常的设定为空
                var image = await loadImage(imagePath, cardPoolId, false);
                if (image != null)
                {
                    if (image.texture.width > 512 && image.texture.height > 512)
                    {
                        var scaledTexture = ResizeImage(image.texture, 512, 512);
                        newPath = await saveImage(cardPoolId, scaledTexture);
                        Debug.Log($"卡图 {imagePath} 已缩小，新路径为 {newPath}");
                    }
                }
                else
                {
                    Debug.LogWarning($"卡图 {imagePath} 加载失败，跳过缩放检查");
                }

                replacedImages[imagePath] = newPath;
                return newPath;
            };

            foreach (var skin in cardPool.cardSkinList)
            {
                var newPath = await replaceImage(skin.imagePath);
                if (!string.IsNullOrEmpty(newPath))
                    skin.imagePath = newPath;
            }
            foreach (var skin in cardPool.buffSkinList)
            {
                var newPath = await replaceImage(skin.imagePath);
                if (!string.IsNullOrEmpty(newPath))
                    skin.imagePath = newPath;
            }
        }

        public async Task<string> saveImage(long cardPoolId, Texture2D texture, bool saveAsJpg = true, int jpgQuality = 90)
        {
            byte[] bytes = saveAsJpg ? texture.EncodeToJPG(jpgQuality) : texture.EncodeToPNG();
            string hash = calcHashString(bytes);
            string fileName = saveAsJpg ? string.Format("{0}.jpg", hash) : string.Format("{0}.png", hash);
            string filePath = Path.Combine(getCardPoolTexturesDirectory(getCardPoolInfo(cardPoolId)), fileName);
            if (!game.fileManager.isFileExist(filePath))
            {
                await game.fileManager.saveFile(filePath, bytes);
                Debug.Log($"另存为图片{fileName}成功");
            }
            return fileName;
        }

        #region 加载
        /// <summary>
        /// 加载指定卡池，指定路径的图片并转换为Sprite。
        /// </summary>
        /// <param name="path">图片路径（要求是相对路径）。</param>
        /// <param name="cardPoolId">卡池的ID。</param>
        /// <param name="useFallbackSprite">如果加载失败，是否应该使用默认的丢失贴图。</param>
        /// <returns>读取到的Sprite。</returns>
        /// <exception cref="KeyNotFoundException">不存在卡池时抛出该异常。</exception>
        public async Task<Sprite> loadImage(string path, long cardPoolId, bool useFallbackSprite = true)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            Sprite sprite = getSpriteFromCache(path);
            if (sprite != null)
            {
                //存在缓存，直接返回缓存
                return sprite;
            }
            if (path.isStreamingAssetsPath(out var streamingPath))
            {
                //读取文件
                byte[] bytes;
                try
                {
                    try
                    {
                        bytes = await game.fileManager.readStreamingBinaryFile(streamingPath);
                    }
                    catch (FileNotFoundException)
                    {
                        //可能是后缀不对
                        bytes = await game.fileManager.readStreamingBinaryFile(
                            path.EndsWith(".png") ?
                            streamingPath.Replace(".png", ".jpg") :
                            streamingPath.Replace(".jpg", ".png"));
                    }
                    Texture2D texture = new Texture2D(512, 512);
                    texture.LoadImage(bytes);
                    sprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.one * .5f);
                }
                catch (Exception e)
                {
                    Debug.LogError($"加载图片{path}失败：{e}");
                }
            }
            else if (path.isResourcesPath(out var resourcesPath))
            {
                //读取文件
                sprite = Resources.Load<Sprite>(resourcesPath);
                if (sprite == null)
                {
                    Debug.LogError($"加载图片{path}为空");
                }
            }
            else
            {
                //外部文件
                byte[] bytes;
                if (getCardPoolInfo(cardPoolId) is CardPoolInfo cardPoolInfo &&
                    getCardPoolPath(cardPoolInfo) is string cardPoolPath)
                {
                    if (cardPoolPath.isStreamingAssetsPath(out _) &&
                        !path.isStreamingAssetsPath(out _) &&
                        !path.isResourcesPath(out _))
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
                        string picId = Path.GetFileNameWithoutExtension(path);
                        string absPath = Path.Combine(getCardPoolTexturesDirectory(cardPoolInfo), path);
                        try
                        {
                            if (game.fileManager.isFileExist(absPath))
                            {
                                bytes = await game.fileManager.readBinaryFile(absPath);
                            }
                            else if (game.cache.FileExists(game.fileManager, ResourceType.Picture, picId))
                            {
                                //如果图片不存在，尝试从缓存资源中读取。
                                bytes = await game.cache.ReadCache(game.fileManager, ResourceType.Picture, picId);
                            }
                            else
                            {
                                throw new FileNotFoundException();
                            }
                        }
                        catch (FileNotFoundException)
                        {
                            Debug.LogWarning($"读取外部卡池的图片{absPath}时遇到问题：无法找到该图片文件。");
                            return useFallbackSprite ? getDefaultSprite() : null;
                        }
                    }
                }
                else
                {
                    throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
                }
                // 如果是空数组，返回默认Sprite.
                if (bytes.Length <= 0)
                {
                    return useFallbackSprite ? getDefaultSprite() : null;
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
        public async Task<Sprite> loadExternalImage(string path, string dirName)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            Sprite sprite;
            if (path.isResourcesPath(out var resourcesPath))
            {
                sprite = getSpriteFromCache(path);
                if (sprite != null)
                {
                    return sprite;
                }
                //Unity资源
                sprite = Resources.Load<Sprite>(resourcesPath);
                if (sprite == null)
                {
                    Debug.LogError($"加载图片{path}为空");
                }
            }
            else if (path.isStreamingAssetsPath(out var streamingPath))
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
                        bytes = await game.fileManager.readStreamingBinaryFile(streamingPath);
                    }
                    catch (FileNotFoundException)
                    {
                        //可能是后缀不对
                        bytes = await game.fileManager.readStreamingBinaryFile(
                            path.EndsWith(".png") ?
                            streamingPath.Replace(".png", ".jpg") :
                            streamingPath.Replace(".jpg", ".png"));
                    }
                    Texture2D texture = new Texture2D(512, 512);
                    texture.LoadImage(bytes);
                    sprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.one * .5f);
                }
                catch (IOException e)
                {
                    Debug.LogError($"加载图片{path}失败：{e}");
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
        public Task<Sprite> loadCardImage(CardDefine define)
        {
            CardSkinData skin = getCardSkin(define);
            if (skin == null)
            {
                return Task.FromResult(getDefaultSprite());
            }
            if (skin.image != null)
                return Task.FromResult(skin.image);
            else
            {
                string path = skin.imagePath;
                var cardPoolRef = getCardPoolReferenceOfCard(define);
                if (string.IsNullOrEmpty(path))
                {
                    return Task.FromResult<Sprite>(null);
                }
                else if (getCardPoolInfo(cardPoolRef) is CardPoolInfo cardPool &&
                    game.card.getCardPoolPath(cardPool) is string cardPoolPath)
                {
                    if (cardPoolPath.isStreamingAssetsPath(out _) &&
                        !path.isStreamingAssetsPath(out _) &&
                        !path.isResourcesPath(out _))
                    {
                        //流文件
                        return loadExternalImage($"sa:{Path.Combine(textureDirName, path)}", null);
                    }
                    else
                    {
                        //绝对路径
                        return loadExternalImage(path, getCardPoolTexturesDirectory(cardPool));
                    }
                }
                else
                {
                    //没有卡池信息
                    return loadExternalImage(path, game.card.textureDirName);
                }
            }
        }
        public Task<Sprite> loadCardImage(ICardPoolReference cardPoolRef, int cardId)
        {
            return loadCardImage(getCardDefine(cardPoolRef, cardId));
        }
        #endregion

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
        public Sprite getSpriteFromCache(string path)
        {
            if (_externalSpriteCacheDict.TryGetValue(path, out var value))
            {
                return value;
            }
            return null;
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
        public string getCardPoolTexturesDirectory(CardPoolInfo cardPoolInfo)
        {
            var path = getCardPoolPath(cardPoolInfo);
            var relativePath = $"../{textureDirName}";
            if (!string.IsNullOrEmpty(cardPoolInfo.texturesDir))
            {
                relativePath = cardPoolInfo.texturesDir;
            }
            return Path.Combine(Path.GetDirectoryName(path), relativePath);
        }

        /// <summary>
        /// 获取默认卡片贴图
        /// </summary>
        /// <returns></returns>
        public Sprite getDefaultSprite()
        {
            return _defaultImage;
        }
        public void setDefaultImagePath(string path)
        {
            _defaultImagePath = path;
        }
        public static string calcHashString(byte[] bytes)
        {
            byte[] hash = _hashProvider.ComputeHash(bytes);
            return string.Join(string.Empty, Array.ConvertAll(hash, b => b.ToString("x2")));
        }
        /// <summary>
        /// 判断文件名是否为32bytes的小写hex哈希值，并且以jpg或png作为文件扩展名
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public static bool isHashNamedImage(string imagePath)
        {
            return
                imagePath.Length == 68 &&
                (imagePath.EndsWith(".jpg") || imagePath.EndsWith(".png")) &&
                imagePath.Substring(0, 32).All(c => ('0' <= c && c <= '9') || ('a' <= c && c <= 'z'));
        }

        /// <summary>
        /// 每次缩小1/2，快速将图像分辨率降低到指定分辨率以下
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <param name="times">缩小次数。输出分辨率 = 输入分辨率 / (2 ^ times)</param>
        /// <returns></returns>
        static Texture2D RapidDownscale(Texture2D texture, int maxWidth, int maxHeight, out int times)
        {
            times = 0;
            while (texture.width > maxWidth && texture.height > maxHeight)
            {
                times++;
                Texture2D txt = new Texture2D(texture.width / 2, texture.height / 2);
                for (int i = 0; i < txt.height; ++i)
                {
                    for (int j = 0; j < txt.width; ++j)
                    {
                        int u0 = j * 2, v0 = i * 2;
                        int u1 = u0 + 1, v1 = v0 + 1;
                        Color c = texture.GetPixel(u0, v0) / 4 + texture.GetPixel(u1, v0) / 4 + texture.GetPixel(u0, v1) / 4 + texture.GetPixel(u1, v1) / 4;
                        txt.SetPixel(j, i, c);
                    }
                }
                texture = txt;
            }
            return texture;
        }

        /// <summary>
        /// 保持比例，将图像缩小到指定大小
        /// </summary>
        /// <remarks>
        /// 会保证图片高度/宽度有一边等于设置值，另一边大于设置值
        /// </remarks>
        /// <param name="original"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <returns></returns>
        public static Texture2D ResizeImage(Texture2D original, int maxWidth, int maxHeight)
        {
            float factorW = maxWidth == 0 ? 1 : ((float)original.width / maxWidth);
            float factorH = maxHeight == 0 ? 1 : ((float)original.height / maxHeight);

            float factor = Math.Min(factorH, factorW);

            // 有一边小于最大值，返回
            if (factor <= 1)
                return original;

            // 缩放大于两倍，为保证图像质量需要预缩放
            if (factor >= 2)
            {
                original = RapidDownscale(original, maxWidth * 2, maxHeight * 2, out int times);
                while (times-- > 0)
                    factor /= 2;
            }

            // 缩放图像
            Texture2D result = new Texture2D((int)(original.width / factor), (int)(original.height / factor));
            for (int i = 0; i < result.height; ++i)
            {
                for (int j = 0; j < result.width; ++j)
                {
                    Color newColor = original.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                    result.SetPixel(j, i, newColor);
                }
            }
            result.Apply();
            return result;
        }

        /// <summary>
        /// 以指定坐标裁剪图像
        /// </summary>
        /// <remarks>
        /// 若裁剪大小大于原图像大小，则会返回小于目标大小的图像
        /// </remarks>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public static Texture2D CropImage(Texture2D image, int x, int y, int w, int h)
        {
            if (x < 0)
            {
                w += x;
                x = 0;
            }
            if (y < 0)
            {
                h += y;
                y = 0;
            }

            if (x + w > image.width)
                w = image.width - x;
            if (y + h > image.height)
                h = image.height - y;

            Texture2D result = new Texture2D(w, h);
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    result.SetPixel(j, i, image.GetPixel(j + x, i + y));
                }
            }
            result.Apply();
            return result;
        }

        /// <summary>
        /// 缩放并裁剪到中间位置
        /// </summary>
        /// <param name="image"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <returns></returns>
        public static Texture2D ResizeAndCropImage(Texture2D image, int maxWidth, int maxHeight)
        {
            var resized = ResizeImage(image, maxWidth, maxHeight);
            return CropImage(resized, (resized.width - maxWidth) / 2, (resized.height - maxHeight) / 2, maxWidth, maxHeight);
        }
        #endregion

        #region 私有方法
        private async void onAwake_Sprite()
        {
            externalImageDir = Path.Combine(Application.persistentDataPath, textureDirName);
            if (_defaultImage == null)
            {
                Texture2D texture = await getManager<THHResourceManager>().loadTexture(_defaultImagePath, null);
                _defaultImage = Sprite.Create(texture, new Rect(0, 0, 512, 512), new Vector2(.5f, .5f), 100);
            }
        }
        #endregion

        #region 属性字段
        public string textureDirName => _textureDirName;
        public string editorTextureDirName => _editorTextureDirName;
        public string externalImageDir { get; private set; }
        private static SHA256 _hashProvider = SHA256.Create();
        private Dictionary<string, Sprite> _externalSpriteCacheDict = new Dictionary<string, Sprite>();
        [SerializeField]
        string _textureDirName = "Textures";
        [SerializeField]
        string _editorTextureDirName = "ExternalData/Textures";
        [SerializeField]
        string _defaultImagePath = "Textures/DrBoom.png";
        [SerializeField]
        Sprite _defaultImage;
        #endregion
    }
}