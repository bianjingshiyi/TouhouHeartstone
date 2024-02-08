using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 群体移动事件
    /// </summary>
    [EventChildren(typeof(MoveCardEventDefine))]
    public class MassMoveCardEventDefine : EventDefine, IMassCardEventDefine
    {
        public MassMoveCardEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_FROM_PILE, type = typeof(Pile) },
                new EventVariableInfo() { name = VAR_TO_PILE, type = typeof(Pile) },
                new EventVariableInfo() { name = VAR_POSITION, type = typeof(int) },
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_FROM_PILE, type = typeof(Pile) },
                new EventVariableInfo() { name = VAR_TO_PILE, type = typeof(Pile) },
                new EventVariableInfo() { name = VAR_POSITION, type = typeof(int) },
                new EventVariableInfo() { name = VAR_MOVED_CARDS, type = typeof(Card[]) },
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardStates(VAR_CARD, arg);
            record.setVar(VAR_FROM_PILE, arg);
            record.setVar(VAR_TO_PILE, arg);
            record.setVar(VAR_POSITION, arg);
            record.setCardStates(VAR_MOVED_CARDS, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var cards = arg.getVar<Card[]>(VAR_CARD);
            var fromPile = arg.getVar<Pile>(VAR_FROM_PILE);
            var toPosition = arg.getVar<int>(VAR_POSITION);
            var toPile = arg.getVar<Pile>(VAR_TO_PILE);
            List<Card> movedCards = new List<Card>();
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
                game.logger.logTrace(toString(arg as EventArg));
                var from = fromPile ?? card.pile;
                var pos = toPosition;
                if (pos < 0 && toPile != null)
                    pos = toPile.count;
                var moveArg = await from.moveTo(game, card, toPile, pos);
                if (moveArg != null && moveArg.getVar<bool>(MoveCardEventDefine.VAR_SUCCESS))
                    movedCards.Add(moveArg.getVar<Card>(VAR_CARD));
            }
            arg.setVar(VAR_MOVED_CARDS, movedCards.ToArray());
        }
        public override string toString(EventArg arg)
        {
            var cards = arg.getVar<Card[]>(VAR_CARD);
            var fromPile = arg.getVar<Pile>(VAR_FROM_PILE);
            var toPile = arg.getVar<Pile>(VAR_TO_PILE);
            var cardString = string.Join<Card>(",", cards);
            return fromPile != null ? $"将 {cardString} 从 {fromPile} 置入 {toPile}" : $"将 {cardString} 置入 {toPile}";
        }
        public static Task<EventArg> doEvent(CardEngine game, Card[] targets, Pile to)
        {
            return doEvent(game, targets, null, to);
        }
        public static Task<EventArg> doEvent(CardEngine game, Card[] targets, Pile from, Pile to)
        {
            return doEvent(game, targets, from, to, -1);
        }
        public static Task<EventArg> doEvent(CardEngine game, Card[] targets, Pile from, Pile to, int position)
        {
            var define = game.getEventDefine<MassMoveCardEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_CARD, targets);
            arg.setVar(VAR_FROM_PILE, from);
            arg.setVar(VAR_TO_PILE, to);
            arg.setVar(VAR_POSITION, position);
            return game.triggers.doEvent(arg);
        }
        ICard[] IMassCardEventDefine.getCards(IEventArg eventArg)
        {
            return eventArg.getVar<Card[]>(VAR_MOVED_CARDS);
        }
        public const string VAR_MOVED_CARDS = EVENTVAR.MOVED_CARDS;
        public const string VAR_FROM_PILE = EVENTVAR.FROM_PILE;
        public const string VAR_TO_PILE = EVENTVAR.TO_PILE;
        public const string VAR_CARD = EVENTVAR.CARD;
        public const string VAR_POSITION = EVENTVAR.POSITION;
    }
}
