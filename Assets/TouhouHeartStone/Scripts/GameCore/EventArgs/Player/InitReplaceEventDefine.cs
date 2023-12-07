using System;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{

    public class InitReplaceEventDefine : EventDefine
    {
        public InitReplaceEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.PLAYER, type = typeof(Player) },
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_REPLACED_CARDS, type = typeof(Card[]) }
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card[])},
                new EventVariableInfo(){name = VAR_REPLACED_CARDS,type=typeof(Card[])}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(EVENTVAR.PLAYER, arg);
            record.setCardStates(EVENTVAR.CARD, arg);
            record.setCardStates(VAR_REPLACED_CARDS, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var player = arg.getVar<Player>(EVENTVAR.PLAYER);
            var cards = arg.getVar<Card[]>(EVENTVAR.CARD);
            var replacedCards = await player.getInit().replaceByRandom(game, cards, player.getDeck());
            arg.setVar(VAR_REPLACED_CARDS, replacedCards);
            game.logger.logTrace(toString(arg as EventArg));
        }
        public override string toString(EventArg arg)
        {
            var player = arg.getVar<Player>(EVENTVAR.PLAYER);
            var cards = arg.getVar<Card[]>(EVENTVAR.CARD);
            var replaced = arg.getVar<Card[]>(VAR_REPLACED_CARDS);
            var replacedString = replaced != null ? string.Join<Card>(",", replaced) : null;
            return $"{player}从初始手牌{string.Join<Card>(",", cards)}中替换{replacedString}";
        }
        public static Task<EventArg> doEvent(CardEngine game, Player player, Card[] cards)
        {
            var define = game.getEventDefine<InitReplaceEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.PLAYER, player);
            arg.setVar(EVENTVAR.CARD, cards);
            return game.triggers.doEvent(arg);
        }
        public const string VAR_REPLACED_CARDS = "替换的卡牌";
    }
}
