using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{
    class CardEditorCtrl : CardPoolEditor
    {
        #region 公有方法
        /// <summary>
        /// 打开一张卡片进行编辑
        /// </summary>
        public async Task display(long cardPoolId, CardDefine cardDefine, CardSkinData[] skins)
        {
            _selectableCharacters = await getSelectableCharacters();
            _curCardPoolId = cardPoolId;
            _cardDefine = cardDefine;
            _cardSkins.Clear();
            _cardSkins.AddRange(skins);
            gameObject.SetActive(true);
            updateSkinNames();
            _ui.reset();
            _ui.setId(cardDefine.id);
            _ui.setType(Array.IndexOf(_types, cardDefine.type), cardDefine.type);
            _ui.setIsToken(cardDefine.isToken());
            bool[] tagsValue = new bool[_tags.Length];
            for (int i = 0; i < _tags.Length; i++)
            {
                tagsValue[i] = cardDefine.getTags().Contains(_tags[i]);
            }
            _ui.setTags(cardDefine.getTags());
            string[] charactersName = new string[_selectableCharacters.Length + 1];
            charactersName[0] = StringTable.CARDVIEW_CHARACTER_NEUTRAL;
            for (int i = 1; i < charactersName.Length; i++)
            {
                charactersName[i] = game.card.getCardSkin(_selectableCharacters[i - 1]).name;
            }
            _ui.setSelectableCharacters(charactersName);
            //中立为0，有角色则为index+1
            _ui.setCharacter(Array.FindIndex(_selectableCharacters, c =>
                c.cardPoolId == cardDefine.getCharacterRef().cardPoolId &&
                c.id == cardDefine.getCharacterRef().defineId) + 1);
            _ui.setCost(cardDefine.getProp<int>(DEFINE.COST).ToString());
            _ui.setAttack(cardDefine.getProp<int>(DEFINE.ATTACK).ToString());
            _ui.setLife(cardDefine.getProp<int>(DEFINE.LIFE).ToString());
            string[] tags = cardDefine.getProp<string[]>(DEFINE.TAGS);
            if (tags != null && tags.Length > 0)
            {
                int index = Array.IndexOf(_races, tags[0]);
                if (index < 0)
                    index = 0;
                _ui.setRace(index, tags[0]);
            }
            else
                _ui.setRace(0, _races[0]);
            GeneratedEffect[] effects = cardDefine.getEffects().OfType<GeneratedEffect>().ToArray();
            await updateCardType();
            await resetSkin();

            updateCustomProperties();
            updateKeywords();
            _effectEditorCtrl.hideActionGraph();
            if (effects.Length > 0)
            {
                await _effectEditorCtrl.display(_curCardPoolId, effects[0]);
            }
            _effectEditorCtrl.updateEventTriggerTimes();
            updateEffectList(effects);

            _graphEditor.display(_curCardPoolId);
        }
        public override async Task display(CardPoolDisplayPage info)
        {
            if (info == null)
                return;
            displayInfo = info;
            if (info?.displayObj?.cardDefine is CardDefine cardDefine)
            {
                var skins = info.skins as CardSkinData[] ?? Array.Empty<CardSkinData>();
                await display(info.cardPoolId, cardDefine, skins);
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
        public static bool isTypeCustomProp(ActionNodeValueType type)
        {
            return customPropTypes.Contains(type);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            if (_ui == null)
            {
                _ui = GetComponentInChildren<CardEditorPanel>();
            }
            var keywords = EditorManager.editorKeywords;
            var keywordNames = keywords.Select(k => game.card.getKeywordName(k)).ToArray();
            var fieldProps = new ValueInputFieldProperties()
            {
                keywordNames = keywordNames,
                keywordValues = keywords.ToArray(),

                pileNames = game.editor.getAllPileNames(),
                pileValues = game.editor.getAllPileValues()
            };
            _ui.initValueInputFieldProperies(fieldProps);

            _types = new string[] { DEFINE.SERVANT, DEFINE.SPELL, DEFINE.ITEM, DEFINE.MASTER, DEFINE.SKILL };
            string[] typeNames = new string[]
            {
                StringTable.GAME_CARD_TYPE_SERVANT,
                StringTable.GAME_CARD_TYPE_SPELL,
                StringTable.GAME_CARD_TYPE_ITEM,
                StringTable.GAME_CARD_TYPE_MASTER,
                StringTable.GAME_CARD_TYPE_SKILL,

            };
            _ui.initTypes(typeNames);
            _ui.onChangeSkin += onChangeSkin;
            _ui.onChangeSkinName += onChangeSkinName;
            _ui.onClickDeleteSkinButton += onDeleteSkin;
            string[] languages = game.langManager.getLanguages();
            for (int i = 0; i < languages.Length; i++)
            {
                languages[i] = game.langManager.getString(game.ui.getTranslateNameForLanguage(languages[i]));
            }
            _ui.initLanguages(languages);
            _ui.onChangeLanguage += onChangeLanguage;
            _ui.onChangeType += onChangeType;
            _ui.onIsTokenToggleValueChanged += onIsTokenToggleValueChanged;
            string[] tagNames = new string[]
            {
                StringTable.GAME_TAG_METAL,
                StringTable.GAME_TAG_WOOD,
                StringTable.GAME_TAG_WATER,
                StringTable.GAME_TAG_FIRE,
                StringTable.GAME_TAG_EARTH,
            };
            _tags = new string[]
            {
                TAG.METAL,
                TAG.WOOD,
                TAG.WATER,
                TAG.FIRE,
                TAG.EARTH
            };
            _ui.onChangeCharacter += onChangeCharacter;
            _ui.onChangeCost += onChangeCost;
            _ui.onChangeName += onChangeName;
            _ui.onChangeSubName += onChangeSubName;
            _ui.onNeedSelectImage += onNeedSelectImage;
            _ui.onChangeDesc += onChangeDesc;
            _ui.onChangeAttack += onChangeAttack;
            _ui.onChangeLife += onChangeLife;
            _ui.onSaveCardFaceToImage += onSaveCardFaceToImage;
            _races = new string[]
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
            string[] racesName = new string[]
            {
                string.Empty,
                game.langManager.getString(StringTable.GAME_RACE_BEAST),
                game.langManager.getString(StringTable.GAME_RACE_DEMON),
                game.langManager.getString(StringTable.GAME_RACE_ELEMENT),
                game.langManager.getString(StringTable.GAME_RACE_FAIRY),
                game.langManager.getString(StringTable.GAME_RACE_GOD),
                game.langManager.getString(StringTable.GAME_RACE_MACHINE),
                game.langManager.getString(StringTable.GAME_RACE_GHOST)
            };
            _ui.initTags(racesName);
            _ui.onChangeRace += onChangeRace;
            _ui.onClickEditEffectButton += onClickEditEffectButton;
            _ui.onClickRemoveEffectButton += onClickRemoveEffectButton;
            _ui.onClickAddEffectMenu += onClickAddEffectMenu;
            _ui.onEffectListItemInputFieldEndEdit += onEffectListItemInputFieldEndEditCallback;
            _ui.setEffectTypes(game.card.getEffectNames());

            _ui.onClickDeleteButton += onClickDeleteCardButton;
            _ui.onClickCopyCardButton += onClickCopyCardButton;
            _ui.onClickSaveButton += onClickSaveButton;
            _ui.onClickCloseButton += onClickCloseButton;
            _ui.onClickSkillRefButton += onClickSkillRefButton;
            _ui.onSkillRefEditButtonClicked += onSkillRefEditButtonClicked;
            _ui.onChangeTag += onChangeTag;

            _cardPoolViewPanelCtrl.onClickCard += onClickCardPoolViewCard;
            _cancelCardPoolSelectButton.onClick.AddListener(onClickCancelCardPoolSelectButton);

            // Card Properties.
            var customPropTypes = CardEditorCtrl.customPropTypes.Select(t => (t, game.editor.getNameByValueType(t))).ToArray();
            _ui.setCustomPropertyTypes(customPropTypes);
            _ui.onConfirmCreateCustomProperty += onConfirmCreateCustomPropertyCallback;
            _ui.onDeleteCustomProperty += onDeleteCustomPropertyCallback;

            _ui.onPropertyValueChanged += onPropertyValueChangedCallback;
            _ui.onClickPropertyButton += onClickPropertyButtonCallback;

            _effectEditorCtrl.onCreateEffect += onCreateEffect;
            _effectEditorCtrl.OnGraphEdited += onGraphEdited;

            _graphEditor.onOpenNodeAnimationDialog += onOpenNodeAnimationDialogCallback;
            _graphEditor.onAnimationPropChanged += onNodeAnimationPropChangedCallback;
        }
        #endregion
        #region 事件回调
        private void onGraphEdited()
        {
            markUnsaved();
        }
        private async void onChangeSkin(int skinIndex)
        {
            markUnsaved();
            if (skinIndex == getCurrentLangaugeSkins().Length)
            {
                //新建皮肤
                _curCardSkin = game.card.createSkin(_curCardPoolId, _cardDefine.id);
                _cardSkins.Add(_curCardSkin);
                updateSkinNames();
            }
            await setSkin(skinIndex);
        }
        private async void onChangeSkinName(string skinName)
        {
            markUnsaved();
            var curLanguageGroup = getCurrentIdSkins();
            foreach (var skin in curLanguageGroup)
            {
                skin.skinName = skinName;
            }
            updateSkinNames();
            await setSkin(_curCardSkin);
        }
        private async void onDeleteSkin()
        {
            markUnsaved();
            var skinGroup = getCurrentLangaugeSkins();
            if (skinGroup.Length < 2)
            {
                Debug.LogWarning("你不能删除卡牌的最后一个皮肤");
                return;
            }
            _cardSkins.Remove(_curCardSkin);
            updateSkinNames();
            await resetSkin();
        }
        private async void onChangeLanguage(int languageIndex)
        {
            string language = game.langManager.getLanguages()[languageIndex];
            var targetSkin = getSkin(language, _curCardSkin.skinId);
            if (targetSkin == null)
            {
                markUnsaved();
                //不存在对应语言的皮肤，新建一个
                targetSkin = game.card.createSkinForDiffLanguage(_curCardSkin, language);
                _cardSkins.Add(targetSkin);
            }
            await setSkin(targetSkin);
        }
        /// <summary>
        /// 当用户更改类型
        /// </summary>
        /// <param name="typeIndex"></param>
        private async void onChangeType(int typeIndex)
        {
            markUnsaved();
            _cardDefine.type = _types[typeIndex];
            _ui.setType(typeIndex, _types[typeIndex]);
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
            _curCardSkin.name = name;
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
            _curCardSkin.subname = subName;
            _ui.setSubName(subName);

        }
        /// <summary>
        /// 当需要用户选择图片，根据平台不同调用不同的插件方法，返回选择的贴图。新建的贴图需要缓存在内存中。
        /// </summary>
        async Task<Sprite> onNeedSelectImage()
        {
            Sprite sprite = await game.card.selectExternalImage();
            if (sprite != null)
            {
                markUnsaved();
                _curCardSkin.imagePath = game.card.getSpritePath(sprite);
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
            _curCardSkin.desc = desc;
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
        #region Effects
        private async void onCreateEffect(GeneratedEffect effect)
        {
            markUnsaved();
            var effects = _cardDefine.getEffects().OfType<GeneratedEffect>();
            effect.name = getEffectDefaultName(effect, effects.Count() + 1);
            var effectArray = effects.Append(effect).ToArray();
            _cardDefine.setEffects(effectArray);

            await _effectEditorCtrl.display(_curCardPoolId, effect);
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
                await _effectEditorCtrl.display(_curCardPoolId, effect);
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
        #endregion Effects


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
                   scene.editor.markCardPoolUnsaved(_curCardPoolId);
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
                    game.lang._(CARD_WARNING_OPTION_SAVE),
                    game.lang._(CARD_WARNING_OPTION_DONT_SAVE),
                    game.lang._(CARD_WARNING_OPTION_CANCEL)
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
        private async void onClickSkillRefButton()
        {
            //打开卡牌选择界面
            long poolId = _curCardPoolId;
            var skillRef = _cardDefine.getSkillRef();
            if (skillRef.defineId != 0)
            {
                poolId = skillRef.cardPoolId;
            }
            await displayCardSelectPanel(cardPoolFiltersId: new long[] { poolId }, cardFilter: card => card.isSkill());
            _cardSelectTarget = CardSelectTarget.Skill;
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
        private void onClickCancelCardPoolSelectButton()
        {
            //打开卡牌选择界面
            _ui.displayCardSelectPanel(false);
            _cardSelectTarget = CardSelectTarget.None;
            _curCardSeekingPropName = null;
            _curCardSeekingPropIndexes = null;
            _curCardSeekingPropIsCustom = false;
        }
        private async void onClickCardPoolViewCard(CardDefine cardDefine)
        {
            switch (_cardSelectTarget)
            {
                case CardSelectTarget.Skill:
                    markUnsaved();
                    await setSkillRef(cardDefine.getDefineRef());
                    break;
                case CardSelectTarget.Property:
                    if (_curCardSeekingPropName == null)
                        break;
                    var itemIndexes = _curCardSeekingPropIndexes;
                    var prop = _cardDefine.getProp<object>(_curCardSeekingPropName);
                    var defineRef = cardDefine.getDefineRef();
                    if (prop.replaceListElementByIndexes(itemIndexes, defineRef, out var newProp))
                    {
                        setCardProp(_curCardSeekingPropIsCustom, _curCardSeekingPropName, newProp);
                    }
                    updatePropertyItem(_curCardSeekingPropIsCustom, _curCardSeekingPropName);
                    _curCardSeekingPropName = null;
                    _curCardSeekingPropIndexes = null;
                    _curCardSeekingPropIsCustom = false;
                    markUnsaved();
                    break;
            }
            _ui.displayCardSelectPanel(false);
        }

        private void onConfirmCreateCustomPropertyCallback(PropertyItemInfo item)
        {
            if (string.IsNullOrEmpty(item.propertyName))
            {
                string errorMessage = game.lang._(CARD_CUSTOM_PROP_NAME_EMPTY);
                _ui.showCustomPropertyDialogError(errorMessage);
                return;
            }
            if (isPropertyNameTaken(item.propertyName))
            {
                string errorMessage = game.lang._(CARD_CUSTOM_PROP_NAME_TAKEN);
                _ui.showCustomPropertyDialogError(errorMessage);
                return;
            }

            if (item.value == null)
            {
                item.value = item.type.getDefaultValue();
            }
            setCardProp(true, item.propertyName, item.value);

            markUnsaved();
            updateCustomProperties();
            _ui.displayCustomPropertyDialog(false);
        }
        private void onPropertyValueChangedCallback(bool isCustom, string name, object value)
        {
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
            long poolId = _curCardPoolId;
            if (prop.tryGetListElementByIndexes(itemIndexes, out DefineReference defRef))
            {
                if (defRef.defineId != 0)
                {
                    poolId = defRef.cardPoolId;
                }
            }
            await displayCardSelectPanel(cardPoolFiltersId: new long[] { poolId }, cardFilter: card => true);
            _cardSelectTarget = CardSelectTarget.Property;
            _curCardSeekingPropName = name;
            _curCardSeekingPropIndexes = itemIndexes;
            _curCardSeekingPropIsCustom = isCustom;
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
                        _cardDefine.setProp(name, newValue);
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
        private void onDeleteCustomPropertyCallback(string name)
        {
            _cardDefine.removeProp(name);
            markUnsaved();
            updateCustomProperties();
        }

        #region 特效
        private void onOpenNodeAnimationDialogCallback(Node node)
        {
            var eventNames = game.anims.getNodeVisualEffectEventEditorNames(node);
            var viewData = game.anims.getNodeAnimationViewData(node, _curCardSkin);
            _graphEditor.initNodeAnimationDialogEvents(eventNames);
            _graphEditor.updateAnimationDialog(viewData);
            _graphEditor.displayNodeAnimationDialog(node);
        }
        private void onNodeAnimationPropChangedCallback(Node node, VisualEffectDialogInfo info)
        {
            game.anims.updateNodeAnimRef(node, _curCardSkin, info);
            markUnsaved();
            var eventViewData = game.anims.getNodeAnimationViewData(node, _curCardSkin);
            _graphEditor.updateAnimationDialog(eventViewData);
        }
        #endregion

        #endregion
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
            _ui.setSkillRefVisible(cardDefine.isMaster());
            if (cardDefine.isMaster())
                await setSkillRef(cardDefine.getSkillRef());

            var cardFaceType = CardFaceType.Servant;
            if (cardDefine.isSpell() || cardDefine.isSkill())
                cardFaceType = CardFaceType.Spell;
            else if (cardDefine.isItem())
                cardFaceType = CardFaceType.Item;
            _ui.switchCardFace(cardFaceType);
        }

        private void setCardProp(bool isCustom, string name, object value)
        {
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

        protected override void setUIEffects(string[] names, int activeIndex)
        {
            _ui.setEffects(names, activeIndex);
        }
        /// <summary>
        /// 返回上一个界面
        /// </summary>
        private async Task exit(bool forceSave = false)
        {
            clearUnsaved();
            await scene.editor.closeEditor(forceSave);
        }
        private async Task displayCardSelectPanel(long[] cardPoolFiltersId = null, DefineReference[] characterRefs = null, Predicate<CardDefine> cardFilter = null)
        {
            var cardPoolsRange = getCardPoolReferenceEnv()
                .Where(poolRef => game.card.isLocalCardPoolRefExists(poolRef))
                .Select(r => r.CardPoolID)
                .ToArray();
            await _cardPoolViewPanelCtrl.display(cardPoolsRange, cardPoolFiltersId, characterRefs, cardFilter);
            _ui.displayCardSelectPanel(true);
        }
        #endregion


        private void updateSkinNames()
        {
            CardSkinData[] skins = getCurrentLangaugeSkins();
            if (skins.Length < 1)
            {
                CardSkinData skin = game.card.createSkin(_curCardPoolId, _cardDefine.id);
                skins = new CardSkinData[] { skin };
                _cardSkins.Add(skin);
            }
            _ui.setSkinNames(skins.Select(s => s.name).ToArray());
        }

        #region 设置卡牌皮肤
        private Task resetSkin()
        {
            return setSkin(0, getDefaultSkin());
        }
        private Task setSkin(int index)
        {
            var skin = getCurrentIdSkins()[index];
            return setSkin(index, skin);
        }
        private Task setSkin(CardSkinData skin)
        {
            var index = Array.FindIndex(getCurrentLangaugeSkins(), s => s.skinId == skin.skinId);
            return setSkin(index, skin);
        }
        private async Task setSkin(int index, CardSkinData skin)
        {
            _curCardSkin = skin;
            var curLang = game.langManager.getLanguages();
            _ui.setSkin(index);
            _ui.setLanguage(Array.IndexOf(curLang, skin.language));
            _ui.setName(skin.name);
            _ui.setSubName(skin.subname);
            _ui.setDesc(skin.desc);
            var poolRef = getCurrentCardPoolRef();
            Sprite image = await game.card.loadCardImage(poolRef, skin);
            _ui.setImage(image);
        }
        #endregion

        #region 获取卡牌皮肤
        private CardSkinData getDefaultSkin()
        {
            return _cardSkins.getDefaultSkin(game.langManager.getLanguage());
        }
        private CardSkinData getSkin(string language, int skinId)
        {
            return _cardSkins.FirstOrDefault(s => s.skinId == skinId && s.language == language);
        }
        private CardSkinData[] getCurrentLangaugeSkins()
        {
            return _cardSkins.getCardSkinsOfLanguage(game.langManager.getLanguage());
        }
        private CardSkinData[] getCurrentIdSkins()
        {
            return _cardSkins.getCardSkinsOfId(_curCardSkin.skinId);
        }
        #endregion

        private async Task setSkillRef(DefineReference defRef)
        {
            _cardDefine.setSkillRef(defRef);
            var poolRef = getCardPoolReference(defRef.cardPoolId);
            var skin = game.card.getCardSkin(poolRef, defRef.defineId);
            Sprite spr;
            string name;
            if (skin != null)
            {
                spr = await game.card.loadCardImage(poolRef, skin);
                name = skin.name;
            }
            else
            {
                spr = game.card.getDefaultSprite();
                name = game.lang._(StringTable.BUILD_UNKNOW_CARD_NAME);
            }
            _ui.setSkillRef(spr, name);
        }

        private void saveCard()
        {
            var poolRef = getCurrentCardPoolRef();
            if (game.card.getCardDefine(poolRef, _cardDefine.id) != null)
            {
                game.card.removeCard(_curCardPoolId, _cardDefine.id);
                game.card.removeAllCardSkins(_curCardPoolId, _cardDefine.id);
            }
            game.card.addCard(_curCardPoolId, _cardDefine);
            foreach (var skin in _cardSkins)
            {
                game.card.addCardSkin(_curCardPoolId, skin);
            }

            scene.editor.markCardPoolUnsaved(_curCardPoolId);
            clearUnsaved();
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

        #region 自定义属性
        /// <summary>
        /// 更新所有关键词。
        /// </summary>
        private async void updateKeywords()
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
        /// 更新所有自定义属性。
        /// </summary>
        private async void updateCustomProperties()
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
            if (!isTypeCustomProp(type.type))
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
            if (type.tryGetValueTypeByType(out var t) && isTypeCustomProp(t.type))
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
        private enum CardSelectTarget
        {
            None,
            Skill,
            Property
        }
        CardEditorPanel _ui;
        string[] _types;

        [SerializeField]
        CardPoolViewPanelCtrl _cardPoolViewPanelCtrl;
        [SerializeField]
        Button _cancelCardPoolSelectButton;


        [TranslateMsg("尝试删除卡牌的警告")]
        public const string EDITOR_WARNING_DELETE_CARD = "确认要删除这张卡牌吗？";
        [TranslateMsg("卡牌编辑器/标题")]
        public const string CARD_EDITOR_TITLE = "卡牌编辑器";
        [TranslateMsg("尝试退出一张卡牌的编辑页面，但没有保存情况下的警告")]
        public const string CARD_WARNING_UNSAVED_CHANGES = "是否将更改保存到该卡牌中？";
        [TranslateMsg("尝试退出一张卡牌的编辑页面，但没有保存情况下的按钮名称")]
        public const string CARD_WARNING_OPTION_SAVE = "保存";
        [TranslateMsg("尝试退出一张卡牌的编辑页面，但没有保存情况下的按钮名称")]
        public const string CARD_WARNING_OPTION_DONT_SAVE = "不保存";
        [TranslateMsg("尝试退出一张卡牌的编辑页面，但没有保存情况下的按钮名称")]
        public const string CARD_WARNING_OPTION_CANCEL = "取消";
        [TranslateMsg("尝试添加重复名称的自定义属性的错误提示")]
        public const string CARD_CUSTOM_PROP_NAME_TAKEN = "该属性名已被使用！";
        [TranslateMsg("尝试添加空名称的自定义属性的错误提示")]
        public const string CARD_CUSTOM_PROP_NAME_EMPTY = "请输入属性名！";
        [TranslateMsg("卡牌图片导出成功")]
        public const string CARD_PICTURE_EXPORTED = "卡牌图片导出成功。";
        [TranslateMsg("卡牌图片导出失败")]
        public const string CARD_PICTURE_EXPORT_FAILED = "卡牌图片导出失败。";


        [TranslateMsg("尝试添加重复名称的特效引用的错误提示")]
        public const string ANIM_REF_KEY_TAKEN = "该名称已被使用！";
        [TranslateMsg("尝试添加空名称的特效引用的错误提示")]
        public const string ANIM_REF_KEY_EMPTY = "请输入特效引用名！";

        private CardSelectTarget _cardSelectTarget;
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
        public static readonly ActionNodeValueType[] customPropTypes = new ActionNodeValueType[]
        {
            ActionNodeValueType.integer,
            ActionNodeValueType.boolean,
            ActionNodeValueType.text,
            ActionNodeValueType.cardReference,
            ActionNodeValueType.pileNameCollection,
            ActionNodeValueType.keywords,
        };


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

        [TranslateMsg("卡牌属性名/濒死")]
        public const string CARD_PROP_NAME_ISDEAD = "濒死";
        [TranslateMsg("卡牌属性名/当前生命值")]
        public const string CARD_PROP_NAME_CURRENT_LIFE = "当前生命值";
        [TranslateMsg("卡牌属性名/护甲值")]
        public const string CARD_PROP_NAME_ARMOR = "护甲值";
        [TranslateMsg("卡牌属性名/被封印")]
        public const string CARD_PROP_NAME_SILENCED = "被封印";
        [TranslateMsg("卡牌属性名/已就绪")]
        public const string CARD_PROP_NAME_ISREADY = "已就绪";
        [TranslateMsg("卡牌属性名/攻击次数")]
        public const string CARD_PROP_NAME_ATTACK_TIMES = "攻击次数";
        public static readonly List<CardPropertyNameItem> cardProps = new List<CardPropertyNameItem>
        {
            new CardPropertyNameItem(CARD.ISDEAD, CARD_PROP_NAME_ISDEAD, true, true, false),
            new CardPropertyNameItem(CARD.CURRENT_LIFE, CARD_PROP_NAME_CURRENT_LIFE, true, true, false),
            new CardPropertyNameItem(CARD.ARMOR, CARD_PROP_NAME_ARMOR, true, true, false),
            new CardPropertyNameItem(CARD.SILENCE, CARD_PROP_NAME_SILENCED, true, true, false),
            new CardPropertyNameItem(CARD.ISREADY, CARD_PROP_NAME_ISREADY, true, true, false),
            new CardPropertyNameItem(CARD.ATTACK_TIMES, CARD_PROP_NAME_ATTACK_TIMES, true, true, false),

            // 只读
            new CardPropertyNameItem(DEFINE.ATTACK, StringTable.GAME_CARD_PROP_NAME_ATTACK),
            new CardPropertyNameItem(DEFINE.COST, StringTable.GAME_CARD_PROP_NAME_COST),
            new CardPropertyNameItem(DEFINE.LIFE, StringTable.GAME_CARD_PROP_NAME_LIFE),
            new CardPropertyNameItem(DEFINE.DAMAGE_REDUCE, StringTable.GAME_CARD_PROP_NAME_DAMAGE_REDUCE),
            new CardPropertyNameItem(DEFINE.SPELL_DAMAGE, StringTable.GAME_CARD_PROP_NAME_SPELL_DAMAGE),
        };
        string[] _races;
        string[] _tags;
        /// <summary>
        /// 当前正在选择的卡牌引用属性的属性名称。
        /// </summary>
        string _curCardSeekingPropName;
        /// <summary>
        /// 当前正在选择的卡牌引用属性的列表索引。
        /// </summary>
        int[] _curCardSeekingPropIndexes;
        /// <summary>
        /// 当前正在选择的卡牌引用属性的卡牌属性，是否是自定义属性。
        /// </summary>
        bool _curCardSeekingPropIsCustom;
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
    }
}
