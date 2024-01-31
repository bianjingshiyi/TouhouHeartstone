using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class AutocastEventDefine : EventDefine, ICardEventDefine
    {
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var player = arg.getVar<Player>(EVENTVAR.PLAYER);
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            var targets = arg.getVar<Card[]>(EVENTVAR.TARGETS);

            await card.pile.moveTo(game, card, player.getWarp());
            await card.triggerActiveEffect(game, targets);
            await card.pile.moveTo(game, card, player.getGrave());
            await game.updateGameState();
            await player.draw(game);
        }

        public override string toString(EventArg arg)
        {
            var player = arg.getVar<Player>(EVENTVAR.PLAYER);
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            return string.Format(TEMPLATE, player, card);
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(EVENTVAR.PLAYER, arg);
            record.setCardState(EVENTVAR.CARD, arg);
            record.setCardStates(EVENTVAR.TARGETS, arg);
        }
        public static Task<EventArg> doEvent(CardEngine game, Player player, Card card, Card[] targets)
        {
            var define = game.getEventDefine<AutocastEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.PLAYER, player);
            arg.setVar(EVENTVAR.CARD, card);
            arg.setVar(EVENTVAR.TARGETS, targets);
            return game.triggers.doEvent(arg);
        }
        ICard ICardEventDefine.getCard(IEventArg arg)
        {
            return arg.getVar<Card>(EVENTVAR.CARD);
        }
        [TranslateMsg("{0}玩家，{1}卡")]
        const string TEMPLATE = "{0}自动施放了{1}";
    }
}
