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
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card[]) },
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card[]) },
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardStates(VAR_CARD, arg);
        }
        ICard[] IMassCardEventDefine.getCards(IEventArg arg)
        {
            return arg.getVar<Card[]>(VAR_CARD);
        }

        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var argCards = arg.getVar<Card[]>(VAR_CARD);
            foreach (var card in argCards)
            {
                await card.silence(game);
            }
        }

        public override string toString(EventArg arg)
        {
            var cards = arg.getVar<Card[]>(VAR_CARD);
            var cardString = string.Join<Card>(",", cards);
            return $"封印 {cardString}";
        }
        public static Task<EventArg> doEvent(CardEngine game, Card[] targets)
        {
            var define = game.getEventDefine<MassSealEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_CARD, targets);
            return game.triggers.doEvent(arg);
        }
        public const string VAR_CARD = EVENTVAR.CARD;
    }
}
