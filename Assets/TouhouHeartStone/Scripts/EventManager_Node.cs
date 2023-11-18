using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;

namespace Game
{
    partial class EventManager
    {
        #region 公有方法
        /// <summary>
        /// 获取一个<b>内置</b>动作所绑定的所有直接事件引用。（不包括事件的子事件）
        /// </summary>
        /// <param name="defineId">内置动作ID。</param>
        /// <returns></returns>
        public EventReference[] getBuiltinActionBindedEvents(int defineId)
        {
            var dic = getOrLoadNodeBindingDic();
            if (dic.TryGetValue(defineId, out string[] names))
            {
                return names.Select(n => new EventReference(0, n)).ToArray();
            }
            return null;
        }
        /// <summary>
        /// 获取一个<b>内置</b>动作所绑定的所有事件引用。（包括事件的子事件）
        /// </summary>
        /// <param name="defineId">内置动作ID。</param>
        /// <returns></returns>
        public EventReference[] getBuiltinActionBindedEventsRecursive(int defineId)
        {
            var eventRefs = getBuiltinActionBindedEvents(defineId);
            if (eventRefs == null)
                return Array.Empty<EventReference>();
            List<EventReference> names = new List<EventReference>();
            foreach (var eventRef in eventRefs)
            {
                if (names.Contains(eventRef))
                    continue;
                names.Add(eventRef);
                getEventReferenceChildrenNonAlloc(eventRef, new ICardPoolReference[] { CardManager.builtinCardPoolReference }, names);
            }
            return names.ToArray();
        }
        [Obsolete]
        public EventReference getDefaultNodeEvent(Node node)
        {
            if (node is not ActionNode action)
                return null;
            foreach (var pair in eventNodeDefaultBindingDic)
            {
                if (action.defineRef.isBuiltinReference(pair.Key))
                {
                    return new EventReference(0, EventHelper.getName(pair.Value));
                }
            }
            return null;
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 根据自定义动作的节点图内的节点，生成该自定义动作所具有的所有可能的事件引用。
        /// </summary>
        /// <param name="generatedActionDefine">自定义动作。</param>
        /// <param name="list">传入列表。</param>
        private void generateActionChildrenEventsNonAlloc(GeneratedActionDefine generatedActionDefine, List<EventReference> list)
        {
            foreach (var node in generatedActionDefine.graph.nodes)
            {
                if (node is ActionNode action)
                {
                    var actionRef = action.defineRef;
                    var actionDefine = game.card.getLocalActionDefine(actionRef);
                    if (actionDefine is GeneratedActionDefine gen && gen.type == NodeDefineType.Event) // 自定义事件。
                    {
                        var eventRef = CardManager.getEventReferenceOfAction(gen);
                        if (list.Contains(eventRef))
                            continue;
                        generateActionChildrenEventsNonAlloc(gen, list);
                    }
                    else if (game.card.isBuiltinCardPool(actionRef.cardPoolId)) // 内置节点事件。
                    {
                        game.card.getLocalActionBindedEventsRecursive(actionRef);
                        var names = getBuiltinActionBindedEventsRecursive(actionRef.defineId);
                        foreach (var name in names)
                        {
                            if (list.Contains(name))
                                continue;
                            list.Add(name);
                        }
                    }
                }
            }
        }
        private Dictionary<int, string[]> getOrLoadNodeBindingDic()
        {
            if (eventNodeBindingDic == null)
            {
                var assemblies = new Assembly[] { typeof(EffectEditorLibrary).Assembly };
                eventNodeBindingDic = loadNodeBindingDic(assemblies);
            }
            return eventNodeBindingDic;
        }
        private Dictionary<int, string[]> loadNodeBindingDic(Assembly[] assemblies)
        {
            Dictionary<int, string[]> bindingDict = new Dictionary<int, string[]>();
            var methods = assemblies.SelectMany(a => a.GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)));
            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<EventNodeAttribute>() is EventNodeAttribute attribute &&
                    method.GetCustomAttribute<ActionNodeMethodAttribute>() is ActionNodeMethodAttribute methodAttribute)
                {
                    bindingDict.Add(methodAttribute.defineId, attribute.types.Select(t => EventHelper.getName(t)).ToArray());
                }
            }
            return bindingDict;
        }
        #endregion

        #region 属性字段
        [Obsolete]
        private static Dictionary<int, Type> eventNodeDefaultBindingDic = new Dictionary<int, Type>()
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
            { BuiltinActionDefineID.CreateTokenToField, typeof(HearthStoneRule.EnterFieldEventArg) },    // 创建衍生卡并置入战场
            { BuiltinActionDefineID.CreateTokenToHand, typeof(THHPile.MoveCardEventArg) },    // 创建衍生卡并置入手牌
            { BuiltinActionDefineID.CreateTokenToDeck, typeof(THHPile.MoveCardEventArg) },    // 创建衍生卡并洗入牌库
            { BuiltinActionDefineID.CreateTokenToItem, typeof(HearthStoneRule.EquipItemEventArg) },    // 创建衍生卡并装备到道具栏
            { BuiltinActionDefineID.SetCardProp, typeof(Card.PropChangeEventArg) },    // 设置属性
            { BuiltinActionDefineID.DealDamageOrSpellDamage, typeof(THHCard.DamageEventArg) },    // 造成伤害或法伤
            { BuiltinActionDefineID.DealDamageSplitAmong, typeof(THHCard.DamageEventArg) },    // 造成伤害或法伤兵分配给目标
            { BuiltinActionDefineID.DealDamageToRandomTargets, typeof(THHCard.DamageEventArg) },    // 对固定数量的随机目标造成伤害或法伤
            { BuiltinActionDefineID.DealSpellDamageToTargets, typeof(THHCard.DamageEventArg) },    // 造成法术伤害
            { BuiltinActionDefineID.DealSpecificSpellDamageToRandomTargets, typeof(THHCard.DamageEventArg) },    // 造成固定法术伤害并分配给随机目标
            { BuiltinActionDefineID.HealTargets, typeof(THHCard.HealEventArg) },    // 治疗目标卡牌
            { BuiltinActionDefineID.Shackle, typeof(THHCard.ShackleEventArg) },    // 禁锢
            { BuiltinActionDefineID.Seal, typeof(THHCard.SilenceEventArg) },    // 沉默
            { BuiltinActionDefineID.Kill, typeof(Card.PropChangeEventArg) },    // 消灭
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
            { BuiltinActionDefineID.MoveToField, typeof(HearthStoneRule.EnterFieldEventArg) },    // 卡牌置入战场
            { BuiltinActionDefineID.MoveToHand, typeof(THHPile.MoveCardEventArg) },    // 卡牌移回手牌
            { BuiltinActionDefineID.MoveToDeck, typeof(THHPile.MoveCardEventArg) },    // 卡牌洗入牌库
            { BuiltinActionDefineID.MoveToGrave, typeof(THHPile.MoveCardEventArg) },    // 卡牌置入墓地
            { BuiltinActionDefineID.MoveToHead, typeof(THHPile.MoveCardEventArg) },    // 卡牌置入延迟区
            { BuiltinActionDefineID.MoveToItem, typeof(THHPile.MoveCardEventArg) },    // 卡牌装备到道具栏
            { BuiltinActionDefineID.MoveToWarp, typeof(THHPile.MoveCardEventArg) },    // 卡牌移动到暂存区
        };
        private Dictionary<int, string[]> eventNodeBindingDic;
        #endregion
    }
}
