using BJSYGameCore;
using Game;

namespace UI
{
    partial class SettingPage
    {
        ConfigManager config => this.findInstance<ConfigManager>();

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
            BackButtonBlack.asButton.onClick.AddListener(() =>
            {
                config.SaveConfig();
                var parent = GetComponentInParent<Main>();
                parent.display(parent.MainMenu);
            });
        }

        private void OnEnable()
        {
            VolumeGameOptionItemSlider.Slider.value = config.Config.Volume;
            BGMGameOptionItemSlider.Slider.value = config.Config.BGM;
            SEGameOptionItemSlider.Slider.value = config.Config.SE;
        }
    }

}
