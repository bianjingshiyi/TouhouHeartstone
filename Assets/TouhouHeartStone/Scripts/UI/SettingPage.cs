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

        LangManager lang => this.findInstance<LangManager>();

        List<string> langItemList = new List<string>();

        
        [SerializeField]
        GameOptionItemSlider _cacheSlider;
        [SerializeField]
        Text _cacheDisplay;
        UnityEngine.UI.Dropdown langDropdown => LanguageGameOptionItemDropdown.DropdownLight.asDropdown;
        partial void onAwake()
        {
            VolumeGameOptionItemSlider.Slider.onValueChanged.AddListener((vol) =>
            {
                config.Config.Volume = vol;
                config.MarkChange();
            });
            BGMGameOptionItemSlider.Slider.onValueChanged.AddListener((vol) =>
            {
                config.Config.BGM = vol;
                config.MarkChange();
            });
            SEGameOptionItemSlider.Slider.onValueChanged.AddListener((vol) =>
            {
                config.Config.SE = vol;
                config.MarkChange();
            });
            _cacheSlider.Slider.onValueChanged.AddListener((cache) =>
            {
                if (cache == 20)
                {
                    config.Config.CacheSize = -1;
                    _cacheDisplay.text = lang._(OPTIONS_CACHE_UNLIMITED);
                }
                else
                {
                    config.Config.CacheSize = cache / 2;
                    _cacheDisplay.text = lang._(CACHE_SIZE, config.Config.CacheSize);
                }
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
        }

        private void OnEnable()
        {
            _isUpdating = true;
            VolumeGameOptionItemSlider.Slider.value = config.Config.Volume;
            BGMGameOptionItemSlider.Slider.value = config.Config.BGM;
            SEGameOptionItemSlider.Slider.value = config.Config.SE;
            FullScreenGameOptionItemToggle.Toggle.isOn = config.Config.FullScreen;
            if (config.Config.CacheSize == -1)
            {
                _cacheSlider.Slider.value = 20;
                _cacheDisplay.text =lang._(OPTIONS_CACHE_UNLIMITED);
            }
            else
            {
                _cacheSlider.Slider.value = config.Config.CacheSize * 2;
                _cacheDisplay.text = lang._(CACHE_SIZE, config.Config.CacheSize) ;
            }

            for (int i = 0; i < langItemList.Count; i++)
            {
                if (config.Config.Language == langItemList[i])
                {
                    langDropdown.value = i;
                    break;
                }
            }

            _isUpdating = false;
        }
        private void onFullScreenToggleValueChange(bool value)
        {
            if (_isUpdating)
                return;
            config.Config.FullScreen = value;
            config.MarkChange();
        }
        bool _isUpdating = false;
        [TranslateMsg("设置-最大缓存大小-无限制")]
        const string OPTIONS_CACHE_UNLIMITED = "无限制";
        [TranslateMsg("缓存大小单位")]
        string CACHE_SIZE = "{0}G";
    }
}
