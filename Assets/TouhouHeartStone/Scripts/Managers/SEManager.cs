using UnityEngine;
using BJSYGameCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public partial class SEManager : Manager
    {
        #region 公有方法
        public void PlaySE(string name, float pitch = 1)
        {
            var clip = GetSE(name);
            if (clip != null)
                playSE(clip, pitch);
            else
                Debug.LogWarning($"Unable to find audio: {name}");
        }
        public void playSE(AudioClip clip, float pitch = 1, bool loop = false)
        {
            createSource(_seSourceTemplate, clip, pitch, loop);
        }
        public void playCustomSE(AudioClip clip, float pitch = 1, bool loop = false)
        {
            createSource(_customSourceTemplate, clip, pitch, loop);
        }
        public void stopSE(AudioClip clip)
        {
            if (clip == null)
                return;
            var sources = _seSourceList.Where(s => s.clip == clip).ToArray();
            foreach (var source in sources)
            {
                removeSource(source);
            }
        }
        public void fadeoutSE(AudioClip clip, float time)
        {
            if (clip == null)
                return;
            var sources = _seSourceList.Where(s => s.clip == clip).ToArray();
            foreach (var source in sources)
            {
                source.fadeOut(time);
            }
        }
        public void stopAllLoopingSE()
        {
            var sources = _seSourceList.Where(s => s.loop).ToArray();
            foreach (var source in sources)
            {
                removeSource(source);
            }
        }
        public AudioClip GetSE(string name)
        {
            if (se == null)
                return null;
            foreach (var item in se)
            {
                if (item.name == name)
                {
                    return item.audio;
                }
            }
            return null;
        }
        public byte[] Compress(AudioClip clip)
        {
            return EncodeOpus(clip);
        }
        public AudioClip Decompress(byte[] bytes, string audioName = null)
        {
            return DecodeOpus(bytes, audioName);
        }
        #endregion
        #region 私有方法

        #region 生命周期
        private void Update()
        {
            _frameSECache.Clear();
        }
        #endregion

        #region 事件回调
        private void onSourceFinished(SESource source)
        {
            removeSource(source);
        }
        #endregion
        
        public void createSource(SESource template, AudioClip clip, float pitch = 1, bool loop = false)
        {
            if (clip == null)
                return;
            SESource source;
            if (_frameSECache.TryGetValue(clip, out var before))
            {
                source = before;
            }
            else
            {
                source = Instantiate(template.gameObject, _seSourceRoot).GetComponent<SESource>();
                source.gameObject.SetActive(true);
                source.onFinished += onSourceFinished;
                source.name = clip.name;
                source.clip = clip;
                _seSourceList.Add(source);
                _frameSECache.Add(clip, source);
            }
            source.loop = loop;
            source.pitch = pitch;
            source.play();
        }
        private void removeSource(SESource source)
        {
            _seSourceList.Remove(source);
            source.onFinished -= onSourceFinished;
            source.remove();
        }
        #endregion
        #region 属性字段
        [SerializeField]
        Transform _seSourceRoot;
        [SerializeField]
        SESource _seSourceTemplate;
        [SerializeField]
        SESource _customSourceTemplate;
        [SerializeField]
        List<SESource> _seSourceList;
        [SerializeField]
        SEItem[] se;
        /// <summary>
        /// 本帧中已经被播放的音频字典。防止同一个音效在同一帧内被播放多次导致非常吵的问题。
        /// </summary>
        Dictionary<AudioClip, SESource> _frameSECache = new Dictionary<AudioClip, SESource>();
        #endregion
        #region 内嵌类
        [Serializable]
        public struct SEItem
        {
            public string name;
            public AudioClip audio;
        }
        #endregion
    }
}
