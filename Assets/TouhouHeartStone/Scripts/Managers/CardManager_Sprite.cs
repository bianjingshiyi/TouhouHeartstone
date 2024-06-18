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
        /// 保存时修正路径不正确的图片
        /// </summary>
        /// <param name="cardPoolInfo"></param>
        /// <param name="cardPool"></param>
        /// <returns></returns>
        public async Task fixIncorrectPathImages(CardPoolInfo cardPoolInfo, CardPool cardPool)
        {
            cardPoolInfo.imageResPathKey = await fixLocalizedResourcePath(cardPoolInfo.localizedResources, cardPoolInfo.imageResPathKey);
            foreach (var skin in cardPool.cardSkinList)
            {
                skin.imagePathKey = await fixLocalizedResourcePath(cardPool.localizedResourceList, skin.imagePathKey);
            }
            foreach (var buffSkin in cardPool.buffSkinList)
            {
                buffSkin.imagePathKey = await fixLocalizedResourcePath(cardPool.localizedResourceList, buffSkin.imagePathKey);
            }

            async Task<string> fixLocalizedResourcePath(IEnumerable<LocalizedResource> resources, string key)
            {
                var newKey = await fixImage(key);
                var localizedRes = resources.getLocalizedObject<string, LocalizedResource>(key);
                if (localizedRes != null)
                {
                    localizedRes.value = newKey;
                    foreach (var lang in localizedRes.getAllLanguages())
                    {
                        var path = localizedRes.getTranslation(lang);
                        if (path == null)
                            continue;
                        localizedRes.setTranslation(lang, await fixImage(path));
                    }
                }
                return newKey;
            }

            async Task<string> fixImage(string path)
            {
                if (!string.IsNullOrEmpty(path) && !isHashNamedImage(path))
                {
                    //图片地址必须是32位哈希.jpg
                    Sprite sprite = await loadImage(cardPoolInfo, path, false);
                    // 不保存丢失的贴图。
                    if (sprite != null)
                    {
                        path = await saveImage(cardPoolInfo.cardPoolId, sprite.texture);
                    }
                }
                return path;
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
                cardPoolInfo.imageResPathKey = await replaceLocalizedResourceImage(cardPoolInfo.localizedResources, cardPoolInfo.imageResPathKey);
            }
            foreach (var skin in cardPool.cardSkinList)
            {
                skin.imagePathKey = await replaceLocalizedResourceImage(cardPool.localizedResourceList, skin.imagePathKey);
            }
            foreach (var skin in cardPool.buffSkinList)
            {
                skin.imagePathKey = await replaceLocalizedResourceImage(cardPool.localizedResourceList, skin.imagePathKey);
            }



            async Task<string> replaceLocalizedResourceImage(IEnumerable<LocalizedResource> resources, string key)
            {
                var oldKey = key;
                var newKey = await replaceImage(key);
                if (!string.IsNullOrEmpty(newKey))
                    key = newKey;

                var localizedRes = resources.getLocalizedObject<string, LocalizedResource>(oldKey);
                if (localizedRes != null)
                {
                    localizedRes.value = key;

                    foreach (var lang in localizedRes.getAllLanguages())
                    {
                        var path = localizedRes.getTranslation(lang);
                        if (path == null)
                            continue;
                        var replaced = await replaceImage(path);
                        if (!string.IsNullOrEmpty(replaced))
                            localizedRes.setTranslation(lang, replaced);
                    }
                }
                return key;
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

        public Task<string> saveImage(long cardPoolId, Texture2D texture, bool saveAsJpg = true, int jpgQuality = 90)
        {
            if (saveAsJpg)
            {
                // 若保存为jpg，将透明像素变为黑色。
                var pixels = texture.GetPixels32();
                var backgroundColor = new Color32(0, 0, 0, 1);
                for (int i = 0; i < pixels.Length; i++)
                {
                    var color = pixels[i];
                    color = Color32.Lerp(color, backgroundColor, 1 - color.a / 255f);
                    color.a = 255;
                    pixels[i] = color;
                }
                texture.SetPixels32(pixels);
                texture.Apply();
            }
            byte[] bytes = saveAsJpg ? texture.EncodeToJPG(jpgQuality) : texture.EncodeToPNG();
            var extension = saveAsJpg ? "jpg" : "png";
            return saveResourceBytes(cardPoolId, bytes, ResourceType.Picture, extension);
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

            string imagePath = getCardImagePath(cardPoolRef, skin);
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

            string imagePath = getBuffImagePath(cardPoolRef, skin);
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
            var imagePath = getCardPoolImagePath(cardPoolInfo);
            return loadImage(cardPoolInfo, imagePath, false);
        }
        public Task<Sprite> loadImage(ICardPoolReference cardPoolRef, string imagePath, bool fallbackDefault = true)
        {
            if (imagePath == null)
                return Task.FromResult<Sprite>(null);
            var defaultImage = fallbackDefault ? getDefaultSprite() : null;
            return loadResource(cardPoolRef, ResourceType.Picture, imagePath, defaultImage);
        }
        #endregion
        /// <summary>
        /// 获取外部图片的绝对路径。
        /// </summary>
        /// <param name="path">图片名</param>
        /// <returns>绝对路径。</returns>
        public string getExternalImagePath(string path)
        {
            return getExternalResourcePath(ResourceType.Picture, path);
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
            return isHashNamedResource(imagePath) && (imagePath.EndsWith(".jpg") || imagePath.EndsWith(".png"));
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

                Sprite sprite = getResourceFromCache<Sprite>(path);
                if (sprite == null)
                {
                    // 创建贴图。
                    Texture2D orig = NativeGallery.LoadImageAtPath(path, markTextureNonReadable: false);
                    var texture = ResizeAndCropImage(orig, width, height);
                    sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f);
                    //缓存外部图片
                    addResourceToCache(path, sprite);
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

            Sprite sprite = getResourceFromCache<Sprite>(path);
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
                    addResourceToCache(path, sprite);
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
        #endregion

        #region 属性字段
        public string textureDirName => _textureDirName;
        public string editorTextureDirName => _editorTextureDirName;
        private static SHA256 _hashProvider = SHA256.Create();
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