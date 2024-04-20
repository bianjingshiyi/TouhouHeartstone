using System.Collections.Generic;
using BJSYGameCore;
using BJSYGameCore.UI;
using Game;
using MukioI18n;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SettingPage : UIObject
    {
        #region 私有方法
        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            _VolumeGameOptionItemSlider.Slider.onValueChanged.AddListener((vol) =>
            {
                config.Config.Volume = vol;
                updateLabels();
                config.MarkChange();
            });
            _BGMGameOptionItemSlider.Slider.onValueChanged.AddListener((vol) =>
            {
                config.Config.BGM = vol;
                updateLabels();
                config.MarkChange();
            });
            _SEGameOptionItemSlider.Slider.onValueChanged.AddListener((vol) =>
            {
                config.Config.SE = vol;
                updateLabels();
                config.MarkChange();
            });
            _customSEGameOptionItemSlider.Slider.onValueChanged.AddListener((vol) =>
            {
                config.Config.CustomSE = vol;
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
            _BackButtonBlack.asButton.onClick.AddListener(() =>
            {
                config.SaveConfig();
                var parent = GetComponentInParent<Main>();
                parent.display(parent.MainMenu);
            });
            _FullScreenGameOptionItemToggle.Toggle.onValueChanged.AddListener(onFullScreenToggleValueChange);

            langDropdown.onValueChanged.AddListener((val) =>
            {
                config.Config.Language = langItemList[val];
                config.MarkChange();
                updatePageTabs();
            });

            langDropdown.ClearOptions();
            langItemList.Clear();
            foreach (var culture in lang.Cultures)
            {
                langItemList.Add(culture.Name);
                langDropdown.options.Add(new Dropdown.OptionData(culture.NativeName));
            }
            _clearCacheButton.onClick.AddListener(() =>
            {
                string desc = $"{main.lang._(SETTING_CLEAR_CACHE_WARNING)}\n{main.lang._(SETTING_CLEAR_CACHE_SIZE, (double)main.cache.GetCacheSize() / 1024 / 1024)}\n{main.lang._(SETTING_CLEAR_CACHE_WARNING_2)}";

                main.ui.dialog.ShowQuestion(main.lang._(StringTable.WARNING), desc, () =>
                {
                    main.cache.ClearCache();
                    main.card.clearCachedCardPools();
                    main.ui.dialog.ShowConfirm(main.lang._(StringTable.HINT_TITLE), main.lang._(SETTING_CACHE_CLEARED));
                });
            });
            _nodeSnapToggle.Toggle.onValueChanged.AddListener(onNodeSnapToggleValueChanged);
        }
        protected override void onDisplay()
        {
            _isUpdating = true;
            _VolumeGameOptionItemSlider.Slider.value = config.Config.Volume;
            _BGMGameOptionItemSlider.Slider.value = config.Config.BGM;
            _SEGameOptionItemSlider.Slider.value = config.Config.SE;
            _customSEGameOptionItemSlider.Slider.value = config.Config.CustomSE;
            _FullScreenGameOptionItemToggle.Toggle.isOn = config.Config.FullScreen;
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
            _nodeSnapToggle.Toggle.isOn = config.Config.NodeSnap;
            updateLabels();

            resetPage();

            _isUpdating = false;
        }
        #endregion
        #region 事件回调
        private void onFullScreenToggleValueChange(bool value)
        {
            if (_isUpdating)
                return;
            config.Config.FullScreen = value;
            config.MarkChange();
        }
        private void onNodeSnapToggleValueChanged(bool value)
        {
            if (_isUpdating)
                return;
            config.Config.NodeSnap = value;
            config.MarkChange();
        }
        private void onPageTabClickCallback(SettingPageTab tab)
        {
            var index = _pageTabs.indexOf(tab);
            setPage(index);
        }
        #endregion
        private void updatePageTabs()
        {
            _pageTabs.updateList(pageNames.Length, (i, rect) =>
            {
                var settingPageTab = rect.GetComponent<SettingPageTab>();
                settingPageTab.setName(main.lang._p(CONTEXT_SETTING_PAGE, pageNames[i]));
                settingPageTab.setSelected(i == _selectedPage);
            },
            rect =>
            {
                var settingPageTab = rect.GetComponent<SettingPageTab>();
                settingPageTab.onClick += onPageTabClickCallback;
            },
            rect =>
            {
                var settingPageTab = rect.GetComponent<SettingPageTab>();
                settingPageTab.onClick -= onPageTabClickCallback;
            });
        }
        private void resetPage()
        {
            setPage(0);
        }
        private void setPage(int index)
        {
            _selectedPage = index;
            updatePageTabs();
            for (int i = 0; i < _pageContents.Length; i++)
            {
                _pageContents[i].SetActive(i == index);
            }
        }
        private void updateLabels()
        {
            _VolumeGameOptionItemSlider.DescText.text = $"{System.Math.Floor(config.Config.Volume * 100)}%";
            _BGMGameOptionItemSlider.DescText.text = $"{System.Math.Floor(config.Config.BGM * 100)}%";
            _SEGameOptionItemSlider.DescText.text = $"{System.Math.Floor(config.Config.SE * 100)}%";
            _customSEGameOptionItemSlider.DescText.text = $"{System.Math.Floor(config.Config.CustomSE * 100)}%";
            if (config.Config.CacheSize < 0)
            {
                _cacheSlider.DescText.text = lang._(OPTIONS_CACHE_UNLIMITED);
            }
            else
            {
                _cacheSlider.DescText.text = lang._(CACHE_SIZE, config.Config.CacheSize);
            }
        }
        #endregion
        #region 字段属性
        [TranslateMsg("设置-最大缓存大小-无限制")]
        const string OPTIONS_CACHE_UNLIMITED = "无限制";
        [TranslateMsg("缓存大小单位")]
        const string CACHE_SIZE = "{0}G";
        [TranslateMsg("设置界面-删除缓存前提示")]
        const string SETTING_CLEAR_CACHE_WARNING = "你确定要清除所有的卡池缓存文件吗？";
        [TranslateMsg("设置界面-删除缓存前提示")]
        const string SETTING_CLEAR_CACHE_WARNING_2 = "清除之后需要重新下载来自网络的卡池文件";
        [TranslateMsg("设置界面-删除缓存前提示（{0}是当前缓存的大小（单位为MB））")]
        const string SETTING_CLEAR_CACHE_SIZE = "当前缓存大小：{0:0.00}MB";
        [TranslateMsg("设置界面-删除缓存后提示")]
        const string SETTING_CACHE_CLEARED = "缓存文件已清除";


        const string CONTEXT_SETTING_PAGE = "设置面板的页面";
        [TranslateMsg("设置界面-页面名称", CONTEXT_SETTING_PAGE)]
        const string SETTING_PAGE_NAME_SYSTEM = "系统";
        [TranslateMsg("设置界面-页面名称", CONTEXT_SETTING_PAGE)]
        const string SETTING_PAGE_NAME_AUDIO = "音频";
        [TranslateMsg("设置界面-页面名称", CONTEXT_SETTING_PAGE)]
        const string SETTING_PAGE_NAME_EDITOR = "编辑器";

        MainManager main => this.findInstance<MainManager>();
        ConfigManager config => this.findInstance<ConfigManager>();
        LangManager lang => this.findInstance<LangManager>();

        List<string> langItemList = new List<string>();
        Dropdown langDropdown => _LanguageGameOptionItemDropdown.DropdownLight.asDropdown;

        private static readonly string[] pageNames =
        {
            SETTING_PAGE_NAME_SYSTEM,
            SETTING_PAGE_NAME_AUDIO,
            SETTING_PAGE_NAME_EDITOR
        };
        bool _isUpdating = false;
        private int _selectedPage;
        [SerializeField()]
        private GameOptionItemDropdown _LanguageGameOptionItemDropdown;
        [SerializeField()]
        private GameOptionItemSlider _VolumeGameOptionItemSlider;
        [SerializeField()]
        private GameOptionItemSlider _BGMGameOptionItemSlider;
        [SerializeField()]
        private GameOptionItemSlider _SEGameOptionItemSlider;
        [SerializeField()]
        private GameOptionItemSlider _customSEGameOptionItemSlider;
        [SerializeField()]
        private GameOptionItemToggle _FullScreenGameOptionItemToggle;
        [SerializeField()]
        private ButtonBlack _BackButtonBlack;
        [SerializeField]
        GameOptionItemSlider _cacheSlider;
        [SerializeField]
        private Button _clearCacheButton;
        [SerializeField]
        private GameOptionItemToggle _nodeSnapToggle;

        [Header("Pages")]
        [SerializeField]
        private GameObject[] _pageContents;
        [SerializeField]
        private ElementList _pageTabs;

        #endregion
    }
}
