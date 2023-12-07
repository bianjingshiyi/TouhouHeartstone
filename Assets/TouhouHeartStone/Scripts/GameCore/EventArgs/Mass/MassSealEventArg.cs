using System;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 群体封印事件
    /// </summary>
    [EventChildren(typeof(SilenceEventDefine))]
    public class MassSealEventDefine : EventDefine, IMassCardEventDefine
    {
        public MassSealEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardStates(EVENTVAR.CARD, arg);
        }
        ICard[] IMassCardEventDefine.getCards(IEventArg arg)
        {
            return arg.getVar<Card[]>(EVENTVAR.CARD);
        }

        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var argCards = arg.getVar<Card[]>(EVENTVAR.CARD);
            foreach (var card in argCards)
            {
                await card.silence(game);
            }
        }

        public override string toString(EventArg arg)
        {
            var cards = arg.getVar<Card[]>(EVENTVAR.CARD);
            var cardString = string.Join<Card>(",", cards);
            return $"封印 {cardString}";
        }
        public static Task<EventArg> doEvent(CardEngine game, Card[] targets)
        {
            var define = game.getEventDefine<MassSealEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.CARD, targets);
            return game.triggers.doEvent(arg);
        }
    }
}
