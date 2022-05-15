using MukioI18n;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI
{
    partial class UIDialog
    {
        protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// 显示一个只有两个选项的询问对话框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="onConfirm"></param>
        /// <param name="onCancle"></param>
        public void ShowQuestion(string title, string content, Action onConfirm, Action onCancle = null)
        {
            titleText.text = title;
            contentText.text = content;

            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                onConfirm?.Invoke();
            });

            confirmButton.gameObject.SetActive(true);
            cancleButton.gameObject.SetActive(true);

            cancleButton.onClick.RemoveAllListeners();
            cancleButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                onCancle?.Invoke();
            });

            gameObject.SetActive(true);
        }

        /// <summary>
        /// 显示一个只有一个选项的提示对话框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="onConfirm"></param>
        public void ShowConfirm(string title, string content, Action onConfirm = null)
        {
            titleText.text = title;
            contentText.text = content;

            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                onConfirm?.Invoke();
            });

            cancleButton.gameObject.SetActive(false);
            confirmButton.gameObject.SetActive(true);

            gameObject.SetActive(true);
        }

        List<Button> btnList = new List<Button>();

        public void ShowOptions(string title, string content, string[] options, Action<int> onSelectOption)
        {
            titleText.text = title;
            contentText.text = content;
            cancleButton.gameObject.SetActive(false);
            confirmButton.gameObject.SetActive(false);

            for (int i = 0; i < options.Length; i++)
            {
                if (btnList.Count <= i)
                {
                    btnList.Add(Instantiate<Button>(templateButton, buttonsRectTransform));
                }

                var btn = btnList[i];
                int index = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    gameObject.SetActive(false);
                    onSelectOption?.Invoke(index);
                });
                btn.GetComponentInChildren<Text>().text = options[i];
                btn.gameObject.SetActive(true);
            }

            for (int i = options.Length; i < btnList.Count; i++)
            {
                btnList[i].gameObject.SetActive(false);
            }

            gameObject.SetActive(true);
        }
    }
}
