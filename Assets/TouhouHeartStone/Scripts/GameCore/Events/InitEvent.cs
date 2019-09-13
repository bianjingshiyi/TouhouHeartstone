using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
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
            //创建主人公和技能卡
            Card[] masterCards = sortedPlayers.Select(p => { return p["Master"][0]; }).ToArray();
            foreach (Card card in masterCards)
            {
                card.setProp("life", 10);
                engine.registerCard(card);
            }
            Card[] skillCards = sortedPlayers.Select(p => { return p["Skill"][0]; }).ToArray();
            foreach (Card card in skillCards)
            {
                engine.registerCard(card);
            }
            //洗牌，然后抽初始卡牌
            for (int i = 0; i < sortedPlayers.Length; i++)
            {
                if (engine.getProp<bool>("shuffle"))
                    sortedPlayers[i]["Deck"].shuffle(engine);
                int count = i == 0 ? 3 : 4;
                Card[] cards = sortedPlayers[i]["Deck"][sortedPlayers[i]["Deck"].count - count, sortedPlayers[i]["Deck"].count - 1];
                sortedPlayers[i]["Deck"].moveTo(cards, sortedPlayers[i]["Init"], 0);
                engine.registerCards(cards);
            }
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new InitWitness();
            //双方玩家所使用的卡组主人公
            witness.setVar("masterCardsRID", engine.getPlayers().Select(p => { return p["Master"][0].getProp<int>("RID"); }).ToArray());
            witness.setVar("masterCardsDID", engine.getPlayers().Select(p => { return p["Master"][0].define.id; }).ToArray());
            //双方玩家所使用的技能
            witness.setVar("skillCardsRID", engine.getPlayers().Select(p => { return p["Skill"][0].getProp<int>("RID"); }).ToArray());
            witness.setVar("skillCardsDID", engine.getPlayers().Select(p => { return p["Skill"][0].define.id; }).ToArray());
            //然后是玩家的先后行动顺序
            witness.setVar("sortedPlayersIndex", engine.getProp<Player[]>("sortedPlayers").Select(p => { return engine.getPlayerIndex(p); }).ToArray());
            //接着是初始手牌
            witness.setVar("initCardsRID", engine.getPlayers().Select(p => { return p["Init"].Select(c => { return c.id; }).ToArray(); }).ToArray());
            witness.setVar("initCardsDID", player["Init"].Select(e => { return e.define.id; }).ToArray());
            //剩余卡组
            witness.setVar("deck", player["Deck"].OrderBy(c => { return c.define.id; }).Select(c => { return c.define.id; }).ToArray());
            return witness;
        }
    }
    /// <summary>
    /// 初始化事件
    /// </summary>
    public class InitWitness : EventWitness
    {
        /// <summary>
        /// 双方玩家所使用的技能卡RID
        /// </summary>
        public int[] skillCardsRID
        {
            get { return getVar<int[]>("skillCardsRID"); }
        }
        /// <summary>
        /// 双方玩家所使用的技能卡DID
        /// </summary>
        public int[] skillCardsDID
        {
            get { return getVar<int[]>("skillCardsDID"); }
        }
        /// <summary>
        /// 双方玩家所使用的卡组主人公RID
        /// </summary>
        public int[] masterCardsRID
        {
            get { return getVar<int[]>("masterCardsRID"); }
        }
        /// <summary>
        /// 双方玩家所使用的卡组主人公DID
        /// </summary>
        public int[] masterCardsDID
        {
            get { return getVar<int[]>("masterCardsDID"); }
        }
        /// <summary>
        /// 按照先后行动顺序排列的玩家索引
        /// </summary>
        public int[] sortedPlayersIndex
        {
            get { return getVar<int[]>("sortedPlayersIndex"); }
        }
        /// <summary>
        /// 所有玩家的初始手牌的RID
        /// </summary>
        public int[][] initCardsRID
        {
            get { return getVar<int[][]>("initCardsRID"); }
        }
        /// <summary>
        /// 所有玩家的初始手牌DID
        /// </summary>
        public int[] initCardsDID
        {
            get { return getVar<int[]>("initCardsDID"); }
        }
        /// <summary>
        /// 玩家自己的卡组中所有卡的DID
        /// </summary>
        public int[] deck
        {
            get { return getVar<int[]>("deck"); }
        }
        public InitWitness() : base("onInit")
        {
        }
    }
    public partial class HeartstoneCardEngine
    {
        public void init()
        {
            doEvent(new InitEvent());
        }
    }
}