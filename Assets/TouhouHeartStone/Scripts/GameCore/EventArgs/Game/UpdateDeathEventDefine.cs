using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class UpdateDeathEventDefine : EventDefine
    {
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardStates(VAR_CLEARED_CARDS, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            List<Card> deathList = new List<Card>();
            foreach (Player player in game.players)
            {
                deathList.AddRange(player.getField().Where(s => s != null && s.isDead(game)));

                var item = player.getItem();
                if (item != null && item.isDead(game))
                    deathList.Add(item);

                var master = player.getMaster();
                if (master != null && master.isDead(game))
                    deathList.Add(master);
            }

            if (deathList.Count > 0 && game.isRunning)
            {
                await deathList.destroy(game);
            }
            arg.setVar(VAR_CLEARED_CARDS, deathList.ToArray());
        }
        public override string toString(EventArg arg)
        {
            return $"死亡结算";
        }
        public static Task<EventArg> doEvent(CardEngine game)
        {
            var define = game.getEventDefine<UpdateDeathEventDefine>();
            var arg = new EventArg(game, define);
            return game.triggers.doEvent(arg);
        }
        public const string VAR_CLEARED_CARDS = "清理的卡牌";
    }
}
