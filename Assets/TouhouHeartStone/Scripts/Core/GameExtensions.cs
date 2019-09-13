using System;
using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    public partial class CardEngine
    {
        public void addBuff(Card[] cards, string[] propNames, int[] changeTypes, int[] values)
        {
            foreach (Card card in cards)
            {
                PropertyModifier[] modifiers = new PropertyModifier[propNames.Length];
                for (int i = 0; i < modifiers.Length; i++)
                {
                    modifiers[i] = new PropertyModifier(propNames[i], (PropertyChangeType)changeTypes[i], values[i]);
                }
                card.addBuff(new GeneratedBuff(modifiers));
            }
        }
        public void addBuff(Card[] cards, string propName, int changeType, int value)
        {
            foreach (Card card in cards)
            {
                card.addBuff(new GeneratedBuff(new PropertyModifier(propName, (PropertyChangeType)changeType, value)));
            }
        }
        public readonly int PropertyChangeType_set = 0;
        public readonly int PropertyChangeType_add = 1;
        public Card[] getNearbyCards(Card card)
        {
            if (card.pile.count > 2)
            {
                if (card.pile.indexOf(card) == 0)
                    return new Card[] { card.pile[1] };
                else if (card.pile.indexOf(card) == card.pile.count - 1)
                    return new Card[] { card.pile[card.pile.count - 2] };
                else
                    return new Card[] { card.pile[card.pile.indexOf(card) - 1], card.pile[card.pile.indexOf(card) + 1] };
            }
            else if (card.pile.count > 1)
            {
                if (card.pile.indexOf(card) == 0)
                    return new Card[] { card.pile[1] };
                else
                    return new Card[] { card.pile[0] };
            }
            else
                return new Card[0];
        }
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
        public Card[] getCharacters(Func<Card, bool> filter = null)
        {
            List<Card> charList = new List<Card>();
            foreach (Player player in getPlayers())
            {
                if (filter != null)
                {
                    charList.AddRange(player["Master"].Where(filter));
                    charList.AddRange(player["Field"].Where(filter));
                }
                else
                {
                    charList.AddRange(player["Master"]);
                    charList.AddRange(player["Field"]);
                }
            }
            return charList.ToArray();
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
        public void summon(Player player, CardDefine define, int position = -1)
        {
            summon(player, new Card(this, define), position < 0 ? player["Field"].count : position);
        }
        public void summon(Player player, Card card, int position = -1)
        {
            doEvent(new SummonEvent(player, card, position < 0 ? player["Field"].count : position));
        }
        public void createToken(Player player, CardDefine define, int position)
        {
            Card card = new Card(this, define);
            registerCard(card);
            doEvent(new SummonEvent(player, card, position));
        }
        public void damage(Card card, int amount)
        {
            doEvent(new DamageEvent(card, new int[] { amount }));
        }
        public void damage(Card[] cards, int amount)
        {
            doEvent(new DamageEvent(cards, cards.Select(c => { return amount; }).ToArray()));
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
}