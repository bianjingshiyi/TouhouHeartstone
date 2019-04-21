using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone.Backend
{
    class InitEvent : VisibleEvent
    {
        public InitEvent() : base("onInit")
        {
        }
        public override void execute(CardEngine engine)
        {
            //决定玩家行动顺序
            List<Player> remainedList = new List<Player>(engine.getPlayers());
            Player[] sortedPlayers = new Player[remainedList.Count];
            for (int i = 0; i < sortedPlayers.Length; i++)
            {
                int index = engine.randomInt(0, remainedList.Count - 1);
                sortedPlayers[i] = remainedList[index];
                remainedList.RemoveAt(index);
            }
            engine.setProp("sortedPlayers", sortedPlayers);
            Card[] masterCards = sortedPlayers.Select(p => { return p["Master"][0]; }).ToArray();
            foreach (Card card in masterCards)
            {
                card.setProp("life", 30);
                engine.allocateRID(card);
            }
            //抽初始卡牌
            for (int i = 0; i < sortedPlayers.Length; i++)
            {
                int count = i == 0 ? 3 : 4;
                Card[] cards = sortedPlayers[i]["Deck"][sortedPlayers[i]["Deck"].count - count, sortedPlayers[i]["Deck"].count - 1];
                engine.moveCard(sortedPlayers[i], "Deck", cards, sortedPlayers[i], "Init", 0);
                engine.allocateRID(cards);
            }
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onInit");
            //双方玩家所使用的卡组主人公
            witness.setVar("masterCardsRID", engine.getPlayers().Select(p => { return p["Master"][0].getProp<int>("RID"); }).ToArray());
            witness.setVar("masterCardsDID", engine.getPlayers().Select(p => { return p["Master"][0].define.id; }).ToArray());
            //然后是玩家的先后行动顺序
            witness.setVar("sortedPlayersIndex", engine.getProp<Player[]>("sortedPlayers").Select(e => { return engine.getPlayerIndex(e); }).ToArray());
            //接着是初始手牌
            witness.setVar("initCardsRID", engine.getPlayers().Select(p => { return p["Init"].Select(c => { return c.getRID(); }).ToArray(); }).ToArray());
            witness.setVar("initCardsDID", player["Init"].Select(e => { return e.define.id; }).ToArray());
            //剩余卡组
            witness.setVar("deck", player["Deck"].OrderBy(c => { return c.define.id; }).Select(c => { return c.define.id; }).ToArray());
            return witness;
        }
    }
}