using UnityEngine;
using BJSYGameCore;
using System;

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

        AudioSource _audio;
        AudioSource Audio { get { _audio = _audio ?? GetComponent<AudioSource>(); return _audio; } }

        public void PlaySE(string name)
        {
            foreach (var item in se)
            {
                if (item.name == name)
                {
                    Audio.PlayOneShot(item.audio);
                    return;
                }
            }
            Debug.LogWarning("Unable to find audio: " + name);
        }
    }
}
