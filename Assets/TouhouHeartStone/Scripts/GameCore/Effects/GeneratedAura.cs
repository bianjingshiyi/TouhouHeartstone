using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using UnityEngine;
using static TouhouHeartstone.HearthStoneRule;
using static UnityEngine.GraphicsBuffer;

namespace TouhouHeartstone
{
    /// <summary>
    /// 新版光环类型的效果
    /// </summary>
    [Serializable]
    public class GeneratedAura : GeneratedEffect
    {
        #region 公有方法
        /// <summary>
        /// 构造一个光环被动
        /// </summary>
        /// <param name="buffDefineId">光环Buff类型Id</param>
        /// <param name="piles">光环生效范围，只有拥有光环的卡牌在生效范围内时光环才会生效</param>
        /// <param name="ranges">光环作用范围</param>
        /// <param name="affectCondition">光环作用条件</param>
        public GeneratedAura(int buffDefineId, IEnumerable<string> piles, IEnumerable<string> ranges, FunctionNode condition, FunctionNode updateAction) :
            base(piles, null, null, null, null)
        {
            if (buffDefineId > 0)
                getAuraBuffDefine = new ActionValueRef(new ActionNode("GetBuffDefine", new object[] { buffDefineId }));
            if (ranges != null)
                rangeList.AddRange(ranges);
            if (condition != null)
                affectCondition = condition;
            if (updateAction != null)
                this.updateAction = updateAction;
        }
        public GeneratedAura()
        {
        }
        public override async Task onEnable(IGame game, ICard card, IBuff buff)
        {
            await base.onEnable(game, card, buff);
            await execute(game, card, buff, null);
        }
        public override async Task onDisable(IGame game, ICard card, IBuff buff)
        {
            await base.onDisable(game, card, buff);
            await execute(game, card, buff, null);
        }
        public override EffectPropertyInfo[] getPropInfos()
        {
            return propertyInfos;
        }
        public override object getProp(string name)
        {
            var prop = base.getProp(name);
            if (prop == null)
            {
                if (name == nameof(affectCondition))
                {
                    prop = new FunctionNode(FUNCTION_DEFINE_NAME_AFFECT_CONDITION);
                    setProp(name, prop);
                }
                else if (name == nameof(updateAction))
                {
                    prop = new FunctionNode(FUNCTION_DEFINE_NAME_UPDATE_ACTION);
                    setProp(name, prop);
                }
            }
            return prop;
        }
        public override async Task execute(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            CardEngine cardEngine = game as CardEngine;
            Card selfCard = card as Card;
            Buff selfBuff = buff as Buff;
            EventArg selfEventArg = eventArg as EventArg;
            Dictionary<Card, Buff> buffDict = getBuffDic(cardEngine, selfCard, selfBuff);
            // 如果该光环已被禁用。
            if (isDisabled(game, card, buff) || (selfCard != null && !pileList.Contains(selfCard.pile.name)))
            {
                // 遍历已经记录的卡牌和增益列表。
                foreach (var pair in buffDict)
                {
                    Card buffedCard = pair.Key;
                    Buff cardBuff = pair.Value;
                    // 移除该卡牌的增益。
                    await buffedCard.removeBuff(game, cardBuff);
                }
                buffDict.Clear();
                return;
            }

            //获取光环的作用范围
            Pile[] ranges;
            if (rangeList != null && rangeList.Count > 0 && cardEngine != null)
            {
                ranges = cardEngine.players.SelectMany(p => p.getPiles(rangeList)).ToArray();
            }
            else
            {
                ranges = new Pile[0];
            }

            List<Card> checkedCards = new List<Card>();
            async Task<bool> condition(Card target)
            {
                if (ranges != null && ranges.Contains(target.pile))
                {
                    //if (affectCondition != null)
                    if (affectCondition != null)
                    {
                        try
                        {
                            return await affectCondition.getFunctionReturnValueAsync<bool>(cardEngine, selfCard, selfBuff, null, 0, target);
                        }
                        catch (Exception e)
                        {
                            string msg = buff != null ?
                                card + "的" + buff + "的光环效果" + this + "的作用条件执行失败" :
                                card + "的光环效果" + this + "的作用条件执行失败";
                            throw new TaskNotCompleteException($"{msg}：{e}");
                        }
                    }
                    return true;
                }
                return false;
            }

            // 检测已经记录的卡牌和增益列表。
            foreach (var pair in buffDict.ToArray())
            {
                Card buffedCard = pair.Key;
                Buff cardBuff = pair.Value;
                // 如果增益被通过某种方式除掉了（如沉默），将其删除已记录的卡牌增益列表。
                if (!buffedCard.containBuff(cardBuff))
                {
                    buffDict.Remove(buffedCard);
                    continue;
                }
                // 卡牌不再具有增益条件。
                if (!await condition(buffedCard))
                {
                    // 移除该卡牌的增益，并删除记录。
                    await buffedCard.removeBuff(game, cardBuff);
                    buffDict.Remove(buffedCard);
                }
                // 卡牌仍然具有增益条件。
                else
                {
                    // 更新增益。
                    //await game.triggers.doEvent(new AuraUpdateEventArg(selfCard.getOwner(), selfCard, buffedCard), async arg =>
                    //{
                    //    await buffedCard.updateBuff(cardEngine, cardBuff);
                    //});

                    await updateAuraBuff(cardEngine, selfCard, selfBuff, buffedCard, cardBuff);
                    await buffedCard.updateBuff(cardEngine, cardBuff);
                }
                checkedCards.Add(buffedCard);
            }

            // 检测范围内的所有卡牌。
            if (ranges != null)
            {
                foreach (Pile pile in ranges)
                {
                    foreach (var target in pile)
                    {
                        if (checkedCards.Contains(target))
                        {
                            continue;
                        }
                        // 卡牌进入作用范围
                        if (!buffDict.ContainsKey(target) && await condition(target))
                        {
                            // 添加增益，并记录增益。
                            //await game.triggers.doEvent(new AuraUpdateEventArg(selfCard.getOwner(), selfCard, target), async arg =>
                            //{
                            //    BuffDefine buffDefine = await (game as CardEngine).getActionReturnValueAsync<BuffDefine>(card as Card, buff as Buff, null, getAuraBuffDefine.action, getAuraBuffDefine.index);
                            //    Buff haloBuff = new GeneratedBuff(buffDefine);
                            //    await target.addBuff(game, haloBuff);
                            //    buffDict.Add(target, haloBuff);
                            //});

                            BuffDefine buffDefine = await cardEngine.getActionReturnValueAsync<BuffDefine>(selfCard, selfBuff, null, getAuraBuffDefine.action, getAuraBuffDefine.index);
                            Buff auraBuff = new GeneratedBuff(buffDefine);
                            await updateAuraBuff(cardEngine, selfCard, selfBuff, target, auraBuff);
                            await target.addBuff(game, auraBuff);
                            buffDict.Add(target, auraBuff);
                        }
                    }
                }
            }
        }
        #endregion
        #region 私有方法
        private async Task updateAuraBuff(CardEngine game, Card card, Buff buff, Card target, Buff addedBuff)
        {
            if (updateAction != null)
            {
                try
                {
                    await updateAction.doFunctionAsync(game, card, buff, null, target, addedBuff);
                }
                catch (Exception e)
                {
                    game.logger?.logError($"更新{card}({buff})的光环增益时出现异常：{e}");
                }
            }
        }
        private Dictionary<Card, Buff> getBuffDic(CardEngine game, Card card, Buff buff)
        {
            const string propName = "BuffDict";
            Dictionary<Card, Buff> buffDict = buff != null ? buff.getProp<Dictionary<Card, Buff>>(game, propName) : card.getProp<Dictionary<Card, Buff>>(game, propName);
            if (buffDict == null)
            {
                buffDict = new Dictionary<Card, Buff>();
                if (buff != null)
                    buff.setProp(game, propName, buffDict);
                else
                    card.setProp(game, propName, buffDict);
            }
            return buffDict;
        }
        #endregion
        #region 属性字段
        public const string FUNCTION_DEFINE_NAME_AFFECT_CONDITION = "GeneratedAura.affectCondition";
        public const string FUNCTION_DEFINE_NAME_UPDATE_ACTION = "GeneratedAura.updateAction";
        /// <summary>
        /// 光环的作用范围
        /// </summary>
        [BsonIgnore]
        public PileNameCollection rangeList
        {
            get
            {
                PileNameCollection value = getProp<PileNameCollection>(nameof(rangeList));
                if (value == null)
                {
                    value = new PileNameCollection();
                    setProp(nameof(rangeList), value);
                }
                return value;
            }
            set
            {
                setProp(nameof(rangeList), value);
            }
        }
        /// <summary>
        /// 光环所添加的增益Id
        /// </summary>
        [BsonIgnore]
        public ActionValueRef getAuraBuffDefine
        {
            get { return getProp<ActionValueRef>(nameof(getAuraBuffDefine)); }
            set
            {
                setProp(nameof(getAuraBuffDefine), value);
            }
        }
        /// <summary>
        /// 光环的作用条件
        /// </summary>
        [BsonIgnore]
        public FunctionNode affectCondition
        {
            get => getProp<FunctionNode>(nameof(affectCondition));
            set => setProp(nameof(affectCondition), value);
        }
        /// <summary>
        /// 光环的更新动作
        /// </summary>
        [BsonIgnore]
        public FunctionNode updateAction
        {
            get => getProp<FunctionNode>(nameof(updateAction));
            set => setProp(nameof(updateAction), value);
        }
        static EffectPropertyInfo[] propertyInfos = new EffectPropertyInfo[]
        {
            new EffectPropertyInfo(typeof(PileNameCollection),nameof(pileList)),
            new EffectPropertyInfo(typeof(PileNameCollection),nameof(rangeList)),
            new EffectPropertyInfo(typeof(FunctionNode),nameof(affectCondition)),
            new EffectPropertyInfo(typeof(ActionValueRef),nameof(getAuraBuffDefine)),
            new EffectPropertyInfo(typeof(FunctionNode),nameof(updateAction)),
        };
        #endregion
    }
}