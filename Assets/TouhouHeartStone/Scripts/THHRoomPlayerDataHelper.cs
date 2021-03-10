using TouhouCardEngine;

namespace Game
{
    static class THHRoomPlayerDataHelper
    {
        const string PROP_DECK = "deck";
        public static void setDeck(this RoomPlayerData playerData, int[] deck)
        {
            playerData.propDict[PROP_DECK] = deck;
        }
        public static int[] getDeck(this RoomPlayerData playerData)
        {
            return playerData.propDict[PROP_DECK] as int[];
        }
    }
}
