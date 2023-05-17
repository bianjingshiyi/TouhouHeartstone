using System;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;

namespace Game
{
    /// <summary>
    /// 用于UI中，描述一张卡牌UI的外观的类。
    /// </summary>
    public class CardDescriptor
    {
        public int id { get; private set; }
        public CardDefine define { get; private set; }
        private Player owner;
        private int cost;
        private int attack;
        private int health;
        private int currentHealth;
        private int armor;
        private string[] tags;
        private Buff[] buffs;
        public bool isFrozen { get; private set; }
        public bool isTaunt { get; private set; }
        public bool isSilenced { get; private set; }
        public bool hasShield { get; private set; }
        public bool isStealth { get; private set; }
        public bool hasDeathRattle { get; private set; }
        public bool isLethal { get; private set; }
        public bool isDrain { get; private set; }
        public bool isUsed { get; private set; }
        public CardDescriptor()
        {
        }
        public CardDescriptor(IGame game, ICardData card) : this()
        {
            Update(game, card);
        }
        public void Update(IGame game, ICardData card)
        {
            id = card.id;
            owner = card.owner;
            define = card.define;
            cost = card.getCost(game);
            attack = card.getAttack(game);
            health = card.getLife(game);
            currentHealth = card.getCurrentLife(game);
            armor = card.getArmor(game);
            tags = card.getTags(game);
            buffs = card.getBuffs();

            isFrozen = card.isFreeze(game);
            isTaunt = card.isTaunt(game);
            isSilenced = card.isSilenced(game);
            hasShield = card.isShield(game);
            isStealth = card.isStealth(game);
            hasDeathRattle = card.hasDeathRattle(game);
            isLethal = card.isPoisonous(game);
            isDrain = card.isDrain(game);
            isUsed = card.isUsed(game);
        }
        public bool isServant() => define.isServant();
        public bool isSpell() => define.isSpell();
        public bool isMaster() => define.isMaster();
        public bool isSkill() => define.isSkill();
        public bool isItem() => define.isItem();
        public Player getOwner() => owner;
        public int getCost() => Math.Max(0, cost);
        public int getAttack() => Math.Max(0, attack);
        public int getLife() => health;
        public int getCurrentLife() => currentHealth;
        public string[] getTags() => tags ?? Array.Empty<string>();
        public Buff[] getBuffs() => buffs ?? Array.Empty<Buff>();
        public int getArmor() => Math.Max(0, armor);
    }
}
