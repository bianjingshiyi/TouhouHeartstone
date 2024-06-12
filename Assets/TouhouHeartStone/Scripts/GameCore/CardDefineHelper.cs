using TouhouCardEngine;
using System.Linq;
using System;
using TouhouCardEngine.Interfaces;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    public static class CardDefineHelper
    {
        public static bool isServant(this CardDefine card)
        {
            return card.type == DEFINE.SERVANT;
        }
        public static bool isMaster(this CardDefine card)
        {
            return card.type == DEFINE.MASTER;
        }
        public static bool isSkill(this CardDefine card)
        {
            return card.type == DEFINE.SKILL;
        }
        public static bool isSpell(this CardDefine card)
        {
            return card.type == DEFINE.SPELL;
        }
        public static bool isItem(this CardDefine card)
        {
            return card.type == DEFINE.ITEM;
        }
        public static int getCost(this CardDefine card)
        {
            return card.getProp<int>(DEFINE.COST);
        }

        public static int getAttack(this CardDefine card)
        {
            int result = card.getProp<int>(DEFINE.ATTACK);
            if (result < 0)
                return 0;
            return result;

        }
        public static int getLife(this CardDefine card)
        {
            return card.getProp<int>(DEFINE.LIFE);
        }
        public static int getSpellDamage(this CardDefine card)
        {
            return card.getProp<int>(DEFINE.SPELL_DAMAGE);
        }
        public static void setSpellDamage(this CardDefine card, int value)
        {
            card.setProp(DEFINE.SPELL_DAMAGE, value);
        }
        public static int getDamageReduce(this CardDefine card)
        {
            return card.getProp<int>(DEFINE.DAMAGE_REDUCE);
        }
        public static void setDamageReduce(this CardDefine card, int value)
        {
            card.setProp(DEFINE.DAMAGE_REDUCE, value);
        }
        public static bool hasKeyword(this CardDefine define, string keyword)
        {
            string[] keywords = define.getProp<string[]>(DEFINE.KEYWORDS);
            if (keywords == null)
                return false;
            return keywords.Contains(keyword);
        }
        public static string[] getKeywords(this CardDefine define)
        {
            return define.getProp<string[]>(DEFINE.KEYWORDS);
        }
        public static void setKeywords(this CardDefine define, string[] keywords)
        {
            define.setProp(DEFINE.KEYWORDS, keywords);
        }
        public static void setTags(this CardDefine define, string[] value)
        {
            define.setProp(DEFINE.TAGS, value);
        }
        public static string[] getTags(this CardDefine define)
        {
            string[] tags = define.getProp<string[]>(DEFINE.TAGS);
            if (tags == null)
                return new string[0];
            else
                return tags;
        }
        public static bool hasTag(this CardDefine define, string tag)
        {
            string[] tags = define.getTags();
            if (tags == null)
                return false;
            return tags.Contains(tag);
        }
        public static bool isToken(this CardDefine define)
        {
            return define.getProp<bool>(DEFINE.ISTOKEN);
        }
        public static void setIsToken(this CardDefine cardDefine, bool value)
        {
            cardDefine.setProp(DEFINE.ISTOKEN, value);
        }
        public static void setCloneSource(this CardDefine cardDefine, DefineReference source)
        {
            cardDefine.setProp(DEFINE.CLONE_SOURCE, source);
        }
        public static DefineReference getCloneSource(this CardDefine cardDefine)
        {
            return cardDefine.getProp<DefineReference>(DEFINE.CLONE_SOURCE);
        }
        public static void setEditable(this CardDefine cardDefine, bool source)
        {
            cardDefine.setProp(DEFINE.EDITABLE, source);
        }
        public static bool isEditable(this CardDefine cardDefine)
        {
            return cardDefine.getProp<bool>(DEFINE.EDITABLE);
        }
        public static void setConjoinedCards(this CardDefine cardDefine, DefineReference[] cards)
        {
            cardDefine.setProp(DEFINE.CONJOINED_CARDS, cards);
        }
        public static DefineReference[] getConjoinedCards(this CardDefine cardDefine)
        {
            return cardDefine.getProp<DefineReference[]>(DEFINE.CONJOINED_CARDS);
        }
        /// <summary>
        /// 通过位运算获取卡牌Id中的卡池Id。卡池Id的掩码为0x7FFF0000
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public static int getCardPoolId(int cardId)
        {
            return cardId << 1 >> 17;
        }
        public static int getCardPoolId(this CardDefine card)
        {
            return getCardPoolId(card.id);
        }
        /// <summary>
        /// 通过位运算获得卡牌Id中卡牌序号。卡牌序号的掩码为0x0000FFFF
        /// </summary>
        /// <param name="cardId">采用符号位表示是否为外来，0x7FFF0000作为卡池掩码的卡牌Id</param>
        /// <returns></returns>
        public static int getCardNumber(int cardId)
        {
            return cardId << 16 >> 16;
        }
        public static int getCardNumber(this CardDefine card)
        {
            return getCardNumber(card.id);
        }


        /// <summary>
        /// 这张卡牌定义的主动效果的可选目标
        /// </summary>
        /// <param name="cardDefine"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static Card[] getAvaliableTargets(this CardDefine cardDefine, CardEngine game, Card source)
        {
            List<Card> targetList = new List<Card>();
            if (cardDefine.getEffects().OfType<GeneratedActiveEffect>().FirstOrDefault() is GeneratedActiveEffect generatedEffect)
            {
                targetList.AddRange(generatedEffect.getAvaliableTargets(game, source));
            }
            return targetList.ToArray();
        }
    }
}