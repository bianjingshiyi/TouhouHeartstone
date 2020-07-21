using UnityEngine;
using BJSYGameCore;
using System;

namespace Game
{
    [RequireComponent(typeof(AudioSource))]
    public class BGMManager : Manager
    {
        AudioSource _audio;
        AudioSource Audio { get { _audio = _audio ?? GetComponent<AudioSource>(); return _audio; } }

        [Serializable]
        public struct BGMList
        {
            public string name;
            public bool shuffle;
            public AudioClip[] musics;

            public void Randomize()
            {
                int count = musics.Length;
                for (int i = 0; i < count; i++)
                {
                    int t = UnityEngine.Random.Range(0, count);
                    var m = musics[t];
                    musics[t] = musics[i];
                    musics[i] = m;
                }
            }
        }

        [SerializeField]
        bool playOnAwake = false;

        [SerializeField]
        BGMList[] bgmLists;

        BGMList? currentList = null;

        int currentPlaybackIndex = 0;

        private void Update()
        {
            if (currentList != null)
            {
                if (Audio.clip != null &&  Audio.time >= Audio.clip.length)
                {
                    currentPlaybackIndex = (currentPlaybackIndex + 1) % currentList.Value.musics.Length;
                    Audio.clip = currentList.Value.musics[currentPlaybackIndex];
                    Audio.Play();
                }
            }
        }

        private void Start()
        {
            if (playOnAwake && bgmLists.Length > 0)
            {
                playList(bgmLists[0]);
            }
        }

        public void PlayList(string name)
        {
            foreach (var item in bgmLists)
            {
                if (item.name == name)
                {
                    playList(item);
                    return;
                }
            }
            Debug.LogWarning("Unable to find audio: " + name);
        }

        private void playList(BGMList item)
        {
            if (item.shuffle == true)
                item.Randomize();
            currentList = item;

            currentPlaybackIndex = 0;
            Audio.clip = item.musics[currentPlaybackIndex];
            Audio.Play();
        }
    }
}
