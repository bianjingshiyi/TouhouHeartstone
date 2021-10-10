using BJSYGameCore.UI;
using System;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    partial class MainMenu : UIObject
    {
        [SerializeField]
        EULAPanel _eulaPanel;

        public EULAPanel EULAPanel => _eulaPanel;

        Main main => GetComponentInParent<Main>();
        #region 私有方法
        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            autoInit();
            _vsBotButton.onClick.AddListener(onVsBotButtonClickCallback);
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
            editorButton.onClick.AddListener(onEditorButtonClickCallback);
        }
        #endregion
        #region 事件回调
        private void onEditorButtonClickCallback()
        {
            onEditorButtonClick?.Invoke();
        }
        private void onVsBotButtonClickCallback()
        {
            onVsBotButtonClick?.Invoke();
        }
        #endregion
        #endregion
        #region 事件
        public event Action onVsBotButtonClick;
        public event Action onEditorButtonClick;
        #endregion
        [SerializeField]
        Button _vsBotButton;
    }

}
