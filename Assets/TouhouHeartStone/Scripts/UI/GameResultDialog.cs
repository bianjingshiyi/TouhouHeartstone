using System;
using BJSYGameCore;
using BJSYGameCore.UI;
using Game;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public partial class GameResultDialog : UIObject
    {
        public void ShowGameResult(bool success)
        {
            gameObject.SetActive(true);
            _winImageObj.SetActive(success);
            _failImageObj.SetActive(!success);
            onGameEnd?.Invoke();
        }
        protected override void Awake()
        {
            base.Awake();
            _saveReplayButton.onClick.AddListener(onSaveReplayButtonClickedCallback);
            _quitButton.onClick.AddListener(onQuitButtonClickedCallback);

        }
        private void onSaveReplayButtonClickedCallback()
        {
            onSaveReplayButtonClicked?.Invoke();
        }
        private void onQuitButtonClickedCallback()
        {
            onQuitButtonClicked?.Invoke();
        }
        public event Action onQuitButtonClicked;
        public event Action onSaveReplayButtonClicked;
        public event Action onGameEnd;

        [SerializeField]
        private GameObject _winImageObj;
        [SerializeField]
        private GameObject _failImageObj;
        [SerializeField]
        private Button _saveReplayButton;
        [SerializeField]
        private Button _quitButton;
    }
}
