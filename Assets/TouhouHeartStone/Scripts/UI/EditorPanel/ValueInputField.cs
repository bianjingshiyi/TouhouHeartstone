using EventArgReference = Game.EventArgReference;
using System;
using System.Collections.Generic;
using Game;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.UI;
using UIHelper = BJSYGameCore.UI.UIHelper;

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
        public void setCard(CardDefine cardDefine, CardSkinData skin, Sprite image)
        {
            if (_valueInfo.type == ActionNodeValueType.cardReference)
            {
                _buildItem.update(cardDefine, image, skin?.name ?? string.Empty, 0);
            }
        }
        public ValueInputField getItemAt(int index)
        {
            return _list[index].GetComponent<ValueInputField>();
        }
        public void initEventNames(string[] eventNames, string[] eventValues)
        {
            _eventNames = eventNames;
            _eventValues = eventValues;
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
            _inputDropdown.onValueChanged += onInputDropdownValueChangedCallback;
            _multiInputDropdown.onValueChanged += onMultiInputDropdownValueChangedCallback;
            _button.onClick.AddListener(onClickButtonCallback);
            _addItemButton.onClick.AddListener(onClickAddItemButtonCallback);
            _removeItemButton.onClick.AddListener(onClickRemoveButtonCallback);
            _buildItem.onClick += onClickCardButtonCallback;
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
            if (_valueInfo.type == ActionNodeValueType.enumValue)
            {
                onValueChanged?.Invoke(this, _valueInfo.optionalValues[index]);
            }
            else
            {
                onValueChanged?.Invoke(this, new EventArgReference(_eventValues[index]));
            }
        }
        private void onInputDropdownValueChangedCallback(string value)
        {
            if (_valueInfo.type == ActionNodeValueType.effectTag)
            {
                onValueChanged?.Invoke(this, new EffectTag(value));
            }
            else
            {
                onValueChanged?.Invoke(this, value);
            }
        }
        private void onMultiInputDropdownValueChangedCallback(string[] values)
        {
            if (_valueInfo.type == ActionNodeValueType.pileNameCollection)
            {
                if (!(_valueInfo.value is PileNameCollection pileNameCollection))
                {
                    pileNameCollection = new PileNameCollection();
                }
                bool isValueChanged = false;
                if (values.Length != pileNameCollection.Count)
                {
                    isValueChanged = true;
                }
                else
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i] != pileNameCollection[i])
                        {
                            isValueChanged = true;
                            break;
                        }
                    }
                }
                if (isValueChanged)
                {
                    pileNameCollection.Clear();
                    pileNameCollection.AddRange(values);
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
                if (values.Length != effectTagCol.Count)
                {
                    isValueChanged = true;
                }
                else
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i] != effectTagCol[i])
                        {
                            isValueChanged = true;
                            break;
                        }
                    }
                }
                if (isValueChanged)
                {
                    effectTagCol.Clear();
                    effectTagCol.AddRange(values);
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
                if (values.Length != keywordCol.Count)
                {
                    isValueChanged = true;
                }
                else
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i] != keywordCol[i])
                        {
                            isValueChanged = true;
                            break;
                        }
                    }
                }
                if (isValueChanged)
                {
                    keywordCol.Clear();
                    keywordCol.AddRange(values);
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
                DefineReference[] newValue;
                if (_valueInfo.value is DefineReference[] cardRefCollection)
                {
                    newValue = new DefineReference[cardRefCollection.Length + 1];
                    Array.Copy(cardRefCollection, 0, newValue, 0, cardRefCollection.Length);
                }
                else
                {
                    newValue = new DefineReference[1];
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
                if (_valueInfo.value is DefineReference[] cardRefCol)
                {
                    List<DefineReference> cardRefList = new List<DefineReference>(cardRefCol);
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

            if (_valueInfo.isArray)
            {
                updateArray();
            }
            else
            {
                updateInputField();
                updateToggle();
                updateDropdown();
                updateInputDropdown();
                updateMultiInputDropdown();
                updateButton();
                updateList();
                updateCard();
            }
        }
        private void updateArray()
        {
            if (_valueInfo.value is Array array)
            {
                _inputField.gameObject.SetActive(false);
                _toggle.gameObject.SetActive(false);
                _dropdown.gameObject.SetActive(false);
                _inputDropdown.gameObject.SetActive(false);
                _multiInputDropdown.gameObject.SetActive(false);
                _button.gameObject.SetActive(false);
                _buildItem.gameObject.SetActive(false);

                var valueType = array.GetType();
                var elementType = valueType.GetElementType();
                if (elementType == null)
                    return;
                _listRoot.gameObject.SetActive(true);
                UIHelper.updateList(_listContentRoot, GetListTemplate(), _list, array.Length, (i, itemRect) =>
                {
                    ValueInputField item = itemRect.GetComponent<ValueInputField>();
                    item.setValueInfo(null, new ActionNodeValueInfo()
                    {
                        type = _valueInfo.type,
                        value = array.GetValue(i)
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
        }
        private void updateInputField()
        {
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
        }
        private void updateToggle()
        {
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
        }
        private void updateDropdown()
        {
            //单选下拉菜单
            if (_valueInfo.type == ActionNodeValueType.enumValue)
            {
                _dropdown.gameObject.SetActive(true);
                _dropdown.ClearOptions();
                _dropdown.AddOptions(new List<string>(_valueInfo.optionalNames));
                if (_valueInfo.value is int index)
                {
                    //如果value为int，那有可能是枚举值序列化之后变成了整数再反序列化的结果，直接使用。
                }
                else
                    index = Array.IndexOf(_valueInfo.optionalValues, _valueInfo.value);
                if (index < 0)
                    index = 0;
                _dropdown.SetValueWithoutNotify(index);
            }
            else if (_valueInfo.type == ActionNodeValueType.eventReference)
            {
                _dropdown.gameObject.SetActive(true);
                _dropdown.ClearOptions();
                _dropdown.AddOptions(new List<string>(_eventNames));
                int index = 0;
                if (_valueInfo.value is int intValue)
                {
                    //如果value为int，那有可能是枚举值序列化之后变成了整数再反序列化的结果，直接使用。
                    index = intValue;
                }
                else if (_valueInfo.value is string str)
                {
                    index = Array.IndexOf(_eventValues, str);
                }
                else if (_valueInfo.value is EventArgReference evtRef)
                {
                    index = Array.IndexOf(_eventValues, evtRef.eventName);
                }
                if (index < 0)
                    index = 0;
                _dropdown.SetValueWithoutNotify(index);
            }
            else
            {
                _dropdown.gameObject.SetActive(false);
            }
        }
        private void updateInputDropdown()
        {
            //自定义单选下拉菜单
            if (_valueInfo.type == ActionNodeValueType.effectTag)
            {
                _inputDropdown.gameObject.SetActive(true);
                _inputDropdown.setDropdownValues(_effectTagNames, _effectTagValues);
                string value = null;
                if (_valueInfo.value is string str)
                {
                    value = str;
                }
                else if (_valueInfo.value is int index)
                {
                    value = _effectTagValues[index];
                }
                else if (_valueInfo.value is EffectTag tag)
                {
                    value = tag.effectTag;
                }
                if (value == null)
                    value = _effectTagValues[0];
                _inputDropdown.setValueWithoutNotify(value);
            }
            else
            {
                _inputDropdown.gameObject.SetActive(false);
            }
        }
        private void updateMultiInputDropdown()
        {
            //可自定义多选下拉菜单
            if (_valueInfo.type == ActionNodeValueType.pileNameCollection)
            {
                _multiInputDropdown.gameObject.SetActive(true);
                _multiInputDropdown.setDropdownValues(_pileNames, _pileValues);
                if (_valueInfo.value is PileNameCollection pileNameCollection)
                {
                    _multiInputDropdown.setValuesWithoutNotify(pileNameCollection.ToArray());
                }
                else
                    _multiInputDropdown.setValuesWithoutNotify(Array.Empty<string>());
            }
            else if (_valueInfo.type == ActionNodeValueType.effectTagCollection)
            {
                _multiInputDropdown.gameObject.SetActive(true);
                _multiInputDropdown.setDropdownValues(_effectTagNames, _effectTagValues);
                if (_valueInfo.value is EffectTagCollection effectTagCol)
                {
                    _multiInputDropdown.setValuesWithoutNotify(effectTagCol.ToArray());
                }
                else
                    _multiInputDropdown.setValuesWithoutNotify(Array.Empty<string>());
            }
            else if (_valueInfo.type == ActionNodeValueType.keywords)
            {
                _multiInputDropdown.gameObject.SetActive(true);
                _multiInputDropdown.setDropdownValues(_keywordNames, _keywordValues);
                if (_valueInfo.value is KeywordCollection keywordCol)
                {
                    _multiInputDropdown.setValuesWithoutNotify(keywordCol.ToArray());
                }
                else
                    _multiInputDropdown.setValuesWithoutNotify(Array.Empty<string>());
            }
            else
            {
                _multiInputDropdown.gameObject.SetActive(false);
            }
        }
        private void updateButton()
        {
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
        }
        private void updateList()
        {
            //列表
            if (_valueInfo.type == ActionNodeValueType.triggerCollection)
            {
                _listRoot.gameObject.SetActive(true);
                if (_valueInfo.value is TriggerCollection triggerCollection)
                {
                    UIHelper.updateList(_listContentRoot, GetListTemplate(), _list, triggerCollection.Count, (i, itemRect) =>
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
                if (!(_valueInfo.value is DefineReference[] cardReferenceCollection))
                {
                    cardReferenceCollection = null;
                }
                UIHelper.updateList(_listContentRoot, GetListTemplate(), _list, cardReferenceCollection != null ? cardReferenceCollection.Length : 0, (i, itemRect) =>
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
        }
        private void updateCard()
        {
            //卡牌
            if (_valueInfo.type == ActionNodeValueType.cardReference)
            {
                _buildItem.gameObject.SetActive(true);
            }
            else
            {
                _buildItem.gameObject.SetActive(false);
            }
        }

        private RectTransform GetListTemplate()
        {
            // 列表项模板直接引用自身，会在创建列表项时连同自身的所有列表项一同复制，造成性能开销指数上升。
            // 这里创建一个自身的复制，用它来当模板。
            if (!_listTemplateClone)
            {
                _listTemplateClone = Instantiate(_listTemplate, transform);
                _listTemplateClone.gameObject.SetActive(false);
            }
            return _listTemplateClone;
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
        BuildDeckListItem _buildItem;
        [SerializeField]
        InputDropdown _inputDropdown;
        [SerializeField]
        MultiInputDropdown _multiInputDropdown;
        [SerializeField]
        RectTransform _listRoot;
        [SerializeField]
        RectTransform _listContentRoot;
        [SerializeField]
        RectTransform _listTemplate;
        RectTransform _listTemplateClone;
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
        private string[] _eventNames;
        [SerializeField]
        private string[] _eventValues;
        [SerializeField]
        ActionNodeValueInfo _valueInfo;
        [SerializeField]
        bool _isListItem;
        #endregion
    }
}