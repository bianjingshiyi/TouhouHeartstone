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

        [SerializeField]
        PlayerInfoEditPanel _playerInfoEditPanel;

        public EULAPanel EULAPanel => _eulaPanel;

        public PlayerInfoEditPanel playerInfoEditPanel => _playerInfoEditPanel;

        Main main => GetComponentInParent<Main>();
        #region 私有方法
        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            autoInit();
#if DEBUG || DEVELOPMENT_BUILD || UNITY_EDITOR
            _pveButton.gameObject.SetActive(true);
#else
            _pveButton.gameObject.SetActive(false);
#endif
            _vsBotButton.onClick.AddListener(onVsBotButtonClickCallback);
            _pveButton.onClick.AddListener(onPVEButtonClickCallback);
            _buildCardButton.onClick.AddListener(onBuildCardButtonClickCallback);
            _replayButton.onClick.AddListener(onReplayButtonClickCallback);
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
        private void onVsBotButtonClickCallback()
        {
            onVsBotButtonClick?.Invoke();
        }
        private void onPVEButtonClickCallback()
        {
            onPVEButtonClick?.Invoke();
        }
        private void onEditorButtonClickCallback()
        {
            onEditorButtonClick?.Invoke();
        }
        private void onBuildCardButtonClickCallback()
        {
            onBuildCardButtonClick?.Invoke();
        }
        private void onReplayButtonClickCallback()
        {
            onReplayButtonClick?.Invoke();
        }
#endregion
#endregion
#region 事件
        public event Action onVsBotButtonClick;
        public event Action onPVEButtonClick;
        public event Action onEditorButtonClick;
        public event Action onBuildCardButtonClick;
        public event Action onReplayButtonClick;
#endregion
        [SerializeField]
        Button _vsBotButton;
        [SerializeField]
        Button _pveButton;
        [SerializeField]
        Button _buildCardButton;
        [SerializeField]
        Button _replayButton;
    }

}
