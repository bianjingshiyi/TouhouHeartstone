using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Threading.Tasks;
using BJSYGameCore;
using Controllers;
using SFB;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using UI;
using UnityEngine;
using UnityEngine.XR;
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
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                //手机端
                return selectExternalImageForMobile(width, height);
            }
            else
            {
                //默认PC
                return selectExternalImageForPC(width, height);
            }
        }
        /// <summary>
        /// 保存外部图片，并将指定卡池中所有对外部图片的引用都替换为保存后的路径。
        /// </summary>
        /// <returns></returns>
        public async Task saveExternalImages(CardPoolInfo cardPoolInfo, CardPool cardPool)
        {
            long cardPoolId = cardPoolInfo.cardPoolId;
            //对缓存的图片进行保存
            List<string> removeSpritePathList = new List<string>();
            List<KeyValuePair<string, Sprite>> savedPathSpritePairList = new List<KeyValuePair<string, Sprite>>();
            foreach (var pair in _externalSpriteCacheDict)
            {
                var cachePath = pair.Key;
                var cacheSprite = pair.Value;
                //已经存在的资源图片不需要保存
                if (isHashNamedImage(cachePath))
                    continue;

                //获取使用这张图片的所有皮肤
                var cardSkins = cardPool.cardSkinList.Where(s => s.imagePath == cachePath);
                var buffSkins = cardPool.buffSkinList.Where(s => s.imagePath == cachePath);

                //没有皮肤使用，跳过
                if (buffSkins.Count() <= 0 && cardSkins.Count() <= 0)
                    continue;
                // 图片为空。
                if (cacheSprite is not Sprite sprite)
                {
                    Debug.LogWarning($"图片{cachePath}为空");
                    continue;
                }

                //另存为图片
                string path = await saveImage(cardPoolId, sprite.texture);
                removeSpritePathList.Add(cachePath);
                savedPathSpritePairList.Add(new KeyValuePair<string, Sprite>(path, sprite));

                //替换所有引用这个图片的皮肤的图片地址
                if (cardPoolInfo.imageResPath == cachePath)
                {
                    cardPoolInfo.imageResPath = cachePath;
                }
                foreach (var otherCardSkin in cardSkins)
                {
                    otherCardSkin.imagePath = path;
                }
                foreach (var otherBuffSkin in buffSkins)
                {
                    otherBuffSkin.imagePath = path;
                }
            }
            foreach (var path in removeSpritePathList)
            {
                _externalSpriteCacheDict.Remove(path);
            }
            foreach (var pair in savedPathSpritePairList)
            {
                var path = pair.Key;
                var sprite = pair.Value;
                if (!_externalSpriteCacheDict.ContainsKey(path))
                {
                    _externalSpriteCacheDict.Add(path, sprite);
                }
            }
        }

        /// <summary>
        /// 保存时修正过大的图片
        /// </summary>
        /// <param name="cardPoolId"></param>
        /// <param name="cardPool"></param>
        /// <returns></returns>
        public async Task fixLargeImage(CardPoolInfo cardPoolInfo, CardPool cardPool)
        {
            long cardPoolId = cardPoolInfo.cardPoolId;
            var cardPoolRef = getLocalCardPoolReference(cardPoolId);
            // 已处理的图片列表，若为null则不需要替换
            Dictionary<string, string> replacedImages = new Dictionary<string, string>();

            // 卡池封面图
            {
                var newPath = await replaceImage(cardPoolInfo.imageResPath);
                if (!string.IsNullOrEmpty(newPath))
                    cardPoolInfo.imageResPath = newPath;
            }
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


            async Task<string> replaceImage(string imagePath)
            {
                if (string.IsNullOrEmpty(imagePath))
                    return imagePath;

                if (replacedImages.ContainsKey(imagePath))
                    return replacedImages[imagePath];

                string newPath = ""; // 加载失败或图像尺寸正常的设定为空
                var image = await loadImage(cardPoolRef, imagePath, false);
                if (image == null)
                {
                    Debug.LogWarning($"卡图 {imagePath} 加载失败，跳过缩放检查");
                }
                else if (image.texture.width > 512 && image.texture.height > 512)
                {
                    var scaledTexture = ResizeImage(image.texture, 512, 512);
                    newPath = await saveImage(cardPoolId, scaledTexture);
                    Debug.Log($"卡图 {imagePath} 已缩小，新路径为 {newPath}");
                }

                replacedImages[imagePath] = newPath;
                return newPath;
            };

        }

        public async Task<string> saveImage(long cardPoolId, Texture2D texture, bool saveAsJpg = true, int jpgQuality = 90)
        {
            byte[] bytes = saveAsJpg ? texture.EncodeToJPG(jpgQuality) : texture.EncodeToPNG();
            string hash = calcHashString(bytes);
            string fileName = saveAsJpg ? $"{hash}.jpg" : $"{hash}.png";

            var poolRef = getLocalCardPoolReference(cardPoolId);
            var poolInfo = getCardPoolInfo(poolRef);
            var texDir = getCardPoolTexturesDirectory(poolInfo);
            string filePath = Path.Combine(texDir, fileName);
            if (!game.fileManager.isFileExist(filePath))
            {
                await game.fileManager.saveFile(filePath, bytes);
                Debug.Log($"另存为图片{fileName}成功");
            }
            return fileName;
        }

        #region 加载
        public Task<Sprite> loadCardImage(CardDefine define, bool fallbackDefault = true)
        {
            var cardPoolRef = getCardPoolReferenceOfCard(define);
            return loadCardImage(cardPoolRef, define.id, fallbackDefault);
        }
        public Task<Sprite> loadCardImage(ICardPoolReference cardPoolRef, int cardId, bool fallbackDefault = true)
        {
            CardSkinData skin = getCardSkin(cardPoolRef, cardId);
            return loadCardImage(cardPoolRef, skin, fallbackDefault);
        }
        public Task<Sprite> loadCardImage(ICardPoolReference cardPoolRef, CardSkinData skin, bool fallbackDefault = true)
        {
            var defaultImage = fallbackDefault ? getDefaultSprite() : null;
            // 不存在卡池引用参数
            if (cardPoolRef == null)
                return Task.FromResult(defaultImage);

            // 不存在皮肤
            if (skin == null)
                return Task.FromResult(defaultImage);

            string imagePath = skin.imagePath;
            return loadImage(cardPoolRef, imagePath, fallbackDefault);
        }
        public Task<Sprite> loadBuffImage(BuffDefine define, bool fallbackDefault = true)
        {
            var cardPoolRef = getCardPoolReferenceOfBuff(define);
            return loadBuffImage(cardPoolRef, define.id, fallbackDefault);
        }
        public Task<Sprite> loadBuffImage(ICardPoolReference cardPoolRef, int buffId, bool fallbackDefault = true)
        {
            var skin = getDefaultBuffSkin(cardPoolRef, buffId);
            return loadBuffImage(cardPoolRef, skin, fallbackDefault);
        }
        public Task<Sprite> loadBuffImage(ICardPoolReference cardPoolRef, BuffSkin skin, bool fallbackDefault = true)
        {
            var defaultImage = fallbackDefault ? getDefaultSprite() : null;
            // 不存在卡池引用参数
            if (cardPoolRef == null)
                return Task.FromResult(defaultImage);

            // 不存在皮肤
            if (skin == null)
                return Task.FromResult(defaultImage);

            string imagePath = skin.imagePath;
            return loadImage(cardPoolRef, imagePath, fallbackDefault);
        }
        public Task<Sprite> loadCardPoolImage(ICardPoolReference cardPoolRef)
        {
            var cardPoolInfo = getCardPoolInfo(cardPoolRef);
            if (cardPoolInfo == null)
                return Task.FromResult<Sprite>(null);
            return loadCardPoolImage(cardPoolInfo);
        }
        public Task<Sprite> loadCardPoolImage(CardPoolInfo cardPoolInfo)
        {
            return loadImage(cardPoolInfo, cardPoolInfo.imageResPath, false);
        }
        public async Task<Sprite> loadImage(ICardPoolReference cardPoolRef, string imagePath, bool fallbackDefault = true)
        {
            var defaultImage = fallbackDefault ? getDefaultSprite() : null;
            // 不存在卡池引用参数
            if (cardPoolRef == null)
                return defaultImage;

            // 图片路径为空
            if (string.IsNullOrEmpty(imagePath))
                return null;

            Sprite sprite = getSpriteFromCache(imagePath);
            if (sprite != null)
                return sprite;

            // 缓存路径
            if (!isLocalCardPool(cardPoolRef))
            {
                var picId = Path.GetFileNameWithoutExtension(imagePath);
                if (game.cache.FileExists(game.fileManager, ResourceType.Picture, picId))
                {
                    //添加到缓存
                    sprite = await loadCacheSprite(picId);
                    if (sprite == null)
                        return defaultImage;

                    addSpriteToCache(imagePath, sprite);
                    return sprite;
                }
            }

            //绝对路径
            var fullPath = getFullImagePath(cardPoolRef, imagePath);
            if (fullPath == null)
                return defaultImage;

            sprite = await loadFullPathImage(fullPath);
            if (sprite == null)
                return defaultImage;

            //添加到缓存
            addSpriteToCache(imagePath, sprite);
            return sprite;
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
        public string getDefaultImagePath()
        {
            return _defaultImagePath;
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

        private Task<Sprite> selectExternalImageForMobile(int width, int height)
        {
            TaskCompletionSource<Sprite> tcs = new TaskCompletionSource<Sprite>();
            var permission = NativeGallery.GetImageFromGallery(func);
            if (permission == NativeGallery.Permission.Denied)
            {
                Debug.LogError("没有打开相册的权限");
                // 拒绝权限后应当弹出框提示需要设置权限
                NativeGallery.OpenSettings();
            }
            return tcs.Task;

            void func(string path)
            {
                if (path == null)
                {
                    tcs.SetResult(null);
                    return;
                }

                Sprite sprite = getSpriteFromCache(path);
                if (sprite != null)
                {
                    // 创建贴图。
                    Texture2D orig = NativeGallery.LoadImageAtPath(path, markTextureNonReadable: false);
                    var texture = ResizeAndCropImage(orig, width, height);
                    sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f);
                    //缓存外部图片
                    addSpriteToCache(path, sprite);
                }
                //更改贴图路径为外部文件路径
                tcs.SetResult(sprite);
            }
        }
        private Task<Sprite> selectExternalImageForPC(int width, int height)
        {
            TaskCompletionSource<Sprite> tcs = new TaskCompletionSource<Sprite>();

            var extFilters = new ExtensionFilter[] { new ExtensionFilter("Image Files", "png", "jpg") };
            string[] paths = StandaloneFileBrowser.OpenFilePanel("", "", extFilters, false);
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
            return tcs.Task;
        }
        private async Task<Sprite> loadStreamingAssetSprite(string path)
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
                    bytes = await game.fileManager.readStreamingBinaryFile(
                        path.EndsWith(".png") ?
                        path.Replace(".png", ".jpg") :
                        path.Replace(".jpg", ".png"));
                }
                return createSprite(bytes);
            }
            catch (Exception e)
            {
                Debug.LogError($"加载流文件中的图片{path}失败：{e}");
            }
            return null;
        }
        private Sprite loadResourcesSprite(string path)
        {
            return Resources.Load<Sprite>(path);
        }
        private async Task<Sprite> loadExternalSprite(string path)
        {
            var fileManager = game.fileManager;
            bool isFullPath = Path.GetFullPath(path) == path;
            byte[] bytes;
            // 绝对路径
            if (isFullPath && fileManager.isFileExist(path))
            {
                //如果直接给出完整路径就直接从完整路径上读
                bytes = await fileManager.readBinaryFile(path);
                return createSprite(bytes);
            }
            // 相对路径
            else if (fileManager.isPersistentFileExist(path))
            {
                // 从外部图片路径读取图片。
                bytes = await fileManager.readPersistentBinaryFile(path);
                return createSprite(bytes);
            }
            // 实在找不到了
            Debug.LogWarning($"未找到路径为{path}的图片。");
            return null;
        }
        private async Task<Sprite> loadCacheSprite(string picId)
        {
            var fileManager = game.fileManager;
            byte[] bytes = await game.cache.ReadCache(fileManager, ResourceType.Picture, picId);
            return createSprite(bytes);
        }
        private Sprite createSprite(byte[] bytes)
        {
            // 如果是空数组，返回默认Sprite.
            if (bytes == null || bytes.Length <= 0)
                return null;
            Texture2D texture = new Texture2D(512, 512);
            texture.LoadImage(bytes);
            return createSprite(texture);
        }
        private Sprite createSprite(Texture2D texture)
        {
            // 如果是空数组，返回默认Sprite.
            if (texture == null)
                return null;
            return Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.one * .5f);
        }
        private async Task<Sprite> loadFullPathImage(string path)
        {
            // 路径为空
            if (string.IsNullOrEmpty(path))
                return null;

            Sprite sprite;
            if (path.isResourcesPath(out var resourcesPath))
                sprite = loadResourcesSprite(resourcesPath);
            else if (path.isStreamingAssetsPath(out var streamingPath))
                sprite = await loadStreamingAssetSprite(streamingPath);
            else
                sprite = await loadExternalSprite(path);

            return sprite;
        }
        private string getFullImagePath(ICardPoolReference cardPoolRef, string relativePath)
        {
            // 卡池信息不存在。
            if (getCardPoolInfo(cardPoolRef) is not CardPoolInfo cardPool || game.card.getCardPoolPath(cardPool) is not string cardPoolPath)
                return null;

            // 卡池路径是流文件路径。
            if (cardPoolPath.isStreamingAssetsPath(out _) && !relativePath.isStreamingAssetsPath(out _) && !relativePath.isResourcesPath(out _))
                return $"sa:{Path.Combine(textureDirName, relativePath)}";

            //绝对路径
            return Path.Combine(getCardPoolTexturesDirectory(cardPool), relativePath);
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