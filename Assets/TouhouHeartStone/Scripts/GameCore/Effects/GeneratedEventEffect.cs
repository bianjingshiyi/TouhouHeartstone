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
        public override async Task onEnable(CardEngine game, Card card, Buff buff)
        {
            if (!isDisabled(game, card, buff))
                return;

            await base.onEnable(game, card, buff);

            if (eventEntry != null)
            {
                game.logger.logTrace("Effect", $"{card}注册触发器{this}");
                Trigger trigger = new EffectTrigger(game, card, buff, this);
                game.triggers.registerDelayed(eventEntry.triggerTime, trigger);
            }
        }
        public override async Task onDisable(CardEngine game, Card card, Buff buff)
        {
            if (isDisabled(game, card, buff))
                return;

            var eventEntry = this.eventEntry;
            if (eventEntry != null)
            {
                game.logger.logTrace("Effect", $"{card}注销触发器{this}");
                var triggerTime = eventEntry.triggerTime;
                var triggers = game.triggers.getTriggers(triggerTime);
                var trigger = triggers.FirstOrDefault(t => t is EffectTrigger eventTrigger && eventTrigger.effect == this);
                game.triggers.remove(triggerTime, trigger);
            }
            await base.onDisable(game, card, buff);
        }
        public override void setPiles(params string[] tags)
        {
            eventEntry?.setPiles(tags);
        }
        public override string[] getPiles() => eventEntry?.getPiles()?.ToArray() ?? Array.Empty<string>();
        public override SerializableEffect Serialize()
        {
            return new SerializableEventEffect(this);
        }
        #endregion
        #region 私有方法
        Task IEventEffect.onTrigger(EffectEnv env)
        {
            var card = env.card;
            if (card.isSpell() && this.isTrap())
            {
                // 陷阱触发
                return SecretTriggerEventDefine.doEvent(env);
            }
            return execute(env);
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