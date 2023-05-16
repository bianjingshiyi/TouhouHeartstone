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
            _cardPoolId = cardPoolId;
            _buffDefine = buffDefine;
            _buffSkins.Clear();
            _buffSkins.AddRange(skins);

            _curLanguage = game.langManager.getLanguage();
            string[] languages = game.langManager.getLanguages();
            _ui.setLanguage(languages.indexOf(_curLanguage));

            updateSkinNames();
            _ui.setId(buffDefine.id);
            GeneratedEffect[] effects = buffDefine.effectList.OfType<GeneratedEffect>().ToArray();
            updateModifiers();
            _ui.closeAllEdit();
            await setCurSkinIndex(0);

            _effectEditorCtrl.hideActionGraph();

            if (effects.Length > 0)
            {
                await _effectEditorCtrl.display(_cardPoolId, _buffDefine, effects[0]);
            }
            updateEffectList(effects);
        }
        public override Task display(CardPoolDisplayInfo info)
        {
            if (info == null)
                return Task.CompletedTask;
            if (info.displayObj is GeneratedBuffDefine buffDefine)
            {
                var skins = info.skins as BuffSkin[] ?? new BuffSkin[0];
                return display(info.cardPoolId, buffDefine, skins);
            }
            return Task.CompletedTask;
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
            _ui.init(getBuffDefinePropertyTypes(), CardEditorCtrl.getKeywords());
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


            _ui.onClickEditEffectButton += onClickEditEffectButton;
            _ui.onClickRemoveEffectButton += onClickRemoveEffectButton;
            _ui.setEffectTypes(game.card.getEffectNames());
            _ui.onClickAddEffectMenu += onClickAddEffectMenu;
            _ui.onEffectListItemInputFieldEndEdit += onEffectListItemInputFieldEndEditCallback;
            _ui.onClickDeleteCardButton += onClickDeleteCardButton;
            _ui.onClickSaveButton += onClickSaveButton;
            _ui.onClickCloseButton += onClickCloseButton;

            _ui.onAddModifier += onAddModifier;
            _ui.OnRemoveModifier += onRemoveModifier;
            _ui.OnChangeModifierProperty += onChangeModifierProperty;
            _ui.OnChangeModifierOperator += onChangeModifierOperator;
            _ui.OnChangeModifierValue += onChangeModifierValue;

            _effectEditorCtrl.onCreateEffect += onCreateEffect;
            _effectEditorCtrl.OnGraphEdited += onGraphEdited;
        }
        #endregion
        #region 事件回调
        private void onGraphEdited()
        {
            markBuffUnsaved();
        }
        #region 皮肤
        private async void onChangeSkin(int skinIndex)
        {
            markBuffUnsaved();
            if (skinIndex == getSkinCount())
            {
                //新建皮肤
                _curBuffSkin = game.card.createBuffSkin(_cardPoolId, _buffDefine.id, _curLanguage);
                _buffSkins.Add(_curBuffSkin);
                updateSkinNames();
            }
            await setCurSkinIndex(skinIndex);
        }
        private async void onChangeSkinName(string skinName)
        {
            markBuffUnsaved();
            var skinGroup = getCurrentLangSkins();
            int index = skinGroup.indexOf(_curBuffSkin);
            _curBuffSkin.name = skinName;
            updateSkinNames();
            await setCurSkinIndex(index);
        }
        private async void onDeleteSkin()
        {
            markBuffUnsaved();
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
            markBuffUnsaved();
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
                markBuffUnsaved();
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
            markBuffUnsaved();
            _curBuffSkin.desc = desc;
            _ui.setDesc(desc);
        }
        #endregion


        private async void onChangeLanguage(int languageIndex)
        {
            string language = game.langManager.getLanguages()[languageIndex];
            _curLanguage = language;
            var skins = getCurrentBuffSkins().Where(s => s.language == language);
            var targetSkin = skins.FirstOrDefault();
            if (targetSkin == null)
            {
                markBuffUnsaved();
                //不存在对应语言的皮肤，新建一个
                targetSkin = game.card.createBuffSkin(_cardPoolId, _buffDefine.id, language);
                _buffSkins.Add(targetSkin);
            }
            updateSkinNames();
            _curBuffSkin = targetSkin;
            _ui.setLanguage(languageIndex);
            _ui.setName(targetSkin.name);
            _ui.setDesc(targetSkin.desc);
            _ui.setSkin(getCurrentLangSkins().indexOf(targetSkin));
            string path = targetSkin.imagePath;
            Sprite image = await game.card.loadImage(targetSkin.imagePath, _buffDefine.cardPoolId);
            _ui.setImage(image);
        }

        #region 效果
        private void onCreateEffect(GeneratedEffect effect)
        {
            markBuffUnsaved();
            _buffDefine.effectList.Add(effect);

            _effectEditorCtrl.display(_cardPoolId, _buffDefine, effect);
            updateEffectList(_buffDefine.effectList.ToArray());
            
            _ui.setEffectScrollValue(1);
        }
        /// <summary>
        /// 当点击编辑效果按钮，打开编辑效果界面。
        /// </summary>
        private void onClickEditEffectButton(int index)
        {
            var effects = _buffDefine.effectList.OfType<GeneratedEffect>();
            GeneratedEffect effect = effects.ElementAt(index);
            if (_effectEditorCtrl.getCurrentEffect() != effect)
            {
                _effectEditorCtrl.display(_cardPoolId, _buffDefine, effect);
                updateEffectList(effects.ToArray());
            }
            else
            {
                string name = effect?.name ?? string.Empty;
                _ui.showEffectListItemInputField(index, name);
            }
        }
        private void onClickRemoveEffectButton(int index)
        {
            markBuffUnsaved();
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
            markBuffUnsaved();
        }
        #endregion

        #region 修改器
        private void onAddModifier()
        {
            PropModifier propModifier = new CostModifier(0);
            _buffDefine.propModifierList.Add(propModifier);
            updateModifiers();
            markBuffUnsaved();
        }
        private void onRemoveModifier(int modifierIndex)
        {
            _buffDefine.propModifierList.RemoveAt(modifierIndex);
            updateModifiers();
            markBuffUnsaved();
        }
        private void onChangeModifierProperty(int modifierIndex, int propertyIndex)
        {
            _buffDefine.propModifierList[modifierIndex] = createPropertyModifierByIndex(propertyIndex);
            updateModifiers();
            markBuffUnsaved();
        }
        private void onChangeModifierOperator(int modifierIndex, int operatorIndex)
        {
            setModifyTypeByIndex(_buffDefine.propModifierList[modifierIndex], operatorIndex);
            updateModifiers();
            markBuffUnsaved();
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
                    intPropModifier.value = (int)value;
                }
                else if (propModifier is KeywordsModifier keywordsModifier)
                {
                    keywordsModifier.value = ((KeywordCollection)value).ToArray();
                }
            }
            updateModifiers();
            markBuffUnsaved();
        }
        #endregion

        private async void onClickDeleteCardButton()
        {
            scene.editor.hasUnsavedChanges = true;
            //删除当前卡牌
            game.card.removeBuffDefine(_cardPoolId, _buffDefine);
            //返回上个界面
            await scene.editor.closeEditorPanel();
            clearBuffUnsaved();
        }
        private void onClickSaveButton()
        {
            saveBuff();
        }
        private void onClickCloseButton()
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
                game.ui.dialog.ShowOptions(title, desc, optNames, (index) =>
                {
                    if (index == 0)
                    {
                        saveBuff();
                        exit();
                    }
                    else if (index == 1)
                    {
                        exit();
                    }
                });
            }
            else
            {
                exit();
            }
        }
        #endregion

        #endregion
        private void updateSkinNames()
        {
            BuffSkin[] skins = getCurrentLangSkins();
            if (skins.Length < 1)
            {
                BuffSkin skin = game.card.createBuffSkin(_cardPoolId, _buffDefine.id, _curLanguage);
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
            _ui.setSkin(skinIndex);
            _ui.setName(skin.name);
            _ui.setDesc(skin.desc);
            Sprite image = await game.card.loadImage(skin.imagePath, _buffDefine.cardPoolId);
            _ui.setImage(image);
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
        private async void exit()
        {
            //返回上一个界面
            await scene.editor.closeEditorPanel();
            clearBuffUnsaved();
        }
        #region 修改器
        private void updateModifiers()
        {
            BuffModifierInfo[] infos = new BuffModifierInfo[_buffDefine.propModifierList.Count];
            for (int i = 0; i < infos.Length; i++)
            {
                var propModifier = _buffDefine.propModifierList[i];
                int typeIndex = getPropertyModifierIndex(propModifier);
                var propertyTypes = getBuffDefinePropertyTypes();
                var modifyTypes = getBuffDefineModifyTypes(typeIndex < 0 ? string.Empty : propertyTypes[typeIndex]);
                var modifyTypeIndex = getModifyTypeIndex(propModifier);
                var valueType = getPropModifierValueType(propModifier);
                var value = getPropModifierValue(propModifier);
                infos[i] = new BuffModifierInfo()
                {
                    propertyTypeIndex = typeIndex,
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
                throw new NotImplementedException();
            }
        }
        private object getPropModifierValue(PropModifier propModifier)
        {
            if (!string.IsNullOrWhiteSpace(propModifier.relatedPropName))
                return propModifier.relatedPropName;
            else if (propModifier is KeywordsModifier keywordsModifier)
                return new KeywordCollection(keywordsModifier.value);
            else
                return propModifier.getValue();
        }
        private PropModifier createPropertyModifierByIndex(int index)
        {
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
        #endregion
        protected override void setUIEffects(string[] names, int activeIndex)
        {
            _ui.setEffects(names, activeIndex);
        }
        private void saveBuff()
        {
            game.card.removeBuffDefine(_cardPoolId, _buffDefine.id);
            game.card.addBuffDefine(_cardPoolId, _buffDefine);

            game.card.removeAllBuffSkins(_cardPoolId, _buffDefine.id);
            foreach (var skin in _buffSkins)
            {
                game.card.addBuffSkin(_cardPoolId, skin);
            }

            scene.editor.hasUnsavedChanges = true;
            clearBuffUnsaved();
        }
        public void markBuffUnsaved()
        {
            hasUnsavedChanges = true;
        }
        public void clearBuffUnsaved()
        {
            hasUnsavedChanges = false;
        }

        BuffEditorPanel _ui;

        #region 属性字段
        [SerializeField]
        CardPoolViewPanelCtrl _cardPoolViewPanelCtrl;
        [SerializeField]
        Button _cancelCardPoolSelectButton;

        [TranslateMsg("尝试退出一个增益的编辑页面，但没有保存情况下的警告")]
        public const string BUFF_WARNING_UNSAVED_CHANGES = "是否将更改保存到该增益中？";
        [TranslateMsg("尝试退出一个增益的编辑页面，但没有保存情况下的按钮名称")]
        public const string BUFF_WARNING_OPTION_SAVE = "保存";
        [TranslateMsg("尝试退出一个增益的编辑页面，但没有保存情况下的按钮名称")]
        public const string BUFF_WARNING_OPTION_DONT_SAVE = "不保存";
        [TranslateMsg("尝试退出一个增益的编辑页面，但没有保存情况下的按钮名称")]
        public const string BUFF_WARNING_OPTION_CANCEL = "取消";

        public static string[] propertyTypes = new string[]
        {
            StringTable.GAME_CARD_PROP_NAME_COST,
            StringTable.GAME_CARD_PROP_NAME_ATTACK,
            StringTable.GAME_CARD_PROP_NAME_LIFE,
            StringTable.GAME_CARD_PROP_NAME_DAMAGE_REDUCE,
            StringTable.GAME_CARD_PROP_NAME_KEYWORDS,
            StringTable.GAME_CARD_PROP_NAME_SPELL_DAMAGE,
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
        public bool hasUnsavedChanges { get; private set; }
        long _cardPoolId;
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
        #endregion
    }
}
