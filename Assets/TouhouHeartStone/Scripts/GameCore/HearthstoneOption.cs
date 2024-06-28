using System;
using MessagePack;
using TouhouCardEngine;

namespace TouhouHeartstone
{
    public class HearthstoneOption : GameOption
    {
        public static HearthstoneOption Default => new HearthstoneOption();

        // public int randomSeed = 0;
        public int[] sortedPlayers = null;
        public bool shuffle = true;
        public float timeoutForInitReplace = 30;
        public float timeoutForTurn = 75;
        public int startGem = 0;
        public int gameRuleMode = GameRuleMode.CUSTOM;
        public CardPoolInfoPack[] cardPools = new CardPoolInfoPack[0];

        public HearthstoneOption()
        {
        }

        public HearthstoneOption(HearthstoneOption copyTarget)
        {
            randomSeed = copyTarget.randomSeed;
            sortedPlayers = copyTarget.sortedPlayers;
            shuffle = copyTarget.shuffle;
            timeoutForInitReplace = copyTarget.timeoutForInitReplace;
            timeoutForTurn = copyTarget.timeoutForTurn;
            cardPools = copyTarget.cardPools;
            gameRuleMode = copyTarget.gameRuleMode;
        }
    }
    [MessagePackObject]
    [Serializable]
    public class SerializableHearthstoneOption
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableHearthstoneOption()
        {
        }
        public SerializableHearthstoneOption(HearthstoneOption option)
        {
            randomSeed = option.randomSeed;
            sortedPlayers = option.sortedPlayers;
            shuffle = option.shuffle;
            timeoutForInitReplace = option.timeoutForInitReplace;
            timeoutForTurn = option.timeoutForTurn;
            startGem = option.startGem;
            gameRule = option.gameRuleMode;
            cardPools = option.cardPools;
        }
        public HearthstoneOption toOption()
        {
            return new HearthstoneOption()
            {
                randomSeed = randomSeed,
                sortedPlayers = sortedPlayers,
                shuffle = shuffle,
                timeoutForInitReplace = timeoutForInitReplace,
                timeoutForTurn = timeoutForTurn,
                startGem = startGem,
                gameRuleMode = gameRule,
                cardPools = cardPools
            };
        }
        [Key(0)]
        public int randomSeed;
        [Key(1)]
        public int[] sortedPlayers;
        [Key(2)]
        public bool shuffle;
        [Key(3)]
        public float timeoutForInitReplace;
        [Key(4)]
        public float timeoutForTurn;
        [Key(5)]
        public int startGem;
        [Key(6)]
        public CardPoolInfoPack[] cardPools;
        [Key(7)]
        public int gameRule;
    }
}
