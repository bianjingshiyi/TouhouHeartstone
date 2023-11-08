using System;
using TouhouCardEngine;

namespace Game.Replays
{
    public class ReplayMeta
    {
        public DefineReference playerAMasterDefRef;
        public DefineReference playerBMasterDefRef;
        public CardPoolInfoPack[] requiredCardPools;
        public string name;
        public string path;
    }
    public class InvalidReplayMeta : ReplayMeta
    {
        public InvalidReplayMeta()
        {
            playerAMasterDefRef = new DefineReference(0, 0);
            playerBMasterDefRef = new DefineReference(0, 0);
            requiredCardPools = new CardPoolInfoPack[0];
        }
    }
    [Serializable]
    public class SerializableReplayMeta
    {
        public SerializableReplayMeta(ReplayMeta meta)
        {
            playerAMasterDefRef = meta.playerAMasterDefRef;
            playerBMasterDefRef = meta.playerBMasterDefRef;
            requiredCardPools = meta.requiredCardPools;
            name = meta.name;
        }
        public ReplayMeta toMeta()
        {
            return new ReplayMeta()
            {
                playerAMasterDefRef = playerAMasterDefRef,
                playerBMasterDefRef = playerBMasterDefRef,
                requiredCardPools = requiredCardPools,
                name = name
            };
        }
        public DefineReference playerAMasterDefRef;
        public DefineReference playerBMasterDefRef;
        public CardPoolInfoPack[] requiredCardPools;
        public string name;
    }
}
