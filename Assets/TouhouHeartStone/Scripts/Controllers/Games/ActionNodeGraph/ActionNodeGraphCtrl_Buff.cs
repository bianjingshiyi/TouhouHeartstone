using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using System;
using UnityEngine;
using System.Threading.Tasks;
using SFB;
using Game;
using System.Linq;

namespace Controllers
{
    partial class ActionNodeGraphCtrl
    {
        #region 私有方法
        #region 事件回调
        private async void onItemBuffChangedCallback(ActionNodeItem item, int index)
        {
            BuffDefine[] buffDefines = game.card.getBuffDefines(_curCardPoolId);
            GeneratedBuffDefine buffDefine = buffDefines[index] as GeneratedBuffDefine;
            await updateBuffActionNode(item, buffDefine, getOrCreateBuffSkin(buffDefine));
        }
        private async void onClickItemNewBuffButtonCallback(ActionNodeItem item)
        {
            GeneratedBuffDefine newBuffDefine = game.card.createBuffDefine(_curCardPoolId);
            await updateBuffActionNode(item, newBuffDefine, getOrCreateBuffSkin(newBuffDefine));
        }
        private async void onClickItemDeleteBuffButtonCallback(ActionNodeItem item)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                game.card.removeBuffDefine(_curCardPoolId, buffDefine))
            {
                GeneratedBuffDefine[] buffDefines = game.card.getBuffDefines(_curCardPoolId);
                buffDefine = buffDefines[buffDefines.Length - 1];
                await updateBuffActionNode(item, buffDefine, getOrCreateBuffSkin(buffDefine));
            }
        }
        private async void onClickItemAddPropModifierButtonCallback(ActionNodeItem item)
        {
            if (getActionNode(item) is ActionNode actionNode &&
               actionNode.consts != null &&
               actionNode.consts.Length > 0 &&
               actionNode.consts[0] is int buffDefineId)
            {
                if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine)
                {
                    PropModifier propModifier = new CostModifier(0);
                    generatedBuffDefine.propModifierList.Add(propModifier);
                    await updateBuffActionNode(item, generatedBuffDefine, null);
                }
            }
        }
        private async void onClickItemRemovePropModifierButtonCallback(ActionNodeItem item, int index)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine)
            {
                generatedBuffDefine.propModifierList.RemoveAt(index);
                await updateBuffActionNode(item, generatedBuffDefine, null);
            }
        }
        private async void onItemPropModifierPropertyTypeChangedCallback(ActionNodeItem item, int index, int propTypeIndex)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine &&
                propTypeIndex != getPropertyModifierIndex(generatedBuffDefine.propModifierList[index]))
            {
                generatedBuffDefine.propModifierList[index] = createPropertyModifierByIndex(propTypeIndex);
                await updateBuffActionNode(item, generatedBuffDefine, null);
            }
        }
        private async void onItemPropModifierModifyTypeChangedCallback(ActionNodeItem item, int index, int modifyTypeIndex)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine &&
                modifyTypeIndex != getModifyTypeIndex(generatedBuffDefine.propModifierList[index]))
            {
                setModifyTypeByIndex(generatedBuffDefine.propModifierList[index], modifyTypeIndex);
                await updateBuffActionNode(item, generatedBuffDefine, null);
            }
        }
        private async void onItemPropModifierValueChangedCallback(ActionNodeItem item, int index, object value)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine)
            {
                PropModifier propModifier = generatedBuffDefine.propModifierList[index];
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
                }
                await updateBuffActionNode(item, generatedBuffDefine, null);
            }
        }
        private async void onItemPropModifierAddEffectCallback(ActionNodeItem item, int index)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine)
            {
                generatedBuffDefine.effectList.Add(game.card.createEffectByIndex(index));
                await updateBuffActionNode(item, generatedBuffDefine, null);
            }
        }
        private async void onItemPropModifierRemoveEffectCallback(ActionNodeItem item, int index)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine)
            {
                generatedBuffDefine.effectList.RemoveAt(index);
                await updateBuffActionNode(item, generatedBuffDefine, null);
            }
        }
        private async void onItemPropModifierEditEffectCallback(ActionNodeItem item, int index)
        {
            ActionNode actionNode = getActionNode(item);
            int buffDefineId = (int)actionNode.consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine)
            {
                await scene.effectEditor.display(_curCardPoolId, generatedBuffDefine, generatedBuffDefine.effectList[index]);
            }
        }
        private async void onItemBuffNameChangedCallback(ActionNodeItem item, string value)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine)
            {
                var buffSkin = getOrCreateBuffSkin(generatedBuffDefine);
                buffSkin.name = value;
                await updateBuffActionNode(item, generatedBuffDefine, buffSkin);
            }
        }
        private async void onItemBuffDescChangedCallback(ActionNodeItem item, string value)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine)
            {
                var buffSkin = getOrCreateBuffSkin(generatedBuffDefine);
                buffSkin.desc = value;
                await updateBuffActionNode(item, generatedBuffDefine, buffSkin);
            }
        }
        private async void onItemBuffLanguageChangedCallback(ActionNodeItem item, int index)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine)
            {
                var buffSkin = getOrCreateBuffSkin(generatedBuffDefine);
                string[] languages = game.langManager.getLanguages();
                buffSkin.language = languages[index];
                await updateBuffActionNode(item, generatedBuffDefine, buffSkin);
            }
        }
        private async void onItemNeedSelectBuffImage(ActionNodeItem item)
        {
            Sprite sprite = await game.card.selectExternalImage();
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine)
            {
                var buffSkin = getOrCreateBuffSkin(generatedBuffDefine);
                buffSkin.imagePath = sprite != null ? game.card.getSpritePath(sprite) : null;
                await updateBuffActionNode(item, generatedBuffDefine, buffSkin);
            }
        }
        #endregion
        private async Task updateBuffActionNode(ActionNodeItem item, GeneratedBuffDefine buffDefine, BuffSkin buffSkin)
        {
            getActionNode(item).consts = new object[] { buffDefine.id };
            BuffDefine[] buffDefines = game.card.getBuffDefines(_curCardPoolId);
            item.setBuffValues(game.card.getBuffDefineNames(buffDefines));
            item.setBuffValue(Array.IndexOf(buffDefines, buffDefine));
            ActionNodePropertyModifierViewData[] propModifiersViewData = getBuffDefinePropModifiersViewData(buffDefine);
            item.updatePropModifierList(propModifiersViewData);
            item.updateEffectList(game.card.getEffectNames(), game.card.getEffectNames(buffDefine.effectList.ToArray()));
            if (buffSkin != null)
            {
                int languageIndex = Array.IndexOf(game.langManager.getLanguages(), buffSkin.language);
                item.setBuffLanguage(languageIndex);
                Sprite sprite = await game.card.loadImage(buffSkin.imagePath, _curCardPoolId);
                item.setBuffSkin(buffSkin.name, buffSkin.desc, sprite);
            }
        }
        private string[] getBuffDefinePropertyTypes()
        {
            return _propertyTypes.Select(s => game.langManager.getString(s)).ToArray();
        }
        private string[] getBuffDefineModifyTypes(string propertyType)
        {
            if (propertyType == StringTable.GAME_CARD_PROP_NAME_KEYWORDS)
            {

            }
            else
                _intModifyTypes.Select(s => game.langManager.getString(s)).ToArray()
        }
        private ActionNodePropertyModifierViewData[] getBuffDefinePropModifiersViewData(GeneratedBuffDefine buffDefine)
        {
            ActionNodePropertyModifierViewData[] propModifiersViewData = new ActionNodePropertyModifierViewData[buffDefine.propModifierList.Count];
            for (int i = 0; i < buffDefine.propModifierList.Count; i++)
            {
                PropModifier propModifier = buffDefine.propModifierList[i];
                ActionNodePropertyModifierViewData propModifierViewData = new ActionNodePropertyModifierViewData();
                propModifiersViewData[i] = propModifierViewData;
                propModifierViewData.propertyTypeIndex = getPropertyModifierIndex(propModifier);
                propModifierViewData.modifyTypeIndex = getModifyTypeIndex(propModifier);
                propModifierViewData.valueType = getPropModifierValueType(propModifier);
                propModifierViewData.value = propModifier.relatedPropName != null ? propModifier.relatedPropName : propModifier.getValue();
            }
            return propModifiersViewData;
        }
        private PropModifier createPropertyModifierByIndex(int index)
        {
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
        private int getModifyTypeIndex(PropModifier propModifier)
        {
            if (propModifier is KeywordsModifier keywordsModifier)
            {
                return 4;
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
                    return ActionNodeValueType.text;
                throw new NotImplementedException();
            }
        }
        private BuffSkin getOrCreateBuffSkin(GeneratedBuffDefine buffDefine)
        {
            BuffSkin[] buffSkins = game.card.getBuffSkins(_curCardPoolId, buffDefine.id);
            BuffSkin buffSkin = buffSkins.Length > 0 ?
                Array.Find(buffSkins, s => s.language == game.langManager.getLanguage()) :
                game.card.createBuffSkin(_curCardPoolId, buffDefine.id, game.langManager.getLanguage());
            if (buffSkin == null)
                buffSkin = buffSkins[0];
            return buffSkin;
        }
        #endregion
    }
}
