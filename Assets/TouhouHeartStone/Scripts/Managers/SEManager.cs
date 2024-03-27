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
            if (clip == null)
                return;
            var source = Instantiate(_seSourceTemplate.gameObject, _seSourceRoot).GetComponent<SESource>();
            source.gameObject.SetActive(true);
            source.name = clip.name;
            source.clip = clip;
            source.loop = loop;
            source.pitch = pitch;
            source.play();
            source.onFinished += onSourceFinished;
            _seSourceList.Add(source);
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
        #endregion
        #region 私有方法
        #region 事件回调
        private void onSourceFinished(SESource source)
        {
            removeSource(source);
        }
        #endregion
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
        List<SESource> _seSourceList;
        [SerializeField]
        SEItem[] se;
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
