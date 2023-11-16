using EventArgReference = Game.EventArgReference;
using EventReferenceProxy = Game.EventReferenceProxy;
using System;
using System.Collections.Generic;
using Game;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.UI;
using UIHelper = BJSYGameCore.UI.UIHelper;
using System.Linq;

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
            _inputField.onEndEdit.AddListener(onEndEditInputField);
            _toggle.onValueChanged.AddListener(onToggleValueChanged);
            _dropdown.onValueChanged.AddListener(onDropdownValueChangedCallback);
            _inputDropdown.onValueChanged += onInputDropdownValueChangedCallback;
            _multiInputDropdown.onValueChanged += onMultiInputDropdownValueChangedCallback;
            _button.onClick.AddListener(onClickButtonCallback);
            _addItemButton.onClick.AddListener(onClickAddItemButtonCallback);
            _removeItemButton.onClick.AddListener(onClickRemoveButtonCallback);
            _buildItem.onClick += onClickCardButtonCallback;
            _buildItemEditButton.onClick.AddListener(onClickCardEditButtonCallback);
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
            else if (_valueInfo.type == ActionNodeValueType.eventReference)
            {
                onValueChanged?.Invoke(this, _eventValues[index]);
            }
        }
        private void onInputDropdownValueChangedCallback(string value)
        {
            onValueChanged?.Invoke(this, value);
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
        private void onClickItemCardButtonCallback(ValueInputField item)
        {
            onClickItemCardButton?.Invoke(this, _list.IndexOf(item.transform as RectTransform));
        }
        private void onClickItemCardEditButtonCallback(ValueInputField item)
        {
            onClickItemCardEditButton?.Invoke(this, _list.IndexOf(item.transform as RectTransform));
        }
        private void onItemValueChangedCallback(ValueInputField item, object value)
        {
            onItemValueChanged?.Invoke(this, _list.IndexOf(item.transform as RectTransform), value);
        }
        private void onClickAddItemButtonCallback()
        {
            if (_valueInfo.type == ActionNodeValueType.cardReferenceCollection)
            {
                if (_valueInfo.value is CardReferenceCollection cardRefCollection)
                {
                    cardRefCollection.Add(null);
                    _valueInfo.value = cardRefCollection;
                }
                else if (_valueInfo.value is DefineReference[] cardRefs)
                {
                    var newCardRefs = new DefineReference[cardRefs.Length + 1];
                    Array.Copy(cardRefs, newCardRefs, cardRefs.Length);
                    _valueInfo.value = newCardRefs;
                }
                else
                {
                    cardRefCollection = new CardReferenceCollection() 
                    {
                        null
                    };
                    _valueInfo.value = cardRefCollection;
                }
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
                if (_valueInfo.value is CardReferenceCollection cardRefCol)
                {
                    cardRefCol.RemoveAt(index);
                    _valueInfo.value = cardRefCol;
                }
                else if (_valueInfo.value is DefineReference[] cardRefs)
                {
                    var newCardRefs = new List<DefineReference>(cardRefs);
                    newCardRefs.RemoveAt(index);
                    _valueInfo.value = newCardRefs.ToArray();
                }
                else
                {
                    cardRefCol = new CardReferenceCollection();
                    _valueInfo.value = cardRefCol;
                }
                onValueChanged?.Invoke(this, _valueInfo.value);
                update();
            }
            else
            {
                onClickItemRemoveButton?.Invoke(this, index);
            }
        }
        private void onClickCardButtonCallback()
        {
            onClickCardButton?.Invoke(this);
        }
        private void onClickCardEditButtonCallback()
        {
            onClickCardEditButton?.Invoke(this);
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
                updateMultiInputDropdown();
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
                _multiInputDropdown.gameObject.SetActive(false);
                var valueType = array.GetType();
                var elementType = valueType.GetElementType();
                if (elementType == null)
                    return;
                _listRoot.gameObject.SetActive(true);
                UIHelper.updateList(_listContentRoot, GetListTemplate(), _list, array.Length, (i, itemRect) =>
                {
                    ValueInputField item = itemRect.GetComponent<ValueInputField>();
                    item.setReadonly(_isReadonly);
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
                    item.onClickCardButton += onClickItemCardButtonCallback;
                    item.onClickCardEditButton += onClickItemCardEditButtonCallback;
                    item.onClickRemoveButton += onClickItemRemoveButtonCallback;
                }, itemRect =>
                {
                    ValueInputField item = itemRect.GetComponent<ValueInputField>();
                    item.onValueChanged -= onItemValueChangedCallback;
                    item.onClickButton -= onClickItemButtonCallback;
                    item.onClickCardButton -= onClickItemCardButtonCallback;
                    item.onClickCardEditButton -= onClickItemCardEditButtonCallback;
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
                else if (_valueInfo.value is EventReferenceProxy proxy)
                {
                    index = Array.IndexOf(_eventValues, proxy.value);
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

            switch (_valueInfo.type)
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
                        item.setReadonly(_isReadonly);
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
                        item.onClickCardButton += onClickItemCardButtonCallback;
                        item.onClickCardEditButton += onClickItemCardEditButtonCallback;
                        item.onClickRemoveButton += onClickItemRemoveButtonCallback;
                    }, itemRect =>
                    {
                        ValueInputField item = itemRect.GetComponent<ValueInputField>();
                        item.onValueChanged -= onItemValueChangedCallback;
                        item.onClickButton -= onClickItemButtonCallback;
                        item.onClickCardButton -= onClickItemCardButtonCallback;
                        item.onClickCardEditButton -= onClickItemCardEditButtonCallback;
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
                if (!(_valueInfo.value is IEnumerable<DefineReference> cardReferenceCollection))
                {
                    cardReferenceCollection = null;
                }
                UIHelper.updateList(_listContentRoot, GetListTemplate(), _list, cardReferenceCollection != null ? cardReferenceCollection.Count() : 0, (i, itemRect) =>
                {
                    ValueInputField item = itemRect.GetComponent<ValueInputField>();
                    item.setReadonly(_isReadonly);
                    item.setValueInfo(null, new ActionNodeValueInfo()
                    {
                        type = ActionNodeValueType.cardReference,
                        value = cardReferenceCollection.ElementAt(i)
                    }, true);
                }, itemRect =>
                {
                    ValueInputField item = itemRect.GetComponent<ValueInputField>();
                    item.onValueChanged += onItemValueChangedCallback;
                    item.onClickButton += onClickItemButtonCallback;
                    item.onClickCardButton += onClickItemCardButtonCallback;
                    item.onClickCardEditButton += onClickItemCardEditButtonCallback;
                    item.onClickRemoveButton += onClickItemRemoveButtonCallback;
                }, itemRect =>
                {
                    ValueInputField item = itemRect.GetComponent<ValueInputField>();
                    item.onValueChanged -= onItemValueChangedCallback;
                    item.onClickButton -= onClickItemButtonCallback;
                    item.onClickCardButton -= onClickItemCardButtonCallback;
                    item.onClickCardEditButton -= onClickItemCardEditButtonCallback;
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
        public event Action<ValueInputField> onClickCardButton;
        public event Action<ValueInputField> onClickCardEditButton;
        public event Action<ValueInputField, int, object> onItemValueChanged;
        public event Action<ValueInputField, int> onClickItemButton;
        public event Action<ValueInputField, int> onClickItemCardButton;
        public event Action<ValueInputField, int> onClickItemCardEditButton;
        public event Action<ValueInputField> onClickAddItemButton;
        public event Action<ValueInputField> onClickRemoveButton;
        public event Action<ValueInputField, int> onClickItemRemoveButton;
        #endregion
        #region 属性字段
        private bool _isReadonly;
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
        Button _buildItemEditButton;
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
        private EventReference[] _eventValues;
        [SerializeField]
        private string[] _eventVarNameNames;
        [SerializeField]
        private string[] _eventVarNameValues;
        [SerializeField]
        private CardPropertyNameItem[] _cardPropertyNameItems;
        [SerializeField]
        ActionNodeValueInfo _valueInfo;
        [SerializeField]
        bool _isListItem;
        #endregion
    }
}