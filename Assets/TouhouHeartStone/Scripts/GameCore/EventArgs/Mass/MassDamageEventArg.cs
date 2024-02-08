using System;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 群体伤害事件
    /// </summary>
    [EventChildren(typeof(DamageEventDefine))]
    public class MassDamageEventDefine : EventDefine, IMassCardEventDefine
    {
        public MassDamageEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_SOURCE, type = typeof(Card) },
                new EventVariableInfo() { name = VAR_VALUE, type = typeof(int) },
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_SOURCE, type = typeof(Card) },
                new EventVariableInfo() { name = VAR_VALUE, type = typeof(int) },
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardStates(VAR_CARD, arg);
            record.setCardState(VAR_SOURCE, arg);
            record.setVar(VAR_VALUE, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var source = arg.getVar<Card>(VAR_SOURCE);
            var cards = arg.getVar<Card[]>(VAR_CARD);
            var value = arg.getVar<int>(VAR_VALUE);
            foreach (var target in cards)
            {
                if (target == null)
                    continue;
                await target.damage(game, source, value);
            }
        }
        public override string toString(EventArg arg)
        {
            var source = arg.getVar<Card>(VAR_SOURCE);
            var cards = arg.getVar<Card[]>(VAR_CARD);
            var value = arg.getVar<int>(VAR_VALUE);
            var cardString = string.Join<Card>(",", cards);
            return $"{source} 对 {cardString} 造成 {value} 点伤害";
        }
        public static Task<EventArg> doEvent(CardEngine game, Card source, Card[] targets, int value)
        {
            var define = game.getEventDefine<MassDamageEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_SOURCE, source);
            arg.setVar(VAR_CARD, targets);
            arg.setVar(VAR_VALUE, value);
            return game.triggers.doEvent(arg);
        }
        ICard[] IMassCardEventDefine.getCards(IEventArg eventArg)
        {
            return eventArg.getVar<Card[]>(VAR_CARD);
        }
        public const string VAR_CARD = EVENTVAR.CARD;
        public const string VAR_SOURCE = EVENTVAR.DAMAGE_SOURCE;
        public const string VAR_VALUE = EVENTVAR.VALUE;
    }
}
