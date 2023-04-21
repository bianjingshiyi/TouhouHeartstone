using System;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class GeneratedEventEffect : GeneratedPassiveEffect, IPileRangedEffect
    {
        #region 公有方法
        #region 构造方法
        public GeneratedEventEffect(ActionGraph graph) : base(graph)
        {
        }
        public GeneratedEventEffect() : base()
        {
        }
        #endregion
        public override async Task onEnable(IGame game, ICard card, IBuff buff)
        {
            await base.onEnable(game, card, buff);

            if (eventEntry != null)
            {
                string triggerName = getEffectName(card, buff, eventEntry.eventName);
                game.logger.log("Effect", card + "注册触发器" + triggerName);
                Trigger trigger = new Trigger(
                    args =>
                    {
                        return checkCondition(game, card, buff, args.OfType<IEventArg>().FirstOrDefault());
                    },
                    args =>
                    {
                        return execute(game, card, buff, args.OfType<IEventArg>().FirstOrDefault());
                    }, name: triggerName);
                await card.setProp(game, triggerName, trigger);
                game.triggers.registerDelayed(eventEntry.eventName, trigger);
            }
        }
        public override async Task onDisable(IGame game, ICard card, IBuff buff)
        {
            var eventEntry = this.eventEntry;
            if (eventEntry != null)
            {
                string triggerName = getEffectName(card, buff, eventEntry.eventName);
                game.logger.log("Effect", card + "注销触发器" + triggerName);
                Trigger trigger = card.getProp<Trigger>(game, triggerName);
                await card.setProp(game, triggerName, null);
                game.triggers.remove(eventEntry.eventName, trigger);
            }
            await base.onDisable(game, card, buff);
        }
        public override void Init()
        {
            graph.createActionNode(EventEffectData.defName, 0, 500);
        }
        public ActionNode getEventDataNode()
        {
            return graph?.findActionNode(EventEffectData.defName);
        }
        public override SerializableEffect Serialize()
        {
            return new SerializableEventEffect(this);
        }
        #endregion
        #region 属性字段
        protected virtual PileNameCollection pileList => getEventDataNode().getConst<PileNameCollection>(EventEffectData.pileListName);
        public override PassiveEntryNode entryNode => eventEntry;
        public virtual EventEntryNode eventEntry => graph?.nodes?.OfType<EventEntryNode>().FirstOrDefault();
        public string[] piles => pileList?.ToArray() ?? Array.Empty<string>();
        #endregion
    }

    [Serializable]
    public class SerializableEventEffect : SerializableEffect
    {
        public SerializableEventEffect(GeneratedEventEffect effect)
        {
            name = effect.name;
            propDict = effect.propDict;
            graph = new SerializableActionNodeGraph(effect.graph);
        }

        public override GeneratedEffect Deserialize(INodeDefiner definer)
        {
            GeneratedEventEffect effect = new GeneratedEventEffect();
            effect.name = name;
            effect.propDict = propDict;
            effect.graph = graph.toActionGraph(definer);
            return effect;
        }
    }
}