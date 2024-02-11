using System.Threading.Tasks;
using TouhouCardEngine.Interfaces;
using TouhouCardEngine;

namespace TouhouHeartstone
{
    public class EffectTriggerEventDefine : EventDefine, IEffectEventDefine
    {
        public EffectTriggerEventDefine() 
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card)},
                new EventVariableInfo() { name = VAR_BUFF, type = typeof(Buff)},
                new EventVariableInfo() { name = VAR_EFFECT, type = typeof(Effect)},
                new EventVariableInfo() { name = VAR_TARGETS, type = typeof(Card[])},
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card)},
                new EventVariableInfo() { name = VAR_BUFF, type = typeof(Buff)},
                new EventVariableInfo() { name = VAR_EFFECT, type = typeof(Effect)},
                new EventVariableInfo() { name = VAR_TARGETS, type = typeof(Card[])},
            };
        }
        public override async Task execute(IEventArg effectTriggerArg)
        {
            var effect = effectTriggerArg.getVar<Effect>(VAR_EFFECT);
            var effectEnv = getEffectEnv(effectTriggerArg);
            await effect.execute(effectEnv);
        }
        public override string toString(EventArg arg)
        {
            var card = arg.getVar<Card>(VAR_CARD);
            var effect = arg.getVar<Effect>(VAR_EFFECT);
            return $"卡牌{card}发动效果{effect}";
        }
        public static EventArg newEvent(CardEngine game, Card card, Buff buff, Effect effect, EventArg eventArg = null, Card[] targets = null)
        {
            var define = game.getEventDefine<EffectTriggerEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_CARD, card);
            arg.setVar(VAR_PLAYER, card?.getOwner());
            arg.setVar(VAR_TARGETS, targets);
            arg.setVar(VAR_EFFECT, effect);
            arg.setVar(VAR_BUFF, buff);
            arg.setVar(VAR_EVENT, eventArg ?? arg);
            return arg;
        }
        public static async Task<EventArg> doEventIfValid(CardEngine game, Card card, Buff buff, Effect effect, EventArg eventArg = null, Card[] targets = null)
        {
            var arg = newEvent(game, card, buff, effect, eventArg, targets);
            var env = getEffectEnv(arg);
            if (!env.effect.checkCondition(env))
                return null;
            return await game.triggers.doEvent(arg);
        }
        public static Task<EventArg> doEvent(CardEngine game, Card card, Buff buff, Effect effect, EventArg eventArg = null, Card[] targets = null)
        {
            var arg = newEvent(game, card, buff, effect, eventArg, targets);
            return game.triggers.doEvent(arg);
        }
        public static Task<EventArg> doEvent(EffectEnv env, Card[] targets = null)
        {
            return doEvent(env.game, env.card, env.buff, env.effect, env.eventArg, targets);
        }
        Card IEffectEventDefine.getCard(EventArg arg)
        {
            return arg.getVar<Card>(VAR_CARD);
        }
        Effect IEffectEventDefine.getEffect(EventArg arg)
        {
            return arg.getVar<Effect>(VAR_EFFECT);
        }
        private static EffectEnv getEffectEnv(IEventArg effectTriggerArg)
        {
            var game = effectTriggerArg.game;
            var card = effectTriggerArg.getVar<Card>(VAR_CARD);
            var buff = effectTriggerArg.getVar<Buff>(VAR_BUFF);
            var effect = effectTriggerArg.getVar<Effect>(VAR_EFFECT);
            var envArg = effectTriggerArg.getVar<EventArg>(VAR_EVENT);
            return new EffectEnv(game, card, buff, envArg, effect);
        }
        public const string VAR_PLAYER = EVENTVAR.PLAYER;
        public const string VAR_CARD = EVENTVAR.CARD;
        public const string VAR_BUFF = EVENTVAR.BUFF;
        public const string VAR_EFFECT = EVENTVAR.EFFECT;
        public const string VAR_EVENT = EVENTVAR.EVENT;
        public const string VAR_TARGETS = EVENTVAR.TARGET_CARD;
    }
}
