using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Game;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Controllers
{
    class CardEditorCtrl : CardPoolEditor
    {
        #region 公有方法
        /// <summary>
        /// 打开一张卡片进行编辑
        /// </summary>
        public async Task display(CardDefine cardDefine, CardSkinData[] skins)
        {
            // 初始化属性。
            _curCardNameLang = LangManager.SOURCE;
            _curCardSubnameLang = LangManager.SOURCE;
            _curCardDescLang = LangManager.SOURCE;

            _selectableCharacters = await getSelectableCharacters();
            _cardDefine = cardDefine;
            _cardSkins.Clear();
            _cardSkins.AddRange(skins);

            gameObject.SetActive(true); // 触发Awake。
            scene.waitingBlocker.addWaitingTask(WaitingTask.CardEditor);
            _ui.reset();

            // 初始化UI。
            // 卡牌类型。
            string[] typeNames = game.editor.getCardTypeNames();
            _cardTypes = game.editor.getCardTypes();
            _ui.initTypes(typeNames);
            // 语言。
            var lanaguages = game.lang.getLanguagesWithSource();
            var languageCodes = lanaguages.Select(l => game.lang.getLanguageShortCode(l)).ToArray();
            _ui.initLanguages(languageCodes);
            // 标签
            _ui.initTags(_raceNames.Select(n => game.lang._(n)).ToArray());
            // 效果类型
            _ui.setEffectTypes(game.card.getEffectNames());
            // 自定义属性。
            var customPropTypes = EditorManager.getCustomPropTypes();
            var customPropTypeNames = game.editor.getCustomPropNames();
            _ui.setCustomPropertyTypes(customPropTypes, customPropTypeNames);
            // 输入控件。
            var keywords = EditorManager.editorKeywords;
            var keywordNames = keywords.Select(k => game.card.getKeywordName(k)).ToArray();
            var fieldProps = new ValueInputFieldProperties()
            {
                backText = game.lang._(StringTable.RETURN),

                keywordNames = keywordNames,
                keywordValues = keywords.ToArray(),

                anyPileName = game.editor.getAnyPileName(),
                pileNames = game.editor.getAllPileNames(),
                pileValues = game.editor.getAllPileValues(),

                buffValues = game.editor.getBuffReferenceRecursions(),
            };
            _ui.initValueInputFieldProperties(fieldProps);



            // 基础页面
            _ui.setId(cardDefine.id);
            _ui.setType(Array.IndexOf(_cardTypes, cardDefine.type), cardDefine.type);
            _ui.setIsToken(cardDefine.isToken());
            _ui.setTags(cardDefine.getTags());
            updateSkinNames();
            // 人物，中立为0，有角色则为index+1
            string[] charactersName = _selectableCharacters
                .Select(c => 
                {
                    var poolRef = getCardPoolReference(c.cardPoolId);
                    var skin = game.card.getCardSkin(poolRef, c.id);
                    return game.card.getCardName(poolRef, skin);
                })
                .Prepend(game.lang._(StringTable.CARDVIEW_CHARACTER_NEUTRAL))
                .ToArray();
            int characterIndex = Array.FindIndex(_selectableCharacters, c =>
            {
                var charRef = cardDefine.getCharacterRef();
                return c.cardPoolId == charRef.cardPoolId && c.id == charRef.defineId;
            }) + 1;
            _ui.setSelectableCharacters(charactersName);
            _ui.setCharacter(characterIndex);
            // 卡牌面板
            _ui.setCost(cardDefine.getCost().ToString());
            _ui.setAttack(cardDefine.getAttack().ToString());
            _ui.setLife(cardDefine.getLife().ToString());
            // 标签
            string[] tags = cardDefine.getTags();
            var tagIndex = 0;
            var tagName = string.Empty;
            if (tags != null && tags.Length > 0)
            {
                tagIndex = Array.IndexOf(_races, tags[0]);
                tagName = tags[0];
            }
            if (tagIndex < 0)
                tagIndex = 0;
            _ui.setRace(tagIndex, tagName);

            await resetSkin();
            await updateCardType();
            await updateCustomProperties();
            await updateKeywords();
            await updateSoundItems();

            // 效果。
            GeneratedEffect[] effects = cardDefine.getEffects().OfType<GeneratedEffect>().ToArray();
            _effectEditorCtrl.hideActionGraph();
            if (effects.Length > 0)
            {
                await _effectEditorCtrl.display(effects[0]);
            }
            _effectEditorCtrl.updateEventTriggerTimes();
            updateEffectList(effects);

            _graphEditor.display();

            scene.waitingBlocker.removeWaitingTask(WaitingTask.CardEditor);
        }
        public override async Task display(CardPoolDisplayPage info)
        {
            if (info == null)
                return;
            displayInfo = info;
            if (info?.displayObj?.cardDefine is CardDefine cardDefine)
            {
                var skins = info.skins as CardSkinData[] ?? Array.Empty<CardSkinData>();
                await display(cardDefine, skins);
                if (info.unsaved)
                    markUnsaved();
                else
                    clearUnsaved();
            }
        }
        public override void hide()
        {
            base.hide();
            AnimationManager.cleanUnusedVisualEffects(_curCardSkin, _cardDefine.getGeneratedEffects());
        }
        public override void markUnsaved()
        {
            base.markUnsaved();
            _ui.setTitleText(scene.editor.getTitleText(hasUnsavedChanges, game.lang._(CARD_EDITOR_TITLE)));
        }
        public override void clearUnsaved()
        {
            base.clearUnsaved();
            _ui.setTitleText(scene.editor.getTitleText(hasUnsavedChanges, game.lang._(CARD_EDITOR_TITLE)));
        }
        #endregion

        #region 私有方法

        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            if (_ui == null)
            {
                _ui = GetComponentInChildren<CardEditorPanel>();
            }

            // 事件回调
            // 基础页面
            _ui.onChangeSkin += onChangeSkin;
            _ui.onChangeSkinName += onChangeSkinName;
            _ui.onClickDeleteSkinButton += onDeleteSkin;
            _ui.onChangeType += onChangeType;
            _ui.onIsTokenToggleValueChanged += onIsTokenToggleValueChanged;
            _ui.onChangeCharacter += onChangeCharacter;
            _ui.onSaveCardFaceToImage += onSaveCardFaceToImage;

            // 卡面。
            _ui.onChangeCost += onChangeCost;
            _ui.onChangeName += onChangeName;
            _ui.onChangeSubName += onChangeSubName;
            _ui.onNeedSelectImage += onNeedSelectImage;
            _ui.onChangeDesc += onChangeDesc;
            _ui.onChangeAttack += onChangeAttack;
            _ui.onChangeLife += onChangeLife;
            _ui.onChangeRace += onChangeRace;
            _ui.onInputFieldLanguageChanged += onInputFieldLangaugeChangedCallback;
            _ui.onInputFieldDeselect += onInputFieldDeselectCallback;

            // 效果。
            _ui.onClickEditEffectButton += onClickEditEffectButton;
            _ui.onClickRemoveEffectButton += onClickRemoveEffectButton;
            _ui.onClickAddEffectMenu += onClickAddEffectMenu;
            _ui.onEffectListItemInputFieldEndEdit += onEffectListItemInputFieldEndEditCallback;
            _effectEditorCtrl.onCreateEffect += onCreateEffect;
            _effectEditorCtrl.OnGraphEdited += onGraphEdited;

            // 侧边栏。
            _ui.onClickDeleteButton += onClickDeleteCardButton;
            _ui.onClickCopyCardButton += onClickCopyCardButton;
            _ui.onClickSaveButton += onClickSaveButton;
            _ui.onClickCloseButton += onClickCloseButton;

            // 属性页面。
            _ui.onClickSkillRefButton += onClickSkillRefButton;
            _ui.onSkillRefEditButtonClicked += onSkillRefEditButtonClicked;
            _ui.onChangeTag += onChangeTag;
            _ui.onSoundButtonClicked += onSoundButtonClickedCallback;

            // 选卡。
            // 卡牌属性。
            _ui.onConfirmCreateCustomProperty += onConfirmCreateCustomPropertyCallback;
            _ui.onDeleteCustomProperty += onDeleteCustomPropertyCallback;
            _ui.onPropertyValueChanged += onPropertyValueChangedCallback;
            _ui.onClickPropertyButton += onClickPropertyButtonCallback;

            // 节点图。
            _graphEditor.onOpenNodeAnimationDialog += onOpenNodeAnimationDialogCallback;
            _graphEditor.onAnimationPropChanged += onNodeAnimationPropChangedCallback;
            _graphEditor.onAnimationResetColorClicked += onAnimationResetColorClickedCallback;
            _graphEditor.onOpenNodeAnimationColorPicker += onOpenNodeAnimationColorPickerCallback;
            _graphEditor.onColorPickerValueChanged += onColorPickerValueChangedCallback;
        }
        #endregion

        #region 事件回调

        #region 基础页面
        private async void onChangeSkin(int skinIndex)
        {
            markUnsaved();
            if (skinIndex == _cardSkins.Count)
            {
                //新建皮肤
                _curCardSkin = game.card.createSkin(curCardPoolId, _cardDefine.id);
                initSkinSoundRefs(_curCardSkin);
                _cardSkins.Add(_curCardSkin);
                updateSkinNames();
            }
            await setSkin(skinIndex);
        }
        private async void onChangeSkinName(string skinName)
        {
            markUnsaved();
            _curCardSkin.skinName = skinName;
            updateSkinNames();
            await setSkin(_curCardSkin);
        }
        private async void onDeleteSkin()
        {
            markUnsaved();
            if (_cardSkins.Count <= 1)
            {
                Debug.LogWarning("你不能删除卡牌的最后一个皮肤");
                return;
            }
            _cardSkins.Remove(_curCardSkin);
            updateSkinNames();
            await resetSkin();
        }
        /// <summary>
        /// 当用户更改类型
        /// </summary>
        /// <param name="typeIndex"></param>
        private async void onChangeType(int typeIndex)
        {
            markUnsaved();
            _cardDefine.type = _cardTypes[typeIndex];
            _ui.setType(typeIndex, _cardTypes[typeIndex]);
            await updateCardType();
        }
        private void onIsTokenToggleValueChanged(bool value)
        {
            markUnsaved();
            _cardDefine.setIsToken(value);
        }
        private void onChangeCharacter(int characterIndex)
        {
            markUnsaved();
            if (characterIndex < 1)
            {
                _cardDefine.setCharacterRef(DefineReference.Empty);
            }
            else
            {
                _cardDefine.setCharacterRef(new DefineReference(
                    _selectableCharacters[characterIndex - 1].cardPoolId,
                    _selectableCharacters[characterIndex - 1].id));
            }
        }
        private async void onSaveCardFaceToImage()
        {
            //使用另一个摄像机对卡牌进行拍摄
            try
            {
                var success = await game.ui.cardshot(_cardDefine, _curCardSkin);
                if (success)
                {
                    var title = game.lang._(StringTable.HINT_TITLE);
                    var content = game.lang._(CARD_PICTURE_EXPORTED);
                    game.ui.dialog.ShowConfirm(title, content);
                }
            }
            catch (Exception)
            {
                var title = game.lang._(StringTable.ERROR);
                var content = game.lang._(CARD_PICTURE_EXPORT_FAILED);
                game.ui.dialog.ShowConfirm(title, content);
                throw;
            }
        }
        #endregion

        #region 卡面
        /// <summary>
        /// 当用户更改花费
        /// </summary>
        /// <param name="cost"></param>
        void onChangeCost(string cost)
        {
            if (int.TryParse(cost, out int value))
            {
                markUnsaved();
                _cardDefine.setProp(DEFINE.COST, value);
                _ui.setCost(cost);
            }
        }
        /// <summary>
        /// 当用户更改卡名
        /// </summary>
        /// <param name="name"></param>
        async void onChangeName(string name)
        {
            markUnsaved();
            game.card.setCardName(getCurrentCardPoolRef(), _curCardSkin, _curCardNameLang, name);
            _ui.setName(name);
            await updateCardType();
        }
        /// <summary>
        /// 当用户更改卡副名
        /// </summary>
        /// <param name="subName"></param>
        void onChangeSubName(string subName)
        {
            markUnsaved();
            game.card.setCardSubname(getCurrentCardPoolRef(), _curCardSkin, _curCardSubnameLang, subName);
            _ui.setSubName(subName);

        }
        /// <summary>
        /// 当需要用户选择图片，根据平台不同调用不同的插件方法，返回选择的贴图。新建的贴图需要缓存在内存中。
        /// </summary>
        private async Task<Sprite> onNeedSelectImage()
        {
            Sprite sprite = await game.card.selectExternalImage();
            if (sprite != null)
            {
                markUnsaved();
                game.card.setCardImagePath(getCurrentCardPoolRef(), _curCardSkin, LangManager.SOURCE, game.card.getResourcePath(sprite));
            }
            return sprite;
        }
        /// <summary>
        /// 当用户更改卡片描述
        /// </summary>
        /// <param name="desc"></param>
        void onChangeDesc(string desc)
        {
            markUnsaved();
            game.card.setCardDesc(getCurrentCardPoolRef(), _curCardSkin, _curCardDescLang, desc);
            _ui.setDesc(desc);
        }
        /// <summary>
        /// 当用户更改攻击力
        /// </summary>
        /// <param name="attack"></param>
        private void onChangeAttack(string attack)
        {
            if (int.TryParse(attack, out int value))
            {
                markUnsaved();
                _cardDefine.setProp(DEFINE.ATTACK, value);
                _ui.setAttack(attack);
            }
        }
        /// <summary>
        /// 当用户更改生命值
        /// </summary>
        /// <param name="life"></param>
        private void onChangeLife(string life)
        {
            if (int.TryParse(life, out int value))
            {
                markUnsaved();
                _cardDefine.setProp(DEFINE.LIFE, value);
                _ui.setLife(life);
            }
        }
        /// <summary>
        /// 当用户更改标签
        /// </summary>
        private void onChangeRace(int index)
        {
            markUnsaved();
            string[] value = _cardDefine.getTags();
            if (value == null || value.Length < 1)
            {
                value = new string[1];
            }
            if (index >= 0)
            {
                value[0] = _races[index];
            }
            else
            {
                value[0] = null;
                index = -1;
            }
            value = value.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            _cardDefine.setTags(value);
            _ui.setTags(value);
            _ui.setRace(index, value.Length > 0 ? value[0] : string.Empty);
        }
        private async void onInputFieldLangaugeChangedCallback(int languageIndex, CardEditorPanel.CardInputField field)
        {
            switch (field)
            {
                case CardEditorPanel.CardInputField.Name:
                    _curCardNameLang = game.lang.getLanguagesWithSource()[languageIndex];
                    break;
                case CardEditorPanel.CardInputField.Subname:
                    _curCardSubnameLang = game.lang.getLanguagesWithSource()[languageIndex];
                    break;
                case CardEditorPanel.CardInputField.Description:
                    _curCardDescLang = game.lang.getLanguagesWithSource()[languageIndex];
                    break;
            }
            await updateSkin();
        }
        private async void onInputFieldDeselectCallback(CardEditorPanel.CardInputField field)
        {
            switch (field)
            {
                case CardEditorPanel.CardInputField.Name:
                    _ui.setNameInputFieldActive(false);
                    break;
                case CardEditorPanel.CardInputField.Subname:
                    _ui.setSubnameInputFieldActive(false);
                    break;
                case CardEditorPanel.CardInputField.Description:
                    _ui.setDescInputFieldActive(false);
                    break;
            }
            await updateSkin();
        }
        #endregion

        #region 音效
        private void onSoundButtonClickedCallback(int soundIndex, SoundItem.Buttons button)
        {
            switch (button)
            {
                case SoundItem.Buttons.Sound:
                    onSoundSelectButtonClickedCallback(soundIndex);
                    break;
                case SoundItem.Buttons.Play:
                    onSoundPlayButtonClickedCallback(soundIndex);
                    break;
                case SoundItem.Buttons.Delete:
                    onSoundDeleteButtonClickedCallback(soundIndex);
                    break;
            }
        }
        /// <summary>
        /// 当需要用户选择音频，根据平台不同调用不同的插件方法，返回选择的音频。新建的音频需要缓存在内存中。
        /// </summary>
        private async void onSoundSelectButtonClickedCallback(int soundIndex)
        {
            _ui.setSoundItemInteractable(soundIndex, false);
            await selectSound(soundIndex);
            _ui.setSoundItemInteractable(soundIndex, true);
        }
        private async void onSoundPlayButtonClickedCallback(int index)
        {
            initSkinSoundRefs(_curCardSkin);
            var soundRef = getCurrentSoundRef(index);
            AudioClip audio = await game.card.loadSound(getCurrentCardPoolRef(), soundRef.path);
            if (audio != null)
            {
                game.seManager.playSE(audio);
            }
        }
        private async void onSoundDeleteButtonClickedCallback(int index)
        {
            var soundRef = getCurrentSoundRef(index);
            soundRef.path = null;
            await updateSoundItems();
        }
        #endregion

        #region 属性页面
        private async void onClickSkillRefButton()
        {
            //打开卡牌选择界面
            long poolId = curCardPoolId;
            var skillRef = _cardDefine.getSkillRef();
            if (skillRef != null && skillRef.defineId != 0)
            {
                poolId = skillRef.cardPoolId;
            }
            var source = new CardSelectSourceSkill();
            await displayCardSelectPanel(source, cardPoolFiltersId: new long[] { poolId }, cardFilter: card => card.isSkill());
        }
        private async void onSkillRefEditButtonClicked()
        {
            //打开卡牌选择界面
            var skillRef = _cardDefine.getSkillRef();
            await scene.editor.openReferedCard(skillRef, async (cardDefine) =>
            {
                markUnsaved();
                await setSkillRef(cardDefine.getDefineRef());
            });
        }
        private void onChangeTag(string text)
        {
            markUnsaved();
            var tags = text.Split(',', ' ', '，');
            tags = tags.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            _cardDefine.setTags(tags);
            _ui.setTags(tags);
            if (tags.Length > 0)
            {
                string race = tags[0];
                int index = Array.IndexOf(_races, race);
                _ui.setRace(index, race);
            }
            else
            {
                _ui.setRace(-1, string.Empty);
            }
        }
        #endregion

        #region 自定义属性
        private async void onConfirmCreateCustomPropertyCallback(PropertyItemInfo item)
        {
            if (string.IsNullOrEmpty(item.propertyName))
            {
                string errorMessage = game.lang._(StringTable.EDITOR_ERROR_NAME_EMPTY);
                _ui.showCustomPropertyDialogError(errorMessage);
                return;
            }
            if (isPropertyNameTaken(item.propertyName))
            {
                string errorMessage = game.lang._(StringTable.EDITOR_ERROR_NAME_TAKEN);
                _ui.showCustomPropertyDialogError(errorMessage);
                return;
            }
            var value = item.value;
            if (value == null)
            {
                value = item.type.getDefaultValue();
            }
            value = value.cloneValue();
            setCardProp(true, item.propertyName, value);

            markUnsaved();
            _ui.displayCustomPropertyDialog(false);
            await updateCustomProperties();
        }
        private void onPropertyValueChangedCallback(bool isCustom, string name, object value)
        {
            value = value.cloneValue();
            setCardProp(isCustom, name, value);
            markUnsaved();
            updatePropertyItem(isCustom, name);
        }
        private void onClickPropertyButtonCallback(bool isCustom, string name, int[] itemIndexes, ValueInputFieldButton button)
        {
            switch (button)
            {
                case ValueInputFieldButton.CardButton:
                    onClickPropertyCardButtonCallback(isCustom, name, itemIndexes);
                    break;
                case ValueInputFieldButton.CardEditButton:
                    onClickPropertyCardEditButtonCallback(isCustom, name, itemIndexes);
                    break;
                case ValueInputFieldButton.BuffEditButton:
                    onClickPropertyBuffEditButtonCallback(isCustom, name, itemIndexes);
                    break;
            }
        }
        private async void onClickPropertyCardButtonCallback(bool isCustom, string name, int[] itemIndexes)
        {
            var prop = _cardDefine.getProp<object>(name);
            if (prop == null)
                return;
            //打开卡牌选择界面
            long poolId = curCardPoolId;
            if (prop.tryGetListElementByIndexes(itemIndexes, out DefineReference defRef))
            {
                if (defRef.defineId != 0)
                {
                    poolId = defRef.cardPoolId;
                }
            }
            var source = new CardSelectSourceCustomProperty()
            {
                propName = name,
                propIndexes = itemIndexes
            };
            await displayCardSelectPanel(source, cardPoolFiltersId: new long[] { poolId }, cardFilter: card => true);
        }
        private async void onClickPropertyCardEditButtonCallback(bool isCustom, string name, int[] itemIndexes)
        {
            var prop = _cardDefine.getProp<object>(name);
            if (prop.tryGetListElementByIndexes(itemIndexes, out DefineReference defRef))
            {
                await scene.editor.openReferedCard(defRef, setPropAction);

                void setPropAction(CardDefine cardDefine)
                {
                    var newDefRef = new DefineReference(cardDefine.cardPoolId, cardDefine.id);
                    if (prop.replaceListElementByIndexes(itemIndexes, newDefRef, out var newValue))
                    {
                        setCardProp(isCustom, name, newValue);
                        markUnsaved();
                        updatePropertyItem(isCustom, name);
                    }
                }
            }
        }
        private async void onClickPropertyBuffEditButtonCallback(bool isCustom, string name, int[] itemIndexes)
        {
            var prop = _cardDefine.getProp<object>(name);
            if (prop == null)
                return;
            if (prop.tryGetListElementByIndexes(itemIndexes, out DefineReference defRef))
            {
                var poolRef = getCardPoolReference(defRef.cardPoolId);
                if (game.card.tryGetBuffDefine(poolRef, defRef.defineId, out GeneratedBuffDefine buffDefine))
                {
                    await scene.editor.openBuffEditor(buffDefine);
                }
            }
        }
        private async void onDeleteCustomPropertyCallback(string name)
        {
            _cardDefine.removeProp(name);
            markUnsaved();
            await updateCustomProperties();
        }
        #endregion

        #region 效果
        private async void onCreateEffect(GeneratedEffect effect)
        {
            markUnsaved();
            effect.cardDefineRef = _cardDefine.getDefineRef();
            effect.buffDefineRef = null;

            var effects = _cardDefine.getEffects().OfType<GeneratedEffect>();
            effect.name = getEffectDefaultName(effect, effects.Count() + 1);
            var effectArray = effects.Append(effect).ToArray();
            _cardDefine.setEffects(effectArray);

            await _effectEditorCtrl.display(effect);
            _graphEditor.updateUndoRedoButton();
            updateEffectList(effectArray);

            _ui.setEffectScrollValue(1);
        }
        /// <summary>
        /// 当点击编辑效果按钮，打开编辑效果界面。
        /// </summary>

        private async void onClickEditEffectButton(int index)
        {
            var effects = _cardDefine.getEffects().OfType<GeneratedEffect>();
            GeneratedEffect effect = effects.ElementAt(index);
            if (_effectEditorCtrl.getCurrentEffect() != effect)
            {
                await _effectEditorCtrl.display(effect);
                updateEffectList(effects.ToArray());
                _graphEditor.updateUndoRedoButton();
            }
            else
            {
                string name = effect?.name ?? string.Empty;
                _ui.showEffectListItemInputField(index, name);
            }
        }
        private void onClickRemoveEffectButton(int index)
        {
            markUnsaved();
            var effects = _cardDefine.getEffects().OfType<GeneratedEffect>();
            var removedEffect = effects.ElementAt(index);
            GeneratedEffect[] newEffects = effects.Where(e => e != removedEffect).ToArray();
            _cardDefine.setEffects(newEffects);
            if (_effectEditorCtrl.getCurrentEffect() == removedEffect)
            {
                _effectEditorCtrl.hideActionGraph();
            }
            updateEffectList(newEffects);
        }
        /// <summary>
        /// 当点击添加效果按钮，为卡牌定义添加一个新的空效果，并刷新效果列表。
        /// </summary>
        private void onClickAddEffectMenu(int index)
        {
            _effectEditorCtrl.createEffectByIndex(index);
        }
        private void onEffectListItemInputFieldEndEditCallback(int index, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            var effects = _cardDefine.getEffects().OfType<GeneratedEffect>();
            GeneratedEffect effect = effects.ElementAt(index);
            effect.name = value;
            _ui.hideEffectListItemInputField(index);
            updateEffectList(effects.ToArray());
        }
        #endregion

        #region 侧边栏
        private void onClickDeleteCardButton()
        {
            //询问玩家
            game.ui.dialog.ShowQuestion(
               game.langManager.getString(StringTable.WARNING),
               game.langManager.getString(EDITOR_WARNING_DELETE_CARD),
               async () =>
               {
                   //删除当前卡牌
                   game.card.removeCard(_cardDefine);
                   scene.editor.markCardPoolUnsaved(curCardPoolId);
                   scene.editor.removeDisplayPageOfObject(_cardDefine);
                   await exit(true);
               });
        }
        public void onClickCopyCardButton()
        {
            scene.editor.copyCard(_cardDefine.getDefineRef());
            _ui.showCopyHint();
        }
        private void onClickSaveButton()
        {
            saveCard();
        }
        private async void onClickCloseButton()
        {
            if (hasUnsavedChanges)
            {
                string title = game.lang._(StringTable.WARNING);

                string desc = game.lang._(CARD_WARNING_UNSAVED_CHANGES);
                string[] optNames = new string[]
                {
                    game.lang._(StringTable.SAVE),
                    game.lang._(StringTable.DONT_SAVE),
                    game.lang._(StringTable.CANCEL)
                };
                game.ui.dialog.ShowOptions(title, desc, optNames, async (index) =>
                {
                    if (index == 0)
                    {
                        saveCard();
                        await exit();
                    }
                    else if (index == 1)
                    {
                        scene.editor.removeDisplayPageOfObject(_cardDefine);
                        await exit();
                    }
                });
            }
            else
            {
                await exit();
            }
        }

        #endregion

        #region 选卡
        #endregion

        #region 节点图
        private void onGraphEdited()
        {
            markUnsaved();
        }
        #endregion

        #region 特效
        private void onOpenNodeAnimationDialogCallback(Node node)
        {
            var eventNames = game.anims.getNodeVisualEffectEventEditorNames(node);
            var viewData = game.anims.getNodeAnimationViewData(node, _curCardSkin);
            var colorViewData = game.anims.getNodeAnimationColorViewData(node, _curCardSkin);
            _graphEditor.initNodeAnimationDialogEvents(eventNames);
            _graphEditor.updateAnimationDialog(viewData);
            _graphEditor.updateAnimationDialogColors(colorViewData);
            _graphEditor.displayNodeAnimationDialog(node);
        }
        private void onNodeAnimationPropChangedCallback(Node node, VisualEffectDialogInfo info)
        {
            game.anims.updateNodeAnimRef(node, _curCardSkin, info);
            markUnsaved();
            var eventViewData = game.anims.getNodeAnimationViewData(node, _curCardSkin);
            var colorViewData = game.anims.getNodeAnimationColorViewData(node, _curCardSkin);
            _graphEditor.updateAnimationDialog(eventViewData);
            _graphEditor.updateAnimationDialogColors(colorViewData);
        }
        private void onAnimationResetColorClickedCallback(Node node)
        {
            game.anims.resetNodeAnimColor(node, _curCardSkin);
            markUnsaved();
            var colorViewData = game.anims.getNodeAnimationColorViewData(node, _curCardSkin);
            _graphEditor.updateAnimationDialogColors(colorViewData);
        }
        private void onOpenNodeAnimationColorPickerCallback(Node node, int index)
        {
            var color = game.anims.getNodeAnimationColor(node, _curCardSkin, index);
            _graphEditor.displayColorPicker(color);
        }
        private void onColorPickerValueChangedCallback(Node node, int colorIndex, Color color)
        {
            game.anims.setNodeAnimColor(node, _curCardSkin, colorIndex, color);
            markUnsaved();
            var colorViewData = game.anims.getNodeAnimationColorViewData(node, _curCardSkin);
            _graphEditor.updateAnimationDialogColors(colorViewData);
        }
        #endregion

        #endregion
        protected override async void onCardSelected(ICardSelectSource source, CardDefine cardDefine)
        {
            base.onCardSelected(source, cardDefine);
            switch (source)
            {
                case CardSelectSourceSkill:
                    markUnsaved();
                    await setSkillRef(cardDefine.getDefineRef());
                    break;
                case CardSelectSourceCustomProperty customProp:
                    var propName = customProp.propName;
                    var itemIndexes = customProp.propIndexes;
                    var prop = _cardDefine.getProp<object>(propName);
                    var defineRef = cardDefine.getDefineRef();
                    var isCustom = !isPropertyNameBuiltin(propName);
                    if (prop.replaceListElementByIndexes(itemIndexes, defineRef, out var newProp))
                    {
                        setCardProp(isCustom, propName, newProp);
                    }
                    updatePropertyItem(isCustom, propName);
                    markUnsaved();
                    break;
            }
        }

        #region 基础页面
        private async Task<CardDefine[]> getSelectableCharacters()
        {
            List<CardDefine> cardList = new List<CardDefine>();
            foreach (var poolRef in getCardPoolReferenceEnv())
            {
                var (info, pool) = await game.card.getOrLoadCardPool(poolRef);
                if (pool == null)
                    continue;
                cardList.AddRange(pool.cardDefineList.Where(c => c.isMaster()));
            }
            return cardList.ToArray();
        }
        private async Task updateCardType()
        {
            var cardDefine = _cardDefine;
            var cardFaceType = CardFaceType.Servant;
            if (cardDefine.isSpell() || cardDefine.isSkill())
                cardFaceType = CardFaceType.Spell;
            else if (cardDefine.isItem())
                cardFaceType = CardFaceType.Item;
            _ui.switchCardFace(cardFaceType);

            _ui.setSkillRefVisible(cardDefine.isMaster());
            if (cardDefine.isMaster())
                await setSkillRef(cardDefine.getSkillRef());
            else
            {
                cardDefine.removeProp(DEFINE.CARD_PROP_SKILL_ID);
                cardDefine.removeProp(DEFINE.CARD_PROP_SKILL_REF);
                await setSkillRef(null);
            }

            await updateSoundItems();
        }
        #endregion

        #region 皮肤
        private void updateSkinNames()
        {
            if (_cardSkins.Count < 1)
            {
                CardSkinData skin = game.card.createSkin(curCardPoolId, _cardDefine.id);
                initSkinSoundRefs(skin);
                _cardSkins.Add(skin);
            }
            _ui.setSkinNames(_cardSkins.Select(s => game.card.getCardName(getCurrentCardPoolRef(), s)).ToArray());
        }

        #region 设置卡牌皮肤
        private Task resetSkin()
        {
            return setSkin(0, getDefaultSkin());
        }
        private Task updateSkin()
        {
            return setSkin(_curCardSkin);
        }
        private Task setSkin(int index)
        {
            var skin = _cardSkins[index];
            return setSkin(index, skin);
        }
        private Task setSkin(CardSkinData skin)
        {
            var index = _cardSkins.FindIndex(s => s.skinId == skin.skinId);
            return setSkin(index, skin);
        }
        private async Task setSkin(int index, CardSkinData skin)
        {
            _curCardSkin = skin;
            var poolRef = getCurrentCardPoolRef();
            _ui.setSkin(index);
            _ui.setName(game.card.getLocalizedCardName(poolRef, skin, _curCardNameLang));
            _ui.setSubName(game.card.getLocalizedCardSubname(poolRef, skin, _curCardSubnameLang));
            _ui.setDesc(game.card.getLocalizedCardDesc(poolRef, skin, _curCardDescLang));
            Sprite image = await game.card.loadCardImage(poolRef, skin);
            _ui.setImage(image);
        }
        #endregion

        #region 获取卡牌皮肤
        private CardSkinData getDefaultSkin()
        {
            return _cardSkins.FirstOrDefault();
        }
        private CardSkinData getSkin(int skinId)
        {
            return _cardSkins.FirstOrDefault(s => s.skinId == skinId);
        }
        #endregion

        #endregion

        #region 属性页面
        private async Task setSkillRef(DefineReference defRef)
        {
            if (defRef == null)
            {
                _ui.setSkillRef(null, string.Empty);
                return;
            }
            _cardDefine.setSkillRef(defRef);
            var poolRef = getCardPoolReference(defRef.cardPoolId);
            var skin = game.card.getCardSkin(poolRef, defRef.defineId);
            Sprite spr;
            string name;
            if (skin != null)
            {
                spr = await game.card.loadCardImage(poolRef, skin);
                name = game.card.getCardName(poolRef, skin);
            }
            else
            {
                spr = game.card.getDefaultSprite();
                name = game.lang._(StringTable.BUILD_UNKNOWN_CARD_NAME);
            }
            _ui.setSkillRef(spr, name);
        }
        #endregion

        #region 增益属性UI
        /// <summary>
        /// 更新所有增益引用，并重新设置UI的增益定义的选项。
        /// </summary>
        private void updateEditorBuffs()
        {
            game.editor.updateBuffDefines();
            var fieldProps = game.editor.getBuffInputProperties();
            _ui.initValueInputFieldProperties(fieldProps);
        }
        #endregion

        #region 自定义属性
        private void setCardProp(bool isCustom, string name, object value)
        {
            value = game.editor.validatePropertyValue(value, updateEditorBuffs);
            if (isCustom)
            {
                _cardDefine.setProp(name, value);
                return;
            }

            // 特殊属性设置方法。
            if (keywordActions.TryGetValue(name, out var actions))
            {
                var setter = actions.setter;
                setter?.Invoke(_cardDefine, value);
                return;
            }

            // 默认属性设置方法。
            // 设置关键词。
            var keywords = _cardDefine.getKeywords();
            if (value is bool boolValue && boolValue)
            {
                if (keywords == null)
                {
                    keywords = new string[] { name };
                }
                else if (!_cardDefine.hasKeyword(name))
                {
                    keywords = keywords.Append(name).Distinct().ToArray();
                }
            }
            else if (keywords != null && _cardDefine.hasKeyword(name))
            {
                List<string> keywordList = new List<string>(keywords);
                keywordList.Remove(name);
                keywords = keywordList.ToArray();
            }
            _cardDefine.setKeywords(keywords);
        }
        /// <summary>
        /// 更新所有关键词。
        /// </summary>
        private async Task updateKeywords()
        {
            var itemInfos = GetCardDefineKeywordItems();
            _ui.keywordsFoldout.updateItems(itemInfos);

            var env = getCardPoolReferenceEnv();
            var fields = _ui.keywordsFoldout.getPropertyItemFields();
            for (int i = 0; i < itemInfos.Length; i++)
            {
                await fields[i].valueInputField.setInputFieldCardByValue(itemInfos[i].value, env, game);
            }
        }
        /// <summary>
        /// 更新所有音频项。
        /// </summary>
        private async Task updateSoundItems()
        {
            var skinNames = getCurrentSoundRefNames();
            var uiNames = getCurrentSoundRefUINames();
            var viewDatas = new (string name, Sprite sprite)[skinNames.Length];
            for (int i = 0; i < skinNames.Length; i++)
            {
                var skinSoundName = skinNames[i];
                var uiName = uiNames[i];
                var soundPath = _curCardSkin.getSoundRef(skinSoundName)?.path;
                viewDatas[i] = (game.lang._(uiName), await game.card.getSoundWaveSprite(getCurrentCardPoolRef(), soundPath));
            }
            _ui.updateSoundItems(viewDatas);
        }
        /// <summary>
        /// 更新所有自定义属性。
        /// </summary>
        private async Task updateCustomProperties()
        {
            var itemInfos = GetCardDefineCustomProps();
            _ui.updateCustomProperties(itemInfos);

            var env = getCardPoolReferenceEnv();
            var fields = _ui.getCustomPropertyItemFields();
            for (int i = 0; i < itemInfos.Length; i++)
            {
                await fields[i].valueInputField.setInputFieldCardByValue(itemInfos[i].value, env, game);
            }
        }
        /// <summary>
        /// 更新一项属性。
        /// </summary>
        /// <param name="isCustom">要更新的属性是否是自定义属性？</param>
        /// <param name="name">属性名称。</param>
        private async void updatePropertyItem(bool isCustom, string name)
        {
            PropertyItemInfo itemInfo;
            PropertyItemField propertyItemField;
            if (isCustom)
            {
                itemInfo = GetCardDefineCustomProperty(name);
                propertyItemField = _ui.getCustomPropertyItemField(name);
            }
            else
            {
                itemInfo = GetCardDefineKeywordItem(name);
                propertyItemField = _ui.keywordsFoldout.getPropertyItemField(name);
            }
            propertyItemField.updateItem(itemInfo);
            await propertyItemField.valueInputField.setInputFieldCardByValue(itemInfo.value, getCardPoolReferenceEnv(), game);
        }
        private PropertyItemInfo[] GetCardDefineCustomProps()
        {
            var names = _cardDefine.getPropNames();
            List<PropertyItemInfo> items = new List<PropertyItemInfo>();
            foreach (string name in names)
            {
                var item = GetCardDefineCustomProperty(name);
                if (item == null)
                    continue;
                items.Add(item);
            }
            return items.ToArray();
        }
        private PropertyItemInfo GetCardDefineCustomProperty(string name)
        {
            if (isPropertyNameBuiltin(name))
            {
                return null;
            }

            var value = _cardDefine[name];
            if (value == null)
                return null;

            var valueType = value.GetType();
            var type = valueType.getNodeValueType();
            if (!EditorManager.isTypeCustomProp(type.type))
                return null;

            var item = new PropertyItemInfo(name, type)
            {
                value = value
            };
            return item;
        }

        private PropertyItemInfo[] GetCardDefineKeywordItems()
        {
            List<PropertyItemInfo> items = new List<PropertyItemInfo>();
            var keywords = EditorManager.editorKeywords;
            foreach (var pair in keywords)
            {
                var item = GetCardDefineKeywordItem(pair);
                if (item == null)
                    continue;
                items.Add(item);
            }
            return items.ToArray();
        }
        private PropertyItemInfo GetCardDefineKeywordItem(string name)
        {
            if (!game.card.tryGetKeywordInfo(name, out var info))
                return null;
            var type = info.type;
            NodeValueType valueType = new NodeValueType(ActionNodeValueType.boolean);
            if (type.tryGetValueTypeByType(out var t) && EditorManager.isTypeCustomProp(t.type))
            {
                valueType = t;
            }

            object value = keywordActions.TryGetValue(name, out var valueAction) ?
                valueAction.Item2(_cardDefine) :
                _cardDefine.hasKeyword(name);

            var item = new PropertyItemInfo(name, game.card.getKeywordName(name), valueType)
            {
                value = value
            };
            return item;
        }
        /// <summary>
        /// 属性名是否已经被使用。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool isPropertyNameTaken(string name)
        {
            if (_cardDefine.hasProp(name) && _cardDefine.getProp<object>(name) != null)
                return true;
            return isPropertyNameBuiltin(name);
        }
        /// <summary>
        /// 属性名是否是内置属性。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool isPropertyNameBuiltin(string name)
        {
            return takenPropNames.Contains(name);
        }
        #endregion

        #region 效果
        protected override void setUIEffects(string[] names, int activeIndex)
        {
            _ui.setEffects(names, activeIndex);
        }
        #endregion

        #region 侧边栏
        private void saveCard()
        {
            var poolRef = getCurrentCardPoolRef();
            if (game.card.getCardDefine(poolRef, _cardDefine.id) != null)
            {
                game.card.removeCard(curCardPoolId, _cardDefine.id);
                game.card.removeAllCardSkins(curCardPoolId, _cardDefine.id);
            }
            game.card.addCard(curCardPoolId, _cardDefine);
            foreach (var skin in _cardSkins)
            {
                game.card.addCardSkin(curCardPoolId, skin);
            }

            scene.editor.markCardPoolUnsaved(curCardPoolId);
            clearUnsaved();
        }
        /// <summary>
        /// 返回上一个界面
        /// </summary>
        private async Task exit(bool forceSave = false)
        {
            clearUnsaved();
            await scene.editor.closeEditor(forceSave);
        }
        #endregion

        private void initSkinSoundRefs(CardSkinData skin)
        {
            foreach (var soundName in getCurrentSoundRefNames())
            {
                if (skin.getSoundRef(soundName) == null)
                    skin.addSoundRef(new SkinSoundRef(soundName, null));
            }
        }
        private SkinSoundRef getCurrentSoundRef(int soundIndex)
        {
            var skinSoundNames = getCurrentSoundRefNames();
            var soundName = skinSoundNames[soundIndex];
            var soundRef = _curCardSkin.getSoundRef(soundName);
            if (soundRef == null)
            {
                soundRef = new SkinSoundRef(soundName, null);
                _curCardSkin.addSoundRef(soundRef);
            }
            return soundRef;
        }
        private string[] getCurrentSoundRefNames()
        {
            return CardSkinData.getSoundNamesByType(_cardDefine.type);
        }
        private string[] getCurrentSoundRefUINames()
        {
            return getCurrentSoundRefNames().Select(n => _soundNameDict.TryGetValue(n, out var name) ? name : SKIN_SOUND_NAME_UNKNOWN).ToArray();
        }
        private async Task selectSound(int soundIndex)
        {
            AudioClip audio = null;
            try
            {
                audio = await game.card.selectExternalSound();
                if (audio == null)
                    return;
                scene.waitingBlocker.addWaitingTask(WaitingTask.CardEditor);
                var newPath = await game.card.saveSound(game.editor.currentCardPoolId, audio);
                var directory = game.card.getCardPoolResourceDirectory(game.card.getLocalCardPoolInfo(game.editor.currentCardPoolId), ResourceType.Sound);
                var fullPath = Path.Combine(directory, newPath);
                if (!File.Exists(fullPath))
                {
                    var title = game.lang._(StringTable.ERROR);
                    var desc = game.lang._(CardManager.SOUND_COMPRESS_FAILED_FILE_NOT_FOUND);
                    game.ui.dialog.ShowConfirm(title, desc);
                    return;
                }
                var fileInfo = new FileInfo(fullPath);
                if (fileInfo.Length > game.card.maxSoundSize)
                {
                    var title = game.lang._(StringTable.ERROR);
                    var desc = game.lang._(CardManager.SOUND_COMPRESSED_SIZE_OUT_OF_LIMIT, game.card.maxSoundSize / 1024, fileInfo.Length / 1024);
                    game.ui.dialog.ShowConfirm(title, desc);

                    File.Delete(fullPath);
                    return;
                }
                markUnsaved();

                initSkinSoundRefs(_curCardSkin);
                var soundRef = getCurrentSoundRef(soundIndex);
                soundRef.path = newPath;
                await updateSoundItems();
            }
            catch (Exception e)
            {
                var title = game.lang._(StringTable.ERROR);
                game.ui.dialog.ShowConfirm(title, e.Message);
            }
            finally
            {
                scene.waitingBlocker.removeWaitingTask(WaitingTask.CardEditor);
            }
        }

        #endregion

        #region 属性字段

        /// <summary>
        /// 正在编辑的卡牌定义。
        /// </summary>
        CardDefine _cardDefine;
        /// <summary>
        /// 正在编辑的卡牌定义的皮肤。
        /// </summary>
        CardSkinData _curCardSkin;
        /// <summary>
        /// 正在编辑的卡牌定义的所有的皮肤。
        /// </summary>
        List<CardSkinData> _cardSkins = new List<CardSkinData>();
        CardDefine[] _selectableCharacters;
        string[] _cardTypes;
        string _curCardNameLang;
        string _curCardSubnameLang;
        string _curCardDescLang;
        private long curCardPoolId => game.editor.currentCardPoolId;

        #region 翻译文本
        [TranslateMsg("尝试删除卡牌的警告")]
        public const string EDITOR_WARNING_DELETE_CARD = "确认要删除这张卡牌吗？";
        [TranslateMsg("卡牌编辑器/标题")]
        public const string CARD_EDITOR_TITLE = "卡牌编辑器";
        [TranslateMsg("尝试退出一张卡牌的编辑页面，但没有保存情况下的警告")]
        public const string CARD_WARNING_UNSAVED_CHANGES = "是否将更改保存到该卡牌中？";
        [TranslateMsg("卡牌图片导出成功")]
        public const string CARD_PICTURE_EXPORTED = "卡牌图片导出成功。";
        [TranslateMsg("卡牌图片导出失败")]
        public const string CARD_PICTURE_EXPORT_FAILED = "卡牌图片导出失败。";

        [TranslateMsg("卡牌皮肤编辑器中卡牌的未知音效的名称")]
        public const string SKIN_SOUND_NAME_UNKNOWN = "未知音效";
        [TranslateMsg("卡牌皮肤编辑器中仆从牌的入场音效的名称")]
        public const string SKIN_SOUND_NAME_ON_ENTER = "入场音效";
        [TranslateMsg("卡牌皮肤编辑器中仆从牌的攻击音效的名称")]
        public const string SKIN_SOUND_NAME_ON_ATTACK = "攻击音效";
        [TranslateMsg("卡牌皮肤编辑器中仆从牌的死亡音效的名称")]
        public const string SKIN_SOUND_NAME_ON_DEATH = "死亡音效";
        [TranslateMsg("卡牌皮肤编辑器中卡牌的使用音效的名称")]
        public const string SKIN_SOUND_NAME_ON_USE = "使用音效";
        [TranslateMsg("卡牌皮肤编辑器中道具牌的装备音效的名称")]
        public const string SKIN_SOUND_NAME_ON_EQUIP = "装备音效";
        #endregion

        #region 静态集合
        private static readonly Dictionary<string, (Action<CardDefine, object> setter, Func<CardDefine, object> getter)> keywordActions =
            new Dictionary<string, (Action<CardDefine, object> setter, Func<CardDefine, object> getter)>()
        {
            { DEFINE.SPELL_DAMAGE,(
                    (card, value) => card.setSpellDamage((int)value),
                    (card) => card.getSpellDamage()
                )
            },
            { DEFINE.DAMAGE_REDUCE,(
                    (card, value) => card.setDamageReduce((int)value),
                    (card) => card.getDamageReduce()
                )
            }
        };

        static readonly string[] _races = new string[]
        {
            null,
            TAG.BEAST,
            TAG.DEMON,
            TAG.ELEMENT,
            TAG.FAIRY,
            TAG.GOD,
            TAG.MACHINE,
            TAG.GHOST
        };
        static readonly string[] _raceNames = new string[]
        {
            string.Empty,
            StringTable.CARD_TAG_BEAST,
            StringTable.CARD_TAG_DEMON,
            StringTable.CARD_TAG_ELEMENTAL,
            StringTable.CARD_TAG_FAIRY,
            StringTable.CARD_TAG_GOD,
            StringTable.CARD_TAG_MECH,
            StringTable.CARD_TAG_GHOST
        };
        static readonly Dictionary<string, string> _soundNameDict = new Dictionary<string, string>()
        {
            { CardSkinData.SOUND_NAME_ON_USE, SKIN_SOUND_NAME_ON_USE },
            { CardSkinData.SOUND_NAME_ON_EQUIP, SKIN_SOUND_NAME_ON_EQUIP },
            { CardSkinData.SOUND_NAME_ON_ENTER, SKIN_SOUND_NAME_ON_ENTER },
            { CardSkinData.SOUND_NAME_ON_ATTACK, SKIN_SOUND_NAME_ON_ATTACK },
            { CardSkinData.SOUND_NAME_ON_DEATH, SKIN_SOUND_NAME_ON_DEATH },
        };
        #region 自定义属性
        public static readonly string[] takenPropNames = new string[]
        {
            DEFINE.CHARACTER_ID,
            DEFINE.LIFE,
            DEFINE.ISTOKEN,
            DEFINE.COST,
            DEFINE.ATTACK,
            DEFINE.SPELL_DAMAGE,
            DEFINE.DAMAGE_REDUCE,
            DEFINE.KEYWORDS,
            DEFINE.TAGS,
            DEFINE.CARD_PROP_SKILL_ID,
            DEFINE.CARD_PROP_SKILL_REF,
            CARD.ISDEAD,
            CARD.CURRENT_LIFE,
            CARD.ARMOR,
            CARD.SILENCE,
            CARD.ISDEAD,
            CARD.ISREADY,
            CARD.ATTACK_TIMES
        };
        #endregion

        #endregion

        #region UI
        [SerializeField]
        CardEditorPanel _ui;
        #endregion

        #endregion
    }
    public struct CardSelectSourceSkill : ICardSelectSource
    {
    }
}
