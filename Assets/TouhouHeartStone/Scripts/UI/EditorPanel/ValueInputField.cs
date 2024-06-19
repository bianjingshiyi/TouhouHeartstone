using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore.UI;
using Game;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;
using EventArgReference = Game.EventArgReference;

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
            _valueTypeValues = info.assignType.getAssignableValueTypes();
            _isListItem = isListItem;
            update();
        }
        public void setButtonText(string text)
        {
            _buttonText.text = text;
        }
        public void setCard(string name, Sprite image)
        {
            var valueType = _valueInfo.type;
            if (valueType.type == ActionNodeValueType.cardReference && !valueType.isArray())
            {
                _buildItem.updateView(name, image, 0);
            }
        }
        public ValueInputFieldListItem getItemAt(int index)
        {
            return _childrenElementList.getElement<ValueInputFieldListItem>(index);
        }
        public void initProperties(ValueInputFieldProperties props)
        {
            setProp(ref _backText, props.backText);

            setProp(ref _eventNames, props.eventNames);
            setProp(ref _eventValues, props.eventValues);

            setProp(ref _eventTriggerTimeNames, props.eventTriggerTimeNames);
            setProp(ref _eventTriggerTimeValues, props.eventTriggerTimeValues);

            setProp(ref _pileNames, props.pileNames);
            setProp(ref _pileValues, props.pileValues);
            setProp(ref _anyPileName, props.anyPileName);

            setProp(ref _effectTagNames, props.effectTagNames);
            setProp(ref _effectTagValues, props.effectTagValues);

            setProp(ref _eventVarNameNames, props.eventVarNameNames);
            setProp(ref _eventVarNameValues, props.eventVarNameValues);

            setProp(ref _keywordNames, props.keywordNames);
            setProp(ref _keywordValues, props.keywordValues);

            setProp(ref _cardTagNames, props.cardTagNames);
            setProp(ref _cardTagValues, props.cardTagValues);

            setProp(ref _buffValues, props.buffValues);

            setProp(ref _cardPropertyNameItems, props.cardProperties);

            setProp(ref _defaultValueTypeName, props.defaultValueTypeName);
            setProp(ref _valueTypeNames, props.valueTypeNames);

            setProp(ref _typeNames, props.graphTypeNames);
            setProp(ref _typeValues, props.graphTypeValues);

            setProp(ref _cardTypeNames, props.cardTypeNames);
            setProp(ref _cardTypeValues, props.cardTypeValues);

            foreach (var child in _childrenElementList.getElements<ValueInputFieldListItem>())
            {
                child.valueField.initProperties(props);
            }
            update();

            void setProp<T>(ref T variable, T value)
            {
                if (value != null)
                    variable = value;
            }
        }
        public ValueInputFieldProperties getFieldProperties()
        {
            return new ValueInputFieldProperties()
            {
                backText = _backText,

                eventNames = _eventNames,
                eventValues = _eventValues,

                eventTriggerTimeNames = _eventTriggerTimeNames,
                eventTriggerTimeValues = _eventTriggerTimeValues,

                anyPileName = _anyPileName,
                pileNames = _pileNames,
                pileValues = _pileValues,

                effectTagNames = _effectTagNames,
                effectTagValues = _effectTagValues,

                eventVarNameNames = _eventVarNameNames,
                eventVarNameValues = _eventVarNameValues,

                keywordNames = _keywordNames,
                keywordValues = _keywordValues,

                cardTagNames = _cardTagNames,
                cardTagValues = _cardTagValues,

                buffValues = _buffValues,

                cardProperties = _cardPropertyNameItems,

                defaultValueTypeName = _defaultValueTypeName,
                valueTypeNames = _valueTypeNames,

                graphTypeNames = _typeNames,
                graphTypeValues = _typeValues,

                cardTypeNames = _cardTypeNames,
                cardTypeValues = _cardTypeValues,
            };
        }
        public void setReadonly(bool isReadonly)
        {
            _isReadonly = isReadonly;
            bool canEdit = !isReadonly;
            _inputField.interactable = canEdit;
            _toggle.interactable = canEdit;
            _button.interactable = canEdit;
            _dropdown.interactable = canEdit;
            _buffDropdown.interactable = canEdit;
            _buffItemEditButton.interactable = canEdit;
            _buildItem.interactable = canEdit;
            _buildItemEditButton.interactable = canEdit;
            _inputDropdown.interactable = canEdit;
            _multiInputDropdown.interactable = canEdit;
            _addItemButton.interactable = canEdit;
            foreach (var field in _childrenElementList.getElements<ValueInputFieldListItem>())
            {
                field.setReadonly(isReadonly);
            }
        }
        #endregion
        #region 私有方法
        protected void Awake()
        {
            _typeDropdown.onValueChanged.AddListener(onTypeDropdownValueChangedCallback);
            _inputField.onEndEdit.AddListener(onEndEditInputField);
            _toggle.onValueChanged.AddListener(onToggleValueChanged);
            _dropdown.onValueChanged.AddListener(onDropdownValueChangedCallback);
            _buffDropdown.onValueChanged.AddListener(onBuffDropdownValueChangedCallback);
            _inputDropdown.onValueChanged += onInputDropdownValueChangedCallback;
            _multiInputDropdown.onValueChanged += onMultiInputDropdownValueChangedCallback;

            _addItemButton.onClick.AddListener(onClickAddItemButtonCallback);

            _button.onClick.AddListener(() => callButtonCallback(ValueInputFieldButton.EditButton));
            _buildItem.onClick += (item, data) => callButtonCallback(ValueInputFieldButton.CardButton);
            _buildItemEditButton.onClick.AddListener(() => callButtonCallback(ValueInputFieldButton.CardEditButton));
            _buffItemEditButton.onClick.AddListener(() => callButtonCallback(ValueInputFieldButton.BuffEditButton));
        }
        #region 事件回调
        private void onTypeDropdownValueChangedCallback(int index)
        {
            var typeValues = _valueTypeValues;
            if (typeValues != null)
            {
                var type = typeValues[index];
                var value = _valueInfo.value;
                bool notSameType = false;
                if (value == null)
                {
                    notSameType = true;
                }
                else
                {
                    var valueType = value.GetType().getNodeValueType();
                    if (valueType != type)
                        notSameType = true;
                }
                if (notSameType)
                {
                    callValueChangedCallback(type.getDefaultValue());
                }
            }
        }
        #region 子输入框
        private void onClickAddItemButtonCallback()
        {
            if (_isReadonly)
                return;
            var childValueType = _valueInfo.type.getElementType();
            var defaultValue = childValueType.getDefaultValue();
            if (_valueInfo.value is Array array)
            {
                var arrayType = array.GetType();
                var elementType = arrayType.GetElementType();
                var newArray = Array.CreateInstance(elementType, array.Length + 1);
                array.CopyTo(newArray, 0);
                newArray.SetValue(defaultValue, newArray.Length - 1);
                _valueInfo.value = newArray;
            }
            else if (_valueInfo.value is IList list)
            {
                list.Add(defaultValue);
            }
            callValueChangedCallback(_valueInfo.value);
            update();
        }
        private void onClickItemRemoveButtonCallback(ValueInputFieldListItem item)
        {
            if (_isReadonly)
                return;
            int index = _childrenElementList.indexOf(item);
            if (_valueInfo.value is Array array)
            {
                var arrayType = array.GetType();
                var elementType = arrayType.GetElementType();
                if (elementType == null)
                    return;
                var newArray = Array.CreateInstance(elementType, array.Length - 1);
                int newIndex = 0;
                for (int i = 0; i < array.Length; i++)
                {
                    if (i == index)
                        continue;
                    newArray.SetValue(array.GetValue(i), newIndex);
                    newIndex++;
                }
                _valueInfo.value = newArray;
            }
            else if (_valueInfo.value is IList list)
            {
                list.RemoveAt(index);
            }
            callValueChangedCallback(_valueInfo.value);
            update();
        }
        private void onItemValueChangedCallback(ValueInputFieldListItem listItem, object value)
        {
            if (_isReadonly)
                return;
            int index = _childrenElementList.indexOf(listItem);
            var arrayValue = _valueInfo.value;
            if (arrayValue is Array array)
            {
                array.SetValue(value, index);
            }
            else if (arrayValue is IList list)
            {
                list[index] = value;
            }
            callValueChangedCallback(_valueInfo.value);
            update();
        }
        #endregion

        #region 按钮
        private void onClickItemButtonCallback(ValueInputFieldListItem listItem, int[] itemIndexes, ValueInputFieldButton type)
        {
            var itemIndex = _childrenElementList.indexOf(listItem);
            onClickButton?.Invoke(this, itemIndexes.Prepend(itemIndex).ToArray(), type);
        }
        private void callButtonCallback(ValueInputFieldButton type)
        {
            onClickButton?.Invoke(this, Array.Empty<int>(), type);
        }
        #endregion

        #region 值改变
        private void onEndEditInputField(string text)
        {
            if (valueType == ActionNodeValueType.integer)
            {
                if (int.TryParse(text, out int value))
                {
                    callValueChangedCallback(value);
                }
                else
                {
                    //非法值
                    _inputField.SetTextWithoutNotify("0");
                }
            }
            else if (valueType == ActionNodeValueType.real)
            {
                if (float.TryParse(text, out float value))
                {
                    callValueChangedCallback(value);
                }
                else
                {
                    _inputField.SetTextWithoutNotify("0");
                }
            }
            else if (valueType == ActionNodeValueType.text)
            {
                callValueChangedCallback(text);
            }
        }
        private void onToggleValueChanged(bool value)
        {
            callValueChangedCallback(value);
        }
        private void onDropdownValueChangedCallback(int index)
        {
            if (valueType == ActionNodeValueType.enumValue)
            {
                callValueChangedCallback(_valueInfo.optionalValues[index]);
            }
            else if (valueType == ActionNodeValueType.eventReference)
            {
                callValueChangedCallback(_eventValues[index]);
            }
            else if (valueType == ActionNodeValueType.eventTriggerTime)
            {
                callValueChangedCallback(_eventTriggerTimeValues[index]);
            }
            else if (valueType == ActionNodeValueType.type)
            {
                callValueChangedCallback(_typeValues[index]);
            }
            else if (valueType == ActionNodeValueType.cardType)
            {
                callValueChangedCallback(_cardTypeValues[index]);
            }
        }
        private void onBuffDropdownValueChangedCallback(int[] value)
        {
            if (valueType == ActionNodeValueType.buffReference)
            {
                callValueChangedCallback(BuffReferenceRecursion.getValueByIndexes(_buffValues, value));
            }
        }
        private void onInputDropdownValueChangedCallback(string value)
        {
            switch (valueType)
            {
                // 牌堆名称
                case ActionNodeValueType.pileName:
                    callValueChangedCallback((PileName)value);
                    break;
                // 关键词名称
                case ActionNodeValueType.keyword:
                    callValueChangedCallback((KeywordName)value);
                    break;
                // 标签名称
                case ActionNodeValueType.cardTag:
                    callValueChangedCallback((CardTagName)value);
                    break;
                // 设置卡牌属性
                case ActionNodeValueType.cardPropertyNameSet:
                    callValueChangedCallback((CardPropertySetterName)value);
                    break;
                // 获取卡牌属性
                case ActionNodeValueType.cardPropertyNameGet:
                    callValueChangedCallback((CardPropertyGetterName)value);
                    break;
                // 获取卡牌定义属性
                case ActionNodeValueType.cardDefinePropertyNameGet:
                    callValueChangedCallback((CardDefinePropertyGetterName)value);
                    break;
                // 设置卡牌定义属性
                case ActionNodeValueType.cardDefinePropertyNameSet:
                    callValueChangedCallback((CardDefinePropertySetterName)value);
                    break;
                // 效果标签
                case ActionNodeValueType.effectTag:
                    callValueChangedCallback((EffectTag)value);
                    break;
                // 事件变量名
                case ActionNodeValueType.eventVarName:
                    callValueChangedCallback((EventVarName)value);
                    break;
                default:
                    callValueChangedCallback(value);
                    break;
            }
        }
        private void onMultiInputDropdownValueChangedCallback(string[] values)
        {
            switch (valueType)
            {
                case ActionNodeValueType.pileNameCollection:
                    callMultiInputDropdownValueChangedCallback<PileNameCollection>(values);
                    break;
                case ActionNodeValueType.pileNameRange:
                    if (_valueInfo.value is not PileNameRange pileNameRange)
                    {
                        pileNameRange = _valueInfo.value is PileNameCollection pileNameCollection ? new PileNameRange(pileNameCollection) : new PileNameRange();
                    }
                    var selected = values.Where(v => !pileNameRange.Contains(v)).ToArray();
                    var deselected = pileNameRange.Where(v => !values.Contains(v)).ToArray();
                    bool isValueChanged = selected.Length + deselected.Length > 0;
                    if (isValueChanged)
                    {
                        if (selected.Contains(Pile.PILE_RANGE_ANY))
                        {
                            bool selectedOther = pileNameRange.Any(p => p != Pile.PILE_RANGE_ANY);
                            pileNameRange.Clear();
                            pileNameRange.Add(Pile.PILE_RANGE_ANY);
                            if (selectedOther)
                            {
                                _multiInputDropdown.setValuesWithoutNotify(pileNameRange.ToArray());
                                _multiInputDropdown.updateOptionToggles();
                            }
                        }
                        else
                        {
                            bool selectedAny = pileNameRange.Contains(Pile.PILE_RANGE_ANY);
                            pileNameRange.Clear();
                            pileNameRange.AddRange(values.Where(v => v != Pile.PILE_RANGE_ANY));
                            if (selectedAny)
                            {
                                _multiInputDropdown.setValuesWithoutNotify(pileNameRange.ToArray());
                                _multiInputDropdown.updateOptionToggles();
                            }
                        }
                        callValueChangedCallback(pileNameRange);
                    }
                    break;
                case ActionNodeValueType.effectTagCollection:
                    callMultiInputDropdownValueChangedCallback<EffectTagCollection>(values);
                    break;
                case ActionNodeValueType.keywordCollection:
                    callMultiInputDropdownValueChangedCallback<KeywordCollection>(values);
                    break;
                case ActionNodeValueType.cardTagCollection:
                    callMultiInputDropdownValueChangedCallback<CardTagCollection>(values);
                    break;
            }
        }
        #endregion
        #endregion
        private void update()
        {
            updateTypeDropdown();

            if (_valueInfo.type.isArray())
            {
                updateArray();
            }
            else
            {
                _listRoot.gameObject.SetActive(false);
                updateInputField();
                updateToggle();
                updateDropdown();
                updateInputDropdown();
                updateMultiInputDropdown();
                updateButton();
                updateCard();
                updateBuff();
                updateMultiInputDropdown();
            }
        }
        private void updateTypeDropdown()
        {
            var typeValues = _valueTypeValues;
            var valueTypeCount = typeValues?.Length ?? 0;
            _typeDropdown.gameObject.SetActive(valueTypeCount > 0);
            _typeDropdown.ClearOptions();
            if (typeValues != null && _valueTypeNames != null)
            {
                var typeNames = typeValues.Select(t => _valueTypeNames.FirstOrDefault(m => m.type == t.type).name ?? _defaultValueTypeName);
                _typeDropdown.AddOptions(typeNames.ToList());
                _typeDropdown.value = Array.IndexOf(typeValues, _valueInfo.type);
            }
        }
        private void updateArray()
        {
            //列表
            if (_valueInfo.value == null)
            {
                _valueInfo.value = _valueInfo.type.getDefaultValue();
            }
            if (_valueInfo.value is IList list)
            {
                _inputField.gameObject.SetActive(false);
                _toggleField.SetActive(false);
                _dropdown.gameObject.SetActive(false);
                _buffItemObj.gameObject.SetActive(false);
                _inputDropdown.gameObject.SetActive(false);
                _multiInputDropdown.gameObject.SetActive(false);
                _button.gameObject.SetActive(false);
                _buildItem.gameObject.SetActive(false);
                _multiInputDropdown.gameObject.SetActive(false);

                _listRoot.gameObject.SetActive(true);
                var childValueType = _valueInfo.type.getElementType();
                _childrenElementList.updateList(list.Count, (i, itemRect) =>
                {
                    ValueInputFieldListItem listItem = itemRect.GetComponent<ValueInputFieldListItem>();
                    var item = listItem.valueField;

                    var value = list[i];
                    var assignType = childValueType;
                    var valueType = assignType.getAssignableValueTypeByValue(value);
                    var childTypes = assignType.getAssignableValueTypes();
                    var valueInfo = new ActionNodeValueInfo(null, valueType, value, assignType);

                    var fieldProps = getFieldProperties();
                    item.initProperties(fieldProps);
                    item.setValueInfo(null, valueInfo, true);
                    item.setReadonly(_isReadonly);
                }, itemRect =>
                {
                    ValueInputFieldListItem listItem = itemRect.GetComponent<ValueInputFieldListItem>();
                    addListItemCallbacks(listItem);
                }, itemRect =>
                {
                    ValueInputFieldListItem listItem = itemRect.GetComponent<ValueInputFieldListItem>();
                    removeListItemCallbacks(listItem);
                });
            }
            else
            {
                _listRoot.gameObject.SetActive(false);
            }
        }
        private void updateInputField()
        {
            //输入框
            if (valueType == ActionNodeValueType.integer)
            {
                _inputField.gameObject.SetActive(true);
                if (_valueInfo.value != null)
                    _inputField.SetTextWithoutNotify(_valueInfo.value.ToString());
                else
                    _inputField.SetTextWithoutNotify("0");
            }
            else if (valueType == ActionNodeValueType.real)
            {
                _inputField.gameObject.SetActive(true);
                if (_valueInfo.value != null)
                    _inputField.SetTextWithoutNotify(_valueInfo.value.ToString());
                else
                    _inputField.SetTextWithoutNotify("0");
            }
            else if (valueType == ActionNodeValueType.text)
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
            if (valueType == ActionNodeValueType.boolean)
            {
                _toggleField.SetActive(true);
                if (_valueInfo.value != null)
                    _toggle.SetIsOnWithoutNotify((bool)_valueInfo.value);
                else
                    _toggle.SetIsOnWithoutNotify(false);
            }
            else
            {
                _toggleField.SetActive(false);
            }
        }
        private void updateDropdown()
        {
            bool isDropdown = true;
            IEnumerable<string> optionNames = null;
            int dropdownIndex = -1;
            //单选下拉菜单
            switch (valueType)
            {
                case ActionNodeValueType.enumValue:
                    {
                        optionNames = _valueInfo.optionalNames;
                        switch (_valueInfo.value)
                        {
                            case int index:
                                //如果value为int，那有可能是枚举值序列化之后变成了整数再反序列化的结果，直接使用。
                                dropdownIndex = index;
                                break;
                            default:
                                dropdownIndex = Array.IndexOf(_valueInfo.optionalValues, _valueInfo.value);
                                break;
                        }
                    }
                    break;

                case ActionNodeValueType.eventReference:
                    {
                        optionNames = _eventNames;
                        switch (_valueInfo.value)
                        {
                            case int intValue:
                                //如果value为int，那有可能是枚举值序列化之后变成了整数再反序列化的结果，直接使用。
                                dropdownIndex = intValue;
                                break;
                            case string str:
                                dropdownIndex = Array.FindIndex(_eventValues, e => e.cardPoolId == 0 && e.eventName == str);
                                break;
                            case EventArgReference evtArgRef:
                                dropdownIndex = Array.FindIndex(_eventValues, e => e.cardPoolId == 0 && e.eventName == evtArgRef.value);
                                break;
                            case EventReference evtRef:
                                dropdownIndex = Array.IndexOf(_eventValues, evtRef);
                                break;
                        }
                    }
                    break;

                case ActionNodeValueType.eventTriggerTime:
                    {
                        optionNames = _eventTriggerTimeNames;
                        switch (_valueInfo.value)
                        {
                            case int intValue:
                                //如果value为int，那有可能是枚举值序列化之后变成了整数再反序列化的结果，直接使用。
                                dropdownIndex = intValue;
                                break;
                            case EventTriggerTime eventTriggerTime:
                                dropdownIndex = Array.IndexOf(_eventTriggerTimeValues, eventTriggerTime);
                                break;
                        }
                    }
                    break;

                case ActionNodeValueType.type:
                    {
                        optionNames = _typeNames;
                        switch (_valueInfo.value)
                        {
                            case int intValue:
                                //如果value为int，那有可能是枚举值序列化之后变成了整数再反序列化的结果，直接使用。
                                dropdownIndex = intValue;
                                break;
                            case string str:
                                dropdownIndex = Array.FindIndex(_typeValues, t => t == str);
                                break;
                            case TypeName typeName:
                                dropdownIndex = Array.IndexOf(_typeValues, typeName);
                                break;
                        }
                    }
                    break;

                case ActionNodeValueType.cardType:
                    {
                        optionNames = _cardTypeNames;
                        switch (_valueInfo.value)
                        {
                            case int intValue:
                                //如果value为int，那有可能是枚举值序列化之后变成了整数再反序列化的结果，直接使用。
                                dropdownIndex = intValue;
                                break;
                            case string str:
                                dropdownIndex = Array.FindIndex(_cardTypeValues, t => t == str);
                                break;
                            case CardType cardType:
                                dropdownIndex = Array.IndexOf(_cardTypeValues, cardType);
                                break;
                        }
                        break;
                    }
                default:
                    isDropdown = false;
                    break;
            }

            if (isDropdown)
            {
                _dropdown.gameObject.SetActive(true);
                _dropdown.ClearOptions();
                _dropdown.AddOptions(new List<string>(optionNames));
                if (dropdownIndex < 0)
                    dropdownIndex = 0;
                _dropdown.SetValueWithoutNotify(dropdownIndex);
            }
            else
            {
                _dropdown.gameObject.SetActive(false);
            }
        }
        private void updateInputDropdown()
        {
            //自定义单选下拉菜单

            switch (valueType)
            {
                // 牌堆名称
                case ActionNodeValueType.pileName:
                    updateInputDropdownValue<PileName>(_pileNames, _pileValues);
                    break;
                // 关键词名称
                case ActionNodeValueType.keyword:
                    updateInputDropdownValue<KeywordName>(_keywordNames, _keywordValues);
                    break;
                // 标签名称
                case ActionNodeValueType.cardTag:
                    updateInputDropdownValue<CardTagName>(_cardTagNames, _cardTagValues);
                    break;
                // 设置卡牌属性
                case ActionNodeValueType.cardPropertyNameSet:
                    {
                        var nameItems = _cardPropertyNameItems.Where(v => v.canSet);
                        var names = nameItems.Select(v => v.name).ToArray();
                        var values = nameItems.Select(v => v.value).ToArray();
                        updateInputDropdownValue<CardPropertySetterName>(names, values);
                        break;
                    }
                // 获取卡牌属性
                case ActionNodeValueType.cardPropertyNameGet:
                    {
                        var nameItems = _cardPropertyNameItems.Where(v => v.canGet);
                        var names = nameItems.Select(v => v.name).ToArray();
                        var values = nameItems.Select(v => v.value).ToArray();
                        updateInputDropdownValue<CardPropertyGetterName>(names, values);
                        break;
                    }
                // 获取卡牌定义属性
                case ActionNodeValueType.cardDefinePropertyNameGet:
                    {
                        var nameItems = _cardPropertyNameItems.Where(v => v.canDefineGet);
                        var names = nameItems.Select(v => v.name).ToArray();
                        var values = nameItems.Select(v => v.value).ToArray();
                        updateInputDropdownValue<CardDefinePropertyGetterName>(names, values);
                        break;
                    }
                // 设置卡牌定义属性
                case ActionNodeValueType.cardDefinePropertyNameSet:
                    {
                        var nameItems = _cardPropertyNameItems.Where(v => v.canDefineSet);
                        var names = nameItems.Select(v => v.name).ToArray();
                        var values = nameItems.Select(v => v.value).ToArray();
                        updateInputDropdownValue<CardDefinePropertySetterName>(names, values);
                        break;
                    }
                // 效果标签
                case ActionNodeValueType.effectTag:
                    updateInputDropdownValue<EffectTag>(_effectTagNames, _effectTagValues);
                    break;
                // 事件变量名
                case ActionNodeValueType.eventVarName:
                    updateInputDropdownValue<EventVarName>(_eventVarNameNames, _eventVarNameValues);
                    break;
                default:
                    _inputDropdown.gameObject.SetActive(false);
                    break;
            }
        }
        private void updateMultiInputDropdown()
        {
            //可自定义多选下拉菜单
            switch (valueType)
            {
                case ActionNodeValueType.pileNameCollection:
                case ActionNodeValueType.pileNameRange:
                    {
                        string[] pileNames;
                        string[] pileValues;
                        if (valueType == ActionNodeValueType.pileNameRange)
                        {
                            pileNames = _pileNames.Prepend(_anyPileName).ToArray();
                            pileValues = _pileValues.Prepend(Pile.PILE_RANGE_ANY).ToArray();
                        }
                        else
                        {
                            pileNames = _pileNames;
                            pileValues = _pileValues;
                        }
                        updateMultiInputDropdownValue<PileName>(pileNames, pileValues);
                        break;
                    }

                case ActionNodeValueType.effectTagCollection:
                    updateMultiInputDropdownValue<EffectTag>(_effectTagNames, _effectTagValues);
                    break;
                case ActionNodeValueType.keywordCollection:
                    updateMultiInputDropdownValue<KeywordName>(_keywordNames, _keywordValues);
                    break;
                case ActionNodeValueType.cardTagCollection:
                    updateMultiInputDropdownValue<CardTagName>(_cardTagNames, _cardTagValues);
                    break;
                default:
                    _multiInputDropdown.gameObject.SetActive(false);
                    break;
            }
        }
        private void updateButton()
        {
            //按钮
            if (valueType == ActionNodeValueType.actionNode)
            {
                _button.gameObject.SetActive(true);
            }
            else
            {
                _button.gameObject.SetActive(false);
            }
        }
        private void updateCard()
        {
            //卡牌
            if (valueType == ActionNodeValueType.cardReference)
            {
                _buildItem.gameObject.SetActive(true);
            }
            else
            {
                _buildItem.gameObject.SetActive(false);
            }
        }
        private void updateBuff()
        {
            //单选下拉菜单
            if (valueType == ActionNodeValueType.buffReference)
            {
                _buffItemObj.SetActive(true);
                _buffDropdown.backText = _backText;
                _buffDropdown.ClearOptions();
                var options = _buffValues.Select(v => v.toOptionData()).ToList();
                _buffDropdown.AddOptions(options);
                int[] values = null;
                if (_valueInfo.value is DefineReference defRef)
                {
                    values = BuffReferenceRecursion.getIndexesByValue(_buffValues.ToList(), defRef);
                }
                if (values == null)
                    values = new int[0];
                _buffDropdown.SetValueWithoutNotify(values);
            }
            else
            {
                _buffItemObj.gameObject.SetActive(false);
            }
        }

        private void updateInputDropdownValue<T>(string[] names, T[] values) where T : INodeConstProxy<string>
        {
            updateInputDropdownValue<T>(names, values.Select(v => v.value).ToArray());
        }
        private void updateInputDropdownValue<T>(string[] names, string[] values) where T : INodeConstProxy<string>
        {
            _inputDropdown.gameObject.SetActive(true);
            _inputDropdown.setDropdownValues(names, values);
            string value = null;
            if (_valueInfo.value is string str)
            {
                value = str;
            }
            else if (_valueInfo.value is int index)
            {
                value = values[index];
            }
            else if (_valueInfo.value is T tag)
            {
                value = tag.value;
            }
            if (value == null)
                value = values.FirstOrDefault();
            _inputDropdown.setValueWithoutNotify(value);
        }
        private void updateMultiInputDropdownValue<T>(string[] names, T[] values) where T : INodeConstProxy<string>
        {
            updateMultiInputDropdownValue<T>(names, values.Select(v => v.value).ToArray());
        }
        private void updateMultiInputDropdownValue<T>(string[] names, string[] values) where T : INodeConstProxy<string>
        {
            _multiInputDropdown.gameObject.SetActive(true);
            _multiInputDropdown.setDropdownValues(names, values);
            if (_valueInfo.value is IList<string> stringList)
            {
                _multiInputDropdown.setValuesWithoutNotify(stringList.ToArray());
            }
            else if(_valueInfo.value is IList<T> value)
            {
                _multiInputDropdown.setValuesWithoutNotify(value.Select(v => v.value).ToArray());
            }
            else
                _multiInputDropdown.setValuesWithoutNotify(Array.Empty<string>());
        }
        private void addListItemCallbacks(ValueInputFieldListItem listItem)
        {
            listItem.onValueChanged += onItemValueChangedCallback;
            listItem.onClickButton += onClickItemButtonCallback;
            listItem.onClickRemoveButton += onClickItemRemoveButtonCallback;
        }
        private void removeListItemCallbacks(ValueInputFieldListItem listItem)
        {
            listItem.onValueChanged -= onItemValueChangedCallback;
            listItem.onClickButton -= onClickItemButtonCallback;
            listItem.onClickRemoveButton -= onClickItemRemoveButtonCallback;
        }
        private void callValueChangedCallback(object value)
        {
            onValueChanged?.Invoke(this, value);
        }
        private void callMultiInputDropdownValueChangedCallback<T>(string[] values) where T : List<string>, new()
        {
            if (_valueInfo.value is not T collection)
            {
                collection = new T();
            }
            bool isValueChanged = false;
            if (values.Length != collection.Count)
            {
                isValueChanged = true;
            }
            else
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i] != collection[i])
                    {
                        isValueChanged = true;
                        break;
                    }
                }
            }
            if (isValueChanged)
            {
                collection.Clear();
                collection.AddRange(values);
                callValueChangedCallback(collection);
            }
        }
        #endregion
        #region 事件
        public event Action<ValueInputField, object> onValueChanged;
        public event Action<ValueInputField, int[], ValueInputFieldButton> onClickButton;
        #endregion
        #region 属性字段
        private ActionNodeValueType valueType => _valueInfo.type.type;
        private bool _isReadonly;
        [SerializeField]
        Dropdown _typeDropdown;
        [SerializeField]
        Text _nameText;

        [Header("操作UI引用")]
        [SerializeField]
        InputField _inputField;
        [SerializeField]
        GameObject _toggleField;
        [SerializeField]
        Toggle _toggle;
        [SerializeField]
        Button _button;
        [SerializeField]
        Text _buttonText;
        [SerializeField]
        Dropdown _dropdown;
        [SerializeField]
        CardViewDeckCardListItem _buildItem;
        [SerializeField]
        Button _buildItemEditButton;
        [SerializeField]
        InputDropdown _inputDropdown;
        [SerializeField]
        MultiInputDropdown _multiInputDropdown;
        [SerializeField]
        GameObject _buffItemObj;
        [SerializeField]
        RecursiveDropdown _buffDropdown;
        [SerializeField]
        Button _buffItemEditButton;

        [Header("列表")]
        [SerializeField]
        RectTransform _listRoot;
        [SerializeField]
        ElementList _childrenElementList;
        [SerializeField]
        Button _addItemButton;

        [Header("下拉列表值")]
        [SerializeField]
        string _backText;
        [SerializeField]
        string _anyPileName;
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
        private string[] _cardTagNames;
        [SerializeField]
        private string[] _cardTagValues;

        [SerializeField]
        private string[] _eventNames;
        [SerializeField]
        private EventReference[] _eventValues;

        [SerializeField]
        private string[] _eventTriggerTimeNames;
        [SerializeField]
        private EventTriggerTime[] _eventTriggerTimeValues;

        [SerializeField]
        private string[] _eventVarNameNames;
        [SerializeField]
        private string[] _eventVarNameValues;

        [SerializeField]
        private CardPropertyNameItem[] _cardPropertyNameItems;
        [SerializeField]
        private BuffReferenceRecursion[] _buffValues;

        [SerializeField]
        private string _defaultValueTypeName;
        [SerializeField]
        private ValueTypeNameMap[] _valueTypeNames;
        [SerializeField]
        private NodeValueType[] _valueTypeValues;

        [SerializeField]
        private string[] _typeNames;
        [SerializeField]
        private string[] _typeValues;

        [SerializeField]
        private string[] _cardTypeNames;
        [SerializeField]
        private string[] _cardTypeValues;

        [SerializeField]
        ActionNodeValueInfo _valueInfo;
        [SerializeField]
        bool _isListItem;
        #endregion
    }

    [Serializable]
    public class BuffReferenceRecursion
    {
        public string name;
        public BuffReference value;
        public List<BuffReferenceRecursion> children;

        public BuffReferenceRecursion(string name = null, BuffReference value = null, List<BuffReferenceRecursion> children = null)
        {
            this.name = name;
            this.value = value;
            this.children = children;
        }
        public RecursiveOptionData toOptionData()
        {
            var optionData = new RecursiveOptionData();
            optionData.text = name;
            optionData.children = children != null ? children.Select(c => c.toOptionData()).ToList() : null;
            return optionData;
        }
        public static int[] getIndexesByValue(List<BuffReferenceRecursion> recursion, DefineReference defineRef)
        {
            if (recursion == null)
                return null;
            var itemIndex = recursion.FindIndex(r => r.value == defineRef);
            if (itemIndex >= 0)
            {
                // 在列表中找到了对应的引用，返回索引。
                return new int[] { itemIndex };
            }
            // 没有在列表中找到对应引用，在子项里找。
            for (int i = 0; i < recursion.Count; i++)
            {
                var buffRef = recursion[i];
                var itemIndexes = getIndexesByValue(buffRef.children, defineRef);
                if (itemIndexes == null)
                    // 子项里没有找到，跳过这个子项。
                    continue;
                // 子项里找到了，在结果前面加上一个当前项的索引。
                return itemIndexes.Prepend(i).ToArray();
            }
            return null;
        }
        public static DefineReference getValueByIndexes(IEnumerable<BuffReferenceRecursion> recursion, int[] indexes)
        {
            if (recursion == null || indexes == null || indexes.Length <= 0)
                return null;
            var index = indexes[0];
            var childRef = recursion.ElementAtOrDefault(index);
            if (childRef == null)
                return null;
            if (indexes.Length <= 1)
            {
                return childRef.value;
            }
            return getValueByIndexes(childRef.children, indexes.Skip(1).ToArray());
        }
    }

    public static class ValueInputFieldHelper 
    {
        public static async Task setInputFieldCardByValue(this ValueInputField inputField, object value, IEnumerable<ICardPoolReference> env, MainManager game)
        {
            if (value is DefineReference cardReference)
            {
                await inputField.setInputFieldCard(cardReference, env, game);
            }
            else if (value is IList list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var element = list[i];
                    var childInputField = inputField.getItemAt(i)?.valueField;
                    if (childInputField == null)
                        continue;
                    await childInputField.setInputFieldCardByValue(element, env, game);
                }
            }
        }
        public static async Task setInputFieldCard(this ValueInputField inputField, DefineReference cardReference, IEnumerable<ICardPoolReference> env, MainManager game)
        {
            if (!inputField)
                return;
            string cardName = string.Empty;
            Sprite image = null;
            if (cardReference != null && cardReference != DefineReference.Empty)
            {
                var localRef = game.card.getCardPoolReferenceInEnv(cardReference.cardPoolId, env);
                // 如果卡池没有被加载，加载卡池。
                await game.card.getOrLoadCardPool(localRef, true);
                var cardDefine = game.card.getCardDefine(localRef, cardReference.defineId);
                if (cardDefine != null)
                {
                    cardName = game.card.getCardName(localRef, cardReference.defineId) ?? game.lang._(StringTable.BUILD_UNKNOWN_CARD_NAME);
                    image = await game.card.loadCardImage(cardDefine, true);
                }
                else
                {
                    cardName = game.lang._(StringTable.BUILD_UNKNOWN_CARD_NAME);
                    image = game.card.getDefaultSprite();
                }
            }
            inputField.setCard(cardName, image);
        }

    }

    public enum ValueInputFieldButton
    {
        EditButton,
        CardButton,
        CardEditButton,
        BuffEditButton
    }
    [Serializable]
    public struct ValueInputFieldProperties
    {
        public string backText;
        public string anyPileName;
        public string[] pileNames;
        public string[] pileValues;

        public string[] keywordNames;
        public string[] keywordValues;

        public string[] cardTagNames;
        public string[] cardTagValues;

        public CardPropertyNameItem[] cardProperties;

        public string[] eventNames;
        public EventReference[] eventValues;

        public string[] eventTriggerTimeNames;
        public EventTriggerTime[] eventTriggerTimeValues;

        public string[] eventVarNameNames;
        public string[] eventVarNameValues;

        public string[] effectTagNames;
        public string[] effectTagValues;

        public BuffReferenceRecursion[] buffValues;

        public string defaultValueTypeName;
        public ValueTypeNameMap[] valueTypeNames;

        public string[] graphTypeNames;
        public string[] graphTypeValues;

        public string[] cardTypeNames;
        public string[] cardTypeValues;

        public static ValueInputFieldProperties eventReference(string[] eventNames, EventReference[] eventValues)
        {
            return new ValueInputFieldProperties()
            {
                eventNames = eventNames,
                eventValues = eventValues
            };
        }
        public static ValueInputFieldProperties eventVar(string[] eventVarNameNames, string[] eventVarNameValues)
        {
            return new ValueInputFieldProperties()
            {
                eventVarNameNames = eventVarNameNames,
                eventVarNameValues = eventVarNameValues
            };
        }
        public static ValueInputFieldProperties keyword(string[] keywordNames, string[] keywordValues)
        {
            return new ValueInputFieldProperties()
            {
                keywordNames = keywordNames,
                keywordValues = keywordValues
            };
        }
        public static ValueInputFieldProperties buffReference(string backText, BuffReferenceRecursion[] buffValues)
        {
            return new ValueInputFieldProperties()
            {
                backText = backText,
                buffValues = buffValues,
            };
        }
        public static ValueInputFieldProperties effectTag(string[] tagNames, string[] tagValues)
        {
            return new ValueInputFieldProperties()
            {
                effectTagNames = tagNames,
                effectTagValues = tagValues,
            };
        }
    }
}