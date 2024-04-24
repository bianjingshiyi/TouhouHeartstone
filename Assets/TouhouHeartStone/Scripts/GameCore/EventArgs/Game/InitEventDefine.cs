using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class InitEventDefine : EventDefine
    {
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
        }

        public override Task execute(IEventArg arg)
        {
            var game = arg.game;
            game.addBuffDefine(new MasterItemAttackBuffDefine());
            return Task.CompletedTask;
        }

        public override string toString(EventArg arg)
        {
            return "初始化";
        }
        public static Task<EventArg> doEvent(CardEngine game)
        {
            var define = game.getEventDefine<InitEventDefine>();
            var arg = new EventArg(game, define);
            return game.triggers.doEvent(arg);
        }
    }
}
