using BJSYGameCore.UI;
using UnityEngine;

namespace UI
{
    partial class MainMenu
    {
        [SerializeField]
        EULAPanel _eulaPanel;

        public EULAPanel EULAPanel => _eulaPanel;

        Main main => GetComponentInParent<Main>();
        partial void onAwake()
        {
            BuildButtonButtonBlack.asButton.onClick.AddListener(() =>
            {
                main.display(main.Build);
            });
            QuitButtonButtonBlack.asButton.onClick.AddListener(() =>
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            });
            AboutButtonButtonBlack.asButton.onClick.AddListener(() =>
            {
                main.display(main.AboutPage);
            });
            SettingButtonButtonBlack.asButton.onClick.AddListener(() =>
            {
                main.display(main.SettingPage);
            });
        }
    }

}
