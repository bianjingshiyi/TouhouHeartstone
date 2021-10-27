using NitoriNetwork.Common;
using TouhouCardEngine;
using TouhouHeartstone;
namespace Game
{
    static class THHRoomPlayerDataHelper
    {
        public const string PROP_DECK = "deck";
        public static void setDeck(this IRoomPlayer playerData, Deck deck)
        {
            playerData.setProp(PROP_DECK, deck);
        }
        public static Deck getDeck(this IRoomPlayer playerData)
        {
            if (!playerData.hasProp(PROP_DECK))
                return null;
            return playerData.getProp<Deck>(PROP_DECK);
        }

        public const string PROP_READY = "ready";

        public static bool getReady(this IRoomPlayer playerData)
        {
            if (!playerData.hasProp(PROP_READY))
                return false;
            return playerData.getProp<bool>(PROP_READY);
        }

        public static void setReady(this IRoomPlayer playerData, bool val)
        {
            playerData.setProp(PROP_READY, val);
        }
    }
}
