using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone.Backend
{
    public static partial class HeartstoneExtension
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
            Dictionary<int, Card> dicRIDCard = engine.getProp<Dictionary<int, Card>>("dicRIDCard");
            if (dicRIDCard == null)
            {
                dicRIDCard = new Dictionary<int, Card>();
                engine.setProp("dicRIDCard", dicRIDCard);
            }
            engine.setProp("RID", engine.getProp<int>("RID") + 1);
            card.setProp("RID", engine.getProp<int>("RID"));
            dicRIDCard.Add(engine.getProp<int>("RID"), card);
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
        public static Card getCard(this CardEngine engine, int rid)
        {
            Dictionary<int, Card> dicRIDCard = engine.getProp<Dictionary<int, Card>>("dicRIDCard");
            if (dicRIDCard == null)
            {
                dicRIDCard = new Dictionary<int, Card>();
                engine.setProp("dicRIDCard", dicRIDCard);
            }
            return dicRIDCard[rid];
        }
        public static void summon(this CardEngine engine, Player player, Card card, int position)
        {
            engine.doEvent(new SummonEvent(player, card, position));
        }
        public static void createToken(this CardEngine engine, Player player, CardDefine define, int position)
        {
            Card card = new Card(engine, define);
            engine.allocateRID(card);
            engine.doEvent(new SummonEvent(player, card, position));
        }
        public static void damage(this CardEngine engine, Card card, int amount)
        {
            engine.doEvent(new DamageEvent(card, new int[] { amount }));
        }
        public static void damage(this CardEngine engine, Card[] cards, int[] amounts)
        {
            engine.doEvent(new DamageEvent(cards, amounts));
        }
        public static void turnEnd(this CardEngine engine, Player player)
        {
            engine.doEvent(new TurnEndEvent(player));
        }
    }
}