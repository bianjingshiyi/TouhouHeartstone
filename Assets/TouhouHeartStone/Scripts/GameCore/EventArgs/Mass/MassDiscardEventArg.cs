using System.Threading.Tasks;
using System;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 群体弃牌事件
    /// </summary>
    [EventChildren(typeof(DiscardEventDefine))]
    public class MassDiscardEventDefine : EventDefine, IMassCardEventDefine
    {
        public MassDiscardEventDefine()
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
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var cards = arg.getVar<Card[]>(VAR_CARD);
            foreach (Card card in cards)
            {
                if (card == null)
                    continue;
                await card.discard(game);
            }
        }
        public override string toString(EventArg arg)
        {
            var cards = arg.getVar<Card[]>(VAR_CARD);
            var cardString = string.Join<Card>(",", cards);
            return $"丢弃 {cardString}";
        }
        public static Task<EventArg> doEvent(CardEngine game, Card[] targets)
        {
            var define = game.getEventDefine<MassDiscardEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_CARD, targets);
            return game.triggers.doEvent(arg);
        }
        ICard[] IMassCardEventDefine.getCards(IEventArg eventArg)
        {
            return eventArg.getVar<Card[]>(VAR_CARD);
        }
        public const string VAR_CARD = EVENTVAR.CARD;
    }
}
