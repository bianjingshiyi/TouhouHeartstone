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
        #endregion
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
            return EffectTriggerEventDefine.doEvent(env);
        }
        protected override async Task onEnable(EffectEnv env)
        {
            await base.onEnable(env);

            if (eventEntry != null)
            {
                var game = env.game;
                var card = env.card;
                var buff = env.buff;
                game.logger.logTrace("Effect", $"{card}注册触发器{this}");
                Trigger trigger = new EffectTrigger(game, card, buff, this);
                game.triggers.registerDelayed(eventEntry.triggerTime, trigger);
            }
        }
        protected override async Task onDisable(EffectEnv env)
        {
            var eventEntry = this.eventEntry;
            if (eventEntry != null)
            {
                var game = env.game;
                var card = env.card;
                var buff = env.buff;
                game.logger.logTrace("Effect", $"{card}注销触发器{this}");
                var triggerTime = eventEntry.triggerTime;
                var triggers = game.triggers.getTriggers(triggerTime);
                var buffInstanceId = buff?.instanceID ?? -1;
                var trigger = triggers.FirstOrDefault(t => t is EffectTrigger eventTrigger && eventTrigger.card == card && (eventTrigger.buff?.instanceID ?? -1) == buffInstanceId);
                game.triggers.remove(triggerTime, trigger);
            }
            await base.onDisable(env);
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
        public SerializableEventEffect(GeneratedEventEffect effect) : base(effect)
        {
        }

        public override GeneratedEffect Deserialize(INodeDefiner definer)
        {
            var graph = this.graph.toActionGraph(definer);
            GeneratedEventEffect effect = new GeneratedEventEffect(graph);
            apply(effect);
            return effect;
        }
    }
}