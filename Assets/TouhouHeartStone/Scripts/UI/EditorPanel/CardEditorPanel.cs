using BJSYGameCore.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game;
using System.Threading.Tasks;
using TouhouHeartstone;
using TouhouCardEngine;
using Controllers;

namespace UI
{
    public class CardEditorPanel : MonoBehaviour
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
            _skillIdGameObject.SetActive(typeName == DEFINE.MASTER);
        }
        public void setIsToken(bool value)
        {
            _isTokenToggle.SetIsOnWithoutNotify(value);
        }
        public void setSpellDamage(int value)
        {
            _spellDamageInputField.SetTextWithoutNotify(value.ToString());
        }
        /// <summary>
        /// 初始化可选的卡牌关键词
        /// </summary>
        /// <param name="keywords"></param>
        public void initKeywords(string[] keywords)
        {
            _keywordsDropdown.ClearOptions();
            _keywordsDropdown.AddOptions(new List<string>(keywords));
        }
        /// <summary>
        /// 设置卡牌关键词的选择状态
        /// </summary>
        /// <param name="value"></param>
        public void setKeywords(bool[] value)
        {
            _keywordsDropdown.SetValueWithoutNotify(value);
        }
        public void initTags(string[] tagNames)
        {
            //_noRaceTagDropdown.ClearOptions();
            //_noRaceTagDropdown.AddOptions(new List<string>(tagNames));
        }
        public void setInitTags(string[] tags)
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
        public void setTags(string[] tags)
        {

            _tagDropdown.ClearOptions();
            _tagDropdown.AddOptions(new List<string>(tags));
        }
        /// <summary>
        /// 设置卡牌的标签
        /// </summary>
        /// <param name="tag"></param>
        public void setRace(int index)
        {
            _tagDropdown.SetValueWithoutNotify(index);
        }
        public void setSkillRef(CardDefine cardDefine, Sprite spr, string name)
        {
            _skillRef.update(cardDefine, spr, name, 0);
        }
        public void setEffectTypes(string[] effectTypeNames)
        {
            _effectList.setAddEffectMenu(effectTypeNames);
        }
        /// <summary>
        /// 设置效果列表
        /// </summary>
        /// <param name="effectNames"></param>
        public void setEffects(string[] effectNames)
        {
            _effectList.updateList(effectNames);
        }
        public void switchToServant()
        {
            _editSubnameButton.gameObject.SetActive(true);
            _editTagButton.gameObject.SetActive(true);
            _editAttackButton.gameObject.SetActive(true);
            _editLifeButton.gameObject.SetActive(true);
            _skillIdGameObject.SetActive(false);
            _nameInputField.transform.position = _card.servantCardFace.nameText.transform.position;
        }
        public void switchToSpell()
        {
            _editSubnameButton.gameObject.SetActive(false);
            _editTagButton.gameObject.SetActive(false);
            _editAttackButton.gameObject.SetActive(false);
            _editLifeButton.gameObject.SetActive(false);
            _skillIdGameObject.SetActive(false);
            _nameInputField.transform.position = _card.spellCardFace.nameText.transform.position;
        }
        public void switchToItem()
        {
            _editSubnameButton.gameObject.SetActive(false);
            _editTagButton.gameObject.SetActive(false);
            _editAttackButton.gameObject.SetActive(true);
            _editLifeButton.gameObject.SetActive(true);
            _skillIdGameObject.SetActive(false);
            _nameInputField.transform.position = _card.itemCardFace.nameText.transform.position;
        }
        public void switchToMaster()
        {
            _skillIdGameObject.SetActive(true);
        }
        public void closeAllEdit()
        {
            _skinDropdown.Hide();
            _languageDropdown.Hide();
            _characterDropdown.Hide();
            _typeDropdown.Hide();
            _tagDropdown.Hide();
            _editTagButton.gameObject.SetActive(true);
            _tagDropdown.gameObject.SetActive(false);
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

        public void displayCardSelectPanel(bool display)
        {
            _cardPoolViewPanelObj.SetActive(display);
        }

        public void setCustomPropertyTypes((ActionNodeValueType type, string name)[] types)
        {
            _createCustomPropDialog.setTypes(types);
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
        public void updateCustomProperties(CardPropertyItem[] items, CardManager cardManager)
        {
            int count = items.Length;
            BJSYGameCore.UI.UIHelper.updateList(_customPropertiesRoot, _customPropertyItemTemplate, _customPropertyItemList, count, async (i, rect) =>
            {
                CardPropertyListItem item = rect.GetComponent<CardPropertyListItem>();
                await item.updateItem(items[i], cardManager);
            },
            rect =>
            {
                CardPropertyListItem item = rect.GetComponent<CardPropertyListItem>();
                item.setPiles(CardEditorCtrl.pileNames, CardEditorCtrl.piles);
                item.setKeywords(CardEditorCtrl.keywordNames, CardEditorCtrl.keywords);
                item.onClickItemButton += onClickCustomPropertyItemButtonCallback;
                item.onItemValueChanged += onCustomPropertyItemValueChangedCallback;
                item.onClickButton += onClickCustomPropertyButtonCallback;
                item.onValueChanged += onCustomPropertyValueChangedCallback;
                item.onClickDelete += onDeleteCustomPropertyCallback;
                item.onClickAddItemButton += onClickCustomPropertyAddItemButtonCallback;
                item.onClickRemoveItemButton += onClickCustomPropertyRemoveItemButtonCallback;
            },
            rect =>
            {
                CardPropertyListItem item = rect.GetComponent<CardPropertyListItem>();
                item.onClickItemButton -= onClickCustomPropertyItemButtonCallback;
                item.onItemValueChanged -= onCustomPropertyItemValueChangedCallback;
                item.onClickButton -= onClickCustomPropertyButtonCallback;
                item.onValueChanged -= onCustomPropertyValueChangedCallback;
                item.onClickDelete -= onDeleteCustomPropertyCallback;
                item.onClickAddItemButton -= onClickCustomPropertyAddItemButtonCallback;
                item.onClickRemoveItemButton -= onClickCustomPropertyRemoveItemButtonCallback;
            });
        }

        public async void updateCustomPropertyItem(string name, CardPropertyItem item, CardManager cardManager)
        {
            CardPropertyListItem listItem = null;
            foreach (var rect in _customPropertyItemList)
            {
                if (rect == null)
                    continue;
                var i = rect.GetComponent<CardPropertyListItem>();
                if (i == null)
                    continue;
                if (i.propertyName == name)
                {
                    listItem = i;
                    break;
                }
            }
            if (listItem == null)
                return;
            await listItem.updateItem(item, cardManager);
        }
        public void showCustomPropertyDialogError(string message)
        {
            _createCustomPropDialog.showErrorTip(message);
        }
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            if (_typeDropdown == null)
                _typeDropdown = transform.Find("DetailPanel/TypeDropdownField/DropdownLight").GetComponent<Dropdown>();
            _typeDropdown.onValueChanged.AddListener(onTypeDropdownValueChanged);
            _skillRef.onClick += onSkillIDInputFieldEndEdit;
            _characterDropdown.onValueChanged.AddListener(onCharacterDropdownValueChanged);
            _isTokenToggle.onValueChanged.AddListener(onIsTokenToggleValueChangedCallback);
            _keywordsDropdown.onValueChanged.AddListener(onKeywordsDropdownValueChanged);
            //_noRaceTagDropdown.onValueChanged.AddListener(onNoRaceTagDropdownValueChangedCallback);
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
            _editTagButton.onClick.AddListener(onEditTagButtonClicked);
            _editCostButton.onClick.AddListener(onEditCostButtonClicked);
            _editAttackButton.onClick.AddListener(onEditAttackButtonClicked);
            _editLifeButton.onClick.AddListener(onEditLifeButtonClicked);
            _nameInputField.onEndEdit.AddListener(onNameInputFieldEndEdit);
            _subnameInputField.onEndEdit.AddListener(onSubNameInputFieldEndEdit);
            _descEndEditButton.onClick.AddListener(onDescEndEditButtonClicked);
            _descInputField.onEndEdit.AddListener(onDescInputFieldEndEdit);
            _tagDropdown.onValueChanged.AddListener(onTagDropdownValueChangedCallback);
            _tagInputField.onEndEdit.AddListener(onTagInputFieldEndEdit);
            _spellDamageInputField.onEndEdit.AddListener(onSpellDamageInputFieldEndEdit);
            EventTrigger trigger = _tagDropdown.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Deselect;
            entry.callback.AddListener(e =>
            {
                if (_tagDropdown.template.gameObject.activeSelf)
                {
                    return;
                }
                onTagDropdownValueChangedCallback(_tagDropdown.value);
            });
            trigger.triggers.Add(entry);
            _costInputField.onEndEdit.AddListener(onCostInputFieldEndEdit);
            _attackInputField.onEndEdit.AddListener(onAttackInputFieldEndEdit);
            _lifeInputField.onEndEdit.AddListener(onLifeInputFieldEndEdit);
            _effectList.onClickAddEffect += onAddEffectButtonClicked;
            _effectList.onClickItemEditButton += onClickEffectListItemEditButton;
            _effectList.onClickItemRemoveButton += onClickEffectListItemRemoveButton;
            _deleteCardButton.onClick.AddListener(onClickDeleteCardButtonCallback);
            _saveCardButton.onClick.AddListener(onClickSaveCardButtonCallback);
            _closeCardButton.onClick.AddListener(onClickCloseCardButtonCallback);


            // Pages.
            _basicPageButton.onClick.AddListener(() => displayPage(_basicPage));
            _propertyPageButton.onClick.AddListener(() => displayPage(_propertyPage));
            _skinPageButton.onClick.AddListener(() => displayPage(_skinPage));

            // Custom Properties.
            _addCustomPropertyButton.onClick.AddListener(() => displayCustomPropertyDialog(true));
            _createCustomPropDialog.onConfirm += item => onConfirmCreateCustomProperty?.Invoke(item);
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
        private void onKeywordsDropdownValueChanged(bool[] value)
        {
            onChangeKeywords?.Invoke(value);
        }
        private void onNoRaceTagDropdownValueChangedCallback(bool[] value)
        {
            onTagDropdownValueChanged?.Invoke(value);
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
                throw new InvalidOperationException("设置卡牌图像失败：" + e, e);
            }
            finally
            {
                _editImageButton.gameObject.SetActive(true);
            }
        }
        private void onEditTagButtonClicked()
        {
            _editTagButton.gameObject.SetActive(false);
            _tagDropdown.gameObject.SetActive(true);
            _tagDropdown.Show();
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
            _tagDropdown.gameObject.SetActive(false);
            _editTagButton.gameObject.SetActive(true);
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
        private void onAddEffectButtonClicked(int index)
        {
            onClickAddEffectMenu?.Invoke(index);
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
        private void onTagInputFieldEndEdit(string text)
        {
            onChangeTag?.Invoke(text);

        }
        private void onSpellDamageInputFieldEndEdit(string text)
        {
            if (int.TryParse(text, out int value))
            {
                onChangeSpellDamage?.Invoke(value);
            }

        }
        private void onSkillIDInputFieldEndEdit()
        {
            onClickSkillRefButton?.Invoke();
        }
        private void onClickCustomPropertyItemButtonCallback(string name, int index)
        {
            onClickCustomPropertyItemButton?.Invoke(name, index);
        }
        private void onCustomPropertyItemValueChangedCallback(string name, int index, object value)
        {
            onCustomPropertyItemValueChanged?.Invoke(name, index, value);
        }
        private void onClickCustomPropertyAddItemButtonCallback(string name)
        {
            onClickCustomPropertyAddItemButton?.Invoke(name);
        }
        private void onClickCustomPropertyRemoveItemButtonCallback(string name, int index)
        {
            onClickCustomPropertyRemoveItemButton?.Invoke(name, index);
        }
        private void onClickCustomPropertyButtonCallback(string name)
        {
            onClickCustomPropertyButton?.Invoke(name);
        }
        private void onCustomPropertyValueChangedCallback(string name, object value)
        {
            onCustomPropertyValueChanged?.Invoke(name, value);
        }
        private void onDeleteCustomPropertyCallback(string name)
        {
            onDeleteCustomProperty?.Invoke(name);
        }
        #endregion

        private void displayPage(GameObject toDisplay)
        {
            _basicPage.SetActive(false);
            _propertyPage.SetActive(false);
            _skinPage.SetActive(false);

            toDisplay.SetActive(true);
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
        /// 当卡牌的关键词发生改变，参数为更改后的关键词选择状态。
        /// </summary>
        public event Action<bool[]> onChangeKeywords;
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
        /// 当更改法术伤害时，参数为更改后的法术伤害。
        /// </summary>
        public event Action<int> onChangeSpellDamage;
        public event Action<int> onChangeRace;
        public event Action<bool[]> onTagDropdownValueChanged;
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
        /// <summary>
        /// 当点击选择技能按钮
        /// </summary>
        public event Action onClickSkillRefButton;


        /// <summary>
        /// 当确认创建自定义属性。
        /// </summary>
        public event Action<CardPropertyItem> onConfirmCreateCustomProperty;

        public event Action<string, int> onClickCustomPropertyItemButton;
        public event Action<string, int, object> onCustomPropertyItemValueChanged;
        public event Action<string> onClickCustomPropertyButton;
        public event Action<string, object> onCustomPropertyValueChanged;
        public event Action<string> onDeleteCustomProperty;
        public event Action<string> onClickCustomPropertyAddItemButton;
        public event Action<string, int> onClickCustomPropertyRemoveItemButton;
        #endregion
        public Card card => _card;
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
        BuildDeckListItem _skillRef;
        [SerializeField]
        MultiSelectDropdown _keywordsDropdown;
        [SerializeField]
        InputField _spellDamageInputField;
        [SerializeField]
        MultiSelectDropdown _noRaceTagDropdown;
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
        EffectList _effectList;
        [SerializeField]
        GameObject _cardPoolViewPanelObj;

        [Header("Edit Buttons")]
        [SerializeField]
        Button _editNameButton;
        [SerializeField]
        Button _editSubnameButton;
        [SerializeField]
        Button _editDescButton;
        [SerializeField]
        Button _editTagButton;
        [SerializeField]
        Button _editCostButton;
        [SerializeField]
        Button _editAttackButton;
        [SerializeField]
        Button _editLifeButton;
        [SerializeField]
        Button _editImageButton;

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
        Button _deleteCardButton;
        [SerializeField]
        Button _saveCardButton;
        [SerializeField]
        Button _closeCardButton;
        [SerializeField]
        InputField _tagInputField;

        [Header("Pages")]
        [SerializeField]
        Button _basicPageButton;
        [SerializeField]
        Button _propertyPageButton;
        [SerializeField]
        Button _skinPageButton;
        [SerializeField]
        GameObject _basicPage;
        [SerializeField]
        GameObject _propertyPage;
        [SerializeField]
        GameObject _skinPage;

        [Header("Custom Properties")]
        [SerializeField]
        Button _addCustomPropertyButton;
        [SerializeField]
        CreateCustomPropertyDialog _createCustomPropDialog;
        [SerializeField]
        RectTransform _customPropertiesRoot;
        [SerializeField]
        RectTransform _customPropertyItemTemplate;
        private List<RectTransform> _customPropertyItemList = new List<RectTransform>();

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
}