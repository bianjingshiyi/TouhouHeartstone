using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public partial class PlayerInfoEditPanel: MonoBehaviour
    {

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
        public void setSteamValid(bool valid)
        {
            _steamObject.SetActive(valid);
        }
        public void setSteamState(string text, bool binded, bool interactable)
        {
            _steamStateText.text = text;
            _steamBindButton.gameObject.SetActive(!binded);
            _steamUnbindButton.gameObject.SetActive(binded);
            _steamInteractable = interactable;
            updateInteractable();
        }
        public PlayerInfoEditInfo getPlayerInfoEditInfo()
        {
            return new PlayerInfoEditInfo()
            {
                nickname = _nicknameField.text,
                newPassword = _newPassField.text,
                repeatPassword = _repeatPassField.text
            };
        }
        private void Awake()
        {
            _closeBtn?.onClick.AddListener(Hide);
            _cancelBtn?.onClick.AddListener(Hide);
            _confirmBtn?.onClick.AddListener(() => onClickConfirmBtn?.Invoke());
            _steamBindButton?.onClick.AddListener(() => onClickSteamBindBtn?.Invoke(false));
            _steamUnbindButton?.onClick.AddListener(() => onClickSteamBindBtn?.Invoke(true));
        }
        public void ShowMessage(string msg)
        {
            _hintText.text = msg;
            _hintText.transform.parent.gameObject.SetActive(true);
        }
        public void HideMessage()
        {
            _hintText.transform.parent.gameObject.SetActive(false);
        }
        public void setInteractable(bool interactable)
        {
            _globalInteractable = interactable;
            updateInteractable();
        }
        private void updateInteractable()
        {
            var global = _globalInteractable;
            var steam = _steamInteractable;
            _nicknameField.interactable = global;
            _oldPassField.interactable = global;
            _newPassField.interactable = global;
            _repeatPassField.interactable = global;

            _steamBindButton.interactable = global && steam;
            _steamUnbindButton.interactable = global && steam;

            _confirmBtn.interactable = global;
            _cancelBtn.interactable = global;
            _closeBtn.interactable = global;
        }
        public event Action onClickConfirmBtn;
        public event Action<bool> onClickSteamBindBtn;

        private bool _steamInteractable = true;
        private bool _globalInteractable = true;

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

        [Header("Steam")]
        [SerializeField]
        GameObject _steamObject;
        [SerializeField]
        Text _steamStateText;
        [SerializeField]
        Button _steamBindButton;
        [SerializeField]
        Button _steamUnbindButton;

        [Header("Buttons")]
        [SerializeField]
        Button _cancelBtn;
        [SerializeField]
        Button _confirmBtn;
    }
    public struct PlayerInfoEditInfo
    {
        public string nickname;
        public string newPassword;
        public string repeatPassword;

    }
}
