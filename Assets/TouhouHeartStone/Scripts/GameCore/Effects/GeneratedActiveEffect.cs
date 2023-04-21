using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class GeneratedActiveEffect : GeneratedEffect, IActiveEffect
    {
        #region 公有方法
        #region 构造方法
        public GeneratedActiveEffect(ActionGraph graph) : base(graph)
        {
        }
        public GeneratedActiveEffect() : base()
        {
        }
        #endregion

        public override void Init()
        {
            base.Init();
            graph.createActiveEntryNode();
        }
        public override bool checkCondition(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            if (isDisabled(game, card, buff))
                return false;
            var entryNode = this.entryNode;
            if (entryNode != null)
            {
                var condition = entryNode.getTriggerCondtionValuePort();
                if (condition == null || condition.getConnectedOutputPort() == null)
                    return true;

                var flow = new Flow(game, card, buff, eventArg);
                var task = game.getValue<bool>(flow, condition);
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
        /// <param name="card">目标卡牌</param>
        /// <param name="buff"></param>
        /// <param name="eventArg"></param>
        /// <param name="invalidMsg"></param>
        /// <returns></returns>
        public virtual bool checkTarget(IGame game, ICard card, IBuff buff, IEventArg eventArg, out string invalidMsg)
        {
            var entryNode = this.entryNode;
            if (entryNode != null)
            {
                invalidMsg = null;
                //如果触发不包含任何目标，那么目标肯定不是合法目标
                if (entryNode.targetCheckerList == null || entryNode.targetCheckerList.Count < 1)
                    return false;
                foreach (var targetChecker in entryNode.targetCheckerList)
                {
                    //检查目标条件
                    if (!targetChecker.isValidTarget(game, card, buff, eventArg, out invalidMsg))
                    {
                        //有条件没有通过，不是合法目标
                        return false;
                    }
                }
                //有目标并且没有条件不通过或者没有条件，返回真
                return true;
            }
            else
            {
                invalidMsg = null;
                return false;
            }
        }
        public override Task execute(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            var entryNode = this.entryNode;
            if (entryNode != null)
            {
                var flow = new Flow(game, card, buff, eventArg);
                return game.runActions(flow, entryNode.getActionOutputPort());
            }
            return Task.CompletedTask;
        }
        public void setTags(params string[] tags)
        {
            entryNode?.setTags(tags);
        }
        public string[] getTags()
        {
            return entryNode?.tags?.ToArray() ?? Array.Empty<string>();
        }
        public bool hasTag(string tag)
        {
            return entryNode?.tags?.Contains(tag) ?? false;
        }
        public override SerializableEffect Serialize()
        {
            return new SerializableActiveEffect(this);
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
        #endregion
    }

    [Serializable]
    public class SerializableActiveEffect : SerializableEffect
    {
        public SerializableActiveEffect(GeneratedActiveEffect effect)
        {
            name = effect.name;
            propDict = effect.propDict;
            graph = new SerializableActionNodeGraph(effect.graph);
        }

        public override GeneratedEffect Deserialize(INodeDefiner definer)
        {
            GeneratedActiveEffect effect = new GeneratedActiveEffect();
            effect.name = name;
            effect.propDict = propDict;
            effect.graph = graph.toActionGraph(definer);
            return effect;
        }
    }
}