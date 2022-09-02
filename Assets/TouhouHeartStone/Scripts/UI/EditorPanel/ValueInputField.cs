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
            _isListItem = isListItem;
            update();
        }
        public void setButtonText(string text)
        {
            _buttonText.text = text;
        }
        public ValueInputField getItemAt(int index)
        {
            return _list[index].GetComponent<ValueInputField>();
        }
        public void initPileNames(string[] pileNames, string[] pileValues)
        {
            _pileNames = pileNames;
            _pileValues = pileValues;
        }
        public void initEffectTagNames(string[] effectTagNames, string[] effectTagValues)
        {
            _effectTagNames = effectTagNames;
            _effectTagValues = effectTagValues;
        }
        public void initKeywords(string[] keywordNames, string[] keywordValues)
        {
            _keywordNames = keywordNames;
            _keywordValues = keywordValues;
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
            if (_valueInfo.type == ActionNodeValueType.pileNameCollection)
            {
                if (!(_valueInfo.value is PileNameCollection pileNameCollection))
                {
                    pileNameCollection = new PileNameCollection();
                }
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
            else if (_valueInfo.type == ActionNodeValueType.effectTagCollection)
            {
                if (!(_valueInfo.value is EffectTagCollection effectTagCol))
                {
                    effectTagCol = new EffectTagCollection();
                }
                bool isValueChanged = false;
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] && !effectTagCol.Contains(_effectTagValues[i]))
                    {
                        effectTagCol.Add(_effectTagValues[i]);
                        isValueChanged = true;
                    }
                    else if (!value[i])
                    {
                        effectTagCol.Remove(_effectTagValues[i]);
                        isValueChanged = true;
                    }
                }
                if (isValueChanged)
                {
                    onValueChanged?.Invoke(this, effectTagCol);
                }
            }
            else if (_valueInfo.type == ActionNodeValueType.keywords)
            {
                if (!(_valueInfo.value is KeywordCollection keywordCol))
                {
                    keywordCol = new KeywordCollection();
                }
                bool isValueChanged = false;
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] && !keywordCol.Contains(_keywordValues[i]))
                    {
                        keywordCol.Add(_keywordValues[i]);
                        isValueChanged = true;
                    }
                    else if (!value[i])
                    {
                        keywordCol.Remove(_keywordValues[i]);
                        isValueChanged = true;
                    }
                }
                if (isValueChanged)
                {
                    onValueChanged?.Invoke(this, keywordCol);
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
            if (_valueInfo.type == ActionNodeValueType.cardReferenceCollection)
            {
                CardReference[] newValue;
                if (_valueInfo.value is CardReference[] cardRefCollection)
                {
                    newValue = new CardReference[cardRefCollection.Length + 1];
                    Array.Copy(cardRefCollection, 0, newValue, 0, cardRefCollection.Length);
                }
                else
                {
                    newValue = new CardReference[1];
                }
                _valueInfo.value = newValue;
                onValueChanged?.Invoke(this, _valueInfo.value);
                update();
            }
            else
                onClickAddItemButton?.Invoke(this);
        }
        private void onClickRemoveButtonCallback()
        {
            onClickRemoveButton?.Invoke(this);
        }
        private void onClickItemRemoveButtonCallback(ValueInputField item)
        {
            int index = _list.IndexOf(item.transform as RectTransform);
            if (_valueInfo.type == ActionNodeValueType.cardReferenceCollection)
            {
                if (_valueInfo.value is CardReference[] cardRefCol)
                {
                    List<CardReference> cardRefList = new List<CardReference>(cardRefCol);
                    cardRefList.RemoveAt(index);
                    _valueInfo.value = cardRefList.ToArray();
                    onValueChanged?.Invoke(this, _valueInfo.value);
                    update();
                }
            }
            else
            {
                onClickItemRemoveButton?.Invoke(this, index);
            }
        }
        private void onClickCardButtonCallback()
        {
            onClickButton?.Invoke(this);
        }
        #endregion
        private void update()
        {
            _removeItemButton.gameObject.SetActive(_isListItem);
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
                int index = Array.IndexOf(_valueInfo.optionalValues, _valueInfo.value);
                if (index < 0)
                    index = 0;
                _dropdown.SetValueWithoutNotify(index);
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
            else if (_valueInfo.type == ActionNodeValueType.effectTagCollection)
            {
                _multiSelectDropdown.gameObject.SetActive(true);
                _multiSelectDropdown.ClearOptions();
                _multiSelectDropdown.AddOptions(new List<string>(_effectTagNames));
                if (_valueInfo.value is EffectTagCollection effectTagCol)
                {
                    for (int i = 0; i < effectTagCol.Count; i++)
                    {
                        //剔除掉未知的值
                        if (Array.IndexOf(_effectTagValues, effectTagCol[i]) < 0)
                        {
                            effectTagCol.RemoveAt(i);
                            i--;
                        }
                    }
                    bool[] boolValue = new bool[_effectTagValues.Length];
                    for (int i = 0; i < boolValue.Length; i++)
                    {
                        boolValue[i] = effectTagCol.Contains(_effectTagValues[i]);
                    }
                    _multiSelectDropdown.SetValueWithoutNotify(boolValue);
                }
                else
                    _multiSelectDropdown.SetValueWithoutNotify(new bool[_effectTagValues.Length]);
            }
            else if (_valueInfo.type == ActionNodeValueType.keywords)
            {
                _multiSelectDropdown.gameObject.SetActive(true);
                _multiSelectDropdown.ClearOptions();
                _multiSelectDropdown.AddOptions(new List<string>(_keywordNames));
                if (_valueInfo.value is KeywordCollection keywordCol)
                {
                    for (int i = 0; i < keywordCol.Count; i++)
                    {
                        //剔除掉未知的值
                        if (Array.IndexOf(_keywordValues, keywordCol[i]) < 0)
                        {
                            keywordCol.RemoveAt(i);
                            i--;
                        }
                    }
                    bool[] boolValue = new bool[_keywordValues.Length];
                    for (int i = 0; i < boolValue.Length; i++)
                    {
                        boolValue[i] = keywordCol.Contains(_keywordValues[i]);
                    }
                    _multiSelectDropdown.SetValueWithoutNotify(boolValue);
                }
                else
                    _multiSelectDropdown.SetValueWithoutNotify(new bool[_keywordValues.Length]);
            }
            else
            {
                _multiSelectDropdown.gameObject.SetActive(false);
            }
            //按钮
            if (_valueInfo.type == ActionNodeValueType.actionNode ||
                _valueInfo.type == ActionNodeValueType.actionValueRef ||
                _valueInfo.type == ActionNodeValueType.trigger)
            {
                _button.gameObject.SetActive(true);
            }
            else
            {
                _button.gameObject.SetActive(false);
            }
            //列表
            if (_valueInfo.type == ActionNodeValueType.triggerCollection)
            {
                _listRoot.gameObject.SetActive(true);
                if (_valueInfo.value is TriggerCollection triggerCollection)
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
            else if (_valueInfo.type == ActionNodeValueType.cardReferenceCollection)
            {
                _listRoot.gameObject.SetActive(true);
                if (!(_valueInfo.value is CardReference[] cardReferenceCollection))
                {
                    cardReferenceCollection = null;
                }
                UIHelper.updateList(_listRoot, _listTemplate, _list, cardReferenceCollection != null ? cardReferenceCollection.Length : 0, (i, itemRect) =>
                {
                    ValueInputField item = itemRect.GetComponent<ValueInputField>();
                    item.setValueInfo(null, new ActionNodeValueInfo()
                    {
                        type = ActionNodeValueType.cardReference,
                        value = cardReferenceCollection[i]
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
        private string[] _pileValues;
        [SerializeField]
        private string[] _effectTagNames;
        [SerializeField]
        private string[] _effectTagValues;
        [SerializeField]
        private string[] _keywordNames;
        [SerializeField]
        private string[] _keywordValues;
        [SerializeField]
        Card _Card;
        [SerializeField]
        Button _cardButton;
        [SerializeField]
        ActionNodeValueInfo _valueInfo;
        [SerializeField]
        bool _isListItem;
        #endregion
    }
}