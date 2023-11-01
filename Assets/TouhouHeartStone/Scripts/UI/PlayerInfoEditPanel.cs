using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public partial class PlayerInfoEditPanel: MonoBehaviour
    {
        [SerializeField]
        Button _closeBtn;

        [SerializeField]
        Text _hintText;

        [SerializeField]
        InputField _nicknameField;

        [SerializeField]
        InputField _oldPassField;

        [SerializeField]
        InputField _newPassField;

        [SerializeField]
        InputField _repeatPassField;

        [SerializeField]
        Button _cancelBtn;

        [SerializeField]
        Button _confirmBtn;

        public void Show()
        {
            HideMessage();
            gameObject.SetActive(true);

            _nicknameField.text = "";
            _oldPassField.text = "";
            _newPassField.text = "";
            _repeatPassField.text = "";
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        public PlayerInfoEditInfo getPlayerInfoEditInfo()
        {
            return new PlayerInfoEditInfo()
            {
                nickname = _nicknameField.text,
                oldPassword = _oldPassField.text,
                newPassword = _newPassField.text,
                repeatPassword = _repeatPassField.text
            };
        }
        private void Awake()
        {
            _closeBtn?.onClick.AddListener(() => Hide());
            _cancelBtn?.onClick.AddListener(() => Hide());
            _confirmBtn?.onClick.AddListener(() => Commit());
        }

        public event Action onClickConfirmBtn;

        public void ShowMessage(string msg)
        {
            _hintText.text = msg;
            _hintText.transform.parent.gameObject.SetActive(true);
        }
        public void HideMessage()
        {
            _hintText.transform.parent.gameObject.SetActive(false);
        }

        public void Commit()
        {
            onClickConfirmBtn?.Invoke();
        }
    }
    public struct PlayerInfoEditInfo
    {
        public string nickname;
        public string oldPassword;
        public string newPassword;
        public string repeatPassword;

    }
}
