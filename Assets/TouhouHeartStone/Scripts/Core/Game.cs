using System;
using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    public class Game
    {
        public Game(int randomSeed)
        {
            random = new Random(randomSeed);
            cards = new CardManager();
            records = new RecordManager(this);
        }
        /// <summary>
        /// 开始游戏，需要提供游戏中玩家的id。
        /// </summary>
        /// <param name="playersId">玩家id数组</param>
        public void start(int[] playersId)
        {
            //初始化玩家
            players = new PlayerManager(playersId);
            //决定回合顺序
            List<int> unorderedPlayers = new List<int>(players.Select(e => { return e.id; }));
            int[] orderedPlayerId = new int[unorderedPlayers.Count];
            for (int i = 0; i < orderedPlayerId.Length; i++)
            {
                int index = random.Next(0, unorderedPlayers.Count);
                orderedPlayerId[i] = unorderedPlayers[index];
                unorderedPlayers.RemoveAt(index);
            }
            records.addRecord(new SetOrderRecord(orderedPlayerId));
            //初始化卡组，抽初始卡牌，并保留或者替换
            for (int i = 0; i < players.orderedPlayers.Length; i++)
            {
                int[] presetDeck = new int[30];//预设卡组是空的。
                records.addRecord(new AddCardRecord(players.orderedPlayers[i].id, RegionType.deck, cards.createInstances(presetDeck)));
                records.addRecord(new InitDrawRecord(players.orderedPlayers[i].id, i == 0 ? 3 : 4));
            }
        }
        public void initReplace(int playerId, CardInstance[] cards)
        {
            if (cards.Length > 0)
                records.addRecord(new InitReplaceRecord(playerId, cards));
            preparedPlayers.Add(playerId);
            //if (players.orderedPlayers.All(e => { return preparedPlayers.Contains(e.id); }))
            duelStart();
        }
        HashSet<int> preparedPlayers { get; } = new HashSet<int>();
        void duelStart()
        {
            records.addRecord(new DuelStartRecord());
            turnStart(players.orderedPlayers[0]);
        }
        void turnStart(Player player)
        {
            records.addRecord(new TurnStartRecord(player.id));
            records.addRecord(new AddCrystalRecord(player.id, 1, CrystalState.normal));
            draw(player, 1);
        }
        void draw(Player player, int count)
        {
            records.addRecord(new DrawRecord(player.id, 1));
        }
        public void use(int playerId, int instance, int position, int target)
        {
            Card card = cards.getCard(instance);
            card.use(position, cards.getCard(target));
        }
        public void turnEnd(int playerId)
        {
            Player player = players.getPlayer(playerId);
            int index = Array.IndexOf(players.orderedPlayers, player) + 1;
            if (index >= players.orderedPlayers.Length)
                index = 0;
            Player nextPlayer = players.orderedPlayers[index];
            turnStart(nextPlayer);
        }
        /// <summary>
        /// 随机整数，注意该函数返回的值可能包括最大值与最小值。
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>介于最大值与最小值之间，可能为最大值也可能为最小值</returns>
        public int randomInt(int min, int max)
        {
            return random.Next(min, max + 1);
        }
        /// <summary>
        /// 随机实数，注意该函数返回的值可能包括最小值，但是不包括最大值。
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>介于最大值与最小值之间，不包括最大值</returns>
        public float randomFloat(float min, float max)
        {
            return (float)(random.NextDouble() * (max - min) + min);
        }
        Random random { get; set; }
        internal CardManager cards { get; private set; }
        internal PlayerManager players { get; private set; }
        public RecordManager records { get; set; }
    }
}