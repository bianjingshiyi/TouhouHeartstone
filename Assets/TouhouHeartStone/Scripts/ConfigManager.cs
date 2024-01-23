using System;
using System.Collections.Generic;
using System.Globalization;
using BJSYGameCore;
using UnityEngine;
using UnityEngine.Audio;

namespace Game
{
    public class ConfigManager : Manager
    {
        #region 公有方法
        public void SaveConfig()
        {
            PlayerPrefs.SetFloat(keyVolume, Config.Volume);
            PlayerPrefs.SetFloat(keyBGM, Config.BGM);
            PlayerPrefs.SetFloat(keySE, Config.SE);
            PlayerPrefs.SetInt(keyFullScreen, Config.FullScreen ? 1 : 0);
            PlayerPrefs.SetString(keyLanguage, Config.Language);
            PlayerPrefs.SetFloat(keyCache, Config.CacheSize);
            PlayerPrefs.SetInt(keyNodeSnap, Config.NodeSnap ? 1 : 0);
        }
        public void MarkChange()
        {
            updateVolumeMixer();
            lang.CurrentCulture = new CultureInfo(Config.Language);
            Screen.fullScreen = Config.FullScreen;
            OnConfigChange?.Invoke(Config);
        }
        public bool isTutorialFinished(TutorialID id)
        {
            if (finishedTutorials.TryGetValue(id, out var value))
                return value;
            return false;
        }
        public void setTutorialFinished(TutorialID id, bool value)
        {
            finishedTutorials[id] = value;
            var key = getTutorialKey(id);
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }
        public void clearTutorialFinishedData()
        {
            foreach (var pair in finishedTutorials)
            {
                var id = pair.Key;
                var key = getTutorialKey(id);
                PlayerPrefs.SetInt(key, 0);
            }
            finishedTutorials.Clear();
        }
        #endregion

        #region 私有方法

        #region 生命周期
        private void Start()
        {
            loadConfig();
        }
        #endregion

        #region 音量
        private void updateVolumeMixer()
        {
            setVol(keyVolume, Config.Volume);
            setVol(keySE, Config.SE);
            setVol(keyBGM, Config.BGM);
        }
        private void setVol(string key, float vol)
        {
            mixer.SetFloat(key, percentageToDbA(vol));
        }
        private float percentageToDbA(float p)
        {
            if (p == 0) return -80;
            return 20 * Mathf.Log10(p);
        }
        #endregion

        private void loadConfig()
        {
            Config.Volume = PlayerPrefs.HasKey(keyVolume) ? PlayerPrefs.GetFloat(keyVolume) : 1;
            Config.BGM = PlayerPrefs.HasKey(keyBGM) ? PlayerPrefs.GetFloat(keyBGM) : 1;
            Config.SE = PlayerPrefs.HasKey(keySE) ? PlayerPrefs.GetFloat(keySE) : 1;
            Config.FullScreen = (PlayerPrefs.HasKey(keyFullScreen) ? PlayerPrefs.GetInt(keyFullScreen, 0) : 0) == 1;
            Config.Language = PlayerPrefs.HasKey(keyLanguage) ? PlayerPrefs.GetString(keyLanguage) : lang.CurrentCulture.Name;
            Config.CacheSize = PlayerPrefs.HasKey(keyCache) ? PlayerPrefs.GetFloat(keyCache) : 0.5f;
            Config.NodeSnap = (PlayerPrefs.HasKey(keyNodeSnap) ? PlayerPrefs.GetInt(keyNodeSnap, 0) : 1) == 1;

            foreach (var tutorialId in tutorialID)
            {
                var key = getTutorialKey(tutorialId);
                finishedTutorials.Add(tutorialId, (PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key, 0) : 0) == 1);
            }

            MarkChange();
        }
        private string getTutorialKey(TutorialID id)
        {
            return string.Format(keyTutorialFinishedTemplate, (int)id);
        }

        #endregion

        #region 事件
        public event Action<PlayerConfig> OnConfigChange;
        #endregion

        #region 属性字段
        const string keyVolume = "Volume";
        const string keyBGM = "BGM";
        const string keySE = "SE";
        const string keyFullScreen = "FullScreen";
        const string keyLanguage = "Language";
        const string keyCache = "Cache Size";
        const string keyNodeSnap = "NodeSnap";

        const string keyTutorialFinishedTemplate = "TutorialFinished_{0}";
        public PlayerConfig Config { get; } = new PlayerConfig();
        LangManager lang => getManager<LangManager>();
        private Dictionary<TutorialID, bool> finishedTutorials = new Dictionary<TutorialID, bool>();
        private static readonly TutorialID[] tutorialID = new TutorialID[]
        {
            TutorialID.Basic
        };
        [SerializeField]
        AudioMixer mixer;
        #endregion

        #region 内嵌类
        public class PlayerConfig
        {
            public float Volume;
            public float BGM;
            public float SE;
            public bool FullScreen;
            public string Language;
            public float CacheSize;
            public bool NodeSnap;
        }

        #endregion
    }

    public enum TutorialID
    {
        Basic = 1
    }
}
