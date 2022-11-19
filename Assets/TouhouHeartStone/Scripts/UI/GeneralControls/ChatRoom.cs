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
            _chatInputField= transform.Find("InputFieldDark").GetComponent<InputField>();
            _displayText = transform.Find("viewPort").Find("textshowPanel").Find("userText").GetComponent<Text>();
            _sendButton = transform.Find("sendButton").GetComponent<Button>();
            scrollbar = transform.Find("viewPort").Find("textshowPanel").Find("ScrollbarVertical").GetComponent<Scrollbar>();
            _sendButton.onClick.AddListener(onClickSendButton);
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
        #endregion

        #endregion
        #region 属性字段
        private InputField _chatInputField;
        private Text _displayText;
        private Button _sendButton;
        private Scrollbar scrollbar;
        public InputField ChatInputField
        {
            get
            {
                return _chatInputField;
            }
            set
            {
                _chatInputField = value;
            }
        }
        public Text DisplayText 
        { 
            get 
            {
                return _displayText;
            }
            set
            {
                _displayText = value;
            }
        }
        public Scrollbar Scrollbar
        {
            get
            {
                return scrollbar;
            }
            set
            {
                scrollbar = value;
            }
        }
        #endregion
    }
}
