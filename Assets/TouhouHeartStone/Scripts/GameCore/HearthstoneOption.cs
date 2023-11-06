using System;
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
        }
    }
    [Serializable]
    public class SerializableHearthstoneOption
    {
        public SerializableHearthstoneOption(HearthstoneOption option)
        {
            randomSeed = option.randomSeed;
            sortedPlayers = option.sortedPlayers;
            shuffle = option.shuffle;
            timeoutForInitReplace = option.timeoutForInitReplace;
            timeoutForTurn = option.timeoutForTurn;
            startGem = option.startGem;
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
                cardPools = cardPools
            };
        }
        public int randomSeed;
        public int[] sortedPlayers;
        public bool shuffle;
        public float timeoutForInitReplace;
        public float timeoutForTurn;
        public int startGem;
        public CardPoolInfoPack[] cardPools;
    }
}
