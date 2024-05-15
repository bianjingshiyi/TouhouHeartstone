using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Controllers;
using MukioI18n;
using SFB;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法
        public async Task<AudioClip> selectExternalSound()
        {
            AudioClip clip = null;
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                //手机端
                clip = await selectExternalSoundForMobile();
            }
            else
            {
                //默认PC
                clip = await selectExternalSoundForPC();
            }
            if (clip != null && clip.length > _maxSoundDuration)
            {
                var desc = game.lang._(SOUND_DURATION_OUT_OF_LIMIT, _maxSoundDuration, clip.length);
                throw new FormatException(desc);
            }
            return clip;
        }
        public async Task<string> saveSound(long cardPoolId, AudioClip audio)
        {
            byte[] bytes = game.seManager.Compress(audio);
            string hash = calcHashString(bytes);
            string fileName = $"{hash}.{_opusExtension}";

            var poolRef = getLocalCardPoolReference(cardPoolId);
            var poolInfo = getCardPoolInfo(poolRef);
            var soundDir = getCardPoolSoundsDirectory(poolInfo);
            string filePath = Path.Combine(soundDir, fileName);
            if (!game.fileManager.isFileExist(filePath))
            {
                await game.fileManager.saveFile(filePath, bytes);
                Debug.Log($"另存为音效{fileName}成功");
            }
            return fileName;
        }
        public string getCardPoolSoundsDirectory(CardPoolInfo cardPoolInfo)
        {
            string cardPoolDir = getCardPoolDirectory(cardPoolInfo);
            if (!string.IsNullOrEmpty(cardPoolInfo.soundsDir))
            {
                return Path.Combine(cardPoolDir, cardPoolInfo.soundsDir);
            }
            else
            {
                var poolDir = Path.GetDirectoryName(cardPoolDir);
                return Path.Combine(poolDir, soundDirName);
            }
        }
        public async Task<AudioClip> loadSound(ICardPoolReference cardPoolRef, string soundPath)
        {
            // 不存在卡池引用参数
            if (cardPoolRef == null)
                return null;

            // 图片路径为空
            if (string.IsNullOrEmpty(soundPath))
                return null;

            AudioClip audio = getResourceFromCache<AudioClip>(soundPath);
            if (audio != null)
                return audio;

            // 缓存路径
            if (!isLocalCardPool(cardPoolRef))
            {
                var id = Path.GetFileNameWithoutExtension(soundPath);
                if (game.cache.FileExists(ResourceType.Sound, id))
                {
                    //添加到缓存
                    audio = await loadCacheSound(id);
                    if (audio == null)
                        return null;

                    addResourceToCache(soundPath, audio);
                    return audio;
                }
            }

            //绝对路径
            var fullPath = getFullSoundPath(cardPoolRef, soundPath);
            if (fullPath == null)
                return null;

            audio = await loadFullPathSound(fullPath);
            if (audio == null)
                return null;

            //添加到缓存
            addResourceToCache(soundPath, audio);
            return audio;
        }
        public bool isExternalSoundExists(string path)
        {
            return game.fileManager.isFileExist(getExternalSoundPath(path));
        }
        /// <summary>
        /// 获取外部音频的绝对路径。
        /// </summary>
        /// <param name="path">音频名</param>
        /// <returns>绝对路径。</returns>
        public string getExternalSoundPath(string path)
        {
            return Path.Combine(_persistentDataPath, soundDirName, path);
        }
        public async Task<Sprite> getSoundWaveSprite(ICardPoolReference cardPoolRef, string path)
        {
            if (cardPoolRef == null || string.IsNullOrEmpty(path))
                return null;
            if (_soundSpriteDict.TryGetValue(path, out var spr))
            {
                return spr;
            }
            var audioClip = await loadSound(cardPoolRef, path);
            if (!audioClip)
                return null;
            var tex = SEManager.GenerateWaveTexture(audioClip, lineWeight: 24);
            spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            spr.name = audioClip.name;
            _soundSpriteDict.Add(path, spr);
            return spr;
        }
        #endregion

        #region 私有方法

        #region 选择音效路径
        private Task<AudioClip> selectExternalSoundForMobile()
        {
            TaskCompletionSource<AudioClip> t = new TaskCompletionSource<AudioClip>();
            NativeFilePicker.Permission permission = NativeFilePicker.PickFile(async path =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    t.SetResult(null);
                    return;
                }

                try
                {
                    AudioClip audio = getResourceFromCache<AudioClip>(path);
                    if (audio == null)
                    {
                        audio = await loadSoundFile(path);
                        //缓存外部图片
                        addResourceToCache(path, audio);
                    }
                    //更改贴图路径为外部文件路径
                    t.SetResult(audio);
                }
                catch (Exception e)
                {
                    t.SetException(e);
                }
            }, "*/*");
            if (permission == NativeFilePicker.Permission.Denied)
            {
                Debug.LogError("没有打开文件的权限");
                // 拒绝权限后应当弹出框提示需要设置权限
                NativeFilePicker.OpenSettings();
            }
            return t.Task;
        }
        private async Task<AudioClip> selectExternalSoundForPC()
        {
            TaskCompletionSource<AudioClip> tcs = new TaskCompletionSource<AudioClip>();

            var extFilters = new ExtensionFilter[] { new ExtensionFilter("Sound Files", "wav", "mp3", "ogg") };
            string[] paths = StandaloneFileBrowser.OpenFilePanel("", "", extFilters, false);
            if (paths == null || paths.Length < 1)
            {
                Debug.Log($"没有选择文件");
                return null;
            }

            string path = paths[0];
            if (!File.Exists(path))
            {
                Debug.Log($"选择的文件 {path} 不存在");
                return null;
            }

            AudioClip audioClip = getResourceFromCache<AudioClip>(path);
            if (audioClip == null)
            {
                try
                {
                    audioClip = await loadSoundFile(path);
                    //缓存外部音频
                    addResourceToCache(path, audioClip);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw;
                }
            }
            //更改贴图路径为外部文件路径
            return audioClip;
        }
        #endregion

        #region 加载音效
        private async Task<AudioClip> loadCacheSound(string id)
        {
            byte[] bytes = await game.cache.ReadCache(ResourceType.Sound, id);
            // 如果是空数组，返回默认Sprite.
            if (bytes == null || bytes.Length <= 0)
                return null;
            return game.seManager.Decompress(bytes, id);
        }
        private async Task<AudioClip> loadFullPathSound(string path)
        {
            // 路径为空
            if (string.IsNullOrEmpty(path))
                return null;

            AudioClip audio;
            if (path.isResourcesPath(out var resourcesPath))
                audio = loadResourceAsset<AudioClip>(resourcesPath);
            else if (path.isStreamingAssetsPath(out var streamingPath))
                audio = await loadStreamingAssetSound(streamingPath);
            else
                audio = await loadExternalSound(path);

            return audio;
        }
        private async Task<AudioClip> loadStreamingAssetSound(string path)
        {
            try
            {
                var bytes = await game.fileManager.readStreamingBinaryFile(path);
                return game.seManager.Decompress(bytes);
            }
            catch (Exception e)
            {
                Debug.LogError($"加载流文件中的音频{path}失败：{e}");
            }
            return null;
        }
        private async Task<AudioClip> loadExternalSound(string path)
        {
            var fileManager = game.fileManager;
            bool isFullPath = Path.GetFullPath(path) == path;
            // 绝对路径
            if (isFullPath && fileManager.isFileExist(path))
            {
                //如果直接给出完整路径就直接从完整路径上读
                var bytes = await game.fileManager.readBinaryFile(path);
                return game.seManager.Decompress(bytes);
            }
            // 相对路径
            else if (fileManager.isPersistentFileExist(path))
            {
                // 从外部图片路径读取音效。
                var bytes = await game.fileManager.readPersistentBinaryFile(path);
                return game.seManager.Decompress(bytes);
            }
            // 实在找不到了
            Debug.LogWarning($"未找到路径为{path}的音频。");
            return null;
        }
        private Task<AudioClip> loadSoundFile(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            AudioType audioType = AudioType.UNKNOWN;
            switch (ext)
            {
                case ".wav":
                    audioType = AudioType.WAV;
                    break;
                case ".mp3":
                    audioType = AudioType.MPEG;
                    break;
                case ".ogg":
                    audioType = AudioType.OGGVORBIS;
                    break;
            }
            if (audioType == AudioType.UNKNOWN)
            {
                var desc = game.lang._(SOUND_NOT_SUPPORTED_EXTENSION, ext);
                return Task.FromException<AudioClip>(new FormatException(desc));
            }
            else
            {
                return loadSoundFile(path, audioType);
            }
        }
        private Task<AudioClip> loadSoundFile(string path, AudioType audioType)
        {
            var tcs = new TaskCompletionSource<AudioClip>();

            var uri = new Uri(path);
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri, audioType);
            var op = request.SendWebRequest();
            op.completed += a =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    tcs.SetResult(DownloadHandlerAudioClip.GetContent(request));
                }
                else
                {
                    tcs.SetException(new InvalidOperationException(request.error));
                }
            };
            return tcs.Task;
        }
        #endregion

        private string getFullSoundPath(ICardPoolReference cardPoolRef, string relativePath)
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
                    return $"sa:{Path.Combine(soundDirName, relativePath)}";
            }
            //绝对路径
            return Path.Combine(getCardPoolSoundsDirectory(cardPool), relativePath);
        }
        #endregion

        #region 属性字段
        [TranslateMsg("尝试选择不支持的音频格式的错误描述，{0}为文件扩展名")]
        public const string SOUND_NOT_SUPPORTED_EXTENSION = "音频格式{0}不受支持。";
        [TranslateMsg("尝试选择音频并压缩后，找不到压缩后的文件的错误描述")]
        public const string SOUND_COMPRESS_FAILED_FILE_NOT_FOUND = "压缩后的音频文件丢失。";
        [TranslateMsg("尝试选择时间过长的音频格式的错误描述，{0}为最大秒数，{1}为当前秒数")]
        public const string SOUND_DURATION_OUT_OF_LIMIT = "音频的长度不能超过{0:####.##}秒\n（选择的文件为{1:####.##}秒）。";
        [TranslateMsg("尝试选择压缩后大小过大的音频格式的错误描述，{0}为最大文件大小，{1}为选择的文件的大小")]
        public const string SOUND_COMPRESSED_SIZE_OUT_OF_LIMIT = "音频压缩后的大小长度不能超过{0}KB\n（当前文件为{1}KB）。";
        public string soundDirName => _soundDirName;
        public long maxSoundSize => _maxSoundSize;
        private Dictionary<string, Sprite> _soundSpriteDict = new Dictionary<string, Sprite>();
        [SerializeField]
        string _soundDirName = "Sounds";
        [SerializeField]
        string _opusExtension = "ogg";
        [SerializeField]
        float _maxSoundDuration = 10;
        [SerializeField]
        long _maxSoundSize = 131072; // 128 * 1024
        #endregion
    }
}
