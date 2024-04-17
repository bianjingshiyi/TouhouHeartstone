using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    public static class SerializeHelper
    {
        public static SerializableEffect ToSerializable(GeneratedEffect effect)
        {
             if (effect is GeneratedActiveEffect activeEffect)
                return new SerializableActiveEffect(activeEffect);
            else if (effect is GeneratedEventEffect eventEffect)
                return new SerializableEventEffect(eventEffect);
            else if (effect is GeneratedAura auraEffect)
                return new SerializableAuraEffect(auraEffect);
            else if (effect is GeneratedPassiveEffect passiveEffect) // 被动效果必须在事件效果和光环效果后面，因为他们也是被动效果
                return new SerializablePassiveEffect(passiveEffect);
            else
                throw new ArgumentException($"无法序列化未知类型的效果{effect.GetType().Name}");
        }
        public static SerializableGeneratedRuleLogic ToSerializable(GeneratedRuleLogic logic)
        {
            if (logic is GeneratedRuleGameLogic gameLogic)
                return new SerializableGeneratedRuleGameLogic(gameLogic);
            else if (logic is GeneratedRuleEventLogic eventLogic)
                return new SerializableGeneratedRuleEventLogic(eventLogic);
            else
                throw new ArgumentException($"无法序列化未知类型的规则逻辑{logic.GetType().Name}");
        }
        public static SerializableNode ToSerializable(Node node)
        {
            if (node is GeneratedActionEntryNode entryNode)
                return new SerializableGeneratedEntryNode(entryNode);
            else if (node is ActionNode actionNode)
                return new SerializableActionNode(actionNode);
            else if (node is GeneratedActionReturnNode returnNode)
                return new SerializableGeneratedReturnNode(returnNode);
            else if (node is GameRuleEntryNode ruleEntryNode)
                return new SerializableGameRuleEntryNode(ruleEntryNode);
            else if (node is ActiveEntryNode activeEntryNode)
                return new SerializableActiveEntryNode(activeEntryNode);
            else if (node is EventRuleEntryNode eventRuleEntryNode)
                return new SerializableEventRuleEntryNode(eventRuleEntryNode);
            else if (node is AuraEntryNode auraEntryNode)
                return new SerializableAuraEntryNode(auraEntryNode);
            else if (node is EventEntryNode eventEntryNode)
                return new SerializableEventEntryNode(eventEntryNode);
            else if (node is PassiveEntryNode passiveEntryNode)// 被动效果必须在事件效果和光环效果后面，因为他们也是被动效果
                return new SerializablePassiveEntryNode(passiveEntryNode);
            else
                throw new ArgumentException($"无法序列化未知类型的节点{node.GetType().Name}");
        }
        public static SerializablePropModifier ToSerializable(PropModifier propModifier)
        {
            if (propModifier is CustomPropertyModifier customPropertyModifier)
                return new SerializableCustomPropertyModifier(customPropertyModifier);
            else if (propModifier is KeywordsModifier keywordsModifier)
                return new SerializableKeywordsModifier(keywordsModifier);
            else if (propModifier is AttackModifier attackModifier)
                return new SerializableAttackModifier(attackModifier);
            else if (propModifier is DamageReduceModifier damageReduceModifier)
                return new SerializableDamageReduceModifier(damageReduceModifier);
            else if (propModifier is CostModifier costModifier)
                return new SerializableCostModifier(costModifier);
            else if (propModifier is LifeModifier lifeModifier)
                return new SerializableLifeModifier(lifeModifier);
            else if (propModifier is SpellDamageModifier spellDamageModifier)
                return new SerializableSpellDamageModifier(spellDamageModifier);
            else
                throw new ArgumentException($"无法序列化未知类型的节点{propModifier.GetType().Name}");
        }
    }
}
