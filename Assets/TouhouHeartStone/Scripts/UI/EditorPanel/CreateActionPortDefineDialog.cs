using System;
using System.Linq;
using MukioI18n;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CreateActionPortDefineDialog : MonoBehaviour
    {
        public void setTypes((Type type, string name)[] types)
        {
            _propTypes = types;
            _typeDropdown.ClearOptions();
            _typeDropdown.AddOptions(types.Select(a => a.name).ToList());
        }
        public void display(ActionPortDefineType portType, string title, bool canBeParams)
        {
            gameObject.SetActive(true);
            _portType = portType;
            _title.text = title;
            _propertyName.text = string.Empty;
            _isArrayToggle.isOn = false;
            _isParamsToggle.isOn = false;
            _isParamsToggle.interactable = canBeParams;
            _errorTipObj.SetActive(false);
        }
        public void hide()
        {
            gameObject.SetActive(false);
        }
        public void showErrorTip(string text)
        {
            _errorTipObj.SetActive(true);
            _errorTipText.text = text;
        }
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _confirmButton.onClick.AddListener(onConfirmClicked);
            _cancelButton.onClick.AddListener(onCancelClicked);
        }
        #endregion
        #region 事件回调
        private void onConfirmClicked()
        {
            var propItem = ToPortDefineItem();
            onConfirm?.Invoke(propItem);
        }
        private void onCancelClicked()
        {
            hide();
        }
        #endregion
        private ActionPortDefineItem ToPortDefineItem()
        {
            Type type = typeof(object);
            var index = _typeDropdown.value;
            if (index >= 0 && index < _propTypes.Length)
                type = _propTypes[index].type;
            var item = new ActionPortDefineItem(_propertyName.text, _portType, type, _isArrayToggle.isOn, _isParamsToggle.isOn);
            return item;
        }
        #endregion

        #region 事件
        public event Action<ActionPortDefineItem> onConfirm;
        #endregion

        private (Type type, string name)[] _propTypes;
        private ActionPortDefineType _portType;

        [SerializeField]
        private Text _title;
        [SerializeField]
        private InputField _propertyName;
        [SerializeField]
        private Dropdown _typeDropdown;
        [SerializeField]
        private Toggle _isArrayToggle;
        [SerializeField]
        private Toggle _isParamsToggle;
        [SerializeField]
        private GameObject _errorTipObj;
        [SerializeField]
        private Text _errorTipText;
        [SerializeField]
        private Button _confirmButton;
        [SerializeField]
        private Button _cancelButton;
    }
}