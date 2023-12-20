using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.UI;
using EventArgReference = Game.EventArgReference;
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
            if (_valueInfo.type.type == ActionNodeValueType.cardReference)
            {
                _buildItem.update(cardDefine, image, skin?.name ?? string.Empty, 0);
            }
        }
        public ValueInputField getItemAt(int index)
        {
            return _list[index].GetComponent<ValueInputField>();
        }
        public void initEventReferences(string[] eventNames, EventReference[] eventValues)
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
        public void initEventVarNames(string[] eventVarNameNames, string[] eventVarNameValues)
        {
            _eventVarNameNames = eventVarNameNames;
            _eventVarNameValues = eventVarNameValues;
        }
        public void initKeywords(string[] keywordNames, string[] keywordValues)
        {
            _keywordNames = keywordNames;
            _keywordValues = keywordValues;
        }
        public void initBuffReferences(string[] buffNames, BuffReference[] buffValues)
        {
            _buffNames = buffNames;
            _buffValues = buffValues;
        }
        public void initCardProperties(CardPropertyNameItem[] values)
        {
            _cardPropertyNameItems = values;
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
            _buildItem.CanClick = canEdit;
            _buildItemEditButton.interactable = canEdit;
            _inputDropdown.interactable = canEdit;
            _multiInputDropdown.interactable = canEdit;
            _addItemButton.interactable = canEdit;
            _removeItemButton.interactable = canEdit;
            foreach (var item in _list)
            {
                var field = item.GetComponent<ValueInputField>();
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
            _removeItemButton.onClick.AddListener(() => onClickRemoveButton?.Invoke(this));
            _buildItem.onClick += () => callButtonCallback(ValueInputFieldButton.CardButton);
            _buildItemEditButton.onClick.AddListener(() => callButtonCallback(ValueInputFieldButton.CardEditButton));
            _buffItemEditButton.onClick.AddListener(() => callButtonCallback(ValueInputFieldButton.BuffEditButton));
        }
        #region 事件回调
        private void onTypeDropdownValueChangedCallback(int index)
        {
            var typeValues = _valueInfo.assignableTypeValues;
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
            var childValueType = new NodeValueType(_valueInfo.type.type, false);
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
        private void onClickItemRemoveButtonCallback(ValueInputField item)
        {
            if (_isReadonly)
                return;
            int index = _list.IndexOf(item.transform as RectTransform);
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
        private void onItemValueChangedCallback(ValueInputField item, object value)
        {
            if (_isReadonly)
                return;
            int index = _list.IndexOf(item.transform as RectTransform);
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
        private void onClickItemButtonCallback(ValueInputField item, ValueInputFieldButton type)
        {
            callItemButtonCallback(item, type);
        }
        private void callButtonCallback(ValueInputFieldButton type)
        {
            onClickButton?.Invoke(this, type);
        }
        private void callItemButtonCallback(ValueInputField item, ValueInputFieldButton type)
        {
            onClickItemButton?.Invoke(this, _list.IndexOf(item.transform as RectTransform), type);
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
                if (string.IsNullOrEmpty(text))
                {
                    callValueChangedCallback(null);
                }
                else
                {
                    callValueChangedCallback(text);
                }
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
        }
        private void onBuffDropdownValueChangedCallback(int index)
        {
            if (valueType == ActionNodeValueType.buffReference)
            {
                callValueChangedCallback(_buffValues[index]);
            }
        }
        private void onInputDropdownValueChangedCallback(string value)
        {
            callValueChangedCallback(value);
        }
        private void onMultiInputDropdownValueChangedCallback(string[] values)
        {
            if (valueType == ActionNodeValueType.pileNameCollection)
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
                    callValueChangedCallback(pileNameCollection);
                }
            }
            else if (valueType == ActionNodeValueType.effectTagCollection)
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
                    callValueChangedCallback(effectTagCol);
                }
            }
            else if (valueType == ActionNodeValueType.keywords)
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
                    callValueChangedCallback(keywordCol);
                }
            }
        }
        #endregion
        #endregion
        private void update()
        {
            _removeItemButton.gameObject.SetActive(_isListItem);

            updateTypeDropdown();

            if (_valueInfo.type.isArray)
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
            var typeNames = _valueInfo.assignableTypeNames;
            var typeValues = _valueInfo.assignableTypeValues;
            var valueTypeCount = typeValues?.Length ?? 0;
            _typeDropdown.gameObject.SetActive(valueTypeCount > 0);
            _typeDropdown.ClearOptions();
            if (typeValues != null && typeNames != null)
            {
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
            if (_valueInfo.value is Array array)
            {
                _inputField.gameObject.SetActive(false);
                _toggle.gameObject.SetActive(false);
                _dropdown.gameObject.SetActive(false);
                _buffItemObj.gameObject.SetActive(false);
                _inputDropdown.gameObject.SetActive(false);
                _multiInputDropdown.gameObject.SetActive(false);
                _button.gameObject.SetActive(false);
                _buildItem.gameObject.SetActive(false);
                _multiInputDropdown.gameObject.SetActive(false);

                _listRoot.gameObject.SetActive(true);
                var childValueType = new NodeValueType(_valueInfo.type.type, false);
                UIHelper.updateList(_listContentRoot, GetListTemplate(), _list, array.Length, (i, itemRect) =>
                {
                    ValueInputField item = itemRect.GetComponent<ValueInputField>();
                    item.setReadonly(_isReadonly);
                    item.setValueInfo(null, new ActionNodeValueInfo()
                    {
                        type = childValueType,
                        value = array.GetValue(i)
                    }, true);
                }, itemRect =>
                {
                    ValueInputField item = itemRect.GetComponent<ValueInputField>();
                    addListItemCallbacks(item);
                }, itemRect =>
                {
                    ValueInputField item = itemRect.GetComponent<ValueInputField>();
                    removeListItemCallbacks(item);
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
            if (valueType == ActionNodeValueType.enumValue)
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
            else if (valueType == ActionNodeValueType.eventReference)
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
                    index = Array.FindIndex(_eventValues, e => e.cardPoolId == 0 && e.eventName == str);
                }
                else if (_valueInfo.value is EventArgReference evtArgRef)
                {
                    index = Array.FindIndex(_eventValues, e => e.cardPoolId == 0 && e.eventName == evtArgRef.value);
                }
                else if (_valueInfo.value is EventReference evtRef)
                {
                    index = Array.IndexOf(_eventValues, evtRef);
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

            switch (valueType)
            {
                // 牌堆名称
                case ActionNodeValueType.pileName:
                    updateInputDropdownValue<PileName>(_pileNames, _pileValues);
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
            if (valueType == ActionNodeValueType.pileNameCollection)
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
            else if (valueType == ActionNodeValueType.effectTagCollection)
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
            else if (valueType == ActionNodeValueType.keywords)
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
                _buffDropdown.ClearOptions();
                _buffDropdown.AddOptions(new List<string>(_buffNames));
                int index = 1;
                if (_valueInfo.value is DefineReference defRef)
                {
                    index = Array.FindIndex(_buffValues, v => v == defRef);
                }
                else if (_valueInfo.value is BuffReference buffRef)
                {
                    index = Array.IndexOf(_buffValues, buffRef);
                }
                if (index < 0)
                    index = 1;
                _buffDropdown.SetValueWithoutNotify(index);
            }
            else
            {
                _buffItemObj.gameObject.SetActive(false);
            }
        }

        private void updateInputDropdownValue<T>(string[] names, string[] values) where T : NodeConstProxy<string>
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
        private void addListItemCallbacks(ValueInputField item)
        {
            item.onValueChanged += onItemValueChangedCallback;
            item.onClickButton += onClickItemButtonCallback;
            item.onClickRemoveButton += onClickItemRemoveButtonCallback;
        }
        private void removeListItemCallbacks(ValueInputField item)
        {
            item.onValueChanged -= onItemValueChangedCallback;
            item.onClickButton -= onClickItemButtonCallback;
            item.onClickRemoveButton -= onClickItemRemoveButtonCallback;
        }
        private void callValueChangedCallback(object value)
        {
            onValueChanged?.Invoke(this, value);
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
        public event Action<ValueInputField, ValueInputFieldButton> onClickButton;
        public event Action<ValueInputField, int, ValueInputFieldButton> onClickItemButton;
        private event Action<ValueInputField> onClickRemoveButton;
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
        Button _buildItemEditButton;
        [SerializeField]
        InputDropdown _inputDropdown;
        [SerializeField]
        MultiInputDropdown _multiInputDropdown;
        [SerializeField]
        GameObject _buffItemObj;
        [SerializeField]
        Dropdown _buffDropdown;
        [SerializeField]
        Button _buffItemEditButton;

        [Header("列表")]
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

        [Header("下拉列表值")]
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
        private EventReference[] _eventValues;
        [SerializeField]
        private string[] _eventVarNameNames;
        [SerializeField]
        private string[] _eventVarNameValues;
        [SerializeField]
        private CardPropertyNameItem[] _cardPropertyNameItems;
        [SerializeField]
        private string[] _buffNames;
        [SerializeField]
        private BuffReference[] _buffValues;
        [SerializeField]
        ActionNodeValueInfo _valueInfo;
        [SerializeField]
        bool _isListItem;
        #endregion
    }

    public enum ValueInputFieldButton
    {
        EditButton,
        CardButton,
        CardEditButton,
        BuffEditButton
    }
}