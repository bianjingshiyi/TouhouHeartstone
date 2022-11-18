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
            _chatInputField = transform.Find("InputFieldDark").GetComponent<InputField>();
            _chatScrollRect = transform.Find("viewPort/textshowPanel").GetComponent<ScrollRect>();
            _displayText = transform.Find("viewPort/textshowPanel/userText").GetComponent<Text>();
            _sendButton = transform.Find("sendButton").GetComponent<Button>();

            _chatInputField.onEndEdit.AddListener(onInputFieldEndEdit);
            _sendButton.onClick.AddListener(onClickSendButton);

        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 当点击发送按钮时
        /// </summary>
        private void onClickSendButton(/*string userName ,*/)
        {
            onClickedSendButton?.Invoke();
        }

        private void onInputFieldEndEdit(string text) 
        {
            
            onInputFieldEditEnded?.Invoke(text);
        }

        private void onMessageSent(string text)
        {
            _displayText.text = text;
            onMessageSend?.Invoke(text);
        }


        #region 事件
        public event Action onClickedSendButton;
        public event Action<string> onInputFieldEditEnded;
        public event Action<string> onMessageSend;
        #endregion

        #endregion
        #region 属性字段
        private InputField _chatInputField;
        private Text _displayText;
        private Button _sendButton;
        private ScrollRect _chatScrollRect;
        #endregion
    }
}
