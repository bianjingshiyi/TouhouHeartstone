using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MultiDropdownList : MonoBehaviour
    {
        #region 公有方法
        public void setOptionSelected(int index, bool selected)
        {
            var options = _optionsRoot.GetComponentsInChildren<Toggle>();
            if (index < 0 || index >= options.Length)
            {
                throw new IndexOutOfRangeException($"设置的多选下拉列表选项索引{index}超出范围{options.Length}。");
            }
            options[index].SetIsOnWithoutNotify(selected);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        private void OnEnable()
        {
            if (isTemplate)
                return;
            _inputField.interactable = _dropdown.interactable;
            _inputField.onEndEdit.AddListener(onEndEditCallback);
            _dropdown.onShowDropdownList(this);
        }
        private void OnDisable()
        {
            if (isTemplate)
                return;
            _inputField.onEndEdit.RemoveListener(onEndEditCallback);
            _dropdown.onHideDropdownList(this);
        }
        #endregion
        #region 事件回调
        private void onEndEditCallback(string text)
        {
            if (!_dropdown.values.Contains(text) && !string.IsNullOrEmpty(text))
            {
                _dropdown.values = _dropdown.values.Append(text).ToArray();
            }
        }
        #endregion
        #endregion
        public bool isTemplate { get; set; }
        [SerializeField]
        InputField _inputField;
        [SerializeField]
        MultiInputDropdown _dropdown;
        [SerializeField]
        Transform _optionsRoot;
    }
}