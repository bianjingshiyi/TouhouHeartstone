using BJSYGameCore;
using Game;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using MukioI18n;

namespace UI
{
    partial class SettingPage
    {
        ConfigManager config => this.findInstance<ConfigManager>();
        CacheManager cache => this.findInstance<CacheManager>();
        LangManager lang => this.findInstance<LangManager>();

        List<string> langItemList = new List<string>();
        
        [SerializeField]
        GameOptionItemSlider _cacheSlider;

        UnityEngine.UI.Dropdown langDropdown => LanguageGameOptionItemDropdown.DropdownLight.asDropdown;
        partial void onAwake()
        {
            VolumeGameOptionItemSlider.Slider.onValueChanged.AddListener((vol) =>
            {
                config.Config.Volume = vol;
                updateLabels();
                config.MarkChange();
            });
            BGMGameOptionItemSlider.Slider.onValueChanged.AddListener((vol) =>
            {
                config.Config.BGM = vol;
                updateLabels();
                config.MarkChange();
            });
            SEGameOptionItemSlider.Slider.onValueChanged.AddListener((vol) =>
            {
                config.Config.SE = vol;
                updateLabels();
                config.MarkChange();
            });
            _cacheSlider.Slider.onValueChanged.AddListener((cache) =>
            {
                if (cache == 20)
                {
                    config.Config.CacheSize = -1;
                }
                else
                {
                    config.Config.CacheSize = cache / 2;
                }
                updateLabels();
                config.MarkChange();
            });
            BackButtonBlack.asButton.onClick.AddListener(() =>
            {
                config.SaveConfig();
                var parent = GetComponentInParent<Main>();
                parent.display(parent.MainMenu);
            });
            FullScreenGameOptionItemToggle.Toggle.onValueChanged.AddListener(onFullScreenToggleValueChange);

            langDropdown.onValueChanged.AddListener((val) =>
            {
                config.Config.Language = langItemList[val];
                config.MarkChange();
            });

            langDropdown.ClearOptions();
            langItemList.Clear();
            foreach (var culture in lang.Cultures)
            {
                langItemList.Add(culture.Name);
                langDropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData(culture.NativeName));
            }
            config.OnConfigChange += OnConfigChange;
        }

        private void updateLabels()
        {
            VolumeGameOptionItemSlider.DescText.text = $"{System.Math.Floor(config.Config.Volume * 100)}%";
            BGMGameOptionItemSlider.DescText.text = $"{System.Math.Floor(config.Config.BGM * 100)}%";
            SEGameOptionItemSlider.DescText.text = $"{System.Math.Floor(config.Config.SE * 100)}%";
            if (config.Config.CacheSize < 0)
            {
                _cacheSlider.DescText.text = lang._(OPTIONS_CACHE_UNLIMITED);
            }
            else
            {
                _cacheSlider.DescText.text = lang._(CACHE_SIZE, config.Config.CacheSize);
            }
        }

        private void OnEnable()
        {
            _isUpdating = true;
            VolumeGameOptionItemSlider.Slider.value = config.Config.Volume;
            BGMGameOptionItemSlider.Slider.value = config.Config.BGM;
            SEGameOptionItemSlider.Slider.value = config.Config.SE;

            FullScreenGameOptionItemToggle.Toggle.isOn = config.Config.FullScreen;
            if (config.Config.CacheSize < 0)
            {
                _cacheSlider.Slider.value = 20;
            }
            else
            {
                _cacheSlider.Slider.value = config.Config.CacheSize * 2;
            }

            for (int i = 0; i < langItemList.Count; i++)
            {
                if (config.Config.Language == langItemList[i])
                {
                    langDropdown.value = i;
                    break;
                }
            }
            updateLabels();

            _isUpdating = false;
        }
        private void onFullScreenToggleValueChange(bool value)
        {
            if (_isUpdating)
                return;
            config.Config.FullScreen = value;
            config.MarkChange();
        }
        private void OnConfigChange(PlayerConfig config)
        {
            if (config.CacheSize < 0)
            {
                cache.MaxSize = -1;
            }
            else
            {
                cache.MaxSize = (long)Mathf.RoundToInt(config.CacheSize * 2) * 536870912;//512MB
            }
        }
        bool _isUpdating = false;
        [TranslateMsg("设置-最大缓存大小-无限制")]
        const string OPTIONS_CACHE_UNLIMITED = "无限制";
        [TranslateMsg("缓存大小单位")]
        const string CACHE_SIZE = "{0}G";
    }
}
