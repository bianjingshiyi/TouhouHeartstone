using System;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class GeneratedEventEffect : GeneratedPassiveEffect, IPileRangedEffect, IEventEffect
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
            if (!isDisabled(game, card, buff))
                return;

            await base.onEnable(game, card, buff);

            if (eventEntry != null)
            {
                game.logger.logTrace("Effect", $"{card}注册触发器{this}");
                Trigger trigger = new EffectTrigger(game, card, buff, this);
                game.triggers.registerDelayed(eventEntry.triggerTime.getTimeName(), trigger);
            }
        }
        public override async Task onDisable(IGame game, ICard card, IBuff buff)
        {
            if (isDisabled(game, card, buff))
                return;

            var eventEntry = this.eventEntry;
            if (eventEntry != null)
            {
                game.logger.logTrace("Effect", $"{card}注销触发器{this}");
                var eventName = eventEntry.triggerTime.getTimeName();
                var triggers = game.triggers.getTriggers(eventName);
                var trigger = triggers.FirstOrDefault(t => t is EffectTrigger eventTrigger && eventTrigger.effect == this);
                game.triggers.remove(eventName, trigger);
            }
            await base.onDisable(game, card, buff);
        }
        public override void setPiles(params string[] tags)
        {
            eventEntry?.setPiles(tags);
        }
        public override SerializableEffect Serialize()
        {
            return new SerializableEventEffect(this);
        }
        #endregion
        #region 私有方法
        Task IEventEffect.onTrigger(IGame game, ICard card, IBuff buff, IEventArg arg)
        {
            if (card.isSpell() && this.isTrap())
            {
                // 陷阱触发
                var gameCard = card as Card;
                return SecretTriggerEventDefine.doEvent(game as CardEngine, gameCard, buff as Buff, arg as EventArg, this);
            }
            return execute(game, card, buff, arg);
        }
        protected override void createEntryNode()
        {
            // 留空
        }
        #endregion
        #region 属性字段
        public override Node entryNode => eventEntry;
        public virtual EventEntryNode eventEntry => graph?.findEventEntryNode();
        public override ControlOutput onEnableAction => null;
        public override ControlOutput onDisableAction => null;
        public override string[] piles => eventEntry?.piles?.ToArray() ?? Array.Empty<string>();
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