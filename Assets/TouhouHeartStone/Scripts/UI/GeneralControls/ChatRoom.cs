using System;
using UnityEngine;
using UnityEngine.UI;


namespace UI
{
    public class ChatRoom : MonoBehaviour
    {
        #region 生命周期
        protected void Awake() 
        {
            _sendButton.onClick.AddListener(onClickSendButton);

            _chatInputField = transform.Find("InputFieldDark").GetComponent<InputField>();
            _chatScrollRect = transform.Find("textshowPanel").GetComponent<ScrollRect>();
            _displayText = transform.Find("userText").GetComponent<Text>();
            _sendButton = transform.Find("sendButton").GetComponent<Button>();

        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 当点击发送按钮时
        /// </summary>
        private void onClickSendButton()
        {
            var text = _chatInputField.text;
            onSendButtonClicked?.Invoke(text);
        }

        #region 事件
        public event Action<string> onSendButtonClicked;
        public event Action<string> onMessageSend;
        #endregion

        #endregion
        #region 属性字段
        [SerializeField]
        private InputField _chatInputField;
        [SerializeField]
        private Text _displayText;
        [SerializeField]
        private Button _sendButton;
        [SerializeField]
        private ScrollRect _chatScrollRect;
        #endregion
    }
}
