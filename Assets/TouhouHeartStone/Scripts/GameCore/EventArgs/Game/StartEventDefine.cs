using System;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class StartEventDefine : EventDefine
    {
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardStates(VAR_START_HAND_OFFENSIVE, arg);
            record.setCardStates(VAR_START_HAND_DEFENSIVE, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            game.logger.logTrace("游戏开始");
            var sortedPlayers = game.getSortedPlayers();
            var option = game.getOption();
            foreach (Player player in sortedPlayers)
            {
                await player.setMaxGem(game, option.startGem);
                var initPile = player.getInit();
                await initPile.moveTo(game, initPile, player.getHand(), 0);
                if (player != sortedPlayers[0])
                {
                    await player.getHand().add(game, game.createCard(game.getDefine(1, ClassicCardID.LuckyCoin)));
                    game.logger.logTrace($"由于后手行动{player}获得一张幸运币");
                    arg.setVar(VAR_START_HAND_DEFENSIVE, player.getHand().ToArray());
                }
                else
                {
                    arg.setVar(VAR_START_HAND_OFFENSIVE, player.getHand().ToArray());
                }
            }
        }
        public override string toString(EventArg arg)
        {
            return "游戏开始";
        }
        public static Task<EventArg> doEvent(CardEngine game)
        {
            var define = game.getEventDefine<StartEventDefine>();
            var arg = new EventArg(game, define);
            return game.triggers.doEvent(arg);
        }
        public const string VAR_START_HAND_OFFENSIVE = EVENTVAR.START_HAND_OFFENSIVE;
        public const string VAR_START_HAND_DEFENSIVE = EVENTVAR.START_HAND_DEFENSIVE;
    }
}
