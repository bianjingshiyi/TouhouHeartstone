using BJSYGameCore.UI;
using System;
using System.Collections.Generic;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ValueInputField : MonoBehaviour
    {
        #region 公有方法
        public void setValueInfo(ActionNodeValueInfo info)
        {
            setValueInfo(null, info);
        }
        public void setValueInfo(string name, ActionNodeValueInfo info, bool isListItem = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                _nameText.gameObject.SetActive(false);
            }
            else
            {
                _nameText.gameObject.SetActive(true);
                _nameText.text = name;
            }
            _valueInfo = info;
            _removeItemButton.gameObject.SetActive(isListItem);
            //输入框
            if (_valueInfo.type == ActionNodeValueType.integer)
            {
                _inputField.gameObject.SetActive(true);
                if (_valueInfo.value != null)
                    _inputField.SetTextWithoutNotify(_valueInfo.value.ToString());
                else
                    _inputField.SetTextWithoutNotify("0");
            }
            else if (_valueInfo.type == ActionNodeValueType.real)
            {
                _inputField.gameObject.SetActive(true);
                if (_valueInfo.value != null)
                    _inputField.SetTextWithoutNotify(_valueInfo.value.ToString());
                else
                    _inputField.SetTextWithoutNotify("0");
            }
            else if (_valueInfo.type == ActionNodeValueType.text)
            {
                _inputField.gameObject.SetActive(true);
                if (_valueInfo.value != null)
                    _inputField.SetTextWithoutNotify((string)_valueInfo.value);
                else
                    _inputField.SetTextWithoutNotify(string.Empty);
            }
            else
            {
                _inputField.gameObject.SetActive(false);
            }
            //勾选框
            if (_valueInfo.type == ActionNodeValueType.boolean)
            {
                _toggle.gameObject.SetActive(true);
                if (_valueInfo.value != null)
                    _toggle.SetIsOnWithoutNotify((bool)_valueInfo.value);
                else
                    _toggle.SetIsOnWithoutNotify(false);
            }
            else
            {
                _toggle.gameObject.SetActive(false);
            }
            //单选下拉菜单
            if (_valueInfo.type == ActionNodeValueType.enumValue)
            {
                _dropdown.gameObject.SetActive(true);
                _dropdown.ClearOptions();
                _dropdown.AddOptions(new List<string>(_valueInfo.optionalNames));
                _dropdown.SetValueWithoutNotify(Array.IndexOf(_valueInfo.optionalValues, _valueInfo.value));
            }
            else
            {
                _dropdown.gameObject.SetActive(false);
            }
            //多选下拉菜单
            if (_valueInfo.type == ActionNodeValueType.pileNameCollection)
            {
                _multiSelectDropdown.gameObject.SetActive(true);
                _multiSelectDropdown.ClearOptions();
                _multiSelectDropdown.AddOptions(new List<string>(_pileNames));
                if (_valueInfo.value is PileNameCollection pileNameCollection)
                {
                    for (int i = 0; i < pileNameCollection.Count; i++)
                    {
                        //剔除掉未知的PileName
                        if (Array.IndexOf(_pileValues, pileNameCollection[i]) < 0)
                        {
                            pileNameCollection.RemoveAt(i);
                            i--;
                        }
                    }
                    bool[] boolValue = new bool[_pileValues.Length];
                    for (int i = 0; i < boolValue.Length; i++)
                    {
                        boolValue[i] = pileNameCollection.Contains(_pileValues[i]);
                    }
                    _multiSelectDropdown.SetValueWithoutNotify(boolValue);
                }
                else
                {
                    //如果集合为null，则立刻给它新建一个空的并触发事件。
                    _valueInfo.value = new PileNameCollection();
                    _multiSelectDropdown.SetValueWithoutNotify(new bool[_pileValues.Length]);
                    onValueChanged?.Invoke(this, _valueInfo.value);
                }
            }
            else
            {
                _multiSelectDropdown.gameObject.SetActive(false);
            }
            //按钮
            if (info.type == ActionNodeValueType.actionNode ||
                info.type == ActionNodeValueType.actionValueRef ||
                info.type == ActionNodeValueType.trigger)
            {
                _button.gameObject.SetActive(true);
            }
            else
            {
                _button.gameObject.SetActive(false);
            }
            //列表
            if (info.type == ActionNodeValueType.triggerCollection)
            {
                _listRoot.gameObject.SetActive(true);
                if (info.value is TriggerCollection triggerCollection)
                {
                    UIHelper.updateList(_listRoot, _listTemplate, _list, triggerCollection.Count, (i, itemRect) =>
                    {
                        ValueInputField item = itemRect.GetComponent<ValueInputField>();
                        item.setValueInfo(null, new ActionNodeValueInfo()
                        {
                            type = ActionNodeValueType.trigger,
                            value = triggerCollection[i]
                        }, true);
                    }, itemRect =>
                    {
                        ValueInputField item = itemRect.GetComponent<ValueInputField>();
                        item.onValueChanged += onItemValueChangedCallback;
                        item.onClickButton += onClickItemButtonCallback;
                        item.onClickRemoveButton += onClickItemRemoveButtonCallback;
                    }, itemRect =>
                    {
                        ValueInputField item = itemRect.GetComponent<ValueInputField>();
                        item.onValueChanged -= onItemValueChangedCallback;
                        item.onClickButton -= onClickItemButtonCallback;
                        item.onClickRemoveButton -= onClickItemRemoveButtonCallback;
                    });
                }
                else
                {
                    _valueInfo.value = new TriggerCollection();
                    onValueChanged?.Invoke(this, _valueInfo.value);
                }
            }
            else
            {
                _listRoot.gameObject.SetActive(false);
            }
            //卡牌
            if (_valueInfo.type == ActionNodeValueType.cardReference)
            {
                _Card.gameObject.SetActive(true);
            }
            else
            {
                _Card.gameObject.SetActive(false);
            }
        }
        public void setupPileNames(string[] pileNames, string[] pileValues)
        {
            _pileNames = pileNames;
            _pileValues = pileValues;
        }
        #endregion
        #region 私有方法
        protected void Awake()
        {
            _inputField.onEndEdit.AddListener(onEndEditInputField);
            _toggle.onValueChanged.AddListener(onToggleValueChanged);
            _dropdown.onValueChanged.AddListener(onDropdownValueChangedCallback);
            _multiSelectDropdown.onValueChanged.AddListener(onMultiSelectDropdownValueChangedCallback);
            _button.onClick.AddListener(onClickButtonCallback);
            _addItemButton.onClick.AddListener(onClickAddItemButtonCallback);
            _removeItemButton.onClick.AddListener(onClickRemoveButtonCallback);
            _cardButton.onClick.AddListener(onClickCardButtonCallback);
        }
        #region 事件回调
        private void onEndEditInputField(string text)
        {
            if (_valueInfo.type == ActionNodeValueType.integer)
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
            else if (_valueInfo.type == ActionNodeValueType.real)
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
            else if (_valueInfo.type == ActionNodeValueType.text)
            {
                onValueChanged?.Invoke(this, text);
            }
        }
        private void onToggleValueChanged(bool value)
        {
            onValueChanged?.Invoke(this, value);
        }
        private void onDropdownValueChangedCallback(int index)
        {
            onValueChanged?.Invoke(this, _valueInfo.optionalValues[index]);
        }
        private void onMultiSelectDropdownValueChangedCallback(bool[] value)
        {
            if (_valueInfo.value is PileNameCollection pileNameCollection)
            {
                bool isValueChanged = false;
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] && !pileNameCollection.Contains(_pileValues[i]))
                    {
                        pileNameCollection.Add(_pileValues[i]);
                        isValueChanged = true;
                    }
                    else if (!value[i])
                    {
                        pileNameCollection.Remove(_pileValues[i]);
                        isValueChanged = true;
                    }
                }
                if (isValueChanged)
                {
                    onValueChanged?.Invoke(this, pileNameCollection);
                }
            }
        }
        private void onClickButtonCallback()
        {
            onClickButton?.Invoke(this);
        }
        private void onClickItemButtonCallback(ValueInputField item)
        {
            onClickItemButton?.Invoke(this, _list.IndexOf(item.transform as RectTransform));
        }
        private void onItemValueChangedCallback(ValueInputField item, object value)
        {
            onItemValueChanged?.Invoke(this, _list.IndexOf(item.transform as RectTransform), value);
        }
        private void onClickAddItemButtonCallback()
        {
            onClickAddItemButton?.Invoke(this);
        }
        private void onClickRemoveButtonCallback()
        {
            onClickRemoveButton?.Invoke(this);
        }
        private void onClickItemRemoveButtonCallback(ValueInputField item)
        {
            onClickItemRemoveButton?.Invoke(this, _list.IndexOf(item.transform as RectTransform));
        }
        private void onClickCardButtonCallback()
        {
            onClickButton?.Invoke(this);
        }
        #endregion
        #endregion
        #region 事件
        public event Action<ValueInputField, object> onValueChanged;
        public event Action<ValueInputField> onClickButton;
        public event Action<ValueInputField, int, object> onItemValueChanged;
        public event Action<ValueInputField, int> onClickItemButton;
        public event Action<ValueInputField> onClickAddItemButton;
        public event Action<ValueInputField> onClickRemoveButton;
        public event Action<ValueInputField, int> onClickItemRemoveButton;
        #endregion
        #region 属性字段
        public Card card => _Card;
        [SerializeField]
        Text _nameText;
        [SerializeField]
        InputField _inputField;
        [SerializeField]
        Toggle _toggle;
        [SerializeField]
        Button _button;
        [SerializeField]
        Text _buttonText;
        [SerializeField]
        Dropdown _dropdown;
        [SerializeField]
        MultiSelectDropdown _multiSelectDropdown;
        [SerializeField]
        RectTransform _listRoot;
        [SerializeField]
        RectTransform _listTemplate;
        [SerializeField]
        List<RectTransform> _list = new List<RectTransform>();
        [SerializeField]
        Button _addItemButton;
        [SerializeField]
        Button _removeItemButton;
        [SerializeField]
        string[] _pileNames;
        [SerializeField]
        string[] _pileValues;
        [SerializeField]
        Card _Card;
        [SerializeField]
        Button _cardButton;
        [SerializeField]
        ActionNodeValueInfo _valueInfo;
        #endregion
    }
}