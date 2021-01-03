using UnityEngine;
using BJSYGameCore;
using System;
using UnityScript.Steps;
using System.Linq;

namespace Game
{
    [RequireComponent(typeof(AudioSource))]
    public class SEManager : Manager
    {
        [Serializable]
        public struct SEItem
        {
            public string name;
            public AudioClip audio;
        }

        [SerializeField]
        SEItem[] se;

        public SEItem[] SEs
        {
            get => se;
            set => se = value;
        }

        AudioSource _audio;
        AudioSource Audio { get { _audio = _audio ?? GetComponent<AudioSource>(); return _audio; } }

        public void PlaySE(string name)
        {
            var clip = GetSE(name);
            if (se != null)
                Audio.PlayOneShot(clip);
            else
                Debug.LogWarning("Unable to find audio: " + name);
        }
        public void playSE(AudioClip clip)
        {
            if (clip != null)
                Audio.PlayOneShot(clip);
        }
        public AudioClip GetSE(string name)
        {
            foreach (var item in se)
            {
                if (item.name == name)
                {
                    return item.audio;
                }
            }
            return null;
        }
        public string getSEName(AudioClip clip)
        {
            foreach (var item in se)
            {
                if (item.audio == clip)
                    return item.name;
            }
            return null;
        }
    }
}
