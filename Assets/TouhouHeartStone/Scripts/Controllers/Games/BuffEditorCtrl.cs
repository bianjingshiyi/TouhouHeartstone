using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using Game;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{
    class BuffEditorCtrl : CardPoolEditor
    {
        #region 公有方法
        /// <summary>
        /// 打开一个增益进行编辑
        /// </summary>
        public async Task display(long cardPoolId, GeneratedBuffDefine buffDefine, BuffSkin[] skins)
        {
            gameObject.SetActive(true);
            _ui.reset();
            _curCardPoolId = cardPoolId;
            _buffDefine = buffDefine;
            _buffSkins.Clear();
            _buffSkins.AddRange(skins);

            _curLanguage = game.langManager.getLanguage();
            string[] languages = game.langManager.getLanguages();
            _ui.setLanguage(languages.indexOf(_curLanguage));

            var persistentAnimNames = game.anims.getAnimationEditorNamesForBuff();
            _ui.initPersistentAnims(persistentAnimNames);

            _persistentAnimRefs = game.anims.getAnimationDefinesForBuff().Select(def => new DefineReference(def.cardPoolId, def.id)).ToArray();

            updateSkinNames();
            _ui.setId(buffDefine.id);
            GeneratedEffect[] effects = buffDefine.effectList.OfType<GeneratedEffect>().ToArray();
            updateModifiers();
            // 持续限制
            _ui.initExistLimits(eventTriggerTimeEditorNames);
            updateExistLimits();
            // 皮肤
            await setCurSkinIndex(0);

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
            if (info?.displayObj?.buffDefine is GeneratedBuffDefine buffDefine)
            {
                var skins = info.skins as BuffSkin[] ?? new BuffSkin[0];
                await display(info.cardPoolId, buffDefine, skins);
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
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            if (_ui == null)
            {
                _ui = GetComponentInChildren<BuffEditorPanel>();
            }
            var keywords = EditorManager.editorKeywords;
            var keywordNames = keywords.Select(k => game.card.getKeywordName(k)).ToArray();
            _ui.init(getBuffDefinePropertyTypes(), keywordNames, keywords.ToArray());
            _ui.onChangeSkin += onChangeSkin;
            _ui.onChangeSkinName += onChangeSkinName;
            _ui.onClickDeleteSkinButton += onDeleteSkin;
            _ui.onChangeName += onChangeName;
            _ui.onNeedSelectImage += onNeedSelectImage;
            _ui.onChangeDesc += onChangeDesc;
            _ui.onChangeBuffImage += onChangeBuffImage;

            string[] languages = game.langManager.getLanguages();
            for (int i = 0; i < languages.Length; i++)
            {
                languages[i] = game.langManager.getString(game.ui.getTranslateNameForLanguage(languages[i]));
            }
            _ui.initLanguages(languages);
            _ui.onChangeLanguage += onChangeLanguage;
            _ui.onChangePersistentAnim += onChangePersistentAnimCallback;


            _ui.onClickEditEffectButton += onClickEditEffectButton;
            _ui.onClickRemoveEffectButton += onClickRemoveEffectButton;
            _ui.setEffectTypes(game.card.getEffectNames());
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
        #region 皮肤
        private async void onChangeSkin(int skinIndex)
        {
            markUnsaved();
            if (skinIndex == getSkinCount())
            {
                //新建皮肤
                _curBuffSkin = game.card.createBuffSkin(_curCardPoolId, _buffDefine.id, _curLanguage);
                _buffSkins.Add(_curBuffSkin);
                updateSkinNames();
            }
            await setCurSkinIndex(skinIndex);
        }
        private async void onChangeSkinName(string skinName)
        {
            markUnsaved();
            var skinGroup = getCurrentLangSkins();
            int index = skinGroup.indexOf(_curBuffSkin);
            _curBuffSkin.name = skinName;
            updateSkinNames();
            await setCurSkinIndex(index);
        }
        private async void onDeleteSkin()
        {
            markUnsaved();
            var skinGroup = getCurrentLangSkins();
            if (skinGroup.Count() <= 1)
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
            _curBuffSkin.name = name;
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
                _curBuffSkin.imagePath = game.card.getSpritePath(sprite);
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
            _curBuffSkin.desc = desc;
            _ui.setDesc(desc);
        }
        private async void onChangeLanguage(int languageIndex)
        {
            string language = game.langManager.getLanguages()[languageIndex];
            _curLanguage = language;
            var skins = getCurrentBuffSkins().Where(s => s.language == language);
            var targetSkin = skins.FirstOrDefault();
            if (targetSkin == null)
            {
                markUnsaved();
                //不存在对应语言的皮肤，新建一个
                targetSkin = game.card.createBuffSkin(_curCardPoolId, _buffDefine.id, language);
                _buffSkins.Add(targetSkin);
            }
            updateSkinNames();
            var skinIndex = getCurrentLangSkins().indexOf(targetSkin);
            await setCurSkinIndex(skinIndex);
        }
        private void onChangePersistentAnimCallback(int index)
        {
            markUnsaved();
            var animRef = index <= 0 ? null : _persistentAnimRefs[index - 1];
            _curBuffSkin.persistentAnimRef = animRef;
            _ui.setPersistentAnim(index);
        }

        #endregion



        #region 效果
        private async void onCreateEffect(GeneratedEffect effect)
        {
            markUnsaved();
            _buffDefine.effectList.Add(effect);

            await _effectEditorCtrl.display(_curCardPoolId, effect);
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
            var eventNames = game.anims.getNodeVisualEffectEventEditorNames(node);
            var eventViewData = game.anims.getNodeAnimationViewData(node, _curBuffSkin);
            _graphEditor.updateAnimationDialog(eventNames, eventViewData);
            _graphEditor.displayNodeAnimationDialog(node);
        }
        private void onNodeAnimationPropChangedCallback(Node node, VisualEffectDialogInfo info)
        {
            game.anims.updateNodeAnimRef(node, _curBuffSkin, info);
            markUnsaved();
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
        private void onClickDeleteButton()
        {
            //询问玩家
            game.ui.dialog.ShowQuestion(
               game.langManager.getString(StringTable.WARNING),
               game.langManager.getString(EDITOR_WARNING_DELETE_BUFF),
               async () =>
               {
                   //删除当前增益
                   game.card.removeBuffDefine(_curCardPoolId, _buffDefine.id);
                   game.card.removeAllBuffSkins(_curCardPoolId, _buffDefine.id);
                   scene.editor.markCardPoolUnsaved(_curCardPoolId);
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
                    game.lang._(BUFF_WARNING_OPTION_SAVE),
                    game.lang._(BUFF_WARNING_OPTION_DONT_SAVE),
                    game.lang._(BUFF_WARNING_OPTION_CANCEL)
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

        #endregion
        private void updateSkinNames()
        {
            BuffSkin[] skins = getCurrentLangSkins();
            if (skins.Length < 1)
            {
                BuffSkin skin = game.card.createBuffSkin(_curCardPoolId, _buffDefine.id, _curLanguage);
                _buffSkins.Add(skin);
                skins = new BuffSkin[] { skin };
            }
            _ui.setSkinNames(skins.Select(g => g.name).ToArray());
        }
        private async Task setCurSkinIndex(int skinIndex)
        {
            var skinGroup = getCurrentLangSkins();
            //获取对应语言的皮肤
            var skin = skinGroup[skinIndex];

            _curBuffSkin = skin;

            string[] languages = game.langManager.getLanguages();
            var languageIndex = Array.IndexOf(languages, skin.language);
            _ui.setLanguage(languageIndex);
            _ui.setName(skin.name);
            _ui.setDesc(skin.desc);
            _ui.setSkin(skinIndex);
            var poolRef = getCurrentCardPoolRef();
            Sprite image = await game.card.loadBuffImage(poolRef, skin);
            _ui.setImage(image);
            var animRef = skin.persistentAnimRef;
            _ui.setPersistentAnim(animRef != null ? Array.IndexOf(_persistentAnimRefs, animRef) + 1 : 0);

        }
        private int getSkinCount()
        {
            return getCurrentLangSkins().Length;
        }
        private BuffSkin[] getCurrentLangSkins()
        {
            return _buffSkins.Where(s => s.language == _curLanguage).ToArray();
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
                var propertyTypes = getBuffDefinePropertyTypes();
                var modifyTypes = getBuffDefineModifyTypes(typeIndex < 0 ? string.Empty : propertyTypes[typeIndex]);
                var modifyTypeIndex = getModifyTypeIndex(propModifier);
                var valueType = getPropModifierValueType(propModifier);
                var value = getPropModifierValue(propModifier);
                infos[i] = new BuffModifierInfo()
                {
                    propertyTypeIndex = typeIndex,
                    isCustomProperty = propModifier is CustomPropertyModifier,
                    customPropertyName = customPropertyName,
                    modifyTypes = modifyTypes,
                    modifyTypeIndex = modifyTypeIndex,
                    valueType = valueType,
                    value = value,
                };
            }
            _ui.updateModifiers(infos);
        }
        private string[] getBuffDefinePropertyTypes()
        {
            return Array.ConvertAll(propertyTypes, s => game.langManager.getString(s));
        }
        private string[] getBuffDefineModifyTypes(string propertyType)
        {
            if (propertyType == StringTable.GAME_CARD_PROP_NAME_KEYWORDS)
                return Array.ConvertAll(keywordModifierTypes, s => game.langManager.getString(s));
            else
                return Array.ConvertAll(intModifierTypes, s => game.langManager.getString(s));
        }
        private int getModifyTypeIndex(PropModifier propModifier)
        {
            if (propModifier is KeywordsModifier keywordsModifier)
            {
                if (keywordsModifier.modifyType == KeywordsModifier.ModifyType.set)
                {
                    if (string.IsNullOrWhiteSpace(propModifier.relatedPropName))
                        return 2;
                    else
                        return 5;
                }
                else if (keywordsModifier.modifyType == KeywordsModifier.ModifyType.remove)
                {
                    if (string.IsNullOrWhiteSpace(propModifier.relatedPropName))
                        return 1;
                    else
                        return 4;
                }
                else if (keywordsModifier.modifyType == KeywordsModifier.ModifyType.add)
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
            if (propModifier is KeywordsModifier keywordsModifier)
            {
                if (index == 5)
                {
                    propModifier.relatedPropName = game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_BUFF_PROP_NAME_DEFAULT);
                    keywordsModifier.modifyType = KeywordsModifier.ModifyType.set;
                }
                else if (index == 4)
                {
                    propModifier.relatedPropName = game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_BUFF_PROP_NAME_DEFAULT);
                    keywordsModifier.modifyType = KeywordsModifier.ModifyType.remove;
                }
                else if (index == 3)
                {
                    propModifier.relatedPropName = game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_BUFF_PROP_NAME_DEFAULT);
                    keywordsModifier.modifyType = KeywordsModifier.ModifyType.add;
                }
                else if (index == 2)
                {
                    propModifier.relatedPropName = null;
                    keywordsModifier.modifyType = KeywordsModifier.ModifyType.set;
                }
                else if (index == 1)
                {
                    propModifier.relatedPropName = null;
                    keywordsModifier.modifyType = KeywordsModifier.ModifyType.remove;
                }
                else
                {
                    propModifier.relatedPropName = null;
                    keywordsModifier.modifyType = KeywordsModifier.ModifyType.add;
                }
            }
            else if (propModifier is IntPropModifier intPropModifier)
            {
                if (index == 3)
                {
                    propModifier.relatedPropName = game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_BUFF_PROP_NAME_DEFAULT);
                    intPropModifier.isSet = true;
                }
                else if (index == 2)
                {
                    propModifier.relatedPropName = game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_BUFF_PROP_NAME_DEFAULT);
                    intPropModifier.isSet = false;
                }
                else if (index == 1)
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
                if (index == 3)
                {
                    propModifier.relatedPropName = game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_BUFF_PROP_NAME_DEFAULT);
                    customPropModifier.isSet = true;
                }
                else if (index == 2)
                {
                    propModifier.relatedPropName = game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_BUFF_PROP_NAME_DEFAULT);
                    customPropModifier.isSet = false;
                }
                else if (index == 1)
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
                else if (propModifier is KeywordsModifier)
                    return ActionNodeValueType.keywords;
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
            else if (propModifier is KeywordsModifier keywordsModifier)
                return new KeywordCollection(keywordsModifier.defaultValue);
            else
                return propModifier.getDefaultValue();
        }
        private PropModifier createPropertyModifierByIndex(int index)
        {
            if (index == 6)
                return new CustomPropertyModifier(string.Empty, 0);
            if (index == 5)
                return new SpellDamageModifier(0);
            if (index == 4)
                return new KeywordsModifier(KeywordsModifier.ModifyType.add);
            if (index == 3)
                return new DamageReduceModifier(0);
            if (index == 2)
                return new LifeModifier(0);
            if (index == 1)
                return new AttackModifier(0);
            return new CostModifier(0);
        }
        private int getPropertyModifierIndex(PropModifier propModifier)
        {
            if (propModifier is CustomPropertyModifier)
                return 6;
            if (propModifier is SpellDamageModifier)
                return 5;
            if (propModifier is KeywordsModifier)
                return 4;
            if (propModifier is DamageReduceModifier)
                return 3;
            if (propModifier is LifeModifier)
                return 2;
            if (propModifier is AttackModifier)
                return 1;
            return 0;
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
        protected override void setUIEffects(string[] names, int activeIndex)
        {
            _ui.setEffects(names, activeIndex);
        }
        private void saveBuff()
        {
            game.card.removeBuffDefine(_curCardPoolId, _buffDefine.id);
            game.card.addBuffDefine(_curCardPoolId, _buffDefine);

            game.card.removeAllBuffSkins(_curCardPoolId, _buffDefine.id);
            foreach (var skin in _buffSkins)
            {
                game.card.addBuffSkin(_curCardPoolId, skin);
            }

            scene.editor.markCardPoolUnsaved(_curCardPoolId);
            clearUnsaved();
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

        BuffEditorPanel _ui;

        #region 属性字段
        [SerializeField]
        Button _cancelCardPoolSelectButton;

        [TranslateMsg("游戏中卡牌的自定义属性的名称")]
        public const string GAME_CARD_PROP_NAME_CUSTOM_PROPERTY = "自定义属性";
        [TranslateMsg("尝试删除增益的警告")]
        public const string EDITOR_WARNING_DELETE_BUFF = "确认要删除这个增益吗？";
        [TranslateMsg("增益编辑器/标题")]
        public const string BUFF_EDITOR_TITLE = "增益编辑器";
        [TranslateMsg("尝试退出一个增益的编辑页面，但没有保存情况下的警告")]
        public const string BUFF_WARNING_UNSAVED_CHANGES = "是否将更改保存到该增益中？";
        [TranslateMsg("尝试退出一个增益的编辑页面，但没有保存情况下的按钮名称")]
        public const string BUFF_WARNING_OPTION_SAVE = "保存";
        [TranslateMsg("尝试退出一个增益的编辑页面，但没有保存情况下的按钮名称")]
        public const string BUFF_WARNING_OPTION_DONT_SAVE = "不保存";
        [TranslateMsg("尝试退出一个增益的编辑页面，但没有保存情况下的按钮名称")]
        public const string BUFF_WARNING_OPTION_CANCEL = "取消";

        [TranslateMsg("尝试添加重复名称的特效引用的错误提示")]
        public const string ANIM_REF_KEY_TAKEN = "该名称已被使用！";
        [TranslateMsg("尝试添加空名称的特效引用的错误提示")]
        public const string ANIM_REF_KEY_EMPTY = "请输入特效引用名！";

        [TranslateMsg("增益编辑器/持续限制/标题（{0}为序号）")]
        public const string BUFF_EXISTLIMIT_HEADER = "限制{0}";

        public static string[] propertyTypes = new string[]
        {
            StringTable.GAME_CARD_PROP_NAME_COST,
            StringTable.GAME_CARD_PROP_NAME_ATTACK,
            StringTable.GAME_CARD_PROP_NAME_LIFE,
            StringTable.GAME_CARD_PROP_NAME_DAMAGE_REDUCE,
            StringTable.GAME_CARD_PROP_NAME_KEYWORDS,
            StringTable.GAME_CARD_PROP_NAME_SPELL_DAMAGE,
            GAME_CARD_PROP_NAME_CUSTOM_PROPERTY,
        };
        public static string[] intModifierTypes = new string[]
        {
            StringTable.GAME_CARD_PROP_MODIFY_ADD,
            StringTable.GAME_CARD_PROP_MODIFY_SETTO,
            StringTable.GAME_CARD_PROP_MODIFY_ADD_PROP,
            StringTable.GAME_CARD_PROP_MODIFY_SETTO_PROP
        };
        public static string[] keywordModifierTypes = new string[]
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
        string _curLanguage;
        DefineReference[] _persistentAnimRefs;
        private EventTriggerTime[] eventTriggerTimes => scene.editor.eventTriggerTimes;
        private string[] eventTriggerTimeEditorNames => scene.editor.eventTriggerTimeEditorNames;
        #endregion
    }
}
