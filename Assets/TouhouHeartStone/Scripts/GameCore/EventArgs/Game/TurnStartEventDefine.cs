using System;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class TurnStartEventDefine : EventDefine
    {
        public TurnStartEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = VAR_PLAYER,type=typeof(Player)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = VAR_PLAYER,type=typeof(Player)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(VAR_PLAYER, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var player = arg.getVar<Player>(VAR_PLAYER);
            game.logger.logTrace(toString(arg as EventArg));
            //玩家的最大能量加1但是不超过10，充满玩家的能量。
            await player.setMaxGem(game, player.getMaxGem() + 1);
            await player.setGem(game, player.getMaxGem());
            //抽一张牌
            await player.draw(game);
            //重置技能
            await player.getSkill().setUsed(game, false);
        }
        public override string toString(EventArg arg)
        {
            var player = arg.getVar<Player>(VAR_PLAYER);
            return $"{player}的回合开始";
        }
        public static Task<EventArg> doEvent(CardEngine game, Player player)
        {
            var define = game.getEventDefine<TurnStartEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_PLAYER, player);
            return game.triggers.doEvent(arg);
        }
        public const string VAR_PLAYER = EVENTVAR.PLAYER;
    }

}
