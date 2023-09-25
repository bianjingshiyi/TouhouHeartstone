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
            return card.type == DEFINE.MASTER || card is MasterCardDefine;
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
            if (card is ServantCardDefine servant)
            {
                return servant.spellDamage;
            }
            return card.getProp<int>(DEFINE.SPELL_DAMAGE);
        }
        public static void setSpellDamage(this CardDefine card, int value)
        {
            if (card is ServantCardDefine servant)
            {
                servant.spellDamage = value;
            }
            else
            {
                card.setProp(DEFINE.SPELL_DAMAGE, value);
            }
        }
        public static int getDamageReduce(this CardDefine card)
        {
            if (card is ServantCardDefine servant)
            {
                return servant.damageReduce;
            }
            return card.getProp<int>(DEFINE.DAMAGE_REDUCE);
        }
        public static void setDamageReduce(this CardDefine card, int value)
        {
            if (card is ServantCardDefine servant)
            {
                servant.damageReduce = value;
            }
            else
            {
                card.setProp(DEFINE.DAMAGE_REDUCE, value);
            }
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
            if (define is THHCardDefine thhCardDefine)
            {
                return thhCardDefine.tags;
            }
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
            if (define is THHCardDefine thhCardDefine)
            {
                return thhCardDefine.isToken;
            }
            else
            {
                return define.getProp<bool>(DEFINE.ISTOKEN);
            }
        }
        public static void setIsToken(this CardDefine cardDefine, bool value)
        {
            if (cardDefine is THHCardDefine thhCardDefine)
            {
                thhCardDefine.isToken = value;
            }
            else
            {
                cardDefine.setProp(DEFINE.ISTOKEN, value);
            }
        }
        /// <summary>
        /// 卡牌卡池ID不为0表示它是外部卡牌
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [Obsolete("Use CardManager.isExternalCard instead")]
        public static bool isExternalCard(DefineReference cardRef)
        {
            return cardRef.cardPoolId != 0;
        }
        [Obsolete("Use CardManager.isExternalCard instead")]
        public static bool isExternalCard(this CardDefine card)
        {
            return card.cardPoolId != 0;
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

        public static IEffect[] getAuras(this CardDefine card)
        {
            return card.getEffects().Where(e => e.isAura()).ToArray();
        }
        public static IEffect[] getBattlecries(this CardDefine card)
        {
            return card.getEffects().Where(e => 
            {
                if (e.isBattlecry())
                    return true;
                if (e is ActiveEffect && (card.isServant() || card.isItem()))
                    return true;
                return false;
            }).ToArray();
        }
        public static bool hasBattlecry(this CardDefine card)
        {
            return card.getEffects().Any(e =>
            {
                if (e.isBattlecry())
                    return true;
                if (e is ActiveEffect && (card.isServant() || card.isItem()))
                    return true;
                return false;
            });
        }
        public static IEffect[] getDeathRattles(this CardDefine card)
        {
            return card.getEffects().Where(e => e.isDeathRattle()).ToArray();
        }
        public static bool hasDeathRattles(this CardDefine card)
        {
            return card.getEffects().Any(e => e.isDeathRattle());
        }
        public static bool isTrap(this CardDefine cardDefine)
        {
            return cardDefine.getGeneratedEffects().Any(e => e.isTrap()) || cardDefine.hasKeyword(KEYWORD.SECRET);
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
            if (cardDefine.getActiveEffect() is ITargetEffect targetEffect)
            {
                targetList.AddRange(targetEffect.getAvaliableTargets(game, source));
            }
            else if (cardDefine.getEffects().OfType<GeneratedActiveEffect>().FirstOrDefault() is GeneratedActiveEffect generatedEffect)
            {
                targetList.AddRange(generatedEffect.getAvaliableTargets(game, source));
            }
            return targetList.ToArray();
        }
    }
}