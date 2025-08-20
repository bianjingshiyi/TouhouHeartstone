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
            AudioClip clip = GetSE(name);
            if (clip != null)
                playSE(clip, pitch: pitch);
            else
                Debug.LogWarning($"Unable to find audio: {name}");
        }
        public void playSE(AudioClip clip, Transform parent = null, float pitch = 1, bool loop = false, float duration = 0)
        {
            createSource(_seSourceTemplate, clip, parent: parent, pitch: pitch, loop: loop, duration: duration);
        }
        public void playCustomSE(AudioClip clip, float pitch = 1, bool loop = false)
        {
            createSource(_customSourceTemplate, clip, pitch: pitch, loop: loop);
        }
        public void stopSE(AudioClip clip)
        {
            if (clip == null)
                return;
            for (int i = 0; i < _seSourceList.Count; i++)
            {
                SESource source = _seSourceList[i];
                if (source == null)
                {
                    _seSourceList.RemoveAt(i);
                    i--;
                }
                else if (source.clip == clip)
                {
                    Destroy(source.gameObject);
                    source.onFinished -= onSourceFinished;
                    _seSourceList.RemoveAt(i);
                    i--;
                }
            }
        }
        public void fadeoutSE(AudioClip clip, float time)
        {
            if (clip == null)
                return;
            for (int i = 0; i < _seSourceList.Count; i++)
            {
                SESource source = _seSourceList[i];
                if (source == null)
                {
                    _seSourceList.RemoveAt(i);
                    i--;
                }
                else if(source.clip == clip)
                {
                    source.fadeOut(time);
                }
            }
        }
        public void stopAllLoopingSE()
        {
            for (int i = 0; i < _seSourceList.Count; i++)
            {
                SESource source = _seSourceList[i];
                if (source == null)
                {
                    _seSourceList.RemoveAt(i);
                    i--;
                }
                if (source.loop)
                {
                    source.onFinished -= onSourceFinished;
                    Destroy(source.gameObject);
                    _seSourceList.RemoveAt(i);
                    i--;
                }
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
            if (source == null)
                return;

            source.onFinished -= onSourceFinished;
            Destroy(source.gameObject);
            _seSourceList.Remove(source);
        }
        #endregion

        public void createSource(SESource template, AudioClip clip, Transform parent = null, float pitch = 1, bool loop = false, float duration = 0)
        {
            if (clip == null)
                return;
            if (!_frameSECache.TryGetValue(clip, out SESource source) || source == null)
            {
                source = Instantiate(template.gameObject, parent != null ? parent : _seSourceRoot).GetComponent<SESource>();
                source.gameObject.SetActive(true);
                source.onFinished += onSourceFinished;
                source.name = clip.name;
                source.clip = clip;
                _seSourceList.Add(source);
                _frameSECache.Add(clip, source);
                if (duration > 0)
                    Destroy(source.gameObject, duration);
            }
            source.loop = loop;
            source.pitch = pitch;
            source.play();
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
