using System;
using System.Collections.Generic;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    partial class EffectEditorLibrary
    {
        private static Dictionary<int, Type> eventNodeBindingDic = new Dictionary<int, Type>()
        {
            // 玩家
            { BuiltinActionDefineID.SetPlayerGem, typeof(HearthStoneRule.SetGemEventArg) },    // 设置当前灵力值
            { BuiltinActionDefineID.DrawTargetCard, typeof(HearthStoneRule.DrawEventArg) },    // 抽目标卡牌
            { BuiltinActionDefineID.AddPlayerGem, typeof(HearthStoneRule.SetGemEventArg) },    // 增加当前灵力值
            { BuiltinActionDefineID.SetPlayerMaxGem, typeof(HearthStoneRule.SetMaxGemEventArg) },    // 设置灵力上限
            { BuiltinActionDefineID.AddPlayerMaxGem, typeof(HearthStoneRule.SetMaxGemEventArg) },    // 增加灵力上限
            { BuiltinActionDefineID.FaithDuel_Obsolete, typeof(THHPlayer.RevealDuelEventArg) },    // 信仰对决
            { BuiltinActionDefineID.FaithDuel, typeof(THHPlayer.RevealDuelEventArg) },    // 信仰对决（过时）

            // 卡牌
            { BuiltinActionDefineID.Damage, typeof(THHCard.DamageEventArg) },    // 造成伤害
            { BuiltinActionDefineID.Discard, typeof(THHCard.DiscardEventArg) },    // 丢弃卡牌
            { BuiltinActionDefineID.CreateTokenToField, typeof(HearthStoneRule.CreateTokenEventArg) },    // 创建衍生卡并置入战场
            { BuiltinActionDefineID.SetCardProp, typeof(Card.PropChangeEventArg) },    // 设置属性
            { BuiltinActionDefineID.DealDamageOrSpellDamage, typeof(THHCard.DamageEventArg) },    // 造成伤害或法伤
            { BuiltinActionDefineID.DealDamageSplitAmong, typeof(THHCard.DamageEventArg) },    // 造成伤害或法伤兵分配给目标
            { BuiltinActionDefineID.DealDamageToRandomTargets, typeof(THHCard.DamageEventArg) },    // 对固定数量的随机目标造成伤害或法伤
            { BuiltinActionDefineID.DealSpellDamageToTargets, typeof(THHCard.DamageEventArg) },    // 造成法术伤害
            { BuiltinActionDefineID.DealSpecificSpellDamageToRandomTargets, typeof(THHCard.DamageEventArg) },    // 造成固定法术伤害并分配给随机目标
            { BuiltinActionDefineID.HealTargets, typeof(THHCard.HealEventArg) },    // 治疗目标卡牌
            { BuiltinActionDefineID.Shackle, typeof(THHCard.ShackleEventArg) },    // 禁锢
            { BuiltinActionDefineID.Seal, typeof(THHCard.SilenceEventArg) },    // 沉默
            { BuiltinActionDefineID.AddAmountOfArmor, typeof(THHCard.SetArmorEventArg) },    // 增加固定数量护甲
            { BuiltinActionDefineID.AddArmor, typeof(THHCard.SetArmorEventArg) },    // 增加护甲
            { BuiltinActionDefineID.LoseArmor, typeof(THHCard.SetArmorEventArg) },    // 失去护甲
            { BuiltinActionDefineID.ForceAttack, typeof(THHCard.AttackEventArg) },    // 强制攻击目标
            { BuiltinActionDefineID.AddAttackChance, typeof(Card.PropChangeEventArg) },    // 本回合攻击次数增加一次
            { BuiltinActionDefineID.RevealCard, typeof(HearthStoneRule.RevealEventArg) },    // 揭示卡牌
            { BuiltinActionDefineID.SetCardDefineOfCard, typeof(Card.SetDefineEventArg) },    // 变形为卡牌定义

            // 增益
            { BuiltinActionDefineID.AddBuff, typeof(Card.AddBuffEventArg) },    // 添加增益
            { BuiltinActionDefineID.RemoveBuff, typeof(Card.RemoveBuffEventArg) },    // 移除增益
            { BuiltinActionDefineID.SetBuffProp, typeof(Buff.PropertyChangeEventArg) },    // 设置增益属性

            // 行动
            { BuiltinActionDefineID.SimpleDraw, typeof(HearthStoneRule.DrawEventArg) },    // 简单抽牌
            { BuiltinActionDefineID.MoveToField, typeof(THHPile.MoveCardEventArg) },    // 卡牌置入战场
            { BuiltinActionDefineID.MoveToHand, typeof(THHPile.MoveCardEventArg) },    // 卡牌移回手牌
            { BuiltinActionDefineID.MoveToDeck, typeof(THHPile.MoveCardEventArg) },    // 卡牌洗入牌库
            { BuiltinActionDefineID.MoveToGrave, typeof(THHPile.MoveCardEventArg) },    // 卡牌置入墓地
            { BuiltinActionDefineID.MoveToHead, typeof(THHPile.MoveCardEventArg) },    // 卡牌置入延迟区
            { BuiltinActionDefineID.MoveToItem, typeof(THHPile.MoveCardEventArg) },    // 卡牌装备到道具栏
            { BuiltinActionDefineID.MoveToWarp, typeof(THHPile.MoveCardEventArg) },    // 卡牌移动到暂存区
        };
        public static bool canActionAddEffect(ActionReference actionRef)
        {
            return getActionBindedEventType(actionRef) != null;
        }
        public static Type getActionBindedEventType(ActionReference actionRef)
        {
            if (actionRef.cardPoolId == 0)
            {
                if (eventNodeBindingDic.TryGetValue(actionRef.defineId, out Type type))
                {
                    return type;
                }
            }
            return null;
        }
    }
}
