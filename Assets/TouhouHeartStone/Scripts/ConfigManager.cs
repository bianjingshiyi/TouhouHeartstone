using System;
using UnityEngine;
using BJSYGameCore;
using UnityEngine.Audio;

namespace Game
{
    public class ConfigManager : Manager
    {
        #region config
        const string keyVolume = "Volume";
        const string keyBGM = "BGM";
        const string keySE = "SE";

        public class PlayerConfig
        {
            public float Volume;
            public float BGM;
            public float SE;
        }

        public PlayerConfig Config { get; } = new PlayerConfig();

        private void Start()
        {
            loadConfig();
        }

        void loadConfig()
        {
            Config.Volume = PlayerPrefs.HasKey(keyVolume) ? PlayerPrefs.GetFloat(keyVolume) : 1;
            Config.BGM = PlayerPrefs.HasKey(keyBGM) ? PlayerPrefs.GetFloat(keyBGM) : 1;
            Config.SE = PlayerPrefs.HasKey(keySE) ? PlayerPrefs.GetFloat(keySE) : 1;

            MarkChange();
        }

        public void SaveConfig()
        {
            PlayerPrefs.SetFloat(keyVolume, Config.Volume);
            PlayerPrefs.SetFloat(keyBGM, Config.BGM);
            PlayerPrefs.SetFloat(keySE, Config.SE);
        }

        public void MarkChange()
        {
            updateVolumeMixer();
            OnConfigChange?.Invoke(Config);
        }

        public event Action<PlayerConfig> OnConfigChange;
        #endregion
        #region Volume
        [SerializeField]
        AudioMixer mixer;

        void updateVolumeMixer()
        {
            setVol(keyVolume, Config.Volume);
            setVol(keySE, Config.SE);
            setVol(keyBGM, Config.BGM);
        }
        void setVol(string key, float vol)
        {
            mixer.SetFloat(key, percentageToDbA(vol));
        }

        float percentageToDbA(float p)
        {
            if (p == 0) return -80;
            return 20 * Mathf.Log10(p);
        }

        #endregion Volume
    }
}
