using System;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class GameEndEventDefine : EventDefine
    {
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(VAR_WINNERS, arg);
        }
        public override Task execute(IEventArg arg)
        {
            return Task.CompletedTask;
        }
        public override string toString(EventArg arg)
        {
            var winners = arg.getVar<Player[]>(VAR_WINNERS);
            var winnerString = string.Join<Player>(",", winners);
            return $"游戏结束，获胜者为：{winnerString}";
        }
        public static Task<EventArg> doEvent(CardEngine game, Player[] winners)
        {
            var define = game.getEventDefine<GameEndEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_WINNERS, winners);
            return game.triggers.doEvent(arg);
        }
        public const string VAR_WINNERS = EVENTVAR.WINNERS;
    }
}
