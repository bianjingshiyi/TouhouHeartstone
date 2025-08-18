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
            Dictionary<int, string[]> dic = getOrLoadNodeBindingDic();
            if (dic.TryGetValue(defineId, out string[] names))
            {
                return names.Select(n => new EventReference(0, n)).ToArray();
            }
            return Array.Empty<EventReference>();
        }
        /// <summary>
        /// 获取一个<b>内置</b>动作所绑定的所有事件引用。（包括事件的子事件）
        /// </summary>
        /// <param name="defineId">内置动作ID。</param>
        /// <returns></returns>
        public EventReference[] getBuiltinActionBindedEventsRecursive(int defineId)
        {
            EventReference[] eventRefs = getBuiltinActionBindedEvents(defineId);
            if (eventRefs == null || eventRefs.Length < 1)
                return eventRefs;
            List<EventReference> allEvents = new List<EventReference>();
            foreach (EventReference eventRef in eventRefs)
            {
                if (allEvents.Contains(eventRef))
                    continue;
                allEvents.Add(eventRef);
                getEventReferenceChildrenNonAlloc(eventRef, new ICardPoolReference[] { CardManager.builtinCardPoolReference }, allEvents);
            }
            return allEvents.ToArray();
        }
        public EventReference[] getActionChildrenEvents(GeneratedActionDefine actionDefine)
        {
            List<EventReference> list = new List<EventReference>();
            getActionChildrenEventsNonAlloc(actionDefine, list);
            return list.ToArray();
        }
        /// <summary>
        /// 根据自定义动作的节点图内的节点，生成该自定义动作所具有的所有可能的事件引用。
        /// </summary>
        /// <param name="generatedActionDefine">自定义动作。</param>
        /// <param name="list">传入列表。</param>
        public void getActionChildrenEventsNonAlloc(GeneratedActionDefine generatedActionDefine, List<EventReference> list)
        {
            foreach (Node node in generatedActionDefine.graph.nodes)
            {
                if (node is ActionNode action)
                {
                    ActionReference actionRef = action.defineRef;
                    ActionDefine actionDefine = game.card.getLocalActionDefine(actionRef);
                    if (actionDefine is GeneratedActionDefine gen && gen.type == NodeDefineType.Event) // 自定义事件。
                    {
                        EventReference eventRef = CardManager.getEventReferenceOfAction(gen);
                        if (list.Contains(eventRef))
                            continue;
                        list.Add(eventRef);
                        getActionChildrenEventsNonAlloc(gen, list);
                    }
                    else if (game.card.isBuiltinCardPool(actionRef.cardPoolId)) // 内置节点事件。
                    {
                        game.card.getLocalActionBindedEventsRecursive(actionRef);
                        EventReference[] events = getBuiltinActionBindedEventsRecursive(actionRef.defineId);
                        foreach (EventReference eventRef in events)
                        {
                            if (list.Contains(eventRef))
                                continue;
                            list.Add(eventRef);
                        }
                    }
                }
            }
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
            { BuiltinActionDefineID.SetPlayerGem, typeof(SetGemEventDefine) },    // 设置当前灵力值
            { BuiltinActionDefineID.DrawTargetCard, typeof(DrawEventDefine) },    // 抽目标卡牌
            { BuiltinActionDefineID.AddPlayerGem, typeof(SetGemEventDefine) },    // 增加当前灵力值
            { BuiltinActionDefineID.SetPlayerMaxGem, typeof(SetMaxGemEventDefine) },    // 设置灵力上限
            { BuiltinActionDefineID.AddPlayerMaxGem, typeof(SetMaxGemEventDefine) },    // 增加灵力上限
            { BuiltinActionDefineID.FaithDuel_Obsolete, typeof(RevealDuelEventDefine) },    // 信仰对决
            { BuiltinActionDefineID.FaithDuel, typeof(RevealDuelEventDefine) },    // 信仰对决（过时）

            // 卡牌
            { BuiltinActionDefineID.Damage, typeof(DamageEventDefine) },    // 造成伤害
            { BuiltinActionDefineID.Discard, typeof(DiscardEventDefine) },    // 丢弃卡牌
            { BuiltinActionDefineID.CreateTokenToField, typeof(EnterFieldEventDefine) },    // 创建衍生卡并置入战场
            { BuiltinActionDefineID.CreateTokenToHand, typeof(MoveCardEventDefine) },    // 创建衍生卡并置入手牌
            { BuiltinActionDefineID.CreateTokenToDeck, typeof(MoveCardEventDefine) },    // 创建衍生卡并洗入牌库
            { BuiltinActionDefineID.CreateTokenToItem, typeof(EquipItemEventDefine) },    // 创建衍生卡并装备到道具栏
            { BuiltinActionDefineID.SetCardProp, typeof(CardPropChangeEventDefine) },    // 设置属性
            { BuiltinActionDefineID.DealDamageOrSpellDamage, typeof(DamageEventDefine) },    // 造成伤害或法伤
            { BuiltinActionDefineID.DealDamageSplitAmong, typeof(DamageEventDefine) },    // 造成伤害或法伤兵分配给目标
            { BuiltinActionDefineID.DealDamageToRandomTargets, typeof(DamageEventDefine) },    // 对固定数量的随机目标造成伤害或法伤
            { BuiltinActionDefineID.DealSpellDamageToTargets, typeof(DamageEventDefine) },    // 造成法术伤害
            { BuiltinActionDefineID.DealSpecificSpellDamageToRandomTargets, typeof(DamageEventDefine) },    // 造成固定法术伤害并分配给随机目标
            { BuiltinActionDefineID.HealTargets, typeof(HealEventDefine) },    // 治疗目标卡牌
            { BuiltinActionDefineID.Shackle, typeof(ShackleEventDefine) },    // 禁锢
            { BuiltinActionDefineID.Seal, typeof(SilenceEventDefine) },    // 沉默
            { BuiltinActionDefineID.Kill, typeof(CardPropChangeEventDefine) },    // 消灭
            { BuiltinActionDefineID.AddAmountOfArmor, typeof(SetArmorEventDefine) },    // 增加固定数量护甲
            { BuiltinActionDefineID.AddArmor, typeof(SetArmorEventDefine) },    // 增加护甲
            { BuiltinActionDefineID.LoseArmor, typeof(SetArmorEventDefine) },    // 失去护甲
            { BuiltinActionDefineID.ForceAttack, typeof(AttackEventDefine) },    // 强制攻击目标
            { BuiltinActionDefineID.AddAttackChance, typeof(CardPropChangeEventDefine) },    // 本回合攻击次数增加一次
            { BuiltinActionDefineID.RevealCard, typeof(RevealEventDefine) },    // 揭示卡牌
            { BuiltinActionDefineID.SetCardDefineOfCard, typeof(SetCardDefineEventDefine) },    // 变形为卡牌定义

            // 增益
            { BuiltinActionDefineID.AddBuff, typeof(AddBuffEventDefine) },    // 添加增益
            { BuiltinActionDefineID.RemoveBuff, typeof(RemoveBuffEventDefine) },    // 移除增益
            { BuiltinActionDefineID.SetBuffProp, typeof(BuffPropertyChangeEventDefine) },    // 设置增益属性

            // 行动
            { BuiltinActionDefineID.SimpleDraw, typeof(DrawEventDefine) },    // 简单抽牌
            { BuiltinActionDefineID.MoveToField, typeof(EnterFieldEventDefine) },    // 卡牌置入战场
            { BuiltinActionDefineID.MoveToHand, typeof(MoveCardEventDefine) },    // 卡牌移回手牌
            { BuiltinActionDefineID.MoveToDeck, typeof(MoveCardEventDefine) },    // 卡牌洗入牌库
            { BuiltinActionDefineID.MoveToGrave, typeof(MoveCardEventDefine) },    // 卡牌置入墓地
            { BuiltinActionDefineID.MoveToHead, typeof(MoveCardEventDefine) },    // 卡牌置入延迟区
            { BuiltinActionDefineID.MoveToItem, typeof(MoveCardEventDefine) },    // 卡牌装备到道具栏
            { BuiltinActionDefineID.MoveToWarp, typeof(MoveCardEventDefine) },    // 卡牌移动到暂存区
        };
        private Dictionary<int, string[]> eventNodeBindingDic;
        #endregion
    }
}
