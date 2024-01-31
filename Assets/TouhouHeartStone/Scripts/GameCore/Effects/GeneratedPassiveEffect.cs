using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class GeneratedPassiveEffect : GeneratedEffect, IPassiveEffect, IPileRangedEffect, IEntryNodeEffect
    {
        #region 公有方法
        #region 构造方法
        public GeneratedPassiveEffect(ActionGraph graph) : base(graph)
        {
        }
        #endregion
        public override void Init()
        {
            base.Init();
            createEntryNode();
        }
        public override bool checkCondition(EffectEnv env)
        {
            var entry = passiveEntry;
            if (entry != null)
            {
                var condition = entry.getTriggerCondtionValuePort();
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
        public override void setTags(params string[] tags)
        {
            passiveEntry?.setTags(tags);
        }
        public override string[] getTags()
        {
            return passiveEntry?.getTags() ?? Array.Empty<string>();
        }
        public override bool hasTag(string tag)
        {
            return getTags()?.Contains(tag) ?? false;
        }
        public override int getPriority()
        {
            return passiveEntry?.getPriorty() ?? 0;
        }
        public virtual void setPiles(params string[] tags)
        {
            passiveEntry?.setPiles(tags);
        }
        public virtual string[] getPiles()
        {
            return passiveEntry?.getPiles()?.ToArray() ?? Array.Empty<string>();
        }
        public override Task execute(EffectEnv env)
        {
            var entry = passiveEntry;
            if (entry != null)
            {
                var flowEnv = env.toFlowEnv();
                var flow = new Flow(flowEnv);
                return env.game.runActions(flow, entry.getActionOutputPort());
            }
            return Task.CompletedTask;
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
        public override ControlOutput executePort => entryNode?.getOutputPort<ControlOutput>(PassiveEntryNode.actionPortName);
        public override ValueInput priorityPort => entryNode?.getInputPort<ValueInput>(PassiveEntryNode.priorityPortName);
        public const string enableActionName = "enableAction";
        public const string disableActionName = "disableAction";
        public static readonly PortDefine enableActionPortDefine = PortDefine.Control(enableActionName, "EnableAction");
        public static readonly PortDefine disableActionPortDefine = PortDefine.Control(disableActionName, "DisableAction");
        Node IEntryNodeEffect.entryNode => entryNode;
        #endregion
    }

    [Serializable]
    public class SerializablePassiveEffect : SerializableEffect
    {
        public SerializablePassiveEffect(GeneratedPassiveEffect effect) : base(effect)
        {
        }

        public override GeneratedEffect Deserialize(INodeDefiner definer)
        {
            var graph = this.graph.toActionGraph(definer);
            GeneratedPassiveEffect effect = new GeneratedPassiveEffect(graph);
            apply(effect);
            return effect;
        }
    }
}