using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CreateCustomPropertyDialog : MonoBehaviour
    {
        public void setTypes((ActionNodeValueType type, string name)[] types)
        {
            _propTypes = types;
            _typeDropdown.ClearOptions();
            _typeDropdown.AddOptions(types.Select(a => a.name).ToList());
        }
        public void display()
        {
            gameObject.SetActive(true);
            _propertyName.text = string.Empty;
            _isArrayToggle.isOn = false;
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
            var propItem = ToCardPropertyItem();
            onConfirm?.Invoke(propItem);
        }
        private void onCancelClicked()
        {
            hide();
        }
        #endregion

        private CardPropertyItem ToCardPropertyItem()
        {
            ActionNodeValueType type = ActionNodeValueType.integer;
            var index = _typeDropdown.value;
            if (index >= 0 && index < _propTypes.Length)
                type = _propTypes[index].type;
            var item = new CardPropertyItem(_propertyName.text, type, _isArrayToggle.isOn);
            return item;
        }
        #endregion

        #region 事件
        public event Action<CardPropertyItem> onConfirm;
        #endregion

        private (ActionNodeValueType type, string name)[] _propTypes;

        [SerializeField]
        private InputField _propertyName;
        [SerializeField]
        private Dropdown _typeDropdown;
        [SerializeField]
        private Toggle _isArrayToggle;
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