using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UI
{
    public class ChatRoom : MonoBehaviour
    {
        #region 生命周期
        protected void Awake() 
        {
            _sendButton.onClick.AddListener(onClickSendButton);
            _chatInputFieldSubmit.onSubmit.AddListener(onSubmit);
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

        private void onSubmit(string text)
        {
            onSendButtonClicked?.Invoke(text);
        }

        IEnumerator scrollToEnd()
        {
            // 调用此方法时，UI尚未重绘，需要等待一帧重绘完成后再移动到最底端
            yield return null;
            _scrollRect.verticalNormalizedPosition = 0;
        }

        #region 事件
        public event Action<string> onSendButtonClicked;
        #endregion
        #endregion

        #region 公共方法
        public void SetContent(string content)
        {
            _displayText.text = content;
        }

        public void ScrollToEnd()
        {
            StartCoroutine(scrollToEnd());
        }

        public void EmptyInputField()
        {
            _chatInputField.text = string.Empty;
        }

        #endregion
        #region 属性字段
        [SerializeField]
        private InputFieldSubmit _chatInputFieldSubmit;
        [SerializeField]
        private InputField _chatInputField;
        [SerializeField]
        private Text _displayText;
        [SerializeField]
        private Button _sendButton;
        [SerializeField]
        private ScrollRect _scrollRect;
        #endregion
    }
}
