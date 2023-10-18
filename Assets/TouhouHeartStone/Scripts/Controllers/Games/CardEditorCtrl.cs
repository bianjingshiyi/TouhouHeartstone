using System.Collections.Generic;
using TouhouCardEngine;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UI;
using BJSYGameCore;
using System;
using System.Linq;
using Game;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using MukioI18n;

namespace Controllers
{
    class CardEditorCtrl : CardPoolEditor, IController<MainCtrl>
    {
        #region 公有方法
        /// <summary>
        /// 打开一张卡片进行编辑
        /// </summary>
        public async Task display(long cardPoolId, CardDefine cardDefine, CardSkinData[] skins)
        {
            gameObject.SetActive(true);
            _ui.reset();
            var selectableCharacters = getSelectableCharacters();
            _selectableCharacters = selectableCharacters;
            _cardPoolId = cardPoolId;
            _cardDefine = cardDefine;
            _cardSkins.Clear();
            _cardSkins.AddRange(skins);
            updateSkinNames();
            _ui.setId(cardDefine.id);
            _ui.setType(Array.IndexOf(_types, cardDefine.type), cardDefine.type);
            _ui.setIsToken(cardDefine.isToken());
            bool[] tagsValue = new bool[_tags.Length];
            for (int i = 0; i < _tags.Length; i++)
            {
                tagsValue[i] = cardDefine.getTags().Contains(_tags[i]);
            }
            _ui.setTags(cardDefine.getTags());
            string[] charactersName = new string[selectableCharacters.Length + 1];
            charactersName[0] = StringTable.CARDVIEW_CHARACTER_NEUTRAL;
            for (int i = 1; i < charactersName.Length; i++)
            {
                charactersName[i] = game.card.getCardSkin(selectableCharacters[i - 1]).name;
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
            if (cardDefine.isSpell())
                _ui.switchToSpell();
            else if (cardDefine.isItem())
                _ui.switchToItem();
            else if (cardDefine.isMaster())
            {
                _ui.switchToMaster();
                await setSkillRef(cardDefine.getSkillRef());
            }
            else
                _ui.switchToServant();
            await setCurSkinIndex(0);
            _textChangedByType();

            updateCustomProperties();
            updateKeywords();
            _effectEditorCtrl.updateEventTriggerTimes(_cardPoolId);
            _effectEditorCtrl.hideActionGraph();
            if (effects.Length > 0)
            {
                await _effectEditorCtrl.display(_cardPoolId, _cardDefine, effects[0]);
            }
            updateEffectList(effects);

            _graphEditorPanelCtrl.display(_cardPoolId);
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
            cleanUnusedVisualEffects();
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
                Patchouli.METAL,
                Patchouli.WOOD,
                Patchouli.WATER,
                Patchouli.FIRE,
                Patchouli.EARTH
            };
            _ui.onChangeCharacter += onChangeCharacter;
            _ui.onChangeCost += onChangeCost;
            _ui.onChangeName += onChangeName;
            _ui.onChangeSubName += onChangeSubName;
            _ui.onNeedSelectImage += onNeedSelectImage;
            _ui.onChangeDesc += onChangeDesc;
            _ui.onChangeAttack += onChangeAttack;
            _ui.onChangeLife += onChangeLife;
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

            _ui.onClickDeleteCardButton += onClickDeleteCardButton;
            _ui.onClickSaveButton += onClickSaveButton;
            _ui.onClickCloseButton += onClickCloseButton;
            _ui.onClickSkillRefButton += onClickSkillRefButton;
            _ui.onSkillRefEditButtonClicked += onSkillRefEditButtonClicked;
            _ui.onChangeTag += onChangeTag;

            _ui.onClickCardPageButton += onClickCardPageButton;
            _ui.onClickPropertyPageButton += onClickPropertyPageButton;
            _ui.onClickNodesPageButton += onClickNodesPageButton;

            _cardPoolViewPanelCtrl.onClickCard += onClickCardPoolViewCard;
            _cancelCardPoolSelectButton.onClick.AddListener(onClickCancelCardPoolSelectButton);

            // Card Properties.
            var customPropTypes = CardEditorCtrl.customPropTypes.Select(a => (a.type, game.lang._(a.name))).ToArray();
            _ui.setCustomPropertyTypes(customPropTypes);
            _ui.onConfirmCreateCustomProperty += onConfirmCreateCustomPropertyCallback;
            _ui.onDeleteCustomProperty += onDeleteCustomPropertyCallback;

            _ui.onPropertyValueChanged += onPropertyValueChangedCallback;
            _ui.onClickPropertyButton += onClickPropertyButtonCallback;
            _ui.onClickPropertyCardButton += onClickPropertyCardButtonCallback;
            _ui.onClickPropertyCardEditButton += onClickPropertyCardEditButtonCallback;
            _ui.onPropertyItemValueChanged += onPropertyItemValueChangedCallback;
            _ui.onClickPropertyItemButton += onClickPropertyItemButtonCallback;
            _ui.onClickPropertyItemCardButton += onClickPropertyItemCardButtonCallback;
            _ui.onClickPropertyItemCardEditButton += onClickPropertyItemCardEditButtonCallback;
            _ui.onClickPropertyAddItemButton += onClickPropertyAddItemButtonCallback;
            _ui.onClickPropertyRemoveItemButton += onClickPropertyRemoveItemButtonCallback;

            _effectEditorCtrl.onCreateEffect += onCreateEffect;
            _effectEditorCtrl.OnGraphEdited += onGraphEdited;

            _graphEditorPanelCtrl.onOpenNodeAnimationDialog += onOpenNodeAnimationDialogCallback;
            _graphEditorPanelCtrl.onAnimationValueChanged += onNodeAnimationValueChangedCallback;
            _graphEditorPanelCtrl.onAnimationPropChanged += onNodeAnimationPropChangedCallback;
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
            if (skinIndex == getSkinCount())
            {
                //新建皮肤
                _curCardSkin = game.card.createSkin(_cardPoolId, _cardDefine.id);
                _cardSkins.Add(_curCardSkin);
                updateSkinNames();
            }
            await setCurSkinIndex(skinIndex);
        }
        private async void onChangeSkinName(string skinName)
        {
            markUnsaved();
            var skinGroup = getCurrentCardSkins().GroupBy(c => c.skinId).ToList();
            int index = skinGroup.FindIndex(g => g.Key == _curCardSkin.skinId);
            var curSkinGroup = skinGroup[index];
            foreach (var skin in curSkinGroup)
            {
                skin.skinName = skinName;
            }
            updateSkinNames();
            await setCurSkinIndex(index);
        }
        private async void onDeleteSkin()
        {
            markUnsaved();
            var skinGroup = getCurrentCardSkins().GroupBy(c => c.skinId).ToList();
            if (skinGroup.Count < 2)
            {
                Debug.LogWarning("你不能删除卡牌的最后一个皮肤");
                return;
            }
            _cardSkins.Remove(_curCardSkin);
            updateSkinNames();
            await setCurSkinIndex(0);
        }
        private async void onChangeLanguage(int languageIndex)
        {
            string language = game.langManager.getLanguages()[languageIndex];
            var skinGroup = getCurrentCardSkins().GroupBy(s => s.skinId).First(g => g.Key == _curCardSkin.skinId);
            var targetSkin = skinGroup.FirstOrDefault(s => s.language == language);
            if (targetSkin == null)
            {
                markUnsaved();
                //不存在对应语言的皮肤，新建一个
                targetSkin = game.card.createSkinForDiffLanguage(_cardPoolId, _curCardSkin, language);
                _cardSkins.Add(targetSkin);
            }
            _curCardSkin = targetSkin;
            _ui.setLanguage(languageIndex);
            _ui.setName(targetSkin.name);
            _ui.setSubName(targetSkin.subname);
            _ui.setDesc(targetSkin.desc);
            string path = targetSkin.imagePath;
            Sprite image = await game.card.loadImage(targetSkin.imagePath, _cardDefine.cardPoolId);
            _ui.setImage(image);
        }
        /// <summary>
        /// 当用户更改类型
        /// </summary>
        /// <param name="typeIndex"></param>
        private void onChangeType(int typeIndex)
        {
            markUnsaved();
            _cardDefine.type = _types[typeIndex];
            _ui.setType(typeIndex, _types[typeIndex]);
            _textChangedByType();
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
        void onChangeName(string name)
        {
            markUnsaved();
            _curCardSkin.name = name;
            _ui.setName(name);
            _textChangedByType();
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
            _ui.setRace(index, value.Length > 0 ? value[0]: string.Empty);
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

        #region Effects
        private void onCreateEffect(GeneratedEffect effect)
        {
            markUnsaved();
            var effects = _cardDefine.getEffects().OfType<GeneratedEffect>();
            effect.name = getEffectDefaultName(effect, effects.Count() + 1);
            var effectArray = effects.Append(effect).ToArray();
            _cardDefine.setEffects(effectArray);

            _effectEditorCtrl.display(_cardPoolId, _cardDefine, effect);
            _graphEditorPanelCtrl.updateUndoRedoButton();
            updateEffectList(effectArray);
            
            _ui.setEffectScrollValue(1);
        }
        /// <summary>
        /// 当点击编辑效果按钮，打开编辑效果界面。
        /// </summary>

        private void onClickEditEffectButton(int index)
        {
            var effects = _cardDefine.getEffects().OfType<GeneratedEffect>();
            GeneratedEffect effect = effects.ElementAt(index);
            if (_effectEditorCtrl.getCurrentEffect() != effect)
            {
                _effectEditorCtrl.display(_cardPoolId, _cardDefine, effect);
                updateEffectList(effects.ToArray());
                _graphEditorPanelCtrl.updateUndoRedoButton();
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
                   //返回上个界面
                   await scene.editor.closeEditorPanel();
                   scene.editor.hasUnsavedChanges = true;
                   clearUnsaved();
               });
        }
        private void onClickSaveButton()
        {
            saveCard();
        }
        private void onClickCloseButton()
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
                game.ui.dialog.ShowOptions(title, desc, optNames, (index) =>
                {
                    if (index == 0)
                    {
                        saveCard();
                        exit();
                    }
                    else if(index == 1)
                    {
                        scene.editor.removeDisplayPageOfObject(_cardDefine);
                        exit();
                    }
                });
            }
            else
            {
                exit();
            }
        }
        private async void onClickSkillRefButton()
        {
            //打开卡牌选择界面
            long poolId = _cardPoolId;
            var skillRef = _cardDefine.getSkillRef();
            if (skillRef.defineId != 0)
            {
                poolId = skillRef.cardPoolId;
            }
            await _cardPoolViewPanelCtrl.display(cardPoolFiltersId: new long[] { poolId }, cardFilter: (card) =>
            {
                return card.isSkill();
            });
            _ui.displayCardSelectPanel(true);
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
        private void onClickCardPageButton()
        {
            //打开卡牌界面
            _ui.displayCardPage();
            _ui.setType(Array.IndexOf(_types, _cardDefine.type), _cardDefine.type);
        }
        private void onClickPropertyPageButton()
        {
            //打开属性界面
            _ui.displayPropertyPage();
        }
        private void onClickNodesPageButton()
        {
            //打开节点界面
            _ui.displayNodesPage();
        }
        private void onClickCancelCardPoolSelectButton()
        {
            //打开卡牌选择界面
            _ui.displayCardSelectPanel(false);
            _cardSelectTarget = CardSelectTarget.None;
            _curCardSeekingPropName = null;
            _curCardSeekingPropIndex = -1;
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
                    if (_curCardSeekingPropIndex >= 0)
                    {
                        var array = _cardDefine.getProp<DefineReference[]>(_curCardSeekingPropName);
                        var defRef = cardDefine.getDefineRef();
                        if (array == null || _curCardSeekingPropIndex >= array.Length)
                        {
                            var newArray = new DefineReference[_curCardSeekingPropIndex + 1];
                            array.CopyTo(newArray, 0);
                            array = newArray;
                            setCardProp(_curCardSeekingPropIsCustom, _curCardSeekingPropName, array);
                        }
                        array[_curCardSeekingPropIndex] = defRef;
                    }
                    else
                    {
                        setCardProp(_curCardSeekingPropIsCustom, _curCardSeekingPropName, cardDefine.getDefineRef());
                    }
                    updatePropertyItem(_curCardSeekingPropIsCustom, _curCardSeekingPropName);
                    _curCardSeekingPropName = null;
                    _curCardSeekingPropIndex = -1;
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
                item.value = getDefaultValue(item.type, item.isArray);
            }
            setCardProp(true, item.propertyName, item.value);

            markUnsaved();
            updateCustomProperties();
            _ui.displayCustomPropertyDialog(false);
        }
        private void onPropertyItemValueChangedCallback(bool isCustom, string name, int index, object value)
        {
            var prop = _cardDefine.getProp<object>(name);
            if (prop is Array array)
            {
                array.SetValue(value, index);
                markUnsaved();
                updatePropertyItem(isCustom, name);
            }
        }
        private void onClickPropertyItemButtonCallback(bool isCustom, string name, int index)
        {
        }
        private async void onClickPropertyItemCardButtonCallback(bool isCustom, string name, int index)
        {
            var prop = _cardDefine.getProp<DefineReference[]>(name);
            if (prop == null)
                return;
            //打开卡牌选择界面
            long poolId = _cardPoolId;
            var define = prop?[index];
            if (define != null && define.defineId != 0)
            {
                poolId = define.cardPoolId;
            }
            await _cardPoolViewPanelCtrl.display(cardPoolFiltersId: new long[] { poolId }, cardFilter: card => true);
            _ui.displayCardSelectPanel(true);
            _cardSelectTarget = CardSelectTarget.Property;
            _curCardSeekingPropName = name;
            _curCardSeekingPropIndex = index;
            _curCardSeekingPropIsCustom = isCustom;
        }
        private async void onClickPropertyItemCardEditButtonCallback(bool isCustom, string name, int index)
        {
            var prop = _cardDefine.getProp<DefineReference[]>(name);
            if (prop == null || index < 0 || index >= prop.Length)
                return;
            var define = prop[index];
            await scene.editor.openReferedCard(define, setPropAction);

            void setPropAction(CardDefine cardDefine)
            {
                prop[index] = new DefineReference(cardDefine.cardPoolId, cardDefine.id);
                _cardDefine.setProp(name, prop);
                markUnsaved();
                updatePropertyItem(isCustom, name);
            }
        }
        private void onClickPropertyAddItemButtonCallback(bool isCustom, string name)
        {
            var array = _cardDefine.getProp<Array>(name);
            if (array == null)
                return;
            var arrayType = array.GetType();
            var elementType = arrayType.GetElementType();
            if (elementType == null)
                return;
            var newArray = Array.CreateInstance(elementType, array.Length + 1);
            array.CopyTo(newArray, 0);


            if (customPropTypeDict.TryGetValue(elementType, out var key))
            {
                newArray.SetValue(getDefaultValue(key, false), newArray.Length - 1);
            }
            setCardProp(isCustom, name, newArray);
            markUnsaved();
            updatePropertyItem(isCustom, name);
        }

        private void onClickPropertyRemoveItemButtonCallback(bool isCustom, string name, int index)
        {
            var array = _cardDefine.getProp<Array>(name);
            if (array == null)
                return;
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
            setCardProp(isCustom, name, newArray);
            markUnsaved();
            updatePropertyItem(isCustom, name);
        }
        private void onPropertyValueChangedCallback(bool isCustom, string name, object value)
        {
            var prop = _cardDefine.getProp<object>(name);
            if (prop is Array array)
            {
                array.SetValue(value, 0);
            }
            else
            {
                setCardProp(isCustom, name, value);
            }
            markUnsaved();
            updatePropertyItem(isCustom, name);
        }
        private void onClickPropertyButtonCallback(bool isCustom, string name)
        {
        }
        private async void onClickPropertyCardButtonCallback(bool isCustom, string name)
        {
            var prop = _cardDefine.getProp<DefineReference>(name);
            if (prop == null)
                return;
            //打开卡牌选择界面
            long poolId = _cardPoolId;
            if (prop.defineId != 0)
            {
                poolId = prop.cardPoolId;
            }
            await _cardPoolViewPanelCtrl.display(cardPoolFiltersId: new long[] { poolId }, cardFilter: card => true);
            _ui.displayCardSelectPanel(true);
            _cardSelectTarget = CardSelectTarget.Property;
            _curCardSeekingPropName = name;
            _curCardSeekingPropIndex = -1;
            _curCardSeekingPropIsCustom = isCustom;
        }
        private async void onClickPropertyCardEditButtonCallback(bool isCustom, string name)
        {
            var prop = _cardDefine.getProp<DefineReference>(name);
            await scene.editor.openReferedCard(prop, setPropAction);

            void setPropAction(CardDefine cardDefine)
            {
                _cardDefine.setProp(name, new DefineReference(cardDefine.cardPoolId, cardDefine.id));
                markUnsaved();
                updatePropertyItem(isCustom, name);
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
            var defines = game.anims.getAllAnimationDefines();
            int animIndex = -1;
            var key = node.getConst<string>(SkinAnimationRef.NODE_CONST_KEY);
            SkinAnimationRef animRef = null;
            if (!string.IsNullOrEmpty(key))
            {
                animRef = _curCardSkin.getAnimRef(key);
                if (animRef != null)
                {
                    var define = defines.FirstOrDefault(d => d.cardPoolId == animRef.animRef.cardPoolId && d.id == animRef.animRef.defineId);
                    animIndex = Array.IndexOf(defines, define);
                }
            }
            _graphEditorPanelCtrl.setNodeAnimationDialog(animIndex, new VisualEffectDialogInfo(animRef));
            _graphEditorPanelCtrl.displayNodeAnimationDialog(node);
        }
        private void onNodeAnimationValueChangedCallback(Node node, int index)
        {
            var defines = game.anims.getAllAnimationDefines();
            var animRef = getOrInitNodeAnimationRef(node);
            if (index < 0)
            {
                animRef.animRef = new DefineReference(0 ,0);
            }
            else
            {
                var define = defines[index];
                animRef.animRef = new DefineReference(define.cardPoolId, define.id);
            }
            markUnsaved();
        }
        private void onNodeAnimationPropChangedCallback(Node node, VisualEffectDialogInfo info)
        {
            var animRef = getOrInitNodeAnimationRef(node);
            info.update(animRef);
            markUnsaved();
        }
        #endregion

        #endregion
        private SkinAnimationRef getOrInitNodeAnimationRef(Node node)
        {
            var key = node.getConst<string>(SkinAnimationRef.NODE_CONST_KEY);
            if (string.IsNullOrEmpty(key))
            {
                var animRefs = _curCardSkin.getAllAnimRefs();
                key = $"AnimRef{animRefs.Length}_{node.id}";
                node.setConst(SkinAnimationRef.NODE_CONST_KEY, key);
            }
            var animRef = _curCardSkin.getAnimRef(key);
            if (animRef == null)
            {
                var defines = game.anims.getAllAnimationDefines();
                var defaultDefine = defines.FirstOrDefault();
                var defRef = new DefineReference(defaultDefine?.cardPoolId ?? 0, defaultDefine?.id ?? 1);
                animRef = new SkinAnimationRef(key, defRef);
                _curCardSkin.addAnimRef(animRef);
            }
            return animRef;
        }
        private CardDefine[] getSelectableCharacters()
        {
            List<CardDefine> cardList = new List<CardDefine>();
            cardList.AddRange(game.card.GetCardDefines(c => c.isMaster(), true));
            return cardList.ToArray();
        }
        private void _textChangedByType()
        {
            if (_cardDefine.type == DEFINE.SPELL || _cardDefine.type == DEFINE.ITEM || _cardDefine.type == DEFINE.SKILL)
            {
                _ui.EditNameButton.transform.position = _ui.card.spellCardFace.nameText.transform.position;
                _ui.NameInputField.transform.position = _ui.card.spellCardFace.nameText.transform.position;
                _ui.NameInputField.textComponent.alignment = TextAnchor.MiddleCenter;
            }

            else if (_cardDefine.type == DEFINE.SERVANT || _cardDefine.type == DEFINE.MASTER)
            {
                _ui.EditNameButton.transform.position = _ui.card.servantCardFace.nameText.transform.position;
                _ui.NameInputField.transform.position = _ui.card.servantCardFace.nameText.transform.position;
                _ui.NameInputField.textComponent.alignment = TextAnchor.MiddleCenter;
            }
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
        private async void exit()
        {
            clearUnsaved();
            await scene.editor.closeEditorPanel();
        }
        #endregion


        private void cleanUnusedVisualEffects()
        {
            var allAnimRefs = _curCardSkin.getAllAnimRefs();
            foreach (var animRef in allAnimRefs)
            {
                var key = animRef.key;
                var keys = _cardDefine.getGeneratedEffects().SelectMany(e => e.graph.nodes).Select(n => n.getConst<string>(SkinAnimationRef.NODE_CONST_KEY));
                var hasKey = keys.Contains(key);
                if (!hasKey)
                {
                    _curCardSkin.removeAnimRef(key);
                }
            }
        }
        private void updateSkinNames()
        {
            CardSkinData[] skins = getCurrentCardSkins();
            if (skins.Length < 1)
            {
                CardSkinData skin = game.card.createSkin(_cardPoolId, _cardDefine.id);
                skins = new CardSkinData[] { skin };
                _cardSkins.Add(skin);
            }
            _ui.setSkinNames(skins.GroupBy(s => s.skinId)
                .Select(g => g.FirstOrDefault(s => s.language == game.langManager.getLanguage()) is CardSkinData s2 ? s2.skinName : g.First().skinName)
                .ToArray());
        }
        private async Task setCurSkinIndex(int skinIndex)
        {
            var skinGroup = getCurrentCardSkins().GroupBy(s => s.skinId).ToArray().ElementAt(skinIndex);
            //获取对应语言的皮肤
            var skin = skinGroup.FirstOrDefault(s => s.language == game.langManager.getLanguage());
            //如果没有对应语言，就第一个
            if (skin == null)
                skin = skinGroup.First();
            _curCardSkin = skin;
            _ui.setSkin(skinIndex);
            _ui.setLanguage(Array.IndexOf(game.langManager.getLanguages(), skin.language));
            _ui.setName(skin.name);
            _ui.setSubName(skin.subname);
            _ui.setDesc(skin.desc);
            Sprite image = await game.card.loadImage(skin.imagePath, _cardDefine.cardPoolId);
            _ui.setImage(image);
        }
        private int getSkinCount()
        {
            return getCurrentCardSkins().GroupBy(s => s.skinId).Count();
        }
        private CardSkinData[] getCurrentCardSkins()
        {
            return _cardSkins.ToArray();
        }

        private async Task setSkillRef(DefineReference defRef)
        {
            _cardDefine.setSkillRef(defRef);
            var poolRef = game.card.getLocalCardPoolReference(defRef.cardPoolId);
            var cardDefine = game.card.getCardDefine(poolRef, defRef.defineId);
            var skin = game.card.getCardSkin(poolRef, defRef.defineId);
            Sprite spr;
            string name;
            if (skin != null)
            {
                spr = await game.card.loadImage(skin.imagePath, defRef.cardPoolId);
                name = skin.name;
            }
            else
            {
                spr = game.card.getDefaultSprite();
                name = game.lang._(StringTable.BUILD_UNKNOW_CARD_NAME);
            }
            _ui.setSkillRef(cardDefine, spr, name);
        }

        private void saveCard()
        {
            if (game.card.getCardDefine(_cardPoolId, _cardDefine.id) != null)
            {
                game.card.removeCard(_cardPoolId, _cardDefine.id);
                game.card.removeAllCardSkins(_cardPoolId, _cardDefine.id);
            }
            game.card.addCard(_cardPoolId, _cardDefine);
            foreach (var skin in _cardSkins)
            {
                game.card.addCardSkin(_cardPoolId, skin);
            }

            scene.editor.hasUnsavedChanges = true;
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

        public static (string name, string value)[] getKeywords()
        {
            return keywordInfos.Select(p => (p.Value.displayName, p.Key)).ToArray();
        }
        #region 自定义属性
        /// <summary>
        /// 更新所有关键词。
        /// </summary>
        private void updateKeywords()
        {
            var item = GetCardDefineKeywordItems();
            _ui.updateKeywords(item, game.card);
        }
        /// <summary>
        /// 更新所有自定义属性。
        /// </summary>
        private void updateCustomProperties()
        {
            var items = GetCardDefineCustomProps();
            _ui.updateCustomProperties(items, game.card);
        }
        /// <summary>
        /// 更新一项属性。
        /// </summary>
        /// <param name="isCustom">要更新的属性是否是自定义属性？</param>
        /// <param name="name">属性名称。</param>
        private void updatePropertyItem(bool isCustom, string name)
        {
            if (isCustom)
            {
                var item = GetCardDefineCustomProperty(name);
                _ui.updateCustomPropertyItem(name, item, game.card);
            }
            else
            {
                var item = GetCardDefineKeywordItem(name);
                _ui.updateKeywordItem(name, item, game.card);
            }
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
            bool isArray = valueType.IsArray;
            if (isArray)
            {
                valueType = valueType.GetElementType();
            }
            // 防止出现什么Array类型的参数，GetElementType会返回null。
            if (valueType == null)
                return null;

            if (!customPropTypeDict.TryGetValue(valueType, out ActionNodeValueType type))
                return null;

            var item = new PropertyItemInfo(name, type, isArray)
            {
                value = value
            };
            return item;
        }

        private PropertyItemInfo[] GetCardDefineKeywordItems()
        {
            List<PropertyItemInfo> items = new List<PropertyItemInfo>();
            foreach (var pair in keywordInfos)
            {
                var item = GetCardDefineKeywordItem(pair.Key);
                if (item == null)
                    continue;
                items.Add(item);
            }
            return items.ToArray();
        }
        private PropertyItemInfo GetCardDefineKeywordItem(string name)
        {
            if (!keywordInfos.TryGetValue(name, out var info))
                return null;
            var type = info.type;
            bool isArray = false;
            if (info.type.IsArray)
            {
                type = info.type.GetElementType();
                isArray = true;
            }
            ActionNodeValueType valueType = ActionNodeValueType.boolean;
            if (customPropTypeDict.TryGetValue(type, out var t))
            {
                valueType = t;
            }

            object value = keywordActions.TryGetValue(name, out var valueAction) ?
                valueAction.Item2(_cardDefine) :
                _cardDefine.hasKeyword(name);

            var item = new PropertyItemInfo(name, game.lang._(info.displayName), valueType, isArray)
            {
                value = value
            };
            return item;
        }
        
        private object getDefaultValue(ActionNodeValueType valueType, bool isArray)
        {
            if (!customPropDefaultValues.TryGetValue(valueType, out var func))
                return null;
            var value = func();
            if (!isArray)
            {
                // 不是数组
                return value;
            }

            // 数组
            Type type = null;
            foreach (var pair in customPropTypeDict)
            {
                if (pair.Value == valueType)
                {
                    type = pair.Key;
                    break;
                }
            }
            if (type == null)
                return null;
            var array = Array.CreateInstance(type, 1);
            array.SetValue(value, 0);
            return array;
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

        [Obsolete]
        public IAppManager app { get; }
        [Obsolete]
        public MainCtrl main { get; }
        CardEditorPanel _ui;
        string[] _types;

        [SerializeField]
        CardPoolViewPanelCtrl _cardPoolViewPanelCtrl;
        [SerializeField]
        Button _cancelCardPoolSelectButton;
        [SerializeField]
        GraphEditorPanelCtrl _graphEditorPanelCtrl;


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


        [TranslateMsg("尝试添加重复名称的特效引用的错误提示")]
        public const string ANIM_REF_KEY_TAKEN = "该名称已被使用！";
        [TranslateMsg("尝试添加空名称的特效引用的错误提示")]
        public const string ANIM_REF_KEY_EMPTY = "请输入特效引用名！";

        private CardSelectTarget _cardSelectTarget;

        private static readonly Dictionary<string, (string displayName, Type type)> keywordInfos = new Dictionary<string, (string displayName, Type type)>
        {
            { KEYWORD.TAUNT, (StringTable.GAME_KEYWORD_TAUNT, typeof(bool)) },
            { KEYWORD.CHARGE, (StringTable.GAME_KEYWORD_CHARGE, typeof(bool)) },
            { KEYWORD.RUSH, (StringTable.GAME_KEYWORD_RUSH, typeof(bool)) },
            { KEYWORD.SHIELD, (StringTable.GAME_KEYWORD_SHIELD, typeof(bool)) },
            { KEYWORD.STEALTH, (StringTable.GAME_KEYWORD_STEALTH, typeof(bool)) },
            { KEYWORD.PERMANENTSTEALTH, (StringTable.GAME_KEYWORD_PERMANENT_STEALTH, typeof(bool)) },
            { KEYWORD.DRAIN, (StringTable.GAME_KEYWORD_DRAIN, typeof(bool)) },
            { KEYWORD.POISONOUS, (StringTable.GAME_KEYWORD_POISONOUS, typeof(bool)) },
            { KEYWORD.ELUSIVE, (StringTable.GAME_KEYWORD_ELUSIVE, typeof(bool)) },
            { KEYWORD.FROZEN, (StringTable.GAME_KEYWORD_FREEZE, typeof(bool)) },
            { KEYWORD.UNIQUE, (StringTable.GAME_KEYWORD_UNIQUE, typeof(bool)) },
            { KEYWORD.AUTOCAST, (StringTable.GAME_KEYWORD_AUTOCAST, typeof(bool)) },
            { KEYWORD.CANTATTACK, (StringTable.GAME_KEYWORD_CANTATTACK, typeof(bool)) },
            { KEYWORD.WINDFURY, (StringTable.GAME_KEYWORD_WINDFURY, typeof(bool)) },
            { KEYWORD.IMMUNITY, (StringTable.GAME_KEYWORD_IMMUNITY, typeof(bool)) },
            { KEYWORD.MISSION, (StringTable.GAME_KEYWORD_MISSION, typeof(bool)) },
            { KEYWORD.PERMANENT_FROZEN, (StringTable.GAME_KEYWORD_PERMANENT_FROZEN, typeof(bool)) },
            { KEYWORD.CAN_BE_CHARGED, (StringTable.GAME_KEYWORD_CAN_BE_CHARGED, typeof(bool)) },
            { DEFINE.SPELL_DAMAGE, (StringTable.GAME_CARD_PROP_NAME_SPELL_DAMAGE, typeof(int)) },
            { DEFINE.DAMAGE_REDUCE, (StringTable.GAME_CARD_PROP_NAME_DAMAGE_REDUCE, typeof(int)) }
        };
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

        public static readonly Dictionary<string, string> piles = new Dictionary<string, string>
        {
            { PILE.FIELD, StringTable.GAME_FIELD },
            { PILE.HAND, StringTable.GAME_HAND },
            { PILE.DECK, StringTable.GAME_DECK },
            { PILE.GRAVE, StringTable.GAME_GRAVE},
            { PILE.MASTER, StringTable.GAME_MASTER },
            { PILE.SKILL, StringTable.GAME_SKILL },
            { PILE.ITEM, StringTable.GAME_ITEM },
            { PILE.HEAD, StringTable.GAME_DELAY }
        };
        public static readonly (ActionNodeValueType type, string name)[] customPropTypes = 
            new (ActionNodeValueType type, string name)[]
        {
            (ActionNodeValueType.integer, StringTable.EDITOR_TYPE_NAME_INT),
            (ActionNodeValueType.boolean, StringTable.EDITOR_TYPE_NAME_BOOL),
            (ActionNodeValueType.text, StringTable.EDITOR_TYPE_NAME_STRING),
            (ActionNodeValueType.cardReference, StringTable.EDITOR_TYPE_NAME_CARD_REFERENCE),
            (ActionNodeValueType.pileNameCollection, StringTable.EDITOR_TYPE_NAME_PILE),
            (ActionNodeValueType.keywords, StringTable.EDITOR_TYPE_NAME_KEYWORDS_TYPE),
        };
        public static readonly Dictionary<Type, ActionNodeValueType> customPropTypeDict = new Dictionary<Type, ActionNodeValueType>()
        {
            { typeof(int), ActionNodeValueType.integer },
            { typeof(bool), ActionNodeValueType.boolean },
            { typeof(string), ActionNodeValueType.text },
            { typeof(DefineReference), ActionNodeValueType.cardReference },
            { typeof(PileNameCollection), ActionNodeValueType.pileNameCollection },
            { typeof(KeywordCollection), ActionNodeValueType.keywords },
        };
        public static readonly Dictionary<ActionNodeValueType, Func<object>> customPropDefaultValues = new Dictionary<ActionNodeValueType, Func<object>>()
        {
            { ActionNodeValueType.integer, () => 0},
            { ActionNodeValueType.boolean, () => false },
            { ActionNodeValueType.text, () => string.Empty },
            { ActionNodeValueType.cardReference, () => new DefineReference() },
            { ActionNodeValueType.pileNameCollection, () => new PileNameCollection() },
            { ActionNodeValueType.keywords, () => new KeywordCollection() },
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
        long _cardPoolId;
        /// <summary>
        /// 当前正在选择的卡牌引用属性的属性名称。
        /// </summary>
        string _curCardSeekingPropName;
        /// <summary>
        /// 当前正在选择的卡牌引用属性的列表索引。
        /// </summary>
        int _curCardSeekingPropIndex;
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
