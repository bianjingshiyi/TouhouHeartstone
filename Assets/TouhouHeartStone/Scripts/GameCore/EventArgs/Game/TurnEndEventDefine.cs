using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class TurnEndEventDefine : EventDefine
    {
        public TurnEndEventDefine()
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
            game.logger.logTrace("Debug", toString(arg as EventArg));

            List<Card> characters = new List<Card>
            {
                player.getMaster()
            };
            characters.AddRange(player.getField());
            foreach (Card chara in characters)
            {
                if (chara.isFreeze(game))
                {
                    var hasAttackTimes = chara.getMaxAttackTimes(game) - chara.getAttackTimes(game) >= 1;
                    var ready = chara.isReady(game) || chara.isCharge(game) || chara.isMaster();
                    if (hasAttackTimes && ready)
                    {
                        game.logger.logTrace($"被禁锢的角色{chara}解除禁锢");
                        await chara.loseShackle(game);
                    }
                }
            }
            // 蓄力。
            foreach (Card hand in player.getHand())
            {
                if (hand.canBeCharged(game) && player.getMaxGem() >= hand.getCost(game))
                {
                    await hand.setChargeLevel(game, hand.getChargedLevel(game) + 1);
                }
            }
            game.answers.cancel(game.answers.getRequests(player.id));
        }
        public override string toString(EventArg arg)
        {
            var player = arg.getVar<Player>(VAR_PLAYER);
            return $"{player}的回合结束";
        }
        public static Task<EventArg> doEvent(CardEngine game, Player player)
        {
            var define = game.getEventDefine<TurnEndEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_PLAYER, player);
            return game.triggers.doEvent(arg);
        }
        public const string VAR_PLAYER = EVENTVAR.PLAYER;
    }
}
