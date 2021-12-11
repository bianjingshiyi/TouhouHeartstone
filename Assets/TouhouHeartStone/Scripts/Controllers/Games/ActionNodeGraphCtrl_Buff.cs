using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using System;

namespace Controllers
{
    partial class ActionNodeGraphCtrl
    {
        #region 私有方法
        #region 事件回调
        private void onItemBuffChangedCallback(ActionNodeItem item, int index)
        {
            BuffDefine[] buffDefines = game.card.getBuffDefines(_curCardPoolId);
            GeneratedBuffDefine buffDefine = buffDefines[index] as GeneratedBuffDefine;
            updateBuffActionNode(item, buffDefine);
        }
        private void onClickItemNewBuffButtonCallback(ActionNodeItem item)
        {
            GeneratedBuffDefine newBuffDefine = game.card.createBuffDefine(_curCardPoolId);
            updateBuffActionNode(item, newBuffDefine);
        }
        private void onClickItemDeleteBuffButtonCallback(ActionNodeItem item)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                game.card.removeBuffDefine(_curCardPoolId, buffDefine))
            {
                GeneratedBuffDefine[] buffDefines = game.card.getBuffDefines(_curCardPoolId);
                buffDefine = buffDefines[buffDefines.Length - 1];
                updateBuffActionNode(item, buffDefine as GeneratedBuffDefine);
            }
        }
        private void onClickItemAddPropModifierButtonCallback(ActionNodeItem item)
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
                    updateBuffActionNode(item, generatedBuffDefine);
                }
            }
        }
        private void onClickItemRemovePropModifierButtonCallback(ActionNodeItem item, int index)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine)
            {
                generatedBuffDefine.propModifierList.RemoveAt(index);
                updateBuffActionNode(item, generatedBuffDefine);
            }
        }
        private void onItemPropModifierPropertyTypeChangedCallback(ActionNodeItem item, int index, int propTypeIndex)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine &&
                propTypeIndex != getPropertyModifierIndex(generatedBuffDefine.propModifierList[index]))
            {
                generatedBuffDefine.propModifierList[index] = getPropertyModifierByIndex(propTypeIndex);
                updateBuffActionNode(item, generatedBuffDefine);
            }
        }
        private void onItemPropModifierModifyTypeChangedCallback(ActionNodeItem item, int index, int modifyTypeIndex)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine &&
                modifyTypeIndex != getModifyTypeIndex(generatedBuffDefine.propModifierList[index]))
            {
                setModifyTypeByIndex(generatedBuffDefine.propModifierList[index], modifyTypeIndex);
                updateBuffActionNode(item, generatedBuffDefine);
            }
        }
        private void onItemPropModifierValueChangedCallback(ActionNodeItem item, int index, object value)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine)
            {
                PropModifier propModifier = generatedBuffDefine.propModifierList[index];
                if (propModifier is IntPropModifier intPropModifier)
                {
                    intPropModifier.value = (int)value;
                }
                updateBuffActionNode(item, generatedBuffDefine);
            }
        }
        private void onItemPropModifierAddEffectCallback(ActionNodeItem item, int index)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine)
            {
                generatedBuffDefine.effectList.Add(game.card.createEffectByIndex(index));
                updateBuffActionNode(item, generatedBuffDefine);
            }
        }
        private void onItemPropModifierRemoveEffectCallback(ActionNodeItem item, int index)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine)
            {
                generatedBuffDefine.effectList.RemoveAt(index);
                updateBuffActionNode(item, generatedBuffDefine);
            }
        }
        private async void onItemPropModifierEditEffectCallback(ActionNodeItem item, int index)
        {
            int buffDefineId = (int)getActionNode(item).consts[0];
            if (game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine) &&
                buffDefine is GeneratedBuffDefine generatedBuffDefine)
            {
                ActionNodeGraphInfo graphInfo = scene.editor.actionNodeGraphInfoList.Find(g => g.cardId == _cardDefine.id && g.effectIndex == index);
                if (graphInfo == null)
                {
                    graphInfo = new ActionNodeGraphInfo(_cardDefine.id, index);
                    scene.editor.actionNodeGraphInfoList.Add(graphInfo);
                }
                await scene.effectEditor.displayEffect(_curCardPoolId, _cardDefine, generatedBuffDefine.effectList[index], graphInfo);
            }
        }
        #endregion
        private void updateBuffActionNode(ActionNodeItem item, GeneratedBuffDefine buffDefine)
        {
            getActionNode(item).consts = new object[] { buffDefine.id };
            item.setBuffId(buffDefine.id.ToString());
            BuffDefine[] buffDefines = game.card.getBuffDefines(_curCardPoolId);
            item.setBuffValues(game.card.getBuffDefineNames(buffDefines));
            item.setBuffValue(Array.IndexOf(buffDefines, buffDefine));
            ActionNodePropertyModifierViewData[] propModifiersViewData = new ActionNodePropertyModifierViewData[buffDefine.propModifierList.Count];
            for (int i = 0; i < buffDefine.propModifierList.Count; i++)
            {
                PropModifier propModifier = buffDefine.propModifierList[i];
                ActionNodePropertyModifierViewData propModifierViewData = new ActionNodePropertyModifierViewData();
                propModifiersViewData[i] = propModifierViewData;
                if (propModifier is CostModifier costModifier)
                {
                    propModifierViewData.propertyTypeIndex = 0;
                    if (costModifier.isSet)
                        propModifierViewData.modifyTypeIndex = 1;
                    else
                        propModifierViewData.modifyTypeIndex = 0;
                    propModifierViewData.valueType = ActionNodeValueType.integer;
                    propModifierViewData.value = costModifier.value;
                }
                else if (propModifier is AttackModifier attackModifier)
                {
                    propModifierViewData.propertyTypeIndex = 1;
                    if (attackModifier.isSet)
                        propModifierViewData.modifyTypeIndex = 1;
                    else
                        propModifierViewData.modifyTypeIndex = 0;
                    propModifierViewData.valueType = ActionNodeValueType.integer;
                    propModifierViewData.value = attackModifier.value;
                }
                else if (propModifier is LifeModifier lifeModifier)
                {
                    propModifierViewData.propertyTypeIndex = 2;
                    if (lifeModifier.isSet)
                        propModifierViewData.modifyTypeIndex = 1;
                    else
                        propModifierViewData.modifyTypeIndex = 0;
                    propModifierViewData.valueType = ActionNodeValueType.integer;
                    propModifierViewData.value = lifeModifier.value;
                }
            }
            item.updatePropModifierList(propModifiersViewData);
            item.updateEffectList(game.card.getEffectNames(), game.card.getEffectNames(buffDefine.effectList.ToArray()));
        }
        private PropModifier getPropertyModifierByIndex(int index)
        {
            if (index == 2)
                return new LifeModifier(0);
            else if (index == 1)
                return new AttackModifier(0);
            else
                return new CostModifier(0);
        }
        private int getPropertyModifierIndex(PropModifier propModifier)
        {
            if (propModifier is LifeModifier)
                return 2;
            else if (propModifier is AttackModifier)
                return 1;
            else
                return 0;
        }
        private int getModifyTypeIndex(PropModifier propModifier)
        {
            if (propModifier is IntPropModifier intPropModifier && intPropModifier.isSet)
            {
                return 1;
            }
            else
                return 0;
        }
        private void setModifyTypeByIndex(PropModifier propModifier, int index)
        {
            if (propModifier is IntPropModifier intPropModifier)
            {
                if (index == 1)
                    intPropModifier.isSet = true;
                else
                    intPropModifier.isSet = false;
            }
        }
        #endregion
    }
}
