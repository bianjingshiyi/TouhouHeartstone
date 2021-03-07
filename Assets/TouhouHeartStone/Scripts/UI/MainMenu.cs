using BJSYGameCore.UI;
using UnityEngine;

namespace UI
{
    partial class MainMenu : UIObject
    {
        [SerializeField]
        EULAPanel _eulaPanel;

        public EULAPanel EULAPanel => _eulaPanel;

        Main main => GetComponentInParent<Main>();
        protected override void Awake()
        {
            base.Awake();
            autoInit();
            buildButtonButtonBlack.asButton.onClick.AddListener(() =>
            {
                main.display(main.Build);
            });
            quitButtonButtonBlack.asButton.onClick.AddListener(() =>
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            });
            aboutButtonButtonBlack.asButton.onClick.AddListener(() =>
            {
                main.display(main.AboutPage);
            });
            settingButtonButtonBlack.asButton.onClick.AddListener(() =>
            {
                main.display(main.SettingPage);
            });
        }
    }

}
