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
            buildButton.onClick.AddListener(() =>
            {
                main.display(main.Build);
            });
            quitButton.onClick.AddListener(() =>
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            });
            aboutButton.onClick.AddListener(() =>
            {
                main.display(main.AboutPage);
            });
            settingButton.onClick.AddListener(() =>
            {
                main.display(main.SettingPage);
            });
            newsButton.onClick.AddListener(() => 
            {
                // 等之后有空再改
                newsPanel.gameObject.SetActive(true);
            });
        }
    }

}
