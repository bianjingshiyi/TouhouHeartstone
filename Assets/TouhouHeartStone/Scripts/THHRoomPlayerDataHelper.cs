using NitoriNetwork.Common;
using TouhouCardEngine;
using TouhouHeartstone;
using System.Linq;
namespace Game
{
    static class THHRoomPlayerDataHelper
    {
        public const string PROP_DECK = "deck";
        public static void setDeck(this IRoomPlayer playerData, Deck deck)
        {
            playerData.setProp(PROP_DECK, deck.toDefRefArray(true));
        }
        public static Deck getDeck(this IRoomPlayer playerData)
        {
            if (!playerData.hasProp(PROP_DECK))
                return null;
            DefineReference[] defRefs = playerData.getProp<DefineReference[]>(PROP_DECK);
            return new Deck(defRefs[0], defRefs.Skip(1));
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
