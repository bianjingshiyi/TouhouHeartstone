using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 群体回手事件
    /// </summary>
    [EventChildren(typeof(MoveCardEventDefine))]
    public class MassReturnHandEventDefine : EventDefine, IMassCardEventDefine
    {
        public MassReturnHandEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = EVENTVAR.PLAYER, type = typeof(Player) },
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = EVENTVAR.PLAYER, type = typeof(Player) },
                new EventVariableInfo() { name = VAR_MOVED_CARDS, type = typeof(Card[]) },
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardStates(EVENTVAR.CARD, arg);
            record.setVar(EVENTVAR.PLAYER, arg);
            record.setCardStates(VAR_MOVED_CARDS, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var cards = arg.getVar<Card[]>(EVENTVAR.CARD);
            var player = arg.getVar<Player>(EVENTVAR.PLAYER);
            var target = player.getHand();
            var movedCards = new List<Card>();
            foreach (var card in cards)
            {
                var moveArg = await card.returnToHand(game, player);
                if (moveArg != null && moveArg.getVar<bool>(MoveCardEventDefine.VAR_SUCCESS) && moveArg.getVar<Pile>(MoveCardEventDefine.VAR_TO_PILE) == target)
                    movedCards.Add(moveArg.getVar<Card>(EVENTVAR.CARD));
            }
            arg.setVar(VAR_MOVED_CARDS, movedCards.ToArray());
        }
        public override string toString(EventArg arg)
        {
            var cards = arg.getVar<Card[]>(EVENTVAR.CARD);
            var player = arg.getVar<Player>(EVENTVAR.PLAYER);
            var cardString = string.Join<Card>(",", cards);
            return $"将 {cardString} 置入 {player} 的手牌";
        }
        public static Task<EventArg> doEvent(CardEngine game, Card[] targets, Player player)
        {
            var define = game.getEventDefine<MassReturnHandEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.CARD, targets);
            arg.setVar(EVENTVAR.PLAYER, player);
            return game.triggers.doEvent(arg);
        }
        ICard[] IMassCardEventDefine.getCards(IEventArg eventArg)
        {
            return eventArg.getVar<Card[]>(VAR_MOVED_CARDS);
        }
        public const string VAR_MOVED_CARDS = "移动的卡牌";
    }
}
