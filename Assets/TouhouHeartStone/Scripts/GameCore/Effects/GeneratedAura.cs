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
        public GeneratedAura(ActionGraph graph) : base(graph)
        {
        }
        public override bool checkCondition(EffectEnv env)
        {
            return true;
        }
        #region 获取值
        /// <summary>
        /// 光环的生效范围
        /// </summary>
        public PileNameRange getPileList()
        {
            return auraEntry?.getPiles();
        }
        /// <summary>
        /// 光环的作用范围
        /// </summary>
        public PileNameRange getRangeList()
        {
            return auraEntry?.getRanges();
        }
        public override string[] getPiles() => getPileList()?.ToArray() ?? Array.Empty<string>();
        #endregion
        public async Task updateAura(CardEngine game, ICard card, IBuff buff, IEventArg eventArg)
        {
            Card selfCard = card as Card;
            Buff selfBuff = buff as Buff;
            EventArg selfEventArg = eventArg as EventArg;
            Dictionary<Card, Buff> buffDict = await getBuffDic(game, selfCard, selfBuff);
            var pileList = getPileList();
            // 如果该光环已被禁用。
            if (isDisabled(game, card, buff) || 
                (buff == null && card.isSilenced(game))||
                (selfCard != null && (pileList == null || !pileList.ContainsPileOrAny(selfCard.pile.name))))
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
            var rangeList = getRangeList();

            // 更新所有增益。
            await updateBuffCards(game, card, buff, buffDict, rangeList);
        }
        public override Task execute(EffectEnv env)
        {
            return updateAura(env.game, env.card, env.buff, env.eventArg);
        }
        public override Effect Clone()
        {
            ActionGraph newGraph = graph?.Clone();
            GeneratedAura newEffect = new GeneratedAura(newGraph);
            newEffect.name = name;
            if (propDict != null && propDict.Count > 0)
            {
                newEffect.propDict = propDict.Clone();
            }
            return newEffect;
        }
        #endregion
        #region 私有方法
        protected override async Task onEnable(EffectEnv env)
        {
            await base.onEnable(env);
            await updateAura(env.game, env.card, env.buff, null);
        }
        protected override async Task onDisable(EffectEnv env)
        {
            await base.onDisable(env);
            await updateAura(env.game, env.card, env.buff, null);
        }
        protected override void createEntryNode()
        {
            graph.createAuraEntryNode(0, 0);
        }
        private async Task updateBuffCards(CardEngine game, ICard card, IBuff buff, Dictionary<Card, Buff> buffDict, IEnumerable<string> rangeList)
        {
            var ranges = game.getSortedPlayers().SelectMany(p => p.getPiles()).Where(p => rangeList.ContainsPileOrAny(p?.name));
            var allCards = buffDict.Keys.Union(ranges.SelectMany(r => r)).ToArray();
            foreach (var targetCard in allCards)
            {
                // 检测增益条件。
                bool shouldContinueBuff = rangeList != null && rangeList.ContainsPileOrAny(targetCard.pile?.name) && await checkAuraCondition(game, card, buff, targetCard);
                // 卡牌不再符合增益条件。
                if (!shouldContinueBuff)
                {
                    if (buffDict.TryGetValue(targetCard, out var cardBuff))
                    {
                        // 移除该卡牌的增益，并删除记录。
                        await targetCard.removeBuff(game, cardBuff);
                        buffDict.Remove(targetCard);
                    }
                }
                else
                {
                    if (!buffDict.TryGetValue(targetCard, out var cardBuff))
                    {
                        // 记录中找不到目标增益，新建一个。
                        var flow = new Flow(game, card, buff, null, this);
                        BuffDefine buffDefine = await game.getValue<BuffDefine>(flow, getAuraBuffDefine);
                        cardBuff = new Buff(buffDefine);
                        cardBuff.canClone = false;
                        buffDict.Add(targetCard, cardBuff);
                    }

                    // 如果增益被通过某种方式除掉了（如沉默），重新添加。
                    if (!targetCard.containsBuff(cardBuff))
                        await targetCard.addBuff(game, cardBuff);

                    // 卡牌仍然具有增益条件。
                    // 更新增益。
                    await updateAuraBuff(game, card, buff, targetCard, cardBuff);
                }
            }
        }
        protected override IEnumerable<ITraversable> getTraversableProps()
        {
            yield return getAuraBuffDefine;
            yield return affectCondition;
            yield return updateAction;
        }
        private async Task<bool> checkAuraCondition(CardEngine game, ICard card, IBuff buff, Card targetCard)
        {
            if (affectCondition == null || !affectCondition.hasValue())
                return false;
            try
            {
                var flowEnv = new FlowEnv(game, card, buff, null, this);
                flowEnv.SetArgument(AuraEntryNode.targetCardName, targetCard);
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
        private async Task updateAuraBuff(CardEngine game, ICard card, IBuff buff, Card targetCard, Buff targetBuff)
        {
            if (updateAction != null)
            {
                try
                {
                    var flowEnv = new FlowEnv(game, card, buff, null, this);
                    flowEnv.SetArgument(AuraEntryNode.targetCardName, targetCard);
                    flowEnv.SetArgument(AuraEntryNode.targetBuffName, targetBuff);
                    var flow = new Flow(flowEnv);
                    await game.runActions(flow, updateAction);
                }
                catch (Exception e)
                {
                    game.logger?.logError($"更新{card}({buff})的光环增益时出现异常：{e}");
                }
            }
        }
        private async Task<Dictionary<Card, Buff>> getBuffDic(CardEngine game, Card card, Buff buff)
        {
            var cardId = card.id;
            Dictionary<Card, Buff> buffDict;
            if (buff != null)
            {
                var effectIndex = Array.IndexOf(buff.getEffects(), this);
                var propName = $"Card{cardId}/Buff{buff.instanceID}/Aura{effectIndex}/BuffDict";
                buffDict = buff.getProp<Dictionary<Card, Buff>>(propName);
                if (buffDict == null)
                {
                    buffDict = new Dictionary<Card, Buff>();
                    await buff.setProp(game, propName, buffDict);
                }
            }
            else
            {
                var effectIndex = Array.IndexOf(card.define.getEffects(), this);
                var propName = $"Card{cardId}/Aura{effectIndex}/BuffDict";
                buffDict = card.getProp<Dictionary<Card, Buff>>(game, propName);
                if (buffDict == null)
                {
                    buffDict = new Dictionary<Card, Buff>();
                    await card.setProp(game, propName, buffDict);
                }
            }
            return buffDict;
        }
        #endregion
        #region 属性字段
        public AuraEntryNode auraEntry => graph?.findAuraEntryNode();
        /// <summary>
        /// 光环所添加的增益Id
        /// </summary>
        public ValueInput getAuraBuffDefine => auraEntry?.getInputPort<ValueInput>(AuraEntryNode.buffDefineName);
        /// <summary>
        /// 光环的作用条件
        /// </summary>
        public ValueInput affectCondition => auraEntry?.getInputPort<ValueInput>(AuraEntryNode.conditionName);
        /// <summary>
        /// 光环的更新动作
        /// </summary>
        public ControlOutput updateAction => auraEntry?.getOutputPort<ControlOutput>(AuraEntryNode.actionPortName);
        public override ControlOutput onEnableAction => null;
        public override ControlOutput onDisableAction => null;
        public override Node entryNode => auraEntry;
        #endregion
    }
}