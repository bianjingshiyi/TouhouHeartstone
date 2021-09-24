using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ActionNodeItemConstValue : MonoBehaviour
    {
        public void setValueInfo(ActionNodeValueInfo info)
        {
            _valueType = info.type;
            if (info.type == ActionNodeValueType.integer)
            {
                _inputField.gameObject.SetActive(true);
                _toggle.gameObject.SetActive(false);
                if (info.value != null)
                    _inputField.SetTextWithoutNotify(info.value.ToString());
                else
                    _inputField.SetTextWithoutNotify("0");
            }
            else if (info.type == ActionNodeValueType.real)
            {
                _inputField.gameObject.SetActive(true);
                _toggle.gameObject.SetActive(false);
                if (info.value != null)
                    _inputField.SetTextWithoutNotify(info.value.ToString());
                else
                    _inputField.SetTextWithoutNotify("0");
            }
            else if (info.type == ActionNodeValueType.boolean)
            {
                _inputField.gameObject.SetActive(false);
                _toggle.gameObject.SetActive(true);
                if (info.value != null)
                    _toggle.SetIsOnWithoutNotify((bool)info.value);
                else
                    _toggle.SetIsOnWithoutNotify(false);
            }
            else if (info.type == ActionNodeValueType.text)
            {
                _inputField.gameObject.SetActive(true);
                _toggle.gameObject.SetActive(false);
                if (info.value != null)
                    _inputField.SetTextWithoutNotify((string)info.value);
                else
                    _inputField.SetTextWithoutNotify(string.Empty);
            }
        }
        protected void Awake()
        {
            _inputField.onEndEdit.AddListener(onEndEditInputField);
            _toggle.onValueChanged.AddListener(onToggleValueChanged);
        }
        private void onEndEditInputField(string text)
        {
            if (_valueType == ActionNodeValueType.integer)
            {
                if (int.TryParse(text, out int value))
                {
                    onValueChanged?.Invoke(this, value);
                }
                else
                {
                    //非法值
                    _inputField.SetTextWithoutNotify("0");
                }
            }
            else if (_valueType == ActionNodeValueType.real)
            {
                if (float.TryParse(text, out float value))
                {
                    onValueChanged?.Invoke(this, value);
                }
                else
                {
                    _inputField.SetTextWithoutNotify("0");
                }
            }
            else if (_valueType == ActionNodeValueType.text)
            {
                onValueChanged?.Invoke(this, text);
            }
        }
        private void onToggleValueChanged(bool value)
        {
            onValueChanged?.Invoke(this, value);
        }
        public event Action<ActionNodeItemConstValue, object> onValueChanged;
        [SerializeField]
        InputField _inputField;
        [SerializeField]
        Toggle _toggle;
        [SerializeField]
        ActionNodeValueType _valueType;
    }
}