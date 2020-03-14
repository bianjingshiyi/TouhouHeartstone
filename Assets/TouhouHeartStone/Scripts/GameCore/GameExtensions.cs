using System;
using System.Linq;
using System.Collections.Generic;

using TouhouCardEngine;

namespace TouhouHeartstone
{
    public static partial class CardEngineExtension /*HeartstoneCardEngine : CardEngine*/
    {
        //public HeartstoneCardEngine(IGameEnvironment env, Rule rule, int randomSeed) : base(env, rule, randomSeed)
        //{
        //}
        //public static void addBuff(this CardEngine engine, Card[] cards, string[] propNames, int[] changeTypes, int[] values)
        //{
        //    foreach (Card card in cards)
        //    {
        //        PropertyModifier[] modifiers = new PropertyModifier[propNames.Length];
        //        for (int i = 0; i < modifiers.Length; i++)
        //        {
        //            modifiers[i] = new PropertyModifier(propNames[i], (PropertyChangeType)changeTypes[i], values[i]);
        //        }
        //        card.addBuff(new GeneratedBuff(modifiers));
        //    }
        //}
        //public static void addBuff(this CardEngine engine, Card[] cards, string propName, int changeType, int value)
        //{
        //    foreach (Card card in cards)
        //    {
        //        card.addBuff(new GeneratedBuff(new PropertyModifier(propName, (PropertyChangeType)changeType, value)));
        //    }
        //}
        //public static readonly int PropertyChangeType_set = 0;
        //public static readonly int PropertyChangeType_add = 1;
        //public static Card[] getNearbyCards(this CardEngine engine, Card card)
        //{
        //    if (card.pile.count > 2)
        //    {
        //        if (card.pile.indexOf(card) == 0)
        //            return new Card[] { card.pile[1] };
        //        else if (card.pile.indexOf(card) == card.pile.count - 1)
        //            return new Card[] { card.pile[card.pile.count - 2] };
        //        else
        //            return new Card[] { card.pile[card.pile.indexOf(card) - 1], card.pile[card.pile.indexOf(card) + 1] };
        //    }
        //    else if (card.pile.count > 1)
        //    {
        //        if (card.pile.indexOf(card) == 0)
        //            return new Card[] { card.pile[1] };
        //        else
        //            return new Card[] { card.pile[0] };
        //    }
        //    else
        //        return new Card[0];
        //}
        //public static Card getRandomEnemy(this CardEngine engine, Player player)
        //{
        //    Card[] enemies = engine.getAllEnemies(player);
        //    return enemies[engine.randomInt(0, enemies.Length - 1)];
        //}
        //public static Card[] getAllEnemies(this CardEngine engine, Player player)
        //{
        //    List<Card> enemyList = new List<Card>();
        //    foreach (Player opponent in engine.getPlayers().Where(p => { return p != player; }))
        //    {
        //        enemyList.AddRange(opponent["Master"]);
        //        enemyList.AddRange(opponent["Field"]);
        //    }
        //    return enemyList.ToArray();
        //}
        //public static Card[] getCharacters(this CardEngine engine, Func<Card, bool> filter = null)
        //{
        //    List<Card> charList = new List<Card>();
        //    foreach (Player player in engine.getPlayers())
        //    {
        //        if (filter != null)
        //        {
        //            charList.AddRange(player["Master"].Where(filter));
        //            charList.AddRange(player["Field"].Where(filter));
        //        }
        //        else
        //        {
        //            charList.AddRange(player["Master"]);
        //            charList.AddRange(player["Field"]);
        //        }
        //    }
        //    return charList.ToArray();
        //}
        //public static void summon(this CardEngine engine, Player player, CardDefine define, int position = -1)
        //{
        //    engine.summon(player, new Card(define), position < 0 ? player["Field"].count : position);
        //}
        //public static void createToken(this CardEngine engine, Player player, CardDefine define, int position)
        //{
        //    Card card = new Card(define);
        //    engine.registerCard(card);
        //    engine.doEvent(new SummonEvent(player, card, position));
        //}
        public static void damage(this CardEngine engine, Card card, int amount)
        {
            engine.doEvent(new DamageEvent(card, new int[] { amount }));
        }
        //public static void damage(this CardEngine engine, Card[] cards, int amount)
        //{
        //    engine.doEvent(new DamageEvent(cards, cards.Select(c => { return amount; }).ToArray()));
        //}
        public static void damage(this CardEngine engine, Card[] cards, int[] amounts)
        {
            engine.doEvent(new DamageEvent(cards, amounts));
        }
    }
}