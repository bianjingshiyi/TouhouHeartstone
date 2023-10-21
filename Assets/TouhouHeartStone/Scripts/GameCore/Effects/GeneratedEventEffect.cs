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
            if (!isDisabled(game, card, buff))
                return;

            await base.onEnable(game, card, buff);

            if (eventEntry != null)
            {
                string triggerName = getEffectName(card, buff, eventEntry.eventName);
                game.logger.logTrace("Effect", $"{card}注册触发器{triggerName}");
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
            if (isDisabled(game, card, buff))
                return;

            var eventEntry = this.eventEntry;
            if (eventEntry != null)
            {
                string triggerName = getEffectName(card, buff, eventEntry.eventName);
                game.logger.logTrace("Effect", $"{card}注销触发器{triggerName}");
                Trigger trigger = card.getProp<Trigger>(game, triggerName);
                await card.setProp(game, triggerName, null);
                game.triggers.remove(eventEntry.eventName, trigger);
            }
            await base.onDisable(game, card, buff);
        }
        public override void setPiles(params string[] tags)
        {
            eventEntry?.setPiles(tags);
        }
        public override async Task execute(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            if (card.isSpell() && this.isTrap())
            {
                // 陷阱触发
                var gameCard = card as Card;
                await game.triggers.doEvent(new THHCard.SecretTriggerEventArg() { card = gameCard }, async arg =>
                {
                    var argCard = arg.card;
                    game.logger.logTrace($"触发陷阱{argCard}");
                    var entry = passiveEntry;
                    if (entry != null)
                    {
                        var flow = new Flow(game, argCard, buff, eventArg, this);
                        await game.runActions(flow, entry.getActionOutputPort());
                    }
                    Player player = argCard.getOwner();
                    await player.getHead().moveTo(game, argCard, player.getGrave());
                });
                return;
            }
            await base.execute(game, card, buff, eventArg);
        }
        public override SerializableEffect Serialize()
        {
            return new SerializableEventEffect(this);
        }
        #endregion
        #region 私有方法
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