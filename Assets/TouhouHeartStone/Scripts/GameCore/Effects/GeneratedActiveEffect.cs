using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class GeneratedActiveEffect : GeneratedEffect, IActiveEffect, IEntryNodeEffect
    {
        #region 公有方法
        #region 构造方法
        public GeneratedActiveEffect(ActionGraph graph) : base(graph)
        {
        }
        #endregion

        public override void Init()
        {
            base.Init();
            graph.createActiveEntryNode();
        }
        public override bool checkCondition(EffectEnv env)
        {
            var entryNode = this.entryNode;
            if (entryNode != null)
            {
                var condition = entryNode.getTriggerCondtionValuePort();
                if (condition == null || !condition.hasValue())
                    return true;

                var flow = new Flow(env.toFlowEnv());
                var task = env.game.getValue<bool>(flow, condition);
                if (task.IsCompleted)
                {
                    return task.Result;
                }
                else
                    throw new InvalidOperationException("不能在条件中调用需要等待的动作");
            }
            else
                return false;
        }
        /// <summary>
        /// 检查目标卡牌是否是效果的合法目标
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="buff"></param>
        /// <param name="eventArg"></param>
        /// <param name="invalidMsg"></param>
        /// <returns></returns>
        public virtual bool checkTarget(CardEngine game, Card card, Buff buff, Card[] selectedTargets, Card target, out string invalidMsg)
        {
            invalidMsg = null;
            if (target == null)
                return false;
            var entryNode = this.entryNode;
            if (entryNode == null)
                return false;

            //如果触发不包含任何目标，那么目标肯定不是合法目标
            var targetIndex = selectedTargets?.Length ?? 0;
            if (!hasTargetIndex(targetIndex))
                return false;

            var targetChecker = targetCheckers.ElementAtOrDefault(targetIndex);
            if (targetChecker == null)
                return false;

            //检查目标条件
            if (target.pile?.name == PILE.MASTER && targetChecker.targetType == DEFINE.SERVANT)
                // 目标类型是仆从，但是卡牌是英雄。
                return false;

            FlowEnv env = new FlowEnv(game as CardEngine, card, buff, null, this);
            env.SetArgument(ActiveEntryNode.cardPortName, card);
            env.SetArgument(ActiveEntryNode.playerPortName, card?.getOwner());
            if (selectedTargets != null)
            {
                for (int i = 0; i < selectedTargets.Length; i++)
                {
                    env.SetArgument(ActiveEntryNode.getTargetPortName(i), selectedTargets[i]);
                }
                env.SetArgument(ActiveEntryNode.getTargetPortName(selectedTargets.Length), target);
            }
            if (!targetChecker.isValidTarget(env, out invalidMsg))
                //有条件没有通过，不是合法目标
                return false;

            //有目标并且没有条件不通过或者没有条件，返回真
            return true;
        }
        public bool hasTargetIndex(int index)
        {
            return targetCheckers != null && targetCheckers.Count() > index;
        }
        public override void setTags(params string[] tags)
        {
            entryNode?.setTags(tags);
        }
        public override string[] getTags()
        {
            return entryNode?.getTags()?.ToArray() ?? Array.Empty<string>();
        }
        public override bool hasTag(string tag)
        {
            return entryNode?.getTags()?.Contains(tag) ?? false;
        }
        public override int getPriority()
        {
            return entryNode?.getPriorty() ?? 0;
        }
        public override Task execute(EffectEnv env)
        {
            var entry = entryNode;
            if (entry != null)
            {
                var flow = new Flow(env.toFlowEnv());
                return env.game.runActions(flow, entry.getActionOutputPort());
            }
            return Task.CompletedTask;
        }
        public override Effect Clone()
        {
            GeneratedActiveEffect effect = new GeneratedActiveEffect(graph);
            return effect;
        }
        #endregion
        #region 私有方法
        protected override IEnumerable<ITraversable> getTraversableProps()
        {
            if (entryNode != null)
                yield return entryNode;
        }
        #endregion
        #region 属性字段
        public virtual ActiveEntryNode entryNode => graph?.nodes?.OfType<ActiveEntryNode>().FirstOrDefault();
        public override ControlOutput executePort => entryNode?.getOutputPort<ControlOutput>(ActiveEntryNode.actionPortName);
        public override ValueInput priorityPort => entryNode?.getInputPort<ValueInput>(ActiveEntryNode.priorityPortName);
        public IEnumerable<TargetChecker> targetCheckers => entryNode?.targetCheckerList;
        Node IEntryNodeEffect.entryNode => entryNode;
        #endregion
    }
}