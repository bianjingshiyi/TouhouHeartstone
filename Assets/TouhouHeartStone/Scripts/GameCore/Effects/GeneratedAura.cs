using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 新版光环类型的效果
    /// </summary>
    [Serializable]
    public class GeneratedAura : GeneratedPassiveEffect, IPileRangedEffect
    {
        #region 公有方法
        /// <summary>
        /// 构造一个光环被动
        /// </summary>
        /// <param name="buffDefineId">光环Buff类型Id</param>
        /// <param name="piles">光环生效范围，只有拥有光环的卡牌在生效范围内时光环才会生效</param>
        /// <param name="ranges">光环作用范围</param>
        /// <param name="affectCondition">光环作用条件</param>
        public GeneratedAura(ActionGraph graph) :
            base(graph)
        {
        }
        public GeneratedAura()
        {
        }
        public override async Task onEnable(IGame game, ICard card, IBuff buff)
        {
            await base.onEnable(game, card, buff);
            await updateAura(game, card, buff, null);
        }
        public override async Task onDisable(IGame game, ICard card, IBuff buff)
        {
            await base.onDisable(game, card, buff);
            await updateAura(game, card, buff, null);
        }
        public override bool checkCondition(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            return true;
        }
        public override Task execute(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            return updateAura(game, card, buff, eventArg);
        }
        public async Task updateAura(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            CardEngine cardEngine = game as CardEngine;
            Card selfCard = card as Card;
            Buff selfBuff = buff as Buff;
            EventArg selfEventArg = eventArg as EventArg;
            Dictionary<Card, Buff> buffDict = getBuffDic(cardEngine, selfCard, selfBuff);
            // 如果该光环已被禁用。
            if (isDisabled(game, card, buff) || 
                (selfCard != null && 
                (pileList == null || !pileList.Contains(selfCard.pile.name))))
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
            IEnumerable<Pile> ranges = null;
            if (rangeList != null && rangeList.Count > 0 && cardEngine != null)
            {
                ranges = cardEngine.players.SelectMany(p => p.getPiles(rangeList));
            }

            // 检测已经记录的卡牌和增益列表。
            await updateBuffedCards(game, card, buff, buffDict, ranges);

            // 检测范围内的所有卡牌。
            await checkAndAddBuffedCards(game, card, buff, buffDict, ranges, buffDict.Keys);
        }
        public ActionNode getEntryNode()
        {
            return graph?.findActionNode(AuraDataDefine.defName);
        }
        public override SerializableEffect Serialize()
        {
            return new SerializableAuraEffect(this);
        }
        #endregion
        #region 私有方法
        protected override void createEntryNode()
        {
            graph.createActionNode(AuraDataDefine.defName, 0, 0);
        }

        private async Task updateBuffedCards(IGame game, ICard card, IBuff buff, Dictionary<Card, Buff> buffDict, IEnumerable<Pile> ranges)
        {
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
                if (ranges == null || !ranges.Contains(buffedCard.pile) || !await checkAuraCondition(game, card, buff, buffedCard))
                {
                    // 移除该卡牌的增益，并删除记录。
                    await buffedCard.removeBuff(game, cardBuff);
                    buffDict.Remove(buffedCard);
                    continue;
                }
                // 卡牌仍然具有增益条件。
                // 更新增益。
                await updateAuraBuff(game, card, buff, buffedCard, cardBuff);
                await buffedCard.updateBuff(game as CardEngine, cardBuff);
            }
        }
        private async Task checkAndAddBuffedCards(IGame game, ICard card, IBuff buff, Dictionary<Card, Buff> buffDict, IEnumerable<Pile> ranges, IEnumerable<Card> excludedCards)
        {
            if (ranges == null)
                return;

            foreach (var target in ranges.SelectMany(r => r).Except(excludedCards))
            {
                // 卡牌进入作用范围
                if (!buffDict.ContainsKey(target) && ranges.Contains(target.pile) && await checkAuraCondition(game, card, buff, target))
                {
                    // 添加增益，并记录增益。
                    var flow = new Flow(game, card, buff, null);
                    BuffDefine buffDefine = await game.getValue<BuffDefine>(flow, getAuraBuffDefine);
                    Buff auraBuff = new GeneratedBuff(buffDefine);
                    await updateAuraBuff(game, card, buff, target, auraBuff);
                    await target.addBuff(game, auraBuff);
                    buffDict.Add(target, auraBuff);
                }
            }
        }
        protected override IEnumerable<ITraversable> getTraversableProps()
        {
            yield return getAuraBuffDefine;
            yield return affectCondition;
            yield return updateAction;
        }
        private async Task<bool> checkAuraCondition(IGame game, ICard card, IBuff buff, Card targetCard)
        {
            if (affectCondition == null)
                return true;
            try
            {
                var flowEnv = new FlowEnv(game, card, buff, null);
                flowEnv.SetArgument(AuraDataDefine.targetCardName, targetCard);
                var flow = new Flow(flowEnv);
                return await game.getValue<bool>(flow, affectCondition);
            }
            catch (Exception e)
            {
                string msg = buff != null ?
                    $"{card}的{buff}的光环效果{this}的作用条件执行失败" :
                    $"{card}的光环效果{this}的作用条件执行失败";
                throw new TaskNotCompleteException($"{msg}：{e}");
            }
        }
        private async Task updateAuraBuff(IGame game, ICard card, IBuff buff, Card targetCard, Buff targetBuff)
        {
            if (updateAction != null)
            {
                try
                {
                    var flowEnv = new FlowEnv(game, card, buff, null);
                    flowEnv.SetArgument(AuraDataDefine.targetCardName, targetCard);
                    flowEnv.SetArgument(AuraDataDefine.targetBuffName, targetBuff);
                    var flow = new Flow(flowEnv);
                    await game.runActions(flow, updateAction);
                }
                catch (Exception e)
                {
                    game.logger?.logError($"更新{card}({buff})的光环增益时出现异常：{e}");
                }
            }
        }
        private Dictionary<Card, Buff> getBuffDic(CardEngine game, Card card, Buff buff)
        {
            var propName = "BuffDict";
            string effectStr = Array.IndexOf(card.define.getEffects(), this).ToString();
            propName = $"Aura{effectStr}/{propName}";
            if (buff != null)
            {
                propName = $"Buff{buff.instanceID}/{propName}";
            }

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
        /// <summary>
        /// 光环的生效范围
        /// </summary>
        protected virtual PileNameCollection pileList => getEntryNode().getConst<PileNameCollection>(AuraDataDefine.pileListName);
        /// <summary>
        /// 光环的作用范围
        /// </summary>
        public PileNameCollection rangeList => getEntryNode()?.getConst<PileNameCollection>(AuraDataDefine.rangeListName);
        /// <summary>
        /// 光环所添加的增益Id
        /// </summary>
        public ValueInput getAuraBuffDefine => getEntryNode()?.getInputPort<ValueInput>(AuraDataDefine.buffDefineName);
        /// <summary>
        /// 光环的作用条件
        /// </summary>
        public ValueInput affectCondition => getEntryNode()?.getInputPort<ValueInput>(AuraDataDefine.conditionName);
        /// <summary>
        /// 光环的更新动作
        /// </summary>
        public ControlOutput updateAction => getEntryNode()?.getOutputPort<ControlOutput>(AuraDataDefine.updateActionName);
        public override ControlOutput onEnableAction => null;
        public override ControlOutput onDisableAction => null;
        public string[] piles => pileList?.ToArray() ?? Array.Empty<string>();
        #endregion
    }
    [Serializable]
    public class SerializableAuraEffect : SerializableEffect
    {
        public SerializableAuraEffect(GeneratedAura effect)
        {
            name = effect.name;
            propDict = effect.propDict;
            graph = new SerializableActionNodeGraph(effect.graph);
        }

        public override GeneratedEffect Deserialize(INodeDefiner nodeDefiner)
        {
            GeneratedAura effect = new GeneratedAura();
            effect.name = name;
            effect.propDict = propDict;
            effect.graph = graph.toActionGraph(nodeDefiner);
            return effect;
        }
    }
}