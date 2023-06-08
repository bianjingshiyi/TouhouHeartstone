using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class GeneratedPassiveEffect : GeneratedEffect, IPassiveEffect
    {
        #region 公有方法
        #region 构造方法
        public GeneratedPassiveEffect(ActionGraph graph) : base(graph)
        {
        }
        public GeneratedPassiveEffect() : base()
        {
        }
        #endregion
        public override void Init()
        {
            base.Init();
            createEntryNode();
        }
        public override bool checkCondition(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            var entry = passiveEntry;
            if (entry != null)
            {
                var condition = entry.getTriggerCondtionValuePort();
                if (condition == null || condition.getConnectedOutputPort() == null)
                    return true;

                var flow = new Flow(game, card, buff, eventArg, this);
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
        public override Task execute(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            var entry = passiveEntry;
            if (entry != null)
            {
                var flow = new Flow(game, card, buff, eventArg, this);
                return game.runActions(flow, entry.getActionOutputPort());
            }
            return Task.CompletedTask;
        }
        public override void setTags(params string[] tags)
        {
            passiveEntry?.setTags(tags);
        }
        public override string[] getTags()
        {
            return passiveEntry?.tags?.ToArray() ?? Array.Empty<string>();
        }
        public override bool hasTag(string tag)
        {
            return passiveEntry?.tags?.Contains(tag) ?? false;
        }
        public override SerializableEffect Serialize()
        {
            return new SerializablePassiveEffect(this);
        }
        #endregion
        #region 私有方法
        protected override IEnumerable<ITraversable> getTraversableProps()
        {
            if (entryNode != null)
                yield return entryNode;
        }
        protected virtual void createEntryNode()
        {
            graph.createPassiveEntryNode();
        }
        #endregion
        #region 属性字段
        public PassiveEntryNode passiveEntry => graph?.findPassiveEntryNode();
        public virtual Node entryNode => passiveEntry;
        public override ControlOutput onEnableAction => entryNode?.getOutputPort<ControlOutput>(enableActionName);
        public override ControlOutput onDisableAction => entryNode?.getOutputPort<ControlOutput>(disableActionName);
        public const string enableActionName = "enableAction";
        public const string disableActionName = "disableAction";
        public static readonly PortDefine enableActionPortDefine = PortDefine.Control(enableActionName, "EnableAction");
        public static readonly PortDefine disableActionPortDefine = PortDefine.Control(disableActionName, "DisableAction");
        #endregion
    }

    [Serializable]
    public class SerializablePassiveEffect : SerializableEffect
    {
        public SerializablePassiveEffect(GeneratedPassiveEffect effect)
        {
            name = effect.name;
            propDict = effect.propDict;
            graph = new SerializableActionNodeGraph(effect.graph);
        }

        public override GeneratedEffect Deserialize(INodeDefiner definer)
        {
            GeneratedPassiveEffect effect = new GeneratedPassiveEffect();
            effect.name = name;
            effect.propDict = propDict;
            effect.graph = graph.toActionGraph(definer);
            return effect;
        }
    }
}