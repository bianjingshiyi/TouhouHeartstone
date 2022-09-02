using System;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace TouhouHeartstone
{
    /// <summary>
    /// 光环类型的效果
    /// </summary>
    [Serializable]
    public class GeneratedHalo : GeneratedEffect
    {
        #region 公有方法
        /// <summary>
        /// 构造一个光环被动
        /// </summary>
        /// <param name="buffDefineId">光环Buff类型Id</param>
        /// <param name="piles">光环生效范围，只有拥有光环的卡牌在生效范围内时光环才会生效</param>
        /// <param name="ranges">光环作用范围</param>
        /// <param name="affectCondition">光环作用条件</param>
        public GeneratedHalo(int buffDefineId, IEnumerable<string> piles, IEnumerable<string> ranges, ActionValueRef affectCondition) : base(piles, null, null, null, null)
        {
            getHaloBuffDefine = new ActionValueRef(new ActionNode("GetBuffDefine", new object[] { buffDefineId }));
            if (ranges != null)
                rangeList.AddRange(ranges);
            this.affectCondition = affectCondition;
        }
        public GeneratedHalo() : this(0, null, null, null)
        {
        }
        public override async Task onEnable(IGame game, ICard card, IBuff buff)
        {
            //获取光环的作用范围
            Pile[] ranges = rangeList != null && rangeList.Count > 0 ? (card as Card).getOwner().getPiles(rangeList) : new Pile[0];
            //注册卡片入场触发器
            Trigger<Pile.MoveCardEventArg> trigger = new Trigger<Pile.MoveCardEventArg>(
                eventArg =>
                {
                    return ranges == null || ranges.Length < 1;
                },
                async eventArg =>
                {
                    Dictionary<Card, Buff> buffDict = getBuffDic(game as CardEngine, card as Card, buff as Buff);
                    if (ranges.Contains(eventArg.to))
                    {
                        //卡牌进入作用范围
                        if (affectCondition != null)
                        {
                            Task<bool> task = (game as CardEngine).getActionReturnValueAsync<bool>(eventArg.card as Card, null, eventArg, affectCondition.action, affectCondition.index);
                            if (!task.IsCompleted)
                                throw new TaskNotCompleteException(buff != null ?
                                    card + "的" + buff + "的光环效果" + this + "的作用条件没有立刻返回值" :
                                    card + "的光环效果" + this + "的作用条件没有立刻返回值");
                            if (!task.Result)
                            {
                                //不符合作用条件，忽略
                                return;
                            }
                        }
                        //为入场卡牌添加光环增益并记录
                        if (!buffDict.ContainsKey(eventArg.card))
                        {
                            BuffDefine buffDefine = await (game as CardEngine).getActionReturnValueAsync<BuffDefine>(card as Card, buff as Buff, eventArg, getHaloBuffDefine.action, getHaloBuffDefine.index);
                            GeneratedBuff haloBuff = new GeneratedBuff(buffDefine);
                            await eventArg.card.addBuff(game, haloBuff);
                            buffDict.Add(eventArg.card, haloBuff);
                        }
                    }
                    else if (ranges.Contains(eventArg.from))
                    {
                        //卡牌离开作用范围
                        if (buffDict.ContainsKey(eventArg.card))
                        {
                            await eventArg.card.removeBuff(game, buffDict[eventArg.card]);
                            buffDict.Remove(eventArg.card);
                        }
                    }
                });
            //记录入场触发器，以便在效果失效的时候注销
            setHaloMoveCardTrigger(game, card, buff, trigger);
            game.triggers.registerAfter(trigger);
            foreach (var range in ranges)
            {
                Dictionary<Card, Buff> buffDict = getBuffDic(game as CardEngine, card as Card, buff as Buff);
                foreach (var target in range)
                {
                    if (affectCondition != null)
                    {
                        Task<bool> task = (game as CardEngine).getActionReturnValueAsync<bool>(target, null, null, affectCondition.action, affectCondition.index);
                        if (!task.IsCompleted)
                            throw new TaskNotCompleteException(buff != null ?
                                card + "的" + buff + "的光环效果" + this + "的作用条件没有立刻返回值" :
                                card + "的光环效果" + this + "的作用条件没有立刻返回值");
                        if (!task.Result)
                            continue;
                    }
                    if (!buffDict.ContainsKey(target))
                    {
                        BuffDefine buffDefine = await (game as CardEngine).getActionReturnValueAsync<BuffDefine>(card as Card, buff as Buff, null, getHaloBuffDefine.action, getHaloBuffDefine.index);
                        Buff haloBuff = new GeneratedBuff(buffDefine);
                        await target.addBuff(game, haloBuff);
                        buffDict.Add(target, haloBuff);
                    }
                }
            }
        }
        public override async Task onDisable(IGame game, ICard card, IBuff buff)
        {
            Dictionary<Card, Buff> buffDict = getBuffDic(game as CardEngine, card as Card, buff as Buff);
            if (getHaloMoveCardTrigger(game, card, buff) != null)
            {
                game.triggers.removeAfter(getHaloMoveCardTrigger(game, card, buff));
                setHaloMoveCardTrigger(game, card, buff, null);
            }
            foreach (var pair in buffDict)
            {
                await pair.Key.removeBuff(game, pair.Value);
            }
            buffDict.Clear();
        }
        public override EffectPropertyInfo[] getPropInfos()
        {
            return propertyInfos;
        }
        #endregion
        #region 私有方法
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
        private void setHaloMoveCardTrigger(IGame game, ICard card, IBuff buff, Trigger<Pile.MoveCardEventArg> trigger)
        {
            if (buff != null)
            {
                //触发器属于Buff
                if (buff is GeneratedBuff generatedBuff && (game as CardEngine).getBuffDefine(generatedBuff.defineId) is GeneratedBuffDefine generatedBuffDefine)
                {
                    //GeneratedBuff至少能知道自己的效果所在索引，所以能稍微规避一下这个问题
                    (buff as Buff).setProp(game as CardEngine, "HaloMoveCardTrigger" + generatedBuffDefine.effectList.IndexOf(this), trigger);
                }
                else
                {
                    //Buff不能获取自己的效果，所以当一个Buff有多个光环效果的时候可能引发Bug。
                    (buff as Buff).setProp(game as CardEngine, "HaloMoveCardTrigger", trigger);
                }
            }
            else
            {
                //触发器属于卡牌
                card.setProp(game, "HaloMoveCardTrigger" + Array.IndexOf(card.define.getEffects(), this), trigger);
            }
        }
        private Trigger<Pile.MoveCardEventArg> getHaloMoveCardTrigger(IGame game, ICard card, IBuff buff)
        {
            if (buff != null)
            {
                //触发器属于Buff
                if (buff is GeneratedBuff generatedBuff && (game as CardEngine).getBuffDefine(generatedBuff.defineId) is GeneratedBuffDefine generatedBuffDefine)
                {
                    //GeneratedBuff至少能知道自己的效果所在索引，所以能稍微规避一下这个问题
                    return (buff as Buff).getProp<Trigger<Pile.MoveCardEventArg>>(game as CardEngine, "HaloMoveCardTrigger" + generatedBuffDefine.effectList.IndexOf(this));
                }
                else
                {
                    //Buff不能获取自己的效果，所以当一个Buff有多个光环效果的时候可能引发Bug。
                    return (buff as Buff).getProp<Trigger<Pile.MoveCardEventArg>>(game as CardEngine, "HaloMoveCardTrigger");
                }
            }
            else
            {
                //触发器属于卡牌
                return card.getProp<Trigger<Pile.MoveCardEventArg>>(game, "HaloMoveCardTrigger" + Array.IndexOf(card.define.getEffects(), this));
            }
        }
        #endregion
        #region 属性字段
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
        /// 光环的作用条件
        /// </summary>
        [BsonIgnore]
        public ActionValueRef affectCondition
        {
            get { return getProp<ActionValueRef>(nameof(affectCondition)); }
            set
            {
                setProp(nameof(affectCondition), value);
            }
        }
        /// <summary>
        /// 光环所添加的增益Id
        /// </summary>
        [BsonIgnore]
        public ActionValueRef getHaloBuffDefine
        {
            get { return getProp<ActionValueRef>(nameof(getHaloBuffDefine)); }
            set
            {
                setProp(nameof(getHaloBuffDefine), value);
            }
        }
        static EffectPropertyInfo[] propertyInfos = new EffectPropertyInfo[]
        {
            new EffectPropertyInfo(typeof(PileNameCollection),nameof(pileList)),
            new EffectPropertyInfo(typeof(PileNameCollection),nameof(rangeList)),
            new EffectPropertyInfo(typeof(ActionValueRef),nameof(affectCondition)),
            new EffectPropertyInfo(typeof(ActionValueRef),nameof(getHaloBuffDefine)),
            new EffectPropertyInfo(typeof(EffectTagCollection),nameof(tagList))
        };
        #endregion
    }
}