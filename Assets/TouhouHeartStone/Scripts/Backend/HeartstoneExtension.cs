using System.Linq;

namespace TouhouHeartstone.Backend
{
    static partial class HeartstoneExtension
    {
        public static int allocateRID(this CardEngine engine, Card card)
        {
            engine.setProp("RID", engine.getProp<int>("RID") + 1);
            card.setProp("RID", engine.getProp<int>("RID"));
            return card.getProp<int>("RID");
        }
        public static int[] allocateRID(this CardEngine engine, Card[] cards)
        {
            return cards.Select(c => { return engine.allocateRID(c); }).ToArray();
        }
        public static int getRID(this Card card)
        {
            return card.getProp<int>("RID");
        }
        public static int[] getRID(this Card[] cards)
        {
            return cards.Select(c => { return c.getRID(); }).ToArray();
        }
    }
}