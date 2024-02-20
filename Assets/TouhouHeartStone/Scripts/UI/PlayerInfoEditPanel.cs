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
        private void Awake()
        {
            _closeBtn?.onClick.AddListener(Hide);
            _nicknameConfirmBtn?.onClick.AddListener(() => onClickNicknameConfirmBtn?.Invoke(_nicknameField.text));
            _passwordConfirmBtn?.onClick.AddListener(() => onClickPasswordConfirmBtn?.Invoke(_newPassField.text, _repeatPassField.text));
            _steamBindButton?.onClick.AddListener(() => onClickSteamBindBtn?.Invoke(false));
            _steamUnbindButton?.onClick.AddListener(() => onClickSteamBindBtn?.Invoke(true));
        }
        public void ShowMessage(string msg)
        {
            _hintText.text = msg;
            _hintObj.SetActive(true);
        }
        public void HideMessage()
        {
            _hintObj.SetActive(false);
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

            _nicknameConfirmBtn.interactable = global;
            _passwordConfirmBtn.interactable = global;
            _closeBtn.interactable = global;
        }
        public event Action<string> onClickNicknameConfirmBtn;
        public event Action<string, string> onClickPasswordConfirmBtn;
        public event Action<bool> onClickSteamBindBtn;

        private bool _steamInteractable = true;
        private bool _globalInteractable = true;

        [SerializeField]
        Button _closeBtn;
        [SerializeField]
        GameObject _hintObj;
        [SerializeField]
        Text _hintText;

        [Header("Nickname")]
        [SerializeField]
        InputField _nicknameField;
        [SerializeField]
        Button _nicknameConfirmBtn;

        [Header("Password")]
        [SerializeField]
        InputField _oldPassField;
        [SerializeField]
        InputField _newPassField;
        [SerializeField]
        InputField _repeatPassField;
        [SerializeField]
        Button _passwordConfirmBtn;

        [Header("Steam")]
        [SerializeField]
        GameObject _steamObject;
        [SerializeField]
        Text _steamStateText;
        [SerializeField]
        Button _steamBindButton;
        [SerializeField]
        Button _steamUnbindButton;
    }
}
