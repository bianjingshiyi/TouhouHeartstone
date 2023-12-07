using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 群体洗入牌库事件
    /// </summary>
    [EventChildren(typeof(MoveCardEventDefine))]
    public class MassShuffleToDeckEventDefine : EventDefine, IMassCardEventDefine
    {
        public MassShuffleToDeckEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = EVENTVAR.PLAYER, type = typeof(Player) },
                new EventVariableInfo() { name = VAR_TOP, type = typeof(bool) },
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = EVENTVAR.PLAYER, type = typeof(Player) },
                new EventVariableInfo() { name = VAR_TOP, type = typeof(bool) },
                new EventVariableInfo() { name = VAR_MOVED_CARDS, type = typeof(Card[]) },
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardStates(EVENTVAR.CARD, arg);
            record.setVar(EVENTVAR.PLAYER, arg);
            record.setVar(VAR_TOP, arg);
            record.setCardStates(VAR_MOVED_CARDS, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var cards = arg.getVar<Card[]>(EVENTVAR.CARD);
            var player = arg.getVar<Player>(EVENTVAR.PLAYER);
            var top = arg.getVar<bool>(VAR_TOP);
            List<Card> movedCards = new List<Card>();
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
                int position = top ? player.getDeck().count : game.randomInt(0, player.getDeck().count);
                game.logger.logTrace($"将{string.Join("，", card)}洗入{player}的牌库");
                var moveArg = await card.pile.moveTo(game, card, player.getDeck(), position);
                if (moveArg != null && moveArg.getVar<bool>(MoveCardEventDefine.VAR_SUCCESS))
                    movedCards.Add(moveArg.getVar<Card>(EVENTVAR.CARD));
            }
            arg.setVar(VAR_MOVED_CARDS, movedCards.ToArray());
        }
        public override string toString(EventArg arg)
        {
            var cards = arg.getVar<Card[]>(EVENTVAR.CARD);
            var player = arg.getVar<Player>(EVENTVAR.PLAYER);
            var top = arg.getVar<bool>(VAR_TOP);
            var cardString = string.Join<Card>(",", cards);
            return top ? $"将 {cardString} 置入 {player} 的牌库顶" : $"将 {cardString} 洗入 {player} 的牌库";
        }
        public static Task<EventArg> doEvent(CardEngine game, Card[] targets, Player player, bool top)
        {
            var define = game.getEventDefine<MassShuffleToDeckEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.CARD, targets);
            arg.setVar(EVENTVAR.PLAYER, player);
            arg.setVar(VAR_TOP, top);
            return game.triggers.doEvent(arg);
        }
        ICard[] IMassCardEventDefine.getCards(IEventArg eventArg)
        {
            return eventArg.getVar<Card[]>(VAR_MOVED_CARDS);
        }
        public const string VAR_TOP = "置入牌库顶";
        public const string VAR_MOVED_CARDS = "移动的卡牌";
    }
}
