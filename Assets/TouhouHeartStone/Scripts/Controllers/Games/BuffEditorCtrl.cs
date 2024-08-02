using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using Game;
using Game.Animations;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Controllers
{
    class BuffEditorCtrl : CardPoolEditor
    {
        #region 公有方法
        /// <summary>
        /// 打开一个增益进行编辑
        /// </summary>
        public async Task display(GeneratedBuffDefine buffDefine, BuffSkin[] skins)
        {
            // 设置数据。
            _curBuffNameLang = LangManager.SOURCE;
            _curBuffDescLang = LangManager.SOURCE;

            _buffDefine = buffDefine;
            _buffSkins.Clear();
            _buffSkins.AddRange(skins);
            _persistentAnimRefs = game.anims.getAnimationDefinesForBuff().Select(def => new DefineReference(def.cardPoolId, def.id)).ToArray();

            gameObject.SetActive(true); // 会触发Awake
            _ui.reset();

            // 初始化UI
            // 事件触发时机
            _ui.initEventTriggerTimeNames(eventTriggerTimeEditorNames);
            // 修改器属性类型
            _ui.initModifierPropertyTypes(getBuffDefinePropertyTypeNames());
            // 语言
            string[] languages = game.lang.getLanguagesWithSource();
            var languageCodes = languages.Select(l => game.lang.getLanguageShortCode(l)).ToArray();
            _ui.initLanguages(languageCodes);
            // 效果类型
            _ui.setEffectTypes(game.card.getEffectNames());
            // 自定义属性
            var customPropTypes = EditorManager.getCustomPropTypes();
            var customPropTypeNames = game.editor.getCustomPropNames();
            _ui.setCustomPropertyTypes(customPropTypes, customPropTypeNames);
            // UI输入控件
            var keywords = EditorManager.editorKeywords;
            var cardTags = EditorManager.editorCardTags;
            var fieldProps = new ValueInputFieldProperties()
            {
                backText = game.lang._(StringTable.RETURN),

                keywordNames = keywords.Select(k => game.card.getKeywordName(k)).ToArray(),
                keywordValues = keywords.ToArray(),

                cardTagNames = cardTags.Select(t => game.card.getCardTagName(t)).ToArray(),
                cardTagValues = cardTags.ToArray(),

                anyPileName = game.editor.getAnyPileName(),
                pileNames = game.editor.getAllPileNames(),
                pileValues = game.editor.getAllPileValues(),

                buffValues = game.editor.getBuffReferenceRecursions(),
            };
            _ui.initValueFields(fieldProps);

            // 设置UI。
            _ui.setId(buffDefine.id);

            // 更新内容。
            updateSkinNames();
            updateModifiers();
            updateExistLimits();
            await updateCustomProperties();
            await setCurSkinIndex(0);

            // 效果。
            _effectEditorCtrl.hideActionGraph();
            GeneratedEffect[] effects = buffDefine.effectList.OfType<GeneratedEffect>().ToArray();
            if (effects.Length > 0)
            {
                await _effectEditorCtrl.display(effects[0]);
            }
            _effectEditorCtrl.updateEventTriggerTimes();
            updateEffectList(effects);
            _graphEditor.display();
        }
        public override async Task display(CardPoolDisplayPage info)
        {
            if (info == null)
                return;
            displayInfo = info;
            if (info?.displayObj?.buffDefine is GeneratedBuffDefine buffDefine)
            {
                var skins = info.skins as BuffSkin[] ?? new BuffSkin[0];
                await display(buffDefine, skins);
                if (info.unsaved)
                    markUnsaved();
                else
                    clearUnsaved();
            }
        }
        public override void hide()
        {
            base.hide();
            AnimationManager.cleanUnusedVisualEffects(_curBuffSkin, _buffDefine.effectList);
        }
        public override void markUnsaved()
        {
            base.markUnsaved();
            _ui.setTitleText(scene.editor.getTitleText(hasUnsavedChanges, game.lang._(BUFF_EDITOR_TITLE)));
        }
        public override void clearUnsaved()
        {
            base.clearUnsaved();
            _ui.setTitleText(scene.editor.getTitleText(hasUnsavedChanges, game.lang._(BUFF_EDITOR_TITLE)));
        }
        #endregion

        #region 私有方法
        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            if (_ui == null)
            {
                _ui = GetComponentInChildren<BuffEditorPanel>();
            }

            _ui.onChangeSkin += onChangeSkin;
            _ui.onChangeSkinName += onChangeSkinName;
            _ui.onClickDeleteSkinButton += onDeleteSkin;
            _ui.onChangeName += onChangeName;
            _ui.onNeedSelectImage += onNeedSelectImage;
            _ui.onChangeDesc += onChangeDesc;
            _ui.onChangeBuffImage += onChangeBuffImage;
            _ui.onInputFieldLanguageChanged += onInputFieldLangaugeChangedCallback;


            _ui.onEditPersistentAnimClick += onPersistentAnimEditClickCallback;


            _ui.onClickEditEffectButton += onClickEditEffectButton;
            _ui.onClickRemoveEffectButton += onClickRemoveEffectButton;
            _ui.onClickAddEffectMenu += onClickAddEffectMenu;
            _ui.onEffectListItemInputFieldEndEdit += onEffectListItemInputFieldEndEditCallback;
            _ui.onClickDeleteButton += onClickDeleteButton;
            _ui.onClickSaveButton += onClickSaveButton;
            _ui.onClickCloseButton += onClickCloseButton;

            _ui.onAddModifier += onAddModifier;
            _ui.OnRemoveModifier += onRemoveModifier;
            _ui.OnChangeModifierProperty += onChangeModifierProperty;
            _ui.OnChangeModifierCustomPropName += onChangeModifierCustomPropName;
            _ui.OnChangeModifierOperator += onChangeModifierOperator;
            _ui.OnChangeModifierValue += onChangeModifierValue;

            _ui.onAddExistLimit += onAddExistLimitCallback;
            _ui.onRemoveExistLimit += onRemoveExistLimitCallback;
            _ui.onChangeExistLimitEvent += onChangeExistLimitEventCallback;
            _ui.onChangeExistLimitCount += onChangeExistLimitCountCallback;

            _ui.onAddCustomProperty += onAddCustomPropertyCallback;
            _ui.onConfirmCreateCustomProperty += onConfirmCreateCustomPropertyCallback;
            _ui.onRemoveCustomProperty += onRemoveCustomPropertyCallback;
            _ui.onClickCustomPropertyButton += onClickCustomPropertyButtonCallback;
            _ui.onCustomPropertyChanged += onCustomPropertyChangedCallback;

            _effectEditorCtrl.onCreateEffect += onCreateEffect;
            _effectEditorCtrl.OnGraphEdited += onGraphEdited;

            _graphEditor.onOpenNodeAnimationDialog += onOpenNodeAnimationDialogCallback;
        }
        #endregion
        #region 事件回调
        private void onGraphEdited()
        {
            markUnsaved();
        }

        #region 皮肤
        private async void onChangeSkin(int skinIndex)
        {
            markUnsaved();
            if (skinIndex == getSkinCount())
            {
                //新建皮肤
                _curBuffSkin = game.card.createBuffSkin(curCardPoolId, _buffDefine.id);
                _buffSkins.Add(_curBuffSkin);
                updateSkinNames();
            }
            await setCurSkinIndex(skinIndex);
        }
        private async void onChangeSkinName(string skinName)
        {
            markUnsaved();
            var skins = getCurrentBuffSkins();
            int index = skins.indexOf(_curBuffSkin);
            // 暂时没有用
            // game.card.setBuffSkinName(_curBuffSkin, skinName);
            updateSkinNames();
            await setCurSkinIndex(index);
        }
        private async void onDeleteSkin()
        {
            markUnsaved();
            var skins = getCurrentBuffSkins();
            if (skins.Length <= 1)
            {
                Debug.LogWarning("你不能删除增益的最后一个皮肤");
                return;
            }
            game.card.removeBuffSkin(_curBuffSkin);
            _buffSkins.Remove(_curBuffSkin);
            updateSkinNames();
            await setCurSkinIndex(0);
        }
        /// <summary>
        /// 当用户更改增益名
        /// </summary>
        /// <param name="name"></param>
        void onChangeName(string name)
        {
            markUnsaved();
            game.card.setBuffName(getCurrentCardPoolRef(), _curBuffSkin, _curBuffNameLang, name);
            _ui.setName(name);
        }
        /// <summary>
        /// 当需要用户选择图片，根据平台不同调用不同的插件方法，返回选择的贴图。新建的贴图需要缓存在内存中。
        /// </summary>
        async Task<Sprite> onNeedSelectImage()
        {
            return await game.card.selectExternalImage();
        }
        /// <summary>
        /// 当增益图片修改时。
        /// </summary>
        void onChangeBuffImage(Sprite sprite)
        {
            if (sprite != null)
            {
                markUnsaved();
                game.card.setBuffImagePath(getCurrentCardPoolRef(), _curBuffSkin, LangManager.SOURCE, game.card.getResourcePath(sprite));
                _ui.setImage(sprite);
            }
        }
        /// <summary>
        /// 当用户更改增益描述
        /// </summary>
        /// <param name="desc"></param>
        void onChangeDesc(string desc)
        {
            markUnsaved();
            game.card.setBuffDesc(getCurrentCardPoolRef(), _curBuffSkin, _curBuffDescLang, desc);
            _ui.setDesc(desc);
        }
        private void onPersistentAnimEditClickCallback()
        {
            var source = new VFXSelectSourcePersistent(this);
            var eventViewData = source.getAnimationViewData();
            var colorViewData = source.getColorViewDatas();
            _graphEditor.displayAnimationDialog(source);
            _graphEditor.updateAnimationDialog(eventViewData);
            _graphEditor.updateAnimationDialogColors(colorViewData);
        }
        private async void onInputFieldLangaugeChangedCallback(int languageIndex, BuffEditorPanel.BuffInputField field)
        {
            switch (field)
            {
                case BuffEditorPanel.BuffInputField.Name:
                    _curBuffNameLang = game.lang.getLanguagesWithSource()[languageIndex];
                    break;
                case BuffEditorPanel.BuffInputField.Description:
                    _curBuffDescLang = game.lang.getLanguagesWithSource()[languageIndex];
                    break;
            }
            await updateSkin();
        }

        #endregion

        #region 效果
        private async void onCreateEffect(GeneratedEffect effect)
        {
            markUnsaved();
            effect.cardDefineRef = null;
            effect.buffDefineRef = _buffDefine.getDefineRef();

            _buffDefine.effectList.Add(effect);

            await _effectEditorCtrl.display(effect);
            updateEffectList(_buffDefine.effectList.ToArray());
            _graphEditor.updateUndoRedoButton();

            _ui.setEffectScrollValue(1);
        }
        /// <summary>
        /// 当点击编辑效果按钮，打开编辑效果界面。
        /// </summary>
        private async void onClickEditEffectButton(int index)
        {
            var effects = _buffDefine.effectList.OfType<GeneratedEffect>();
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
            if (index < 0 || index >= _buffDefine.effectList.Count)
                return;
            var removedEffect = _buffDefine.effectList[index];
            _buffDefine.effectList.Remove(removedEffect);
            if (_effectEditorCtrl.getCurrentEffect() == removedEffect)
            {
                _effectEditorCtrl.hideActionGraph();
            }
            updateEffectList(_buffDefine.effectList.ToArray());
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
            var effects = _buffDefine.effectList.OfType<GeneratedEffect>();
            GeneratedEffect effect = effects.ElementAt(index);
            effect.name = value;
            _ui.hideEffectListItemInputField(index);
            updateEffectList(effects.ToArray());
            markUnsaved();
        }
        #endregion

        #region 特效
        private void onOpenNodeAnimationDialogCallback(Node node)
        {
            var source = new VFXSelectSourceNode(this, node);
            var eventNames = game.anims.getNodeVisualEffectEventEditorNames(node);
            var eventViewData = source.getAnimationViewData();
            var colorViewData = source.getColorViewDatas();
            _graphEditor.displayAnimationDialog(source);
            _graphEditor.initNodeAnimationDialogEvents(eventNames);
            _graphEditor.updateAnimationDialog(eventViewData);
            _graphEditor.updateAnimationDialogColors(colorViewData);
        }
        #endregion

        #region 修改器
        private void onAddModifier()
        {
            PropModifier propModifier = new CostModifier(0);
            _buffDefine.propModifierList.Add(propModifier);
            updateModifiers();
            markUnsaved();
        }
        private void onRemoveModifier(int modifierIndex)
        {
            _buffDefine.propModifierList.RemoveAt(modifierIndex);
            updateModifiers();
            markUnsaved();
        }
        private void onChangeModifierProperty(int modifierIndex, int propertyIndex)
        {
            _buffDefine.propModifierList[modifierIndex] = createPropertyModifierByIndex(propertyIndex);
            updateModifiers();
            markUnsaved();
        }
        private void onChangeModifierCustomPropName(int modifierIndex, string name)
        {
            PropModifier propModifier = _buffDefine.propModifierList[modifierIndex];
            if (propModifier is CustomPropertyModifier customPropModifier)
            {
                customPropModifier.propertyName = name;
                updateModifiers();
                markUnsaved();
            }
        }
        private void onChangeModifierOperator(int modifierIndex, int operatorIndex)
        {
            setModifyTypeByIndex(_buffDefine.propModifierList[modifierIndex], operatorIndex);
            updateModifiers();
            markUnsaved();
        }
        private void onChangeModifierValue(int modifierIndex, object value)
        {
            PropModifier propModifier = _buffDefine.propModifierList[modifierIndex];
            if (propModifier.relatedPropName != null)
            {
                propModifier.relatedPropName = (string)value;
            }
            else
            {
                if (propModifier is IntPropModifier intPropModifier)
                {
                    intPropModifier.defaultValue = (int)value;
                }
                else if (propModifier is KeywordsModifier keywordsModifier)
                {
                    keywordsModifier.defaultValue = ((KeywordCollection)value).ToArray();
                }
                else if (propModifier is TagModifier tagModifier)
                {
                    tagModifier.defaultValue = ((CardTagCollection)value).ToArray();
                }
                else if (propModifier is CustomPropertyModifier customPropModifier)
                {
                    customPropModifier.defaultValue = value;
                }
            }
            updateModifiers();
            markUnsaved();
        }
        #endregion

        #region 持续限制
        private void onAddExistLimitCallback()
        {
            BuffExistLimitDefine limit = new BuffExistLimitDefine();
            limit.triggerTime = eventTriggerTimes.FirstOrDefault();
            _buffDefine.existLimitList.Add(limit);
            updateExistLimits();
            markUnsaved();
        }
        private void onRemoveExistLimitCallback(int existLimitIndex)
        {
            _buffDefine.existLimitList.RemoveAt(existLimitIndex);
            updateExistLimits();
            markUnsaved();
        }
        private void onChangeExistLimitEventCallback(int existLimitIndex, int eventIndex)
        {
            var limit = _buffDefine.existLimitList[existLimitIndex];
            limit.triggerTime = eventTriggerTimes[eventIndex];
            updateExistLimits();
            markUnsaved();
        }
        private void onChangeExistLimitCountCallback(int existLimitIndex, int count)
        {
            var limit = _buffDefine.existLimitList[existLimitIndex];
            limit.count = count;
            updateExistLimits();
            markUnsaved();
        }
        #endregion

        #region 自定义属性
        private void onAddCustomPropertyCallback()
        {
            _ui.setCustomPropertyDialogVisible(true);
        }
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
            setBuffProp(item.propertyName, value);

            markUnsaved();
            await updateCustomProperties();
            _ui.setCustomPropertyDialogVisible(false);
        }
        private async void onRemoveCustomPropertyCallback(string name)
        {
            _buffDefine.removeProp(name);
            await updateCustomProperties();
            markUnsaved();
        }
        private void onClickCustomPropertyButtonCallback(string name, int[] itemIndexes, ValueInputFieldButton button)
        {
            switch (button)
            {
                case ValueInputFieldButton.CardButton:
                    onClickPropertyCardButtonCallback(name, itemIndexes);
                    break;
                case ValueInputFieldButton.CardEditButton:
                    onClickPropertyCardEditButtonCallback(name, itemIndexes);
                    break;
                case ValueInputFieldButton.BuffEditButton:
                    onClickPropertyBuffEditButtonCallback(name, itemIndexes);
                    break;
                case ValueInputFieldButton.CardPoolButton:
                    onClickPropertyCardPoolButtonCallback(name, itemIndexes);
                    break;
            }
        }
        private async void onClickPropertyCardButtonCallback(string name, int[] itemIndexes)
        {
            var prop = _buffDefine.getProp(name);
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
        private async void onClickPropertyCardPoolButtonCallback(string name, int[] itemIndexes)
        {
            var prop = _buffDefine.getProp(name);
            if (prop == null)
                return;
            //打开卡池选择界面
            var source = new CardPoolSelectSourceCustomProperty(this, getCardPoolReferenceEnv())
            {
                title = game.lang._(EditorCtrl.EDITOR_TITLE_SELECT_CARD_POOL),
                propName = name,
                propIndexes = itemIndexes
            };
            await scene.displayCardPoolSingleSelectPanel(source);
        }
        private async void onClickPropertyCardEditButtonCallback(string name, int[] itemIndexes)
        {
            var prop = _buffDefine.getProp(name);
            if (prop.tryGetListElementByIndexes(itemIndexes, out DefineReference defRef))
            {
                await scene.editor.openReferedCard(defRef, setPropAction);

                async void setPropAction(CardDefine cardDefine)
                {
                    var newDefRef = new DefineReference(cardDefine.cardPoolId, cardDefine.id);
                    if (prop.replaceListElementByIndexes(itemIndexes, newDefRef, out var newValue))
                    {
                        _buffDefine.setProp(name, newValue);
                        markUnsaved();
                        await updatePropertyItem(name);
                    }
                }
            }
        }
        private async void onClickPropertyBuffEditButtonCallback(string name, int[] itemIndexes)
        {
            var prop = _buffDefine.getProp(name);
            if (prop == null)
                return;
            if (prop.tryGetListElementByIndexes(itemIndexes, out DefineReference defRef))
            {
                // 不能跳转到自己
                if (defRef.cardPoolId == _buffDefine.cardPoolId && defRef.defineId == _buffDefine.id)
                    return;
                var poolRef = getCardPoolReference(defRef.cardPoolId);
                if (game.card.tryGetBuffDefine(poolRef, defRef.defineId, out GeneratedBuffDefine buffDefine))
                {
                    await scene.editor.openBuffEditor(buffDefine);
                }
            }
        }
        private async void onCustomPropertyChangedCallback(string name, object value)
        {
            value = value.cloneValue();
            setBuffProp(name, value);
            markUnsaved();
            await updatePropertyItem(name);
        }
        #endregion
        private void onClickDeleteButton()
        {
            //询问玩家
            game.ui.dialog.ShowQuestion(
               game.langManager.getString(StringTable.WARNING),
               game.langManager.getString(EDITOR_WARNING_DELETE_BUFF),
               async () =>
               {
                   //删除当前增益
                   game.card.removeBuffDefine(curCardPoolId, _buffDefine.id);
                   game.card.removeAllBuffSkins(curCardPoolId, _buffDefine.id);
                   scene.editor.markCardPoolUnsaved(curCardPoolId);
                   scene.editor.removeDisplayPageOfObject(_buffDefine);
                   await exit(true);
               });
        }
        private void onClickSaveButton()
        {
            saveBuff();
        }
        private async void onClickCloseButton()
        {
            if (hasUnsavedChanges)
            {
                string title = game.lang._(StringTable.WARNING);

                string desc = game.lang._(BUFF_WARNING_UNSAVED_CHANGES);
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
                        saveBuff();
                        await exit();
                    }
                    else if (index == 1)
                    {
                        scene.editor.removeDisplayPageOfObject(_buffDefine);
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

        protected override async void onCardSelected(ICardSelectSource source, CardDefine cardDefine)
        {
            base.onCardSelected(source, cardDefine);
            switch (source)
            {
                case CardSelectSourceCustomProperty customProp:
                    var propName = customProp.propName;
                    var itemIndexes = customProp.propIndexes;
                    var prop = _buffDefine.getProp(propName);
                    var defineRef = cardDefine.getDefineRef();
                    if (prop.replaceListElementByIndexes(itemIndexes, defineRef, out var newProp))
                    {
                        setBuffProp(propName, newProp);
                    }
                    await updatePropertyItem(propName);
                    markUnsaved();
                    break;
            }
        }
        protected override void setUIEffects(string[] names, int activeIndex)
        {
            _ui.setEffects(names, activeIndex);
        }

        private void updateSkinNames()
        {
            BuffSkin[] skins = getCurrentBuffSkins();
            if (skins.Length < 1)
            {
                BuffSkin skin = game.card.createBuffSkin(curCardPoolId, _buffDefine.id);
                _buffSkins.Add(skin);
                skins = new BuffSkin[] { skin };
            }
            var poolRef = getCurrentCardPoolRef();
            _ui.setSkinNames(skins.Select(g => game.card.getBuffName(poolRef, g)).ToArray());
        }
        private async Task setCurSkinIndex(int skinIndex)
        {
            var skins = getCurrentBuffSkins();
            //获取对应语言的皮肤
            var skin = skins[skinIndex];

            _curBuffSkin = skin;

            var poolRef = getCurrentCardPoolRef();
            _ui.setName(game.card.getLocalizedBuffName(poolRef, skin, _curBuffNameLang));
            _ui.setDesc(game.card.getLocalizedBuffDesc(poolRef, skin, _curBuffDescLang));
            _ui.setSkin(skinIndex);
            Sprite image = await game.card.loadBuffImage(poolRef, skin);
            _ui.setImage(image);
            updatePersistentAnim();

        }
        private Task updateSkin()
        {
            return setSkin(_curBuffSkin);
        }
        private Task setSkin(BuffSkin skin)
        {
            var skins = getCurrentBuffSkins();
            var skinIndex = Array.IndexOf(skins, skin);
            return setSkin(skinIndex, skin);
        }
        private Task setSkin(int skinIndex)
        {
            var skins = getCurrentBuffSkins();
            var skin = skins[skinIndex];
            return setSkin(skinIndex, skin);
        }
        private async Task setSkin(int skinIndex, BuffSkin skin)
        {
            _curBuffSkin = skin;
            var poolRef = getCurrentCardPoolRef();
            _ui.setName(game.card.getLocalizedBuffName(poolRef, skin, _curBuffNameLang));
            _ui.setDesc(game.card.getLocalizedBuffDesc(poolRef, skin, _curBuffDescLang));
            _ui.setSkin(skinIndex);
            Sprite image = await game.card.loadBuffImage(poolRef, skin);
            _ui.setImage(image);
            updatePersistentAnim();
        }
        private int getSkinCount()
        {
            return getCurrentBuffSkins().Length;
        }
        private BuffSkin[] getCurrentBuffSkins()
        {
            return _buffSkins.ToArray();
        }
        private async Task exit(bool forceSave = false)
        {
            //返回上一个界面
            clearUnsaved();
            await scene.editor.closeEditor(forceSave);
        }
        #region 修改器
        private void updateModifiers()
        {
            BuffModifierInfo[] infos = new BuffModifierInfo[_buffDefine.propModifierList.Count];
            for (int i = 0; i < infos.Length; i++)
            {
                var propModifier = _buffDefine.propModifierList[i];
                int typeIndex = getPropertyModifierIndex(propModifier);
                var customPropertyName = getPropertyModifierCustomPropName(propModifier);
                var modifyTypeNames = getBuffDefineModifyTypeNames(typeIndex);
                var modifyTypeIndex = getModifyTypeIndex(propModifier);
                var valueType = getPropModifierValueType(propModifier);
                var value = getPropModifierValue(propModifier);
                infos[i] = new BuffModifierInfo()
                {
                    propertyTypeIndex = typeIndex,
                    isCustomProperty = propModifier is CustomPropertyModifier,
                    customPropertyName = customPropertyName,
                    modifyTypes = modifyTypeNames,
                    modifyTypeIndex = modifyTypeIndex,
                    valueType = valueType,
                    value = value,
                };
            }
            _ui.updateModifiers(infos);
        }
        private string[] getBuffDefinePropertyTypeNames()
        {
            return Array.ConvertAll(propertyTypeNames, s => game.lang._p(StringTable.CONTEXT_CARD_PROPERTY, s));
        }
        private string[] getBuffDefineModifyTypeNames(int typeIndex)
        {
            if (typeIndex == MODIFY_TYPE_KEYWORDS || typeIndex == MODIFY_TYPE_TAG) // Keywords.
                return Array.ConvertAll(stringArrayModifierTypeNames, s => game.langManager.getString(s));
            else
                return Array.ConvertAll(intModifierTypeNames, s => game.langManager.getString(s));
        }
        private int getModifyTypeIndex(PropModifier propModifier)
        {
            if (propModifier is StringArrayModifier stringArrayModifier)
            {
                if (stringArrayModifier.modifyType == StringArrayModifier.ModifyType.set)
                {
                    if (string.IsNullOrWhiteSpace(propModifier.relatedPropName))
                        return 2;
                    else
                        return 5;
                }
                else if (stringArrayModifier.modifyType == StringArrayModifier.ModifyType.remove)
                {
                    if (string.IsNullOrWhiteSpace(propModifier.relatedPropName))
                        return 1;
                    else
                        return 4;
                }
                else if (stringArrayModifier.modifyType == StringArrayModifier.ModifyType.add)
                {
                    if (string.IsNullOrWhiteSpace(propModifier.relatedPropName))
                        return 0;
                    else
                        return 3;
                }
            }
            else if (propModifier is IntPropModifier intPropModifier)
            {
                if (propModifier.relatedPropName != null && intPropModifier.isSet)
                    return 3;
                else if (propModifier.relatedPropName != null)
                    return 2;
                else if (intPropModifier.isSet)
                    return 1;
                else
                    return 0;
            }
            else if (propModifier is CustomPropertyModifier customPropModifier)
            {
                if (customPropModifier.relatedPropName != null && customPropModifier.isSet)
                    return 3;
                else if (customPropModifier.relatedPropName != null)
                    return 2;
                else if (customPropModifier.isSet)
                    return 1;
                else
                    return 0;
            }
            throw new NotImplementedException();
        }
        private void setModifyTypeByIndex(PropModifier propModifier, int index)
        {
            if (propModifier is StringArrayModifier stringArrayModifier)
            {
                if (index == STRING_ARRAY_OPERATOR_SET_PROP)
                {
                    propModifier.relatedPropName = game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_BUFF_PROP_NAME_DEFAULT);
                    stringArrayModifier.modifyType = KeywordsModifier.ModifyType.set;
                }
                else if (index == STRING_ARRAY_OPERATOR_REMOVE_PROP)
                {
                    propModifier.relatedPropName = game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_BUFF_PROP_NAME_DEFAULT);
                    stringArrayModifier.modifyType = KeywordsModifier.ModifyType.remove;
                }
                else if (index == STRING_ARRAY_OPERATOR_ADD_PROP)
                {
                    propModifier.relatedPropName = game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_BUFF_PROP_NAME_DEFAULT);
                    stringArrayModifier.modifyType = KeywordsModifier.ModifyType.add;
                }
                else if (index == STRING_ARRAY_OPERATOR_SET)
                {
                    propModifier.relatedPropName = null;
                    stringArrayModifier.modifyType = KeywordsModifier.ModifyType.set;
                }
                else if (index == STRING_ARRAY_OPERATOR_REMOVE)
                {
                    propModifier.relatedPropName = null;
                    stringArrayModifier.modifyType = KeywordsModifier.ModifyType.remove;
                }
                else
                {
                    propModifier.relatedPropName = null;
                    stringArrayModifier.modifyType = KeywordsModifier.ModifyType.add;
                }
            }
            else if (propModifier is IntPropModifier intPropModifier)
            {
                if (index == INT_OPERATOR_SET_PROP)
                {
                    propModifier.relatedPropName = game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_BUFF_PROP_NAME_DEFAULT);
                    intPropModifier.isSet = true;
                }
                else if (index == INT_OPERATOR_ADD_PROP)
                {
                    propModifier.relatedPropName = game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_BUFF_PROP_NAME_DEFAULT);
                    intPropModifier.isSet = false;
                }
                else if (index == INT_OPERATOR_SET)
                {
                    propModifier.relatedPropName = null;
                    intPropModifier.isSet = true;
                }
                else
                {
                    propModifier.relatedPropName = null;
                    intPropModifier.isSet = false;
                }
            }
            else if (propModifier is CustomPropertyModifier customPropModifier)
            {
                if (index == INT_OPERATOR_SET_PROP)
                {
                    propModifier.relatedPropName = game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_BUFF_PROP_NAME_DEFAULT);
                    customPropModifier.isSet = true;
                }
                else if (index == INT_OPERATOR_ADD_PROP)
                {
                    propModifier.relatedPropName = game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_BUFF_PROP_NAME_DEFAULT);
                    customPropModifier.isSet = false;
                }
                else if (index == INT_OPERATOR_SET)
                {
                    propModifier.relatedPropName = null;
                    customPropModifier.isSet = true;
                }
                else
                {
                    propModifier.relatedPropName = null;
                    customPropModifier.isSet = false;
                }
            }
        }
        private ActionNodeValueType getPropModifierValueType(PropModifier propModifier)
        {
            if (propModifier.relatedPropName != null)
            {
                return ActionNodeValueType.text;
            }
            else
            {
                if (propModifier is CostModifier)
                    return ActionNodeValueType.integer;
                else if (propModifier is AttackModifier)
                    return ActionNodeValueType.integer;
                else if (propModifier is LifeModifier)
                    return ActionNodeValueType.integer;
                else if (propModifier is DamageReduceModifier)
                    return ActionNodeValueType.integer;
                else if (propModifier is TagModifier)
                    return ActionNodeValueType.cardTagCollection;
                else if (propModifier is KeywordsModifier)
                    return ActionNodeValueType.keywordCollection;
                else if (propModifier is SpellDamageModifier)
                    return ActionNodeValueType.integer;
                else if (propModifier is CustomPropertyModifier)
                    return ActionNodeValueType.integer;
                throw new NotImplementedException();
            }
        }
        private object getPropModifierValue(PropModifier propModifier)
        {
            if (!string.IsNullOrWhiteSpace(propModifier.relatedPropName))
                return propModifier.relatedPropName;
            else if (propModifier is TagModifier tagModifier)
                return new CardTagCollection(tagModifier.defaultValue);
            else if (propModifier is KeywordsModifier keywordsModifier)
                return new KeywordCollection(keywordsModifier.defaultValue);
            else
                return propModifier.getDefaultValue();
        }
        private PropModifier createPropertyModifierByIndex(int index)
        {
            if (index == MODIFY_TYPE_CUSTOM_PROPERTY)
                return new CustomPropertyModifier(string.Empty, 0);
            if (index == MODIFY_TYPE_TAG)
                return new TagModifier(TagModifier.ModifyType.add);
            if (index == MODIFY_TYPE_SPELL_DAMAGE)
                return new SpellDamageModifier(0);
            if (index == MODIFY_TYPE_KEYWORDS)
                return new KeywordsModifier(KeywordsModifier.ModifyType.add);
            if (index == MODIFY_TYPE_DAMAGE_REDUCE)
                return new DamageReduceModifier(0);
            if (index == MODIFY_TYPE_LIFE)
                return new LifeModifier(0);
            if (index == MODIFY_TYPE_ATTACK)
                return new AttackModifier(0);
            return new CostModifier(0);
        }
        private int getPropertyModifierIndex(PropModifier propModifier)
        {
            if (propModifier is CustomPropertyModifier)
                return MODIFY_TYPE_CUSTOM_PROPERTY;
            if (propModifier is TagModifier)
                return MODIFY_TYPE_TAG;
            if (propModifier is SpellDamageModifier)
                return MODIFY_TYPE_SPELL_DAMAGE;
            if (propModifier is KeywordsModifier)
                return MODIFY_TYPE_KEYWORDS;
            if (propModifier is DamageReduceModifier)
                return MODIFY_TYPE_DAMAGE_REDUCE;
            if (propModifier is LifeModifier)
                return MODIFY_TYPE_LIFE;
            if (propModifier is AttackModifier)
                return MODIFY_TYPE_ATTACK;
            return MODIFY_TYPE_COST;
        }
        private string getPropertyModifierCustomPropName(PropModifier propModifier)
        {
            if (propModifier is CustomPropertyModifier customModifier)
                return customModifier.propertyName;
            return string.Empty;
        }
        #endregion

        #region 持续限制
        private void updateExistLimits()
        {
            validateExistLimits();
            BuffExistLimitInfo[] infos = new BuffExistLimitInfo[_buffDefine.existLimitList.Count];
            for (int i = 0; i < infos.Length; i++)
            {
                var existLimit = _buffDefine.existLimitList[i];
                var header = game.lang._(BUFF_EXISTLIMIT_HEADER, i + 1);
                var eventIndex = getExistLimitEventIndex(existLimit);
                var count = getExistLimitCount(existLimit);
                infos[i] = new BuffExistLimitInfo()
                {
                    header = header,
                    eventIndex = eventIndex,
                    count = count
                };
            }
            _ui.updateExistLimits(infos);
        }
        private void validateExistLimits()
        {
            foreach (var limit in _buffDefine.existLimitList)
            {
                if (!eventTriggerTimes.Any(t => limit.triggerTime == t))
                {
                    limit.triggerTime = eventTriggerTimes.FirstOrDefault();
                }
            }
        }
        private int getExistLimitEventIndex(BuffExistLimitDefine existLimit)
        {
            if (existLimit == null)
                return -1;
            return Array.IndexOf(eventTriggerTimes, existLimit.triggerTime);
        }

        private int getExistLimitCount(BuffExistLimitDefine existLimit)
        {
            if (existLimit == null)
                return 0;
            return existLimit.count;
        }
        #endregion

        #region 自定义属性
        private async Task updateCustomProperties()
        {
            List<PropertyItemInfo> infos = new List<PropertyItemInfo>();
            var propNames = _buffDefine.getPropNames();
            foreach (var propName in propNames)
            {
                var prop = _buffDefine.getProp(propName);
                Type type = prop.GetType();
                var valueType = type.getNodeValueType();
                infos.Add(new PropertyItemInfo(propName, valueType)
                {
                    value = prop
                });
            }
            _ui.updateCustomProperties(infos.ToArray());

            var env = getCardPoolReferenceEnv();
            var fields = _ui.getCustomPropertyItemFields();
            for (int i = 0; i < infos.Count; i++)
            {
                var field = fields[i];
                object value = infos[i].value;
                await field.valueInputField.setInputFieldByValue(value, env, game);
            }
        }
        /// <summary>
        /// 更新一项属性。
        /// </summary>
        /// <param name="name">属性名称。</param>
        private async Task updatePropertyItem(string name)
        {
            PropertyItemInfo itemInfo;
            PropertyItemField propertyItemField;
            itemInfo = getPropertyInfo(name);
            propertyItemField = _ui.getCustomPropertyItemField(name);
            propertyItemField.updateItem(itemInfo);
            await propertyItemField.valueInputField.setInputFieldByValue(itemInfo.value, getCardPoolReferenceEnv(), game);
        }
        /// <summary>
        /// 属性名是否已经被使用。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool isPropertyNameTaken(string name)
        {
            if (_buffDefine.hasProp(name) && _buffDefine.getProp(name) != null)
                return true;
            return false;
        }
        private PropertyItemInfo getPropertyInfo(string name)
        {
            var value = _buffDefine.getProp(name);
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
        private void setBuffProp(string name, object value)
        {
            value = game.editor.validatePropertyValue(value, updateEditorBuffs);
            _buffDefine.setProp(name, value);
        }


        #endregion

        #region 自定义属性的增益相关
        /// <summary>
        /// 更新所有增益引用，并重新设置UI的增益定义的选项。
        /// </summary>
        private void updateEditorBuffs()
        {
            game.editor.updateBuffDefines();
            var fieldProps = game.editor.getBuffInputProperties();
            _ui.initValueFields(fieldProps);
        }
        #endregion
        private async void setCustomPropertyCardPool(string propName, int[] itemIndexes, CardPoolReference poolRef)
        {
            var prop = _buffDefine.getProp(propName);
            if (prop.replaceListElementByIndexes(itemIndexes, new CardPoolIDProxy(poolRef.cardPoolId), out var newProp))
            {
                setBuffProp(propName, newProp);
            }
            markUnsaved();
            await updatePropertyItem(propName);
        }
        private void saveBuff()
        {
            game.card.removeBuffDefine(curCardPoolId, _buffDefine.id);
            game.card.addBuffDefine(curCardPoolId, _buffDefine);

            game.card.removeAllBuffSkins(curCardPoolId, _buffDefine.id);
            foreach (var skin in _buffSkins)
            {
                game.card.addBuffSkin(curCardPoolId, skin);
            }

            scene.editor.markCardPoolUnsaved(curCardPoolId);
            clearUnsaved();
        }
        private void setPersistentAnimIndex(int index)
        {
            var defines = game.anims.getAnimationDefinesForBuff();
            var animRef = _curBuffSkin.persistentAnimRef;
            if (index < 0)
            {
                animRef.animRef = new DefineReference(0, 0);
            }
            else
            {
                var define = defines[index];
                animRef.animRef = new DefineReference(define.cardPoolId, define.id);
            }
            updatePersistentAnim();
        }
        private void updatePersistentAnim()
        {
            var animNames = game.anims.getAnimationEditorNamesForBuff();
            var animRef = _curBuffSkin.persistentAnimRef?.animRef;
            var animIndex = animRef != null ? Array.IndexOf(_persistentAnimRefs, animRef) + 1 : 0;
            _ui.setPersistentAnim(animNames[animIndex]);
        }
        #endregion

        #region 属性字段
        BuffEditorPanel _ui;

        [TranslateMsg("尝试删除增益的警告")]
        public const string EDITOR_WARNING_DELETE_BUFF = "确认要删除这个增益吗？";
        [TranslateMsg("增益编辑器/标题")]
        public const string BUFF_EDITOR_TITLE = "增益编辑器";
        [TranslateMsg("尝试退出一个增益的编辑页面，但没有保存情况下的警告")]
        public const string BUFF_WARNING_UNSAVED_CHANGES = "是否将更改保存到该增益中？";

        [TranslateMsg("增益编辑器/持续限制/标题（{0}为序号）")]
        public const string BUFF_EXISTLIMIT_HEADER = "限制{0}";

        public const int MODIFY_TYPE_COST = 0;
        public const int MODIFY_TYPE_ATTACK = 1;
        public const int MODIFY_TYPE_LIFE = 2;
        public const int MODIFY_TYPE_DAMAGE_REDUCE = 3;
        public const int MODIFY_TYPE_SPELL_DAMAGE = 4;
        public const int MODIFY_TYPE_KEYWORDS = 5;
        public const int MODIFY_TYPE_TAG = 6;
        public const int MODIFY_TYPE_CUSTOM_PROPERTY = 7;

        public const int INT_OPERATOR_ADD = 0;
        public const int INT_OPERATOR_SET = 1;
        public const int INT_OPERATOR_ADD_PROP = 2;
        public const int INT_OPERATOR_SET_PROP = 3;

        public const int STRING_ARRAY_OPERATOR_ADD = 0;
        public const int STRING_ARRAY_OPERATOR_REMOVE = 1;
        public const int STRING_ARRAY_OPERATOR_SET = 2;
        public const int STRING_ARRAY_OPERATOR_ADD_PROP = 3;
        public const int STRING_ARRAY_OPERATOR_REMOVE_PROP = 4;
        public const int STRING_ARRAY_OPERATOR_SET_PROP = 5;

        public static string[] propertyTypeNames = new string[]
        {
            StringTable.CARD_PROP_NAME_COST,
            StringTable.CARD_PROP_NAME_ATTACK,
            StringTable.CARD_PROP_NAME_LIFE,
            StringTable.CARD_PROP_NAME_DAMAGE_REDUCE,
            StringTable.CARD_PROP_NAME_SPELL_DAMAGE,
            StringTable.CARD_PROP_NAME_KEYWORDS,
            StringTable.CARD_PROP_NAME_CARD_TAG,
            StringTable.CARD_PROP_NAME_CUSTOM_PROPERTY,
        };
        public static string[] intModifierTypeNames = new string[]
        {
            StringTable.GAME_CARD_PROP_MODIFY_ADD,
            StringTable.GAME_CARD_PROP_MODIFY_SETTO,
            StringTable.GAME_CARD_PROP_MODIFY_ADD_PROP,
            StringTable.GAME_CARD_PROP_MODIFY_SETTO_PROP
        };
        public static string[] stringArrayModifierTypeNames = new string[]
        {
            StringTable.GAME_CARD_PROP_MODIFY_ADD,
            StringTable.GAME_CARD_PROP_MODIFY_SUB,
            StringTable.GAME_CARD_PROP_MODIFY_SETTO,
            StringTable.GAME_CARD_PROP_MODIFY_ADD_PROP,
            StringTable.GAME_CARD_PROP_MODIFY_SUB_PROP,
            StringTable.GAME_CARD_PROP_MODIFY_SETTO_PROP
        };
        /// <summary>
        /// 正在编辑的增益定义。
        /// </summary>
        GeneratedBuffDefine _buffDefine;
        /// <summary>
        /// 正在编辑的增益定义的皮肤。
        /// </summary>
        BuffSkin _curBuffSkin;
        /// <summary>
        /// 正在编辑的增益定义的所有皮肤。
        /// </summary>
        List<BuffSkin> _buffSkins = new List<BuffSkin>();
        string _curBuffNameLang;
        string _curBuffDescLang;
        DefineReference[] _persistentAnimRefs;
        private long curCardPoolId => game.editor.currentCardPoolId;
        private EventTriggerTime[] eventTriggerTimes => scene.editor.eventTriggerTimes;
        private string[] eventTriggerTimeEditorNames => scene.editor.eventTriggerTimeEditorNames;
        #endregion

        #region 内嵌类
        public class CardPoolSelectSourceCustomProperty : CardPoolSelectSource
        {
            public CardPoolSelectSourceCustomProperty(BuffEditorCtrl editor, IEnumerable<ICardPoolReference> poolRef) : base(poolRef)
            {
                _editor = editor;
            }
            public override bool Confirm()
            {
                _editor.setCustomPropertyCardPool(propName, propIndexes, _selectedCardPools.FirstOrDefault()?.ToReference());
                return true;
            }
            /// <summary>
            /// 当前正在选择的卡牌引用属性的属性名称。
            /// </summary>
            public string propName;
            /// <summary>
            /// 当前正在选择的卡牌引用属性的列表索引。
            /// </summary>
            public int[] propIndexes;
            private BuffEditorCtrl _editor;
        }
        private class VFXSelectSourceNode : VFXSelectSource
        {
            public VFXSelectSourceNode(BuffEditorCtrl editor, Node node)
            {
                editorCtrl = editor;
                this.node = node;
            }
            public override void changeProp(VisualEffectDialogInfo info)
            {
                game.anims.updateNodeAnimRef(node, cardSkin, info);
                editorCtrl.markUnsaved();
            }
            public override VisualEffectDialogViewData getAnimationViewData()
            {
                return game.anims.getNodeAnimationViewData(node, cardSkin);
            }
            public override ColorModifyViewData[] getColorViewDatas()
            {
                return game.anims.getAnimationColorViewDataForNode(node, cardSkin);
            }
            public override void resetColors()
            {
                game.anims.resetNodeAnimColor(node, cardSkin);
                editorCtrl.markUnsaved();
            }
            public override Color getAnimationColor(int index)
            {
                return game.anims.getAnimationColorForNode(node, cardSkin, index);
            }
            public override void setAnimationColor(int index, Color color)
            {
                game.anims.setNodeAnimColor(node, cardSkin, index, color);
                editorCtrl.markUnsaved();
            }
            public override VFXPreviewData getPreviewData()
            {
                var animRef = node.getNodeAnimationRef(cardSkin);
                if (animRef == null)
                    return default;
                return new VFXPreviewData()
                {
                    animDefine = game.anims.findAnimationDefineForNode(animRef.animRef),
                    colorModifies = animRef.getColorModifies(),
                    sourcePosition = animRef.sourcePosition,
                    targetPosition = animRef.targetPosition,
                };
            }
            public override string[] getAnimationNames()
            {
                return game.anims.getAnimationEditorNamesForNode();
            }
            private MainManager game => editorCtrl.game;
            private BuffSkin cardSkin => editorCtrl._curBuffSkin;
            private BuffEditorCtrl editorCtrl;
            private Node node;
        }
        private class VFXSelectSourcePersistent : VFXSelectSource
        {
            public VFXSelectSourcePersistent(BuffEditorCtrl editor)
            {
                editorCtrl = editor;
            }
            public override void changeProp(VisualEffectDialogInfo info)
            {
                editorCtrl.setPersistentAnimIndex(info.animIndex);
                editorCtrl.markUnsaved();
            }
            public override VisualEffectDialogViewData getAnimationViewData()
            {
                var defines = game.anims.getAnimationDefinesForBuff();
                var animRef = buffSkin.persistentAnimRef;
                var animIndex = Array.FindIndex(defines, def => def.getDefineRef() == animRef?.animRef);
                return new VisualEffectDialogViewData()
                {
                    animIndex = animIndex + 1,
                    hideFlags = VFXDialogHideFlags.Event | VFXDialogHideFlags.SourcePosition | VFXDialogHideFlags.TargetPosition | VFXDialogHideFlags.SourcePositionVarName | VFXDialogHideFlags.TargetPositionVarName
                };
            }
            public override ColorModifyViewData[] getColorViewDatas()
            {
                if (buffSkin?.persistentAnimRef?.animRef == null)
                    return Array.Empty<ColorModifyViewData>();
                var define = game.anims.findAnimationDefineForBuff(buffSkin.persistentAnimRef.animRef);
                return game.anims.getAnimationColorViewData(define, buffSkin.persistentAnimRef.colorModifies);
            }
            public override void resetColors()
            {
                game.anims.resetAnimColor(buffSkin.persistentAnimRef);
                editorCtrl.markUnsaved();
            }
            public override Color getAnimationColor(int index)
            {
                if (buffSkin?.persistentAnimRef?.animRef == null)
                    return Color.red;
                var define = game.anims.findAnimationDefineForBuff(buffSkin.persistentAnimRef.animRef);
                return game.anims.getAnimationColor(define, buffSkin.persistentAnimRef.colorModifies, index);
            }
            public override void setAnimationColor(int index, Color color)
            {
                game.anims.setAnimationColor(buffSkin.persistentAnimRef, index, color);
                editorCtrl.markUnsaved();
            }
            public override string[] getAnimationNames()
            {
                return game.anims.getAnimationEditorNamesForBuff();
            }
            public override VFXPreviewData getPreviewData()
            {
                var animRef = buffSkin.persistentAnimRef;
                if (animRef == null)
                    return default;
                return new VFXPreviewData()
                {
                    animDefine = game.anims.findAnimationDefineForBuff(animRef.animRef),
                    colorModifies = animRef.getColorModifies(),
                    sourcePosition = VisualEffectPosition.THIS_CARD,
                    targetPosition = VisualEffectPosition.THIS_CARD,
                };
            }
            private MainManager game => editorCtrl.game;
            private BuffSkin buffSkin => editorCtrl._curBuffSkin;
            private BuffEditorCtrl editorCtrl;
        }
        #endregion
    }
}
