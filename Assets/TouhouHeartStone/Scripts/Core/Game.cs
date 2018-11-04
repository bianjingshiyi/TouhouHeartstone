using System;
using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    class Game
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
            SetOrderRecord setOrder = new SetOrderRecord(orderedPlayerId);
            records.addRecord(setOrder);
            //初始化卡组，抽初始卡牌，并保留或者替换
            for (int i = 0; i < players.orderedPlayers.Length; i++)
            {
                int[] presetDeck = new int[30];//预设卡组是空的。
                AddCardRecord setDeck = new AddCardRecord(players.orderedPlayers[i].id, RegionType.deck, cards.createInstances(presetDeck));
                records.addRecord(setDeck);
                InitDrawRecord initDraw = new InitDrawRecord(players.orderedPlayers[i].id, i == 0 ? 3 : 4);
                records.addRecord(initDraw);
            }
        }
        public void initReplace(int playerId, CardInstance[] cards)
        {
            InitReplaceRecord initReplace = new InitReplaceRecord(playerId, cards);
            records.addRecord(initReplace);
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
        public CardManager cards { get; private set; }
        public PlayerManager players { get; private set; }
        public RecordManager records { get; set; }
    }
}