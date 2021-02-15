using BJSYGameCore.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 具有标题，内容，可生成选项的对话框
    /// </summary>
    class GenericDialog : UIObject
    {
        public void display(string title, string info, string[] options, Action<int> onSelectOption)
        {
            display();
            if (string.IsNullOrEmpty(title))
                _titleText.gameObject.SetActive(false);
            else
            {
                _titleText.gameObject.SetActive(true);
                _titleText.text = title;
            }
            if (string.IsNullOrEmpty(info))
                _contentText.gameObject.SetActive(false);
            else
            {
                _contentText.gameObject.SetActive(true);
                _contentText.text = info;
            }
            for (int i = 0; i < options.Length; i++)
            {
                Button button;
                if (_buttonList.Count > i)
                    button = _buttonList[i];
                else
                {
                    button = Instantiate(_buttonTemplate, _buttonGroup.transform);
                    _buttonList.Add(button);
                }
                button.gameObject.SetActive(true);
                button.onClick.RemoveAllListeners();
                int localI = i;
                button.onClick.AddListener(() =>
                {
                    onSelectOption?.Invoke(localI);
                    hide();
                });
            }
            for (int i = options.Length; i < _buttonList.Count; i++)
            {
                _buttonList[i].gameObject.SetActive(false);
            }
        }
        [SerializeField]
        Text _titleText;
        [SerializeField]
        Text _contentText;
        [SerializeField]
        LayoutGroup _buttonGroup;
        [SerializeField]
        Button _buttonTemplate;
        [SerializeField]
        List<Button> _buttonList;
    }
}
