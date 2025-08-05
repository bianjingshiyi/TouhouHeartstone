using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MukioI18n;
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
#if !UNITY_EDITOR && UNITY_ANDROID
            clip = await selectExternalSoundForMobile();
#else
            clip = await selectExternalSoundForPC();
#endif
            if (clip != null)
            {
                if (clip.length > _maxSoundDuration)
                {
                    var desc = game.lang._(SOUND_DURATION_OUT_OF_LIMIT, _maxSoundDuration, clip.length);
                    throw new FormatException(desc);
                }
                else if (clip.samples == 0)
                {
                    var desc = game.lang._(SOUND_EMPTY);
                    throw new FormatException(desc);
                }
            }
            return clip;
        }
        public Task<string> saveSound(long cardPoolId, AudioClip audio)
        {
            byte[] bytes = game.seManager.Compress(audio);
            return saveResourceBytes(cardPoolId, bytes, ResourceType.Sound, _opusExtension);
        }
        public Task<AudioClip> loadSound(ICardPoolReference cardPoolRef, string soundPath)
        {
            return loadResource<AudioClip>(cardPoolRef, ResourceType.Sound, soundPath);
        }
        public bool isExternalSoundExists(string path)
        {
            return game.fileManager.isFileExist(getExternalSoundPath(path));
        }
        /// <summary>
        /// 获取外部音频的目录路径。
        /// </summary>
        /// <returns>目录路径。</returns>
        public string getExternalSoundDirectory()
        {
            return getExternalResourceDirectory(ResourceType.Sound);
        }
        /// <summary>
        /// 获取外部音频的绝对路径。
        /// </summary>
        /// <param name="path">音频名</param>
        /// <returns>绝对路径。</returns>
        public string getExternalSoundPath(string path)
        {
            return getExternalResourcePath(ResourceType.Sound, path);
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
#if !UNITY_EDITOR && UNITY_ANDROID
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
#else
        private async Task<AudioClip> selectExternalSoundForPC()
        {
            TaskCompletionSource<AudioClip> tcs = new TaskCompletionSource<AudioClip>();

            var extFilters = new SFB.ExtensionFilter[] { new SFB.ExtensionFilter("Sound Files", "wav", "mp3", "ogg") };
            string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("", "", extFilters, false);
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
#endif
        #endregion

        #region 加载音效
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

#endregion

        #region 属性字段
        [TranslateMsg("尝试选择不支持的音频格式的错误描述，{0}为文件扩展名")]
        public const string SOUND_NOT_SUPPORTED_EXTENSION = "音频格式{0}不受支持。";
        [TranslateMsg("尝试选择音频并压缩后，找不到压缩后的文件的错误描述")]
        public const string SOUND_COMPRESS_FAILED_FILE_NOT_FOUND = "压缩后的音频文件丢失。";
        [TranslateMsg("尝试选择时间过长的音频格式的错误描述，{0}为最大秒数，{1}为当前秒数")]
        public const string SOUND_DURATION_OUT_OF_LIMIT = "音频的长度不能超过{0:####.##}秒\n（选择的文件为{1:####.##}秒）。";
        [TranslateMsg("读取的音频没有任何采样的错误描述")]
        public const string SOUND_EMPTY = "音频读取失败，音频格式可能不正确。";
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
