using System.Threading.Tasks;
using System;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    /// <summary>
    /// 群体入场事件
    /// </summary>
    [EventChildren(typeof(EnterFieldEventDefine))]
    public class MassEnterFieldEventDefine : EventDefine, IMassCardEventDefine
    {
        public MassEnterFieldEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_PLAYER, type = typeof(Player) },
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_PLAYER, type = typeof(Player) },
                new EventVariableInfo() { name = VAR_MOVED_CARDS, type = typeof(Card[]) },
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardStates(VAR_CARD, arg);
            record.setVar(VAR_PLAYER, arg);
            record.setCardStates(VAR_MOVED_CARDS, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var cards = arg.getVar<Card[]>(VAR_CARD);
            var player = arg.getVar<Player>(VAR_PLAYER);
            List<Card> movedCards = new List<Card>();
            foreach (var card in cards)
            {
                var moveArg = await card.moveToField(game, player);
                if (moveArg != null && moveArg.getVar<bool>(MoveCardEventDefine.VAR_SUCCESS))
                    movedCards.Add(moveArg.getVar<Card>(VAR_CARD));
            }
            arg.setVar(VAR_MOVED_CARDS, movedCards.ToArray());
        }
        public override string toString(EventArg arg)
        {
            var cards = arg.getVar<Card[]>(VAR_CARD);
            var player = arg.getVar<Player>(VAR_PLAYER);
            var cardString = string.Join<Card>(",", cards);
            return $"将 {cardString} 置入 {player} 的战场";
        }
        public static Task<EventArg> doEvent(CardEngine game, Card[] targets, Player player)
        {
            var define = game.getEventDefine<MassEnterFieldEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_CARD, targets);
            arg.setVar(VAR_PLAYER, player);
            return game.triggers.doEvent(arg);
        }
        ICard[] IMassCardEventDefine.getCards(IEventArg eventArg)
        {
            return eventArg.getVar<Card[]>(VAR_MOVED_CARDS);
        }
        public const string VAR_MOVED_CARDS = EVENTVAR.MOVED_CARDS;
        public const string VAR_PLAYER = EVENTVAR.PLAYER;
        public const string VAR_CARD = EVENTVAR.CARD;
    }
}
