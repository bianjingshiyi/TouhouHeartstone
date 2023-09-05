using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BJSYGameCore.UI;
using Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BuffEditorPanel : MonoBehaviour
    {
        #region 公有方法
        public void setTitleText(string title)
        {
            _editorTitle.text = title;
        }
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
                    type = info.valueType, 
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
        public void init(string[] properties, (string name, string value)[] keywords)
        {
            var item = _propertyModifierItemTemplate.GetComponent<PropertyModifierListItem>();
            item.updateSelectableValues(properties);
            item.init(keywords);
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
        public void reset()
        {
            displayPage(_buffPage, _buffPageIndicator);
            _skinDropdown.Hide();
            _skinNameInputField.DeactivateInputField();
            _languageDropdown.Hide();
            _nameInputField.DeactivateInputField();
            _descInputField.DeactivateInputField();
        }

        public void displayCardSelectPanel(bool display)
        {
            _cardPoolViewPanelObj.SetActive(display);
        }
        #endregion

        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            // Generic
            _deleteCardButton.onClick.AddListener(onClickDeleteCardButtonCallback);
            _saveCardButton.onClick.AddListener(onClickSaveCardButtonCallback);
            _closeCardButton.onClick.AddListener(onClickCloseCardButtonCallback);

            _addPropertyModifierButton.onClick.AddListener(onClickAddModifierButton);

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
            _nodesPageButton.onClick.AddListener(() => displayPage(_nodesPage, _nodesPageIndicator));
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
                throw new InvalidOperationException("设置增益图像失败：" + e, e);
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
        #endregion

        #region 语言
        private void onLanguageDropdownValueChanged(int value)
        {
            onChangeLanguage?.Invoke(value);
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
        private void onClickDeleteCardButtonCallback()
        {
            onClickDeleteCardButton?.Invoke();
        }
        private void onClickSaveCardButtonCallback()
        {
            onClickSaveButton?.Invoke();
        }
        private void onClickCloseCardButtonCallback()
        {
            onClickCloseButton?.Invoke();
        }
        #endregion
        private void displayPage(GameObject toDisplay, GameObject indicator)
        {
            _buffPage.SetActive(false);
            _propertyPage.SetActive(false);
            _nodesPage.SetActive(false);

            _buffPageIndicator.SetActive(false);
            _propertyPageIndicator.SetActive(false);
            _nodesPageIndicator.SetActive(false);

            toDisplay.SetActive(true);
            indicator.SetActive(true);
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
        #endregion

        /// <summary>
        /// 当更换了语言，参数为更换的语言索引。
        /// </summary>
        public event Action<int> onChangeLanguage;

        #region 修改器
        public event Action<int> OnRemoveModifier;
        public event Action onAddModifier;
        public event Action<int, int> OnChangeModifierProperty;
        public event Action<int, string> OnChangeModifierCustomPropName;
        public event Action<int, int> OnChangeModifierOperator;
        public event Action<int, object> OnChangeModifierValue;
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
        /// <summary>
        /// 当点击删除卡牌按钮
        /// </summary>
        public event Action onClickDeleteCardButton;
        ///// <summary>
        ///// 当点击保存按钮
        ///// </summary>
        public event Action onClickSaveButton;
        ///// <summary>
        ///// 当点击关闭按钮
        ///// </summary>
        public event Action onClickCloseButton;
        #endregion

        #region 属性字段
        [SerializeField]
        Text _editorTitle;
        [SerializeField]
        Text _idText;
        [SerializeField]
        Button _editBuffImageButton;
        [SerializeField]
        Image _buffImage;

        [Header("General")]
        [SerializeField]
        Button _deleteCardButton;
        [SerializeField]
        Button _saveCardButton;
        [SerializeField]
        Button _closeCardButton;

        [Header("Property Modifiers")]
        [SerializeField]
        RectTransform _propertyModifierRoot;
        [SerializeField]
        RectTransform _propertyModifierItemTemplate;
        [SerializeField]
        List<RectTransform> _propertyModifierList;
        [SerializeField]
        Button _addPropertyModifierButton;

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

        [Header("Pages")]
        [SerializeField]
        Button _buffPageButton;
        [SerializeField]
        Button _propertyPageButton;
        [SerializeField]
        Button _nodesPageButton;
        [SerializeField]
        GameObject _buffPage;
        [SerializeField]
        GameObject _propertyPage;
        [SerializeField]
        GameObject _nodesPage;
        [SerializeField]
        GameObject _buffPageIndicator;
        [SerializeField]
        GameObject _propertyPageIndicator;
        [SerializeField]
        GameObject _nodesPageIndicator;
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
}