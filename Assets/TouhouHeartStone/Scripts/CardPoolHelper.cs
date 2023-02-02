using Controllers;
using TouhouCardEngine;

namespace Game
{
    public static class CardPoolHelper
    {
        public static CardPoolInfoPack ToCardPoolInfoPack(this CardPoolInfo info)
        {
            return new CardPoolInfoPack(info.cardPoolId, info.cardPoolName, info.editTime, info.dependencies ?? new long[0]);
        }
        public readonly static CardPoolInfoPack BuiltinCardPoolInfoPack = new CardPoolInfoPack(0, StringTable.CARDPOOL_NAME_BUILTIN, 0);
        
    }
}
