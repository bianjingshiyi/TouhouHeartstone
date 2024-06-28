using System;
using BJSYGameCore.UI;
using Game.BattleTable;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public partial class GameResultDialog : UIObject
    {
        public void setReplayButtonVisible(bool visible)
        {
            _saveReplayButton.gameObject.SetActive(visible);
        }
        public void setRatingDialogVisible(bool visible)
        {
            _ratingDialog.setActive(visible);
        }
        public void setButtonGroupVisible(bool visible)
        {
            _buttonGroup.SetActive(visible);
        }
        public void ShowGameResult(bool success)
        {
            gameObject.SetActive(true);
            foreach (var obj in _winImageObjs)
            {
                obj.SetActive(success);
            }
            foreach (var obj in _failImageObjs)
            {
                obj.SetActive(!success);
            }
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

        public CardRatingDialog ratingDialog => _ratingDialog;
        [SerializeField]
        private GameObject[] _winImageObjs;
        [SerializeField]
        private GameObject[] _failImageObjs;
        [SerializeField]
        private CardRatingDialog _ratingDialog;
        [SerializeField]
        private GameObject _buttonGroup;
        [SerializeField]
        private Button _saveReplayButton;
        [SerializeField]
        private Button _quitButton;
    }
}
