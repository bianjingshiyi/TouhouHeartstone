using System;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RegisterViaSteamPanel : UIObject
    {
        public void Display()
        {
            HideMessage();
            display();
        }
        public void ShowMessage(string msg)
        {
            _hintText.text = msg;
            _hintImage.gameObject.SetActive(true);
        }
        public void HideMessage()
        {
            _hintImage.gameObject.SetActive(false);
        }
        public void setInteractable(bool interactable)
        {
            _nickNameInputField.interactable = interactable;
            _emailInputField.interactable = interactable;
            _registerButton.interactable = interactable;
            _closeButton.interactable = interactable;
            _eulaButton.interactable = interactable;
        }
        public RegisterViaSteamInfo getRegisterInfo()
        {
            return new RegisterViaSteamInfo
            {
                nickName = _nickNameInputField.text,
                email = _emailInputField.text
            };
        }
        protected override void Awake()
        {
            base.Awake();
            _registerButton.onClick.AddListener(onRegisterClickedCallback);
            _closeButton.onClick.AddListener(onCloseClickedCallback);
            _eulaButton.onClick.AddListener(onShowEULAClickCallback);
        }
        private void onRegisterClickedCallback()
        {
            onRegisterClick?.Invoke();
        }
        private void onCloseClickedCallback()
        {
            hide();
        }
        private void onShowEULAClickCallback()
        {
            onShowEULAClick?.Invoke();
        }
        public event Action onRegisterClick;
        public event Action onShowEULAClick;
        [SerializeField]
        private InputField _nickNameInputField;
        [SerializeField]
        private InputField _emailInputField;
        [SerializeField]
        private Text _hintText;
        [SerializeField]
        private Image _hintImage;
        [SerializeField]
        private Button _registerButton;
        [SerializeField]
        private Button _closeButton;
        [SerializeField]
        private Button _eulaButton;
    }
    public struct RegisterViaSteamInfo
    {
        public string nickName;
        public string email;
    }
}
