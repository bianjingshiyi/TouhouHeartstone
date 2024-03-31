using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Controllers;
using Game;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class CardEditorPanel : EditorPanelBase
    {
        public void setId(int id)
        {
            _idText.text = id.ToString();
        }
        public void setSelectableCharacters(string[] charactersName)
        {
            _characterDropdown.ClearOptions();
            _characterDropdown.AddOptions(new List<string>(charactersName));
        }
        public void setCharacter(int index)
        {
            _characterDropdown.SetValueWithoutNotify(index);
        }
        /// <summary>
        /// 设置皮肤切换选项
        /// </summary>
        /// <param name="skinNames"></param>
        public void setSkinNames(string[] skinNames)
        {
            _skinDropdown.ClearOptions();
            List<string> optionList = new List<string>(skinNames);
            optionList.Add("新建皮肤");
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
        /// <summary>
        /// 设置可选的卡牌类型
        /// </summary>
        /// <param name="types"></param>
        public void initTypes(string[] types)
        {
            _typeDropdown.ClearOptions();
            _typeDropdown.AddOptions(new List<string>(types));
        }
        /// <summary>
        /// 设置卡牌的卡框类型索引
        /// </summary>
        /// <param name="typeIndex"></param>
        public void setType(int typeIndex, string typeName)
        {
            _typeDropdown.SetValueWithoutNotify(typeIndex);
            _card.setTypeByName(typeName);
        }
        public void setIsToken(bool value)
        {
            _isTokenToggle.SetIsOnWithoutNotify(value);
        }
        public void setTags(string[] tags)
        {
            List<string> tagList = new List<string>(tags);
            tagList.RemoveAll(s => string.IsNullOrEmpty(s));
            tags = tagList.ToArray();
            _tagInputField.text = string.Join(",", tags);
        }
        /// <summary>
        /// 设置费用
        /// </summary>
        /// <param name="cost"></param>
        public void setCost(string cost)
        {
            _card.setCost(cost);
            _costInputField.SetTextWithoutNotify(cost);
        }
        /// <summary>
        /// 设置卡牌名称
        /// </summary>
        /// <param name="name"></param>
        public void setName(string name)
        {
            _card.setName(name);
            _nameInputField.SetTextWithoutNotify(name);
        }
        /// <summary>
        /// 设置卡牌副名
        /// </summary>
        /// <param name="subName"></param>
        public void setSubName(string subName)
        {
            _card.setSubname(subName);
            _subnameInputField.SetTextWithoutNotify(subName);
        }
        /// <summary>
        /// 设置卡牌的图片。
        /// </summary>
        /// <param name="sprite">卡牌图片</param>
        public void setImage(Sprite sprite)
        {
            _card.setImage(sprite);
        }
        /// <summary>
        /// 设置卡牌的描述
        /// </summary>
        /// <param name="text"></param>
        public void setDesc(string text)
        {
            _card.setDesc(text);
            _descInputField.SetTextWithoutNotify(text);
        }
        /// <summary>
        /// 设置卡牌的攻击力
        /// </summary>
        /// <param name="attack"></param>
        public void setAttack(string attack)
        {
            _card.setAttack(attack);
            _attackInputField.SetTextWithoutNotify(attack);
        }
        /// <summary>
        /// 设置卡牌的生命值
        /// </summary>
        /// <param name="life"></param>
        public void setLife(string life)
        {

            _card.setLife(life);
            _lifeInputField.SetTextWithoutNotify(life);
        }
        /// <summary>
        /// 设置卡牌可选的标签
        /// </summary>
        /// <param name="tags"></param>
        public void initTags(string[] tags)
        {

            _tagDropdown.ClearOptions();
            _tagDropdown.AddOptions(new List<string>(tags));
        }
        /// <summary>
        /// 设置卡牌的标签
        /// </summary>
        /// <param name="tag"></param>
        public void setRace(int index, string name)
        {
            _card.setTag(name);
            _tagDropdown.SetValueWithoutNotify(index);
        }
        public void setSkillRef(Sprite spr, string name)
        {
            _skillRef.updateView(name, spr);
        }
        public void setEffectTypes(string[] effectTypeNames)
        {
            _effectList.setAddEffectMenu(effectTypeNames);
        }
        public void setEffectScrollValue(float value)
        {
            _effectList.setScrollValue(value);
        }
        public void initValueInputFieldProperties(ValueInputFieldProperties props)
        {
            var tempField = _customPropertyItemTemplate.GetComponent<PropertyItemField>();
            tempField.initInputFieldProperties(props);
            foreach (var element in _customPropertyItemList)
            {
                var itemField = element.GetComponent<PropertyItemField>();
                itemField.initInputFieldProperties(props);
            }
        }
        /// <summary>
        /// 设置效果列表
        /// </summary>
        /// <param name="effectNames"></param>
        public void setEffects(string[] effectNames, int activeIndex = -1)
        {
            _effectList.updateList(effectNames, activeIndex);
        }
        public void setSkillRefVisible(bool visible)
        {
            _skillIdGameObject.SetActive(visible);
        }
        public void updateSoundItems((string name, Sprite sprite)[] viewDatas)
        {
            _sounds.updateList(viewDatas.Length, (i, rect) =>
            {
                var soundItem = rect.GetComponent<LabeledSoundItem>();
                var viewData = viewDatas[i];
                soundItem.updateView(viewData.name, viewData.sprite);
            },
            rect =>
            {
                var soundItem = rect.GetComponent<LabeledSoundItem>();
                soundItem.onSoundItemClick += onSoundButtonClickedCallback;
                soundItem.onSoundItemPlayClick += onSoundPlayButtonClickedCallback;
            },
            rect =>
            {
                var soundItem = rect.GetComponent<LabeledSoundItem>();
                soundItem.onSoundItemClick -= onSoundButtonClickedCallback;
                soundItem.onSoundItemPlayClick -= onSoundPlayButtonClickedCallback;
            });
        }
        public void switchCardFace(CardFaceType type)
        {
            _editSubnameButton.gameObject.SetActive(type != CardFaceType.Spell && type != CardFaceType.Item);
            _tagDropdown.gameObject.SetActive(type == CardFaceType.Servant);
            _editAttackButton.gameObject.SetActive(type != CardFaceType.Spell);
            _editLifeButton.gameObject.SetActive(type != CardFaceType.Spell);

            Vector3 namePosition;
            switch (type)
            {
                case CardFaceType.Spell:
                    namePosition = _card.spellCardFace.nameText.transform.position;
                    break;
                case CardFaceType.Item:
                    namePosition = _card.itemCardFace.nameText.transform.position;
                    break;
                default:
                    namePosition = _card.servantCardFace.nameText.transform.position;
                    break;
            }
            _editNameButton.transform.position = namePosition;
            _nameInputField.transform.position = namePosition;
            _nameInputField.textComponent.alignment = TextAnchor.MiddleCenter;
        }
        public override void reset()
        {
            displayPage(_cardPage, _cardPageIndicator);
            hideCopyHint();
            _skinDropdown.Hide();
            _languageDropdown.Hide();
            _characterDropdown.Hide();
            _typeDropdown.Hide();
            _tagDropdown.Hide();
            _tagDropdown.gameObject.SetActive(true);
            _costInputField.DeactivateInputField();
            _editCostButton.gameObject.SetActive(true);
            _costInputField.gameObject.SetActive(false);
            _attackInputField.DeactivateInputField();
            _editAttackButton.gameObject.SetActive(true);
            _attackInputField.gameObject.SetActive(false);
            _lifeInputField.DeactivateInputField();
            _editLifeButton.gameObject.SetActive(true);
            _lifeInputField.gameObject.SetActive(false);
            _skinNameInputField.DeactivateInputField();
            _nameInputField.DeactivateInputField();
            _editNameButton.gameObject.SetActive(true);
            _nameInputField.gameObject.SetActive(false);
            _subnameInputField.DeactivateInputField();
            _editSubnameButton.gameObject.SetActive(true);
            _subnameInputField.gameObject.SetActive(false);
            _descInputField.DeactivateInputField();
            _editDescButton.gameObject.SetActive(true);
            _descEndEditButton.gameObject.SetActive(false);
            _descInputField.gameObject.SetActive(false);
        }
        public void showEffectListItemInputField(int index, string value)
        {
            _effectList.showItemInputField(index, value);
        }
        public void hideEffectListItemInputField(int index)
        {
            _effectList.hideItemInputField(index);
        }
        public void setCustomPropertyTypes(ActionNodeValueType[] types, string[] names)
        {
            _createCustomPropDialog.setTypes(types, names);
        }
        public void displayCustomPropertyDialog(bool display)
        {
            if (display)
            {
                _createCustomPropDialog.display();
            }
            else
            {
                _createCustomPropDialog.hide();
            }
        }
        public void updateCustomProperties(PropertyItemInfo[] items)
        {
            int count = items.Length;
            BJSYGameCore.UI.UIHelper.updateList(_customPropertiesRoot, _customPropertyItemTemplate, _customPropertyItemList, count, (i, rect) =>
            {
                PropertyItemField item = rect.GetComponent<PropertyItemField>();
                item.updateItem(items[i]);
            },
            rect =>
            {
                PropertyItemField item = rect.GetComponent<PropertyItemField>();
                item.onValueChanged += onCustomPropertyValueChangedCallback;
                item.onClickButton += onClickCustomPropertyButtonCallback;
                item.onClickDelete += onDeleteCustomPropertyCallback;
            },
            rect =>
            {
                PropertyItemField item = rect.GetComponent<PropertyItemField>();
                item.onClickButton -= onClickCustomPropertyButtonCallback;
                item.onValueChanged -= onCustomPropertyValueChangedCallback;
                item.onClickDelete -= onDeleteCustomPropertyCallback;
            });
        }
        public PropertyItemField[] getCustomPropertyItemFields()
        {
            return _customPropertyItemList.Select(i => i.GetComponent<PropertyItemField>()).ToArray();
        }
        public PropertyItemField getCustomPropertyItemField(string name)
        {
            foreach (var rect in _customPropertyItemList)
            {
                if (rect == null)
                    continue;
                var field = rect.GetComponent<PropertyItemField>();
                if (field == null)
                    continue;
                if (field.propertyName == name)
                {
                    return field;
                }
            }
            return null;
        }
        public void showCustomPropertyDialogError(string message)
        {
            _createCustomPropDialog.showErrorTip(message);
        }
        public void showCopyHint()
        {
            _copyHintTextObj.SetActive(false);
            _copyHintTextObj.SetActive(true);
        }
        public void hideCopyHint()
        {
            _copyHintTextObj.SetActive(false);
        }
        #region 私有方法
        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            if (_typeDropdown == null)
                _typeDropdown = transform.Find("DetailPanel/TypeDropdownField/DropdownLight").GetComponent<Dropdown>();
            _typeDropdown.onValueChanged.AddListener(onTypeDropdownValueChanged);
            _skillRef.onClick += onSkillIDInputFieldEndEdit;
            _skillRefEditButton.onClick.AddListener(onSkillRefEditButtonClickedCallback);
            _characterDropdown.onValueChanged.AddListener(onCharacterDropdownValueChanged);
            _isTokenToggle.onValueChanged.AddListener(onIsTokenToggleValueChangedCallback);
            _skinDropdown.gameObject.SetActive(true);
            _skinDropdown.onValueChanged.AddListener(onSkinDropdownValueChanged);
            _changeSkinNameButton.gameObject.SetActive(true);
            _changeSkinNameButton.onClick.AddListener(onChangeSkinNameButtonClicked);
            _deleteSkinButton.gameObject.SetActive(true);
            _deleteSkinButton.onClick.AddListener(onDeleteSkinButtonClicked);
            _skinNameInputField.gameObject.SetActive(false);
            _skinNameInputField.onEndEdit.AddListener(onSkinNameInputFieldEndEdit);
            _languageDropdown.onValueChanged.AddListener(onLanguageDropdownValueChanged);
            _editNameButton.onClick.AddListener(onEditNameButtonClicked);
            _editSubnameButton.onClick.AddListener(onEditSubnameButtonClicked);
            _editDescButton.onClick.AddListener(onEditDescButtonClicked);
            _editImageButton.onClick.AddListener(onEditImageButtonClicked);
            _editCostButton.onClick.AddListener(onEditCostButtonClicked);
            _editAttackButton.onClick.AddListener(onEditAttackButtonClicked);
            _editLifeButton.onClick.AddListener(onEditLifeButtonClicked);
            _saveToImageButton.onClick.AddListener(onSaveToImageButtonClickedCallback);
            _nameInputField.onEndEdit.AddListener(onNameInputFieldEndEdit);
            _subnameInputField.onEndEdit.AddListener(onSubNameInputFieldEndEdit);
            _descEndEditButton.onClick.AddListener(onDescEndEditButtonClicked);
            _descInputField.onEndEdit.AddListener(onDescInputFieldEndEdit);
            _tagDropdown.onValueChanged.AddListener(onTagDropdownValueChangedCallback);
            _tagInputField.onEndEdit.AddListener(onTagInputFieldEndEdit);
            _costInputField.onEndEdit.AddListener(onCostInputFieldEndEdit);
            _attackInputField.onEndEdit.AddListener(onAttackInputFieldEndEdit);
            _lifeInputField.onEndEdit.AddListener(onLifeInputFieldEndEdit);
            _effectList.onClickAddEffect += onAddEffectButtonClicked;
            _effectList.onClickItemEditButton += onClickEffectListItemEditButton;
            _effectList.onClickItemRemoveButton += onClickEffectListItemRemoveButton;
            _effectList.onItemInputFieldEndEdit += onEffectListItemInputFieldEndEditCallback;
            _copyCardButton.onClick.AddListener(onClickCopyCardButtonCallback);


            // Pages.
            _cardPageButton.onClick.AddListener(onClickCardPageButtonCallback);
            _propertyPageButton.onClick.AddListener(onClickPropertyPageButtonCallback);

            // Custom Properties.
            _addCustomPropertyButton.onClick.AddListener(() => displayCustomPropertyDialog(true));
            _createCustomPropDialog.onConfirm += item => onConfirmCreateCustomProperty?.Invoke(item);

            // Keywords.
            _keywordsFoldout.onClickPropButton += onClickKeywordButtonCallback;
            _keywordsFoldout.onPropValueChanged += onKeywordValueChangedCallback;
        }
        #endregion
        #region 事件回调
        private void onTypeDropdownValueChanged(int value)
        {
            onChangeType?.Invoke(value);

        }
        private void onIsTokenToggleValueChangedCallback(bool value)
        {
            onIsTokenToggleValueChanged?.Invoke(value);
        }
        private void onCharacterDropdownValueChanged(int value)
        {
            onChangeCharacter?.Invoke(value);
        }
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
        private void onLanguageDropdownValueChanged(int value)
        {
            onChangeLanguage?.Invoke(value);
        }
        private void onEditNameButtonClicked()
        {
            _editNameButton.gameObject.SetActive(false);
            _nameInputField.gameObject.SetActive(true);
            _nameInputField.SetTextWithoutNotify(_card.getName());
            _nameInputField.Select();
            _card.spellCardFace.nameText.gameObject.SetActive(false);

            // Hide background text
            _card.itemCardFace.nameText.gameObject.SetActive(false);
            _card.servantCardFace.nameText.gameObject.SetActive(false);
            _card.servantCardFace.nameText.gameObject.SetActive(false);
        }
        private void onEditSubnameButtonClicked()
        {
            _editSubnameButton.gameObject.SetActive(false);
            _subnameInputField.gameObject.SetActive(true);
            _subnameInputField.SetTextWithoutNotify(_card.getSubname());
            _subnameInputField.Select();

            // Hide background text
            _card.servantCardFace.subNameText.gameObject.SetActive(false);
        }
        private void onEditDescButtonClicked()
        {
            _editDescButton.gameObject.SetActive(false);
            _descEndEditButton.gameObject.SetActive(true);
            _descInputField.gameObject.SetActive(true);
            _descInputField.SetTextWithoutNotify(_card.getDesc());
            _descInputField.Select();

            // Hide background text
            _card.itemCardFace.descText.gameObject.SetActive(false);
            _card.servantCardFace.descText.gameObject.SetActive(false);
            _card.spellCardFace.descText.gameObject.SetActive(false);
        }
        private async void onEditImageButtonClicked()
        {
            _editImageButton.gameObject.SetActive(false);
            try
            {
                Task<Sprite> task = onNeedSelectImage?.Invoke();
                Sprite sprite = task != null ? await task : null;
                if (sprite != null)
                {
                    _card.setImage(sprite);
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"设置卡牌图像失败：{e}", e);
            }
            finally
            {
                _editImageButton.gameObject.SetActive(true);
            }
        }
        private void onEditCostButtonClicked()
        {
            _editCostButton.gameObject.SetActive(false);
            _costInputField.gameObject.SetActive(true);
            _costInputField.Select();

            // Hide background text
            _card.itemCardFace.costPropNumber.text.gameObject.SetActive(false);
            _card.servantCardFace.costPropNumber.text.gameObject.SetActive(false);
            _card.spellCardFace.costPropNumber.text.gameObject.SetActive(false);
        }
        private void onEditAttackButtonClicked()
        {
            _editAttackButton.gameObject.SetActive(false);
            _attackInputField.gameObject.SetActive(true);
            _attackInputField.Select();

            // Hide background text
            _card.itemCardFace.attackPropNumber.text.gameObject.SetActive(false);
            _card.servantCardFace.attackPropNumber.text.gameObject.SetActive(false);
        }
        private void onEditLifeButtonClicked()
        {
            _editLifeButton.gameObject.SetActive(false);
            _lifeInputField.gameObject.SetActive(true);
            _lifeInputField.Select();

            // Hide background text
            _card.itemCardFace.lifePropNumber.text.gameObject.SetActive(false);
            _card.servantCardFace.lifePropNumber.text.gameObject.SetActive(false);
        }
        private void onNameInputFieldEndEdit(string text)
        {
            _nameInputField.gameObject.SetActive(false);
            _editNameButton.gameObject.SetActive(true);
            onChangeName?.Invoke(text);

            // Show background text
            _card.itemCardFace.nameText.gameObject.SetActive(true);
            _card.servantCardFace.nameText.gameObject.SetActive(true);
            _card.spellCardFace.nameText.gameObject.SetActive(true);
        }
        private void onSubNameInputFieldEndEdit(string text)
        {
            _subnameInputField.gameObject.SetActive(false);
            _editSubnameButton.gameObject.SetActive(true);
            onChangeSubName?.Invoke(text);

            // Show background text
            _card.servantCardFace.subNameText.gameObject.SetActive(true);
        }
        private void onDescEndEditButtonClicked()
        {
            _descInputField.DeactivateInputField();
        }
        private void onDescInputFieldEndEdit(string text)
        {
            _editDescButton.gameObject.SetActive(true);
            _descEndEditButton.gameObject.SetActive(false);
            _descInputField.gameObject.SetActive(false);
            onChangeDesc?.Invoke(text);

            // Show background text
            _card.itemCardFace.descText.gameObject.SetActive(true);
            _card.servantCardFace.descText.gameObject.SetActive(true);
            _card.spellCardFace.descText.gameObject.SetActive(true);
        }
        private void onTagDropdownValueChangedCallback(int value)
        {
            onChangeRace?.Invoke(value);
        }
        private void onCostInputFieldEndEdit(string text)
        {
            _costInputField.gameObject.SetActive(false);
            _editCostButton.gameObject.SetActive(true);
            onChangeCost?.Invoke(text);

            // Show background text
            _card.itemCardFace.costPropNumber.text.gameObject.SetActive(true);
            _card.servantCardFace.costPropNumber.text.gameObject.SetActive(true);
            _card.spellCardFace.costPropNumber.text.gameObject.SetActive(true);
        }
        private void onAttackInputFieldEndEdit(string text)
        {
            _attackInputField.gameObject.SetActive(false);
            _editAttackButton.gameObject.SetActive(true);
            onChangeAttack?.Invoke(text);

            // Show background text
            _card.itemCardFace.attackPropNumber.text.gameObject.SetActive(true);
            _card.servantCardFace.attackPropNumber.text.gameObject.SetActive(true);
        }
        private void onLifeInputFieldEndEdit(string text)
        {
            _lifeInputField.gameObject.SetActive(false);
            _editLifeButton.gameObject.SetActive(true);
            onChangeLife?.Invoke(text);

            // Show background text
            _card.itemCardFace.lifePropNumber.text.gameObject.SetActive(true);
            _card.servantCardFace.lifePropNumber.text.gameObject.SetActive(true);
        }
        private void onClickEffectListItemEditButton(int index)
        {
            onClickEditEffectButton?.Invoke(index);
        }
        private void onClickEffectListItemRemoveButton(int index)
        {
            onClickRemoveEffectButton?.Invoke(index);
        }
        private void onEffectListItemInputFieldEndEditCallback(int index, string value)
        {
            onEffectListItemInputFieldEndEdit?.Invoke(index, value);
        }
        private void onAddEffectButtonClicked(int index)
        {
            onClickAddEffectMenu?.Invoke(index);
        }
        private void onClickCopyCardButtonCallback()
        {
            onClickCopyCardButton?.Invoke();
        }
        private void onClickCardPageButtonCallback()
        {
            displayPage(_cardPage, _cardPageIndicator);
        }
        private void onClickPropertyPageButtonCallback()
        {
            displayPage(_propertyPage, _propertyPageIndicator);
        }
        private void onTagInputFieldEndEdit(string text)
        {
            onChangeTag?.Invoke(text);

        }
        private void onSkillIDInputFieldEndEdit(CardViewDeckCardListItem item, PointerEventData data)
        {
            onClickSkillRefButton?.Invoke();
        }
        private void onSkillRefEditButtonClickedCallback()
        {
            onSkillRefEditButtonClicked?.Invoke();
        }
        private void onSaveToImageButtonClickedCallback()
        {
            onSaveCardFaceToImage?.Invoke();
        }
        private async void onSoundButtonClickedCallback(LabeledSoundItem soundItem)
        {
            int index = _sounds.indexOf(soundItem);
            soundItem.soundItem.interactable = false;
            try
            {
                Task<AudioClip> task = onNeedSelectSound?.Invoke(index);
                AudioClip audio = task != null ? await task : null;
                if (audio != null)
                {
                    // TODO: 设置音频图片
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"设置卡牌图像失败：{e}", e);
            }
            finally
            {
                soundItem.soundItem.interactable = true;
            }
        }
        private void onSoundPlayButtonClickedCallback(LabeledSoundItem soundItem)
        {
            onSoundPlayButtonClicked?.Invoke(_sounds.indexOf(soundItem));
        }

        #region 关键词
        private void onClickKeywordButtonCallback(string name, int[] itemIndexes, ValueInputFieldButton button)
        {
            onClickPropertyButton?.Invoke(false, name, itemIndexes, button);
        }
        private void onKeywordValueChangedCallback(string name, object value)
        {
            onPropertyValueChanged?.Invoke(false, name, value);
        }
        #endregion

        #region 自定义属性
        private void onClickCustomPropertyButtonCallback(string name, int[] itemIndexes, ValueInputFieldButton button)
        {
            onClickPropertyButton?.Invoke(true, name, itemIndexes, button);
        }
        private void onCustomPropertyValueChangedCallback(string name, object value)
        {
            onPropertyValueChanged?.Invoke(true, name, value);
        }
        private void onDeleteCustomPropertyCallback(string name)
        {
            onDeleteCustomProperty?.Invoke(name);
        }
        #endregion

        #endregion

        protected override IEnumerable<GameObject> getPages()
        {
            return base.getPages().Append(_cardPage).Append(_propertyPage);
        }
        protected override IEnumerable<GameObject> getPageIndicators()
        {
            return base.getPageIndicators().Append(_cardPageIndicator).Append(_propertyPageIndicator);
        }
        #endregion
        #region 事件
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
        /// 当更换了语言，参数为更换的语言索引。
        /// </summary>
        public event Action<int> onChangeLanguage;
        /// <summary>
        /// 当更换卡牌类型，参数为更改类型索引。
        /// </summary>
        public event Action<int> onChangeType;
        public event Action<bool> onIsTokenToggleValueChanged;
        /// <summary>
        /// 当更换所属角色，参数为更换角色索引。
        /// </summary>
        public event Action<int> onChangeCharacter;
        /// <summary>
        /// 当卡牌费用值被更改，参数为用户输入费用。
        /// </summary>
        public event Action<string> onChangeCost;
        /// <summary>
        /// 当卡牌名称被更改，参数为更改后的名称。
        /// </summary>
        public event Action<string> onChangeName;
        /// <summary>
        /// 当卡牌副名被更改，参数为更改后的卡牌副名。
        /// </summary>
        public event Action<string> onChangeSubName;
        /// <summary>
        /// 当需要选择卡牌图片，需要返回一个被选择图片（可以为空）
        /// </summary>
        public event Func<Task<Sprite>> onNeedSelectImage;
        /// <summary>
        /// 当更改卡牌描述，参数为更改后的卡牌描述。
        /// </summary>
        public event Action<string> onChangeDesc;
        /// <summary>
        /// 当更改卡牌攻击力，参数为更改输入的攻击力。
        /// </summary>
        public event Action<string> onChangeAttack;
        /// <summary>
        /// 当更改卡牌生命值，参数为更改输入的生命值。
        /// </summary>
        public event Action<string> onChangeLife;
        /// <summary>
        /// 当更改卡牌标签，参数为更改后的卡牌标签。
        /// </summary>
        public event Action<string> onChangeTag;
        /// <summary>
        /// 当将卡图保存为图片
        /// </summary>
        public event Action onSaveCardFaceToImage;
        public event Action<int> onChangeRace;
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
        /// 当点击复制按钮
        /// </summary>
        public event Action onClickCopyCardButton;
        /// <summary>
        /// 当点击选择技能按钮
        /// </summary>
        public event Action onClickSkillRefButton;
        /// <summary>
        /// 当点击编辑技能按钮
        /// </summary>
        public event Action onSkillRefEditButtonClicked;
        /// <summary>
        /// 当点击选择卡牌音效
        /// </summary>
        public event Func<int, Task<AudioClip>> onNeedSelectSound;
        /// <summary>
        /// 当点击播放卡牌音效
        /// </summary>
        public event Action<int> onSoundPlayButtonClicked;

        #region Properties
        /// <summary>
        /// 当确认创建自定义属性。
        /// </summary>
        public event Action<PropertyItemInfo> onConfirmCreateCustomProperty;
        public event Action<bool, string, object> onPropertyValueChanged;
        public event Action<bool, string, int[], ValueInputFieldButton> onClickPropertyButton;
        public event Action<string> onDeleteCustomProperty;
        #endregion

        #endregion
        public Card card => _card;
        public CardPropertiesFoldout keywordsFoldout => _keywordsFoldout;
        [Header("编辑内容")]
        [SerializeField]
        Card _card;
        [SerializeField]
        Text _idText;
        [SerializeField]
        Dropdown _typeDropdown;
        [SerializeField]
        Toggle _isTokenToggle;
        [SerializeField]
        GameObject _skillIdGameObject;
        [SerializeField]
        CardViewDeckCardListItem _skillRef;
        [SerializeField]
        Button _skillRefEditButton;
        [SerializeField]
        CardPropertiesFoldout _keywordsFoldout;
        [SerializeField]
        Dropdown _characterDropdown;
        [SerializeField]
        Dropdown _skinDropdown;
        [SerializeField]
        Button _changeSkinNameButton;
        [SerializeField]
        Button _deleteSkinButton;
        [SerializeField]
        InputField _skinNameInputField;
        [SerializeField]
        Dropdown _languageDropdown;
        [SerializeField]
        ElementList _sounds;
        [SerializeField]
        EffectList _effectList;


        [Header("Edit Buttons")]
        [SerializeField]
        Button _editNameButton;
        [SerializeField]
        Button _editSubnameButton;
        [SerializeField]
        Button _editDescButton;
        [SerializeField]
        Button _editCostButton;
        [SerializeField]
        Button _editAttackButton;
        [SerializeField]
        Button _editLifeButton;
        [SerializeField]
        Button _editImageButton;
        [SerializeField]
        Button _saveToImageButton;

        [Header("Fields")]
        [SerializeField]
        InputField _nameInputField;
        [SerializeField]
        InputField _subnameInputField;
        [SerializeField]
        Button _descEndEditButton;
        [SerializeField]
        InputField _descInputField;
        [SerializeField]
        Dropdown _tagDropdown;
        [SerializeField]
        InputField _costInputField;
        [SerializeField]
        InputField _attackInputField;
        [SerializeField]
        InputField _lifeInputField;
        [SerializeField]
        Button _copyCardButton;
        [SerializeField]
        InputField _tagInputField;

        [Header("Pages")]
        [SerializeField]
        Button _cardPageButton;
        [SerializeField]
        Button _propertyPageButton;
        [SerializeField]
        GameObject _cardPage;
        [SerializeField]
        GameObject _propertyPage;
        [SerializeField]
        GameObject _cardPageIndicator;
        [SerializeField]
        GameObject _propertyPageIndicator;

        [Header("Custom Properties")]
        [SerializeField]
        Button _addCustomPropertyButton;
        [SerializeField]
        CreatePropertyItemDialog _createCustomPropDialog;
        [SerializeField]
        RectTransform _customPropertiesRoot;
        [SerializeField]
        RectTransform _customPropertyItemTemplate;
        private List<RectTransform> _customPropertyItemList = new List<RectTransform>();

        [Header("Misc")]
        public GameObject _copyHintTextObj;

        public Button EditNameButton
        {
            get { return _editNameButton; }
            set { _editNameButton = value; }
        }
        public InputField NameInputField
        {
            get { return _nameInputField; }
            set { _nameInputField = value; }
        }
    }
    public enum CardFaceType
    {
        Servant,
        Spell,
        Item
    }
}