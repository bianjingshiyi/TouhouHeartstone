using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    public partial class CardEngine
    {
        public Card getRandomEnemy(Player player)
        {
            Card[] enemies = getAllEnemies(player);
            return enemies[randomInt(0, enemies.Length - 1)];
        }
        public Card[] getAllEnemies(Player player)
        {
            List<Card> enemyList = new List<Card>();
            foreach (Player opponent in getPlayers().Where(p => { return p != player; }))
            {
                enemyList.AddRange(opponent["Master"]);
                enemyList.AddRange(opponent["Field"]);
            }
            return enemyList.ToArray();
        }
        public void setMaxGem(Player player, int value)
        {
            doEvent(new MaxGemChangeEvent(player, value));
        }
        public void setGem(Player player, int value)
        {
            doEvent(new GemChangeEvent(player, value));
        }
        public void draw(Player player)
        {
            if (player["Deck"].count > 0)
            {
                if (player["Hand"].count > 9)
                    burn(player, player["Deck"].top);
                else
                    doEvent(new DrawEvent(player));
            }
            else
                doEvent(new TiredEvent(player));
        }
        public void burn(Player player, Card card)
        {
            doEvent(new BurnEvent(player, card));
        }
        public int allocateRID(Card card)
        {
            Dictionary<int, Card> dicRIDCard = getProp<Dictionary<int, Card>>("dicRIDCard");
            if (dicRIDCard == null)
            {
                dicRIDCard = new Dictionary<int, Card>();
                setProp("dicRIDCard", dicRIDCard);
            }
            setProp("RID", getProp<int>("RID") + 1);
            card.setProp("RID", getProp<int>("RID"));
            dicRIDCard.Add(getProp<int>("RID"), card);
            return card.getProp<int>("RID");
        }
        public int[] allocateRID(Card[] cards)
        {
            return cards.Select(c => { return allocateRID(c); }).ToArray();
        }
        public Card getCard(int rid)
        {
            Dictionary<int, Card> dicRIDCard = getProp<Dictionary<int, Card>>("dicRIDCard");
            if (dicRIDCard == null)
            {
                dicRIDCard = new Dictionary<int, Card>();
                setProp("dicRIDCard", dicRIDCard);
            }
            return dicRIDCard[rid];
        }
        public void summon(Player player, Card card, int position)
        {
            doEvent(new SummonEvent(player, card, position));
        }
        public void createToken(Player player, CardDefine define, int position)
        {
            Card card = new Card(this, define);
            allocateRID(card);
            doEvent(new SummonEvent(player, card, position));
        }
        public void damage(Card card, int amount)
        {
            doEvent(new DamageEvent(card, new int[] { amount }));
        }
        public void damage(Card[] cards, int[] amounts)
        {
            doEvent(new DamageEvent(cards, amounts));
        }
        public void turnEnd(Player player)
        {
            doEvent(new TurnEndEvent(player));
        }
    }
    public static class CardExtensions
    {
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