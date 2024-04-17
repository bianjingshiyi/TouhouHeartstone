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
        public void loadConfig()
        {
            Config.Volume = PlayerPrefs.HasKey(keyVolume) ? PlayerPrefs.GetFloat(keyVolume) : 1;
            Config.BGM = PlayerPrefs.HasKey(keyBGM) ? PlayerPrefs.GetFloat(keyBGM) : 1;
            Config.SE = PlayerPrefs.HasKey(keySE) ? PlayerPrefs.GetFloat(keySE) : 1;
            Config.CustomSE = PlayerPrefs.HasKey(keyCustomSE) ? PlayerPrefs.GetFloat(keyCustomSE) : 1;
            Config.FullScreen = (PlayerPrefs.HasKey(keyFullScreen) ? PlayerPrefs.GetInt(keyFullScreen, 0) : 0) == 1;
            Config.Language = PlayerPrefs.HasKey(keyLanguage) ? PlayerPrefs.GetString(keyLanguage) : lang.getSystemLanguage();
            Config.CacheSize = PlayerPrefs.HasKey(keyCache) ? PlayerPrefs.GetFloat(keyCache) : 0.5f;
            Config.NodeSnap = (PlayerPrefs.HasKey(keyNodeSnap) ? PlayerPrefs.GetInt(keyNodeSnap, 0) : 1) == 1;
            MarkChange();
        }
        public void SaveConfig()
        {
            PlayerPrefs.SetFloat(keyVolume, Config.Volume);
            PlayerPrefs.SetFloat(keyBGM, Config.BGM);
            PlayerPrefs.SetFloat(keySE, Config.SE);
            PlayerPrefs.SetFloat(keyCustomSE, Config.CustomSE);
            PlayerPrefs.SetInt(keyFullScreen, Config.FullScreen ? 1 : 0);
            PlayerPrefs.SetString(keyLanguage, Config.Language);
            PlayerPrefs.SetFloat(keyCache, Config.CacheSize);
            PlayerPrefs.SetInt(keyNodeSnap, Config.NodeSnap ? 1 : 0);
        }
        public void MarkChange()
        {
            updateVolumeMixer();
            lang.setLanguage(Config.Language);
            Screen.fullScreen = Config.FullScreen;
            OnConfigChange?.Invoke(Config);
        }
        #endregion

        #region 私有方法

        #region 音量
        private void updateVolumeMixer()
        {
            setVol(keyVolume, Config.Volume);
            setVol(keySE, Config.SE);
            setVol(keyBGM, Config.BGM);
            setVol(keyCustomSE, Config.CustomSE);
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

        #endregion

        #region 事件
        public event Action<PlayerConfig> OnConfigChange;
        #endregion

        #region 属性字段
        const string keyVolume = "Volume";
        const string keyBGM = "BGM";
        const string keySE = "SE";
        const string keyCustomSE = "CustomSE";
        const string keyFullScreen = "FullScreen";
        const string keyLanguage = "Language";
        const string keyCache = "Cache Size";
        const string keyNodeSnap = "NodeSnap";
        public PlayerConfig Config { get; } = new PlayerConfig();
        LangManager lang => getManager<LangManager>();
        [SerializeField]
        AudioMixer mixer;
        #endregion

        #region 内嵌类
        public class PlayerConfig
        {
            public float Volume;
            public float BGM;
            public float SE;
            public float CustomSE;
            public bool FullScreen;
            public string Language;
            public float CacheSize;
            public bool NodeSnap;
        }

        #endregion
    }
}
