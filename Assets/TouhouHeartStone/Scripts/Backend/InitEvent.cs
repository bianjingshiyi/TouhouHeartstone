using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone.Backend
{
    class InitEvent : VisibleEvent
    {
        public InitEvent() : base("onInit")
        {
        }
        public override void execute(CardEngine core)
        {
            //决定玩家行动顺序
            List<Player> remainedList = new List<Player>(core.getPlayers());
            Player[] sortedPlayers = new Player[remainedList.Count];
            for (int i = 0; i < sortedPlayers.Length; i++)
            {
                int index = core.randomInt(0, remainedList.Count - 1);
                sortedPlayers[i] = remainedList[index];
                remainedList.RemoveAt(index);
            }
            core.setVar("sortedPlayers", sortedPlayers);
            //抽初始卡牌
            for (int i = 0; i < sortedPlayers.Length; i++)
            {
                int count = i == 0 ? 3 : 4;
                sortedPlayers[i]["Deck"].moveCardTo(sortedPlayers[i]["Deck"][sortedPlayers[i]["Deck"].count - count, sortedPlayers[i]["Deck"].count - 1], sortedPlayers[i]["Init"], 0);
            }
        }
        public override EventWitness getWitness(CardEngine core, Player player)
        {
            EventWitness witness = new EventWitness("onInit");
            //双方玩家所使用的卡组主人公
            witness.setVar("players_master_define_id", core.getPlayers().Select(e => { return e["Master"][0].define.id; }).ToArray());
            //然后是玩家的先后行动顺序
            witness.setVar("sortedPlayers_id", core.getVar<Player[]>("sortedPlayers").Select(e => { return e.id; }).ToArray());
            //接着是初始手牌
            witness.setVar("self_init_define_id", player["Init"].Select(e => { return e.define.id; }).ToArray());
            return witness;
        }
    }
}