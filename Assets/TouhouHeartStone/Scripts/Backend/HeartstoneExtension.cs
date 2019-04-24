using System.Linq;

namespace TouhouHeartstone.Backend
{
    static partial class HeartstoneExtension
    {
        public static void setMaxGem(this CardEngine engine, Player player, int value)
        {
            engine.doEvent(new MaxGemChangeEvent(player, value));
        }
        public static void setGem(this CardEngine engine, Player player, int value)
        {
            engine.doEvent(new GemChangeEvent(player, value));
        }
        public static void draw(this CardEngine engine, Player player)
        {
            if (player["Deck"].count > 0)
            {
                if (player["Hand"].count > 9)
                    engine.burn(player, player["Deck"].top);
                else
                    engine.doEvent(new DrawEvent(player));
            }
            else
                engine.doEvent(new TiredEvent(player));
        }
        public static void burn(this CardEngine engine, Player player, Card card)
        {
            engine.doEvent(new BurnEvent(player, card));
        }
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
        public static void damage(this CardEngine engine, Card card, int amount)
        {
            engine.doEvent(new DamageEvent(card, new int[] { amount }));
        }
        public static void damage(this CardEngine engine, Card[] cards, int[] amounts)
        {
            engine.doEvent(new DamageEvent(cards, amounts));
        }
    }
}