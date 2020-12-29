using MukioI18n;
using System;
namespace UI
{
    partial class UIDialog
    {
        /// <summary>
        /// 显示一个只有两个选项的询问对话框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="onConfirm"></param>
        /// <param name="onCancle"></param>
        public void ShowQuestion(string title, string content, Action onConfirm, Action onCancle = null)
        {
            TitleText.text = title;
            ContentText.text = content;

            ConfirmButtonBlack.asButton.onClick.RemoveAllListeners();
            ConfirmButtonBlack.asButton.onClick.AddListener(() => {
                onConfirm?.Invoke();
                gameObject.SetActive(false);
            });

            CancleButtonBlack.gameObject.SetActive(true);

            CancleButtonBlack.asButton.onClick.RemoveAllListeners();
            CancleButtonBlack.asButton.onClick.AddListener(() => { 
                onCancle?.Invoke(); 
                gameObject.SetActive(false);
            });

            gameObject.SetActive(true);
        }
        [TranslateMsg]
        private const String DIALOG_QUESTION = "提示";
        public void ShowQuestion(string content, Action onConfirm, Action onCancle = null)
        {
            ShowQuestion(I18n._(DIALOG_QUESTION), content, onConfirm, onCancle);
        }

        /// <summary>
        /// 显示一个只有一个选项的提示对话框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="onConfirm"></param>
        public void ShowConfirm(string title, string content, Action onConfirm)
        {
            TitleText.text = title;
            ContentText.text = content;

            ConfirmButtonBlack.asButton.onClick.RemoveAllListeners();
            ConfirmButtonBlack.asButton.onClick.AddListener(() => {
                onConfirm?.Invoke();
                gameObject.SetActive(false);
            });

            CancleButtonBlack.gameObject.SetActive(false);

            gameObject.SetActive(true);
        }

        public void ShowConfirm(string content, Action onConfirm)
        {
            ShowConfirm(I18n._(DIALOG_QUESTION), content, onConfirm);
        }
    }
}
