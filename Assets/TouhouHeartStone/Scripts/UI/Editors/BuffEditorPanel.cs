using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BuffEditorPanel : EditorPanelBase
    {
        #region 公有方法
        /// <summary>
        /// 设置增益的图片。
        /// </summary>
        /// <param name="sprite">卡牌图片</param>
        public void setImage(Sprite sprite)
        {
            _buffImage.sprite = sprite;
        }
        public void setId(int id)
        {
            _idText.text = id.ToString();
        }
        #region 修改器
        public void updateModifiers(BuffModifierInfo[] modifierInfos)
        {
            UIHelper.updateList(_propertyModifierRoot, _propertyModifierItemTemplate, _propertyModifierList, modifierInfos.Length, (i, rect) =>
            {
                var item = rect.GetComponent<PropertyModifierListItem>();
                var info = modifierInfos[i];
                var valueInfo = new ActionNodeValueInfo()
                {
                    type = new NodeValueType(info.valueType),
                    value = info.value
                };
                item.updateValues(info.propertyTypeIndex, info.customPropertyName, info.modifyTypes, info.modifyTypeIndex, valueInfo);
                item.setCustomPropertyFieldVisible(info.isCustomProperty);
            },
            rect =>
            {
                var item = rect.GetComponent<PropertyModifierListItem>();
                item.onClickRemoveButton += onClickRemoveModifierButton;
                item.onPropertyDropdownValueChanged += onModifierPropertyChanged;
                item.onCustomPropFieldValueChanged += onModifierCustomPropNameChanged;
                item.onModifyDropdownValueChanged += onModifierOperatorChanged;
                item.onValueFieldValueChanged += onModifierValueChanged;
            },
            rect =>
            {
                var item = rect.GetComponent<PropertyModifierListItem>();
                item.onClickRemoveButton -= onClickRemoveModifierButton;
                item.onPropertyDropdownValueChanged -= onModifierPropertyChanged;
                item.onCustomPropFieldValueChanged -= onModifierCustomPropNameChanged;
                item.onModifyDropdownValueChanged -= onModifierOperatorChanged;
                item.onValueFieldValueChanged -= onModifierValueChanged;
            }
            );
        }
        public void init(string[] properties)
        {
            var item = _propertyModifierItemTemplate.GetComponent<PropertyModifierListItem>();
            item.updateSelectableValues(properties);
        }
        public void initValueInputFields(ValueInputFieldProperties fieldProperties)
        {
            var modifierItem = _propertyModifierItemTemplate.GetComponent<PropertyModifierListItem>();
            modifierItem.initInputFieldProperties(fieldProperties);

            var propItem = _customPropertyList.getTemplate<PropertyItemField>();
            propItem.initInputFieldProperties(fieldProperties);
        }
        #endregion

        #region 持续限制
        public void updateExistLimits(BuffExistLimitInfo[] modifierInfos)
        {
            UIHelper.updateList(_existLimitRoot, _existLimitItemTemplate, _existLimitList, modifierInfos.Length, (i, rect) =>
            {
                var item = rect.GetComponent<ExistLimitListItem>();
                var info = modifierInfos[i];
                item.update(info.header, info.eventIndex, info.count);
            },
            rect =>
            {
                var item = rect.GetComponent<ExistLimitListItem>();
                item.onClickRemoveButton += onClickRemoveExistLimitButton;
                item.onEventDropdownValueChanged += onExistLimitEventChanged;
                item.onCountValueChanged += onExistLimitCountChanged;
            },
            rect =>
            {
                var item = rect.GetComponent<ExistLimitListItem>();
                item.onClickRemoveButton -= onClickRemoveExistLimitButton;
                item.onEventDropdownValueChanged -= onExistLimitEventChanged;
                item.onCountValueChanged -= onExistLimitCountChanged;
            }
            );
        }
        public void initExistLimits(string[] eventNames)
        {
            var item = _existLimitItemTemplate.GetComponent<ExistLimitListItem>();
            item.initEvents(eventNames);
        }
        #endregion

        #region 自定义属性
        public void setCustomPropertyDialogVisible(bool visible)
        {
            if (visible)
            {
                _customPropertyDialog.display();
            }
            else
            {
                _customPropertyDialog.hide();
            }
        }
        public void showCustomPropertyDialogError(string message)
        {
            _customPropertyDialog.showErrorTip(message);
        }
        public PropertyItemField getCustomPropertyItemField(string name)
        {
            return _customPropertyList.getElements<PropertyItemField>().FirstOrDefault(f => f.propertyName == name);
        }
        public void updateCustomProperties(PropertyItemInfo[] propertyInfos)
        {
            _customPropertyList.updateList(propertyInfos.Length, (i, rect) =>
            {
                var item = rect.GetComponent<PropertyItemField>();
                item.updateItem(propertyInfos[i]);
            },
            rect =>
            {
                var item = rect.GetComponent<PropertyItemField>();
                item.onClickButton += onClickCustomPropertyButtonCallback;
                item.onClickDelete += onCustomPropertyDeletedCallback;
                item.onValueChanged += onCustomPropertyChangedCallback;
            },
            rect =>
            {
                var item = rect.GetComponent<PropertyItemField>();
                item.onClickButton -= onClickCustomPropertyButtonCallback;
                item.onClickDelete -= onCustomPropertyDeletedCallback;
                item.onValueChanged -= onCustomPropertyChangedCallback;
            }
            );
        }
        public PropertyItemField[] getCustomPropertyItemFields()
        {
            return _customPropertyList.getElements<PropertyItemField>().ToArray();
        }
        public void setCustomPropertyTypes(ActionNodeValueType[] types, string[] names)
        {
            _customPropertyDialog.setTypes(types, names);
        }
        #endregion

        #region 皮肤
        /// <summary>
        /// 设置皮肤切换选项
        /// </summary>
        /// <param name="skinNames"></param>
        public void setSkinNames(string[] skinNames)
        {
            _skinDropdown.ClearOptions();
            List<string> optionList = new List<string>(skinNames)
            {
                "新建皮肤"
            };
            _skinDropdown.AddOptions(optionList);
        }
        /// <summary>
        /// 设置当前皮肤索引
        /// </summary>
        /// <param name="skinIndex"></param>
        public void setSkin(int skinIndex)
        {
            _skinDropdown.SetValueWithoutNotify(skinIndex);
        }
        /// <summary>
        /// 设置增益名称
        /// </summary>
        /// <param name="name"></param>
        public void setName(string name)
        {
            _nameInputField.SetTextWithoutNotify(name);
        }
        /// <summary>
        /// 设置卡牌的描述
        /// </summary>
        /// <param name="text"></param>
        public void setDesc(string text)
        {
            _descInputField.SetTextWithoutNotify(text);
        }
        #endregion

        #region 语言
        public void initLanguages(string[] languages)
        {
            _languageDropdown.ClearOptions();
            _languageDropdown.AddOptions(new List<string>(languages));
        }
        /// <summary>
        /// 设置当前语言索引
        /// </summary>
        /// <param name="languageIndex"></param>
        public void setLanguage(int languageIndex)
        {
            _languageDropdown.SetValueWithoutNotify(languageIndex);
        }
        #endregion

        #region 语言
        public void initPersistentAnims(string[] animNames)
        {
            _persistentAnimDropdown.ClearOptions();
            _persistentAnimDropdown.AddOptions(new List<string>(animNames));
        }
        /// <summary>
        /// 设置当前语言索引
        /// </summary>
        /// <param name="index"></param>
        public void setPersistentAnim(int index)
        {
            _persistentAnimDropdown.SetValueWithoutNotify(index);
        }
        #endregion

        #region 效果
        public void showEffectListItemInputField(int index, string value)
        {
            _effectList.showItemInputField(index, value);
        }
        public void hideEffectListItemInputField(int index)
        {
            _effectList.hideItemInputField(index);
        }
        public void setEffectTypes(string[] effectTypeNames)
        {
            _effectList.setAddEffectMenu(effectTypeNames);
        }
        public void setEffectScrollValue(float value)
        {
            _effectList.setScrollValue(value);
        }
        /// <summary>
        /// 设置效果列表
        /// </summary>
        /// <param name="effectNames"></param>
        public void setEffects(string[] effectNames, int activeIndex = -1)
        {
            _effectList.updateList(effectNames, activeIndex);
        }
        #endregion
        public override void reset()
        {
            displayPage(_buffPage, _buffPageIndicator);
            _skinDropdown.Hide();
            _skinNameInputField.DeactivateInputField();
            _languageDropdown.Hide();
            _persistentAnimDropdown.Hide();
            _nameInputField.DeactivateInputField();
            _descInputField.DeactivateInputField();
        }

        public void updateAnimationRefs(AnimationRefViewData[] viewDatas, string[] anims, string[] positions)
        {
            _animationRefList.update(viewDatas, anims, positions);
        }
        #endregion

        #region 私有方法
        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            _addPropertyModifierButton.onClick.AddListener(onClickAddModifierButton);
            _addExistLimitButton.onClick.AddListener(onClickAddExistLimitButton);

            // Skin
            _editBuffImageButton.onClick.AddListener(onEditImageButtonClicked);
            _skinDropdown.gameObject.SetActive(true);
            _skinDropdown.onValueChanged.AddListener(onSkinDropdownValueChanged);
            _changeSkinNameButton.gameObject.SetActive(true);
            _changeSkinNameButton.onClick.AddListener(onChangeSkinNameButtonClicked);
            _deleteSkinButton.gameObject.SetActive(true);
            _deleteSkinButton.onClick.AddListener(onDeleteSkinButtonClicked);
            _skinNameInputField.gameObject.SetActive(false);
            _skinNameInputField.onEndEdit.AddListener(onSkinNameInputFieldEndEdit);
            _languageDropdown.onValueChanged.AddListener(onLanguageDropdownValueChanged);
            _persistentAnimDropdown.onValueChanged.AddListener(onPersistentAnimDropdownValueChanged);

            _nameInputField.onEndEdit.AddListener(onNameInputFieldEndEdit);
            _descInputField.onEndEdit.AddListener(onDescInputFieldEndEdit);

            // Effects
            _effectList.onClickAddEffect += onAddEffectButtonClicked;
            _effectList.onClickItemEditButton += onClickEffectListItemEditButton;
            _effectList.onClickItemRemoveButton += onClickEffectListItemRemoveButton;
            _effectList.onItemInputFieldEndEdit += onEffectListItemInputFieldEndEditCallback;

            // Pages.
            _buffPageButton.onClick.AddListener(() => displayPage(_buffPage, _buffPageIndicator));
            _propertyPageButton.onClick.AddListener(() => displayPage(_propertyPage, _propertyPageIndicator));

            // Custom Properties.
            _addCustomPropertyButton.onClick.AddListener(() => onAddCustomProperty?.Invoke());
            _customPropertyDialog.onConfirm += info => onConfirmCreateCustomProperty?.Invoke(info);

            _animationRefList.onAddButtonClicked += onAddAnimationRefButtonClickedCallback;
            _animationRefList.onAnimationChanged += onAnimationRefAnimationChangedCallback;
            _animationRefList.onSourcePositionChanged += onAnimationRefSourcePositionChangedCallback;
            _animationRefList.onTargetPositionChanged += onAnimationRefTargetPositionChangedCallback;
            _animationRefList.onRemoveButtonClicked += onRemoveAnimationRefButtonClickedCallback;
        }
        #endregion
        #region 事件回调

        #region 皮肤
        private void onSkinDropdownValueChanged(int value)
        {
            onChangeSkin?.Invoke(value);
        }
        private void onChangeSkinNameButtonClicked()
        {
            _skinDropdown.gameObject.SetActive(false);
            _changeSkinNameButton.gameObject.SetActive(false);
            _deleteSkinButton.gameObject.SetActive(false);
            _skinNameInputField.gameObject.SetActive(true);
            _skinNameInputField.SetTextWithoutNotify(null);
            _skinNameInputField.Select();
        }
        private void onSkinNameInputFieldEndEdit(string text)
        {
            _skinDropdown.gameObject.SetActive(true);
            _changeSkinNameButton.gameObject.SetActive(true);
            _deleteSkinButton.gameObject.SetActive(true);
            _skinNameInputField.gameObject.SetActive(false);
            onChangeSkinName?.Invoke(text);
        }
        private void onDeleteSkinButtonClicked()
        {
            onClickDeleteSkinButton?.Invoke();
        }
        private async void onEditImageButtonClicked()
        {
            _editBuffImageButton.gameObject.SetActive(false);
            try
            {
                Task<Sprite> task = onNeedSelectImage?.Invoke();
                Sprite sprite = task != null ? await task : null;
                if (sprite != null)
                {
                    onChangeBuffImage?.Invoke(sprite);
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"设置增益图像失败：{e}", e);
            }
            finally
            {
                _editBuffImageButton.gameObject.SetActive(true);
            }
        }
        private void onNameInputFieldEndEdit(string text)
        {
            onChangeName?.Invoke(text);
        }
        private void onDescInputFieldEndEdit(string text)
        {
            onChangeDesc?.Invoke(text);
        }
        private void onLanguageDropdownValueChanged(int value)
        {
            onChangeLanguage?.Invoke(value);
        }
        private void onPersistentAnimDropdownValueChanged(int value)
        {
            onChangePersistentAnim?.Invoke(value);
        }
        #endregion
        #region 修改器
        private void onClickAddModifierButton()
        {
            onAddModifier?.Invoke();
        }
        private void onClickRemoveModifierButton(PropertyModifierListItem item)
        {
            OnRemoveModifier?.Invoke(_propertyModifierList.IndexOf(item.transform as RectTransform));
        }
        private void onModifierPropertyChanged(PropertyModifierListItem item, int propertyIndex)
        {
            OnChangeModifierProperty?.Invoke(_propertyModifierList.IndexOf(item.transform as RectTransform), propertyIndex);
        }
        private void onModifierCustomPropNameChanged(PropertyModifierListItem item, string name)
        {
            OnChangeModifierCustomPropName?.Invoke(_propertyModifierList.IndexOf(item.transform as RectTransform), name);
        }
        private void onModifierOperatorChanged(PropertyModifierListItem item, int operatorIndex)
        {
            OnChangeModifierOperator?.Invoke(_propertyModifierList.IndexOf(item.transform as RectTransform), operatorIndex);
        }
        private void onModifierValueChanged(PropertyModifierListItem item, object value)
        {
            OnChangeModifierValue?.Invoke(_propertyModifierList.IndexOf(item.transform as RectTransform), value);
        }
        #endregion
        #region 持续限制
        private void onClickAddExistLimitButton()
        {
            onAddExistLimit?.Invoke();
        }
        private void onClickRemoveExistLimitButton(ExistLimitListItem item)
        {
            onRemoveExistLimit?.Invoke(_existLimitList.IndexOf(item.transform as RectTransform));
        }
        private void onExistLimitEventChanged(ExistLimitListItem item, int eventIndex)
        {
            onChangeExistLimitEvent?.Invoke(_existLimitList.IndexOf(item.transform as RectTransform), eventIndex);
        }
        private void onExistLimitCountChanged(ExistLimitListItem item, int count)
        {
            onChangeExistLimitCount?.Invoke(_existLimitList.IndexOf(item.transform as RectTransform), count);
        }
        #endregion

        #region 自定义属性
        private void onClickCustomPropertyButtonCallback(string name, int[] indexes, ValueInputFieldButton button)
        {
            onClickCustomPropertyButton?.Invoke(name, indexes, button);
        }
        private void onCustomPropertyDeletedCallback(string name)
        {
            onRemoveCustomProperty?.Invoke(name);
        }
        private void onCustomPropertyChangedCallback(string name, object value)
        {
            onCustomPropertyChanged?.Invoke(name, value);
        }
        #endregion
        private void onClickEffectListItemEditButton(int index)
        {
            onClickEditEffectButton?.Invoke(index);
        }
        private void onClickEffectListItemRemoveButton(int index)
        {
            onClickRemoveEffectButton?.Invoke(index);
        }
        private void onAddEffectButtonClicked(int index)
        {
            onClickAddEffectMenu?.Invoke(index);
        }
        private void onEffectListItemInputFieldEndEditCallback(int index, string value)
        {
            onEffectListItemInputFieldEndEdit?.Invoke(index, value);
        }

        #region 特效
        private void onAddAnimationRefButtonClickedCallback()
        {
            onAddAnimationRefButtonClicked?.Invoke();
        }
        private void onAnimationRefAnimationChangedCallback(int index, int animationIndex)
        {
            onAnimationRefAnimationChanged?.Invoke(index, animationIndex);
        }
        private void onAnimationRefSourcePositionChangedCallback(int index, int sourcePosition)
        {
            onAnimationRefSourcePositionChanged?.Invoke(index, sourcePosition);
        }
        private void onAnimationRefTargetPositionChangedCallback(int index, int targetPosition)
        {
            onAnimationRefTargetPositionChanged?.Invoke(index, targetPosition);
        }
        private void onRemoveAnimationRefButtonClickedCallback(int index)
        {
            onRemoveAnimationRefButtonClicked?.Invoke(index);
        }
        #endregion

        #endregion

        protected override IEnumerable<GameObject> getPages()
        {
            return base.getPages().Prepend(_propertyPage).Prepend(_buffPage);
        }
        protected override IEnumerable<GameObject> getPageIndicators()
        {
            return base.getPageIndicators().Prepend(_propertyPageIndicator).Prepend(_buffPageIndicator);
        }

        #endregion

        #region 事件
        #region 皮肤
        /// <summary>
        /// 当更换了皮肤，参数为更换的皮肤索引。
        /// </summary>
        public event Action<int> onChangeSkin;
        /// <summary>
        /// 当更改当前皮肤名字，参数为更改名称。
        /// </summary>
        public event Action<string> onChangeSkinName;
        /// <summary>
        /// 当点击删除当前皮肤按钮
        /// </summary>
        public event Action onClickDeleteSkinButton;
        /// <summary>
        /// 当卡牌名称被更改，参数为更改后的名称。
        /// </summary>
        public event Action<string> onChangeName;
        /// <summary>
        /// 当需要选择卡牌图片，需要返回一个被选择图片（可以为空）
        /// </summary>
        public event Func<Task<Sprite>> onNeedSelectImage;
        /// <summary>
        /// 当修改增益图片时触发的事件。
        /// </summary>
        public event Action<Sprite> onChangeBuffImage;
        /// <summary>
        /// 当更改卡牌描述，参数为更改后的卡牌描述。
        /// </summary>
        public event Action<string> onChangeDesc;
        /// <summary>
        /// 当更换了语言，参数为更换的语言索引。
        /// </summary>
        public event Action<int> onChangeLanguage;
        /// <summary>
        /// 当更换了持续特效，参数为更换的特效索引。
        /// </summary>
        public event Action<int> onChangePersistentAnim;
        #endregion

        #region 修改器
        public event Action<int> OnRemoveModifier;
        public event Action onAddModifier;
        public event Action<int, int> OnChangeModifierProperty;
        public event Action<int, string> OnChangeModifierCustomPropName;
        public event Action<int, int> OnChangeModifierOperator;
        public event Action<int, object> OnChangeModifierValue;
        #endregion

        #region 持续限制
        public event Action<int> onRemoveExistLimit;
        public event Action onAddExistLimit;
        public event Action<int, int> onChangeExistLimitEvent;
        public event Action<int, int> onChangeExistLimitCount;
        #endregion

        #region 自定义属性
        public event Action onAddCustomProperty;
        public event Action<PropertyItemInfo> onConfirmCreateCustomProperty;
        public event Action<string, int[], ValueInputFieldButton> onClickCustomPropertyButton;
        public event Action<string> onRemoveCustomProperty;
        public event Action<string, object> onCustomPropertyChanged;
        #endregion

        /// <summary>
        /// 当点击编辑效果按钮，参数为效果索引。
        /// </summary>
        public event Action<int> onClickEditEffectButton;
        /// <summary>
        /// 当点击移除效果按钮，参数为效果索引。
        /// </summary>
        public event Action<int> onClickRemoveEffectButton;
        /// <summary>
        /// 当点击添加效果按钮
        /// </summary>
        public event Action<int> onClickAddEffectMenu;
        /// <summary>
        /// 当效果标题输入框结束编辑
        /// </summary>
        public event Action<int, string> onEffectListItemInputFieldEndEdit;
        #region 特效
        public event Action onAddAnimationRefButtonClicked;
        public event Action<int, int> onAnimationRefAnimationChanged;
        public event Action<int, int> onAnimationRefSourcePositionChanged;
        public event Action<int, int> onAnimationRefTargetPositionChanged;
        public event Action<int> onRemoveAnimationRefButtonClicked;
        #endregion
        #endregion

        #region 属性字段
        [SerializeField]
        Text _idText;
        [SerializeField]
        Button _editBuffImageButton;
        [SerializeField]
        Image _buffImage;

        [Header("Property Modifiers")]
        [SerializeField]
        RectTransform _propertyModifierRoot;
        [SerializeField]
        RectTransform _propertyModifierItemTemplate;
        [SerializeField]
        List<RectTransform> _propertyModifierList;
        [SerializeField]
        Button _addPropertyModifierButton;

        [Header("Exist Limits")]
        [SerializeField]
        RectTransform _existLimitRoot;
        [SerializeField]
        RectTransform _existLimitItemTemplate;
        [SerializeField]
        List<RectTransform> _existLimitList;
        [SerializeField]
        Button _addExistLimitButton;

        [Header("Custom Propertiess")]
        [SerializeField]
        ElementList _customPropertyList;
        [SerializeField]
        Button _addCustomPropertyButton;
        [SerializeField]
        CreatePropertyItemDialog _customPropertyDialog;

        [Header("Skin")]
        [SerializeField]
        Dropdown _skinDropdown;
        [SerializeField]
        Button _changeSkinNameButton;
        [SerializeField]
        Button _deleteSkinButton;
        [SerializeField]
        InputField _skinNameInputField;
        [SerializeField]
        EffectList _effectList;
        [SerializeField]
        GameObject _cardPoolViewPanelObj;
        [SerializeField]
        InputField _nameInputField;
        [SerializeField]
        InputField _descInputField;
        [SerializeField]
        Dropdown _languageDropdown;
        [SerializeField]
        Dropdown _persistentAnimDropdown;
        [SerializeField]
        AnimationRefList _animationRefList;

        [Header("Pages")]
        [SerializeField]
        Button _buffPageButton;
        [SerializeField]
        Button _propertyPageButton;
        [SerializeField]
        GameObject _buffPage;
        [SerializeField]
        GameObject _propertyPage;
        [SerializeField]
        GameObject _buffPageIndicator;
        [SerializeField]
        GameObject _propertyPageIndicator;
        #endregion
    }

    public struct BuffModifierInfo
    {
        public int propertyTypeIndex;
        public bool isCustomProperty;
        public string customPropertyName;
        public string[] modifyTypes;
        public int modifyTypeIndex;
        public ActionNodeValueType valueType;
        public object value;
    }

    public struct BuffExistLimitInfo
    {
        public string header;
        public int eventIndex;
        public int count;
    }
}