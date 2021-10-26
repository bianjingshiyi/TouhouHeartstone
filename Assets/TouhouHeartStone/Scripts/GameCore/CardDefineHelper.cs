using TouhouCardEngine;
using System.Linq;

namespace TouhouHeartstone
{
    public static class CardDefineHelper
    {
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

        public static bool hasKeyword(this CardDefine define, string keyword)
        {
            string[] keywords = define.getProp<string[]>(DEFINE.KEYWORDS);
            if (keywords == null)
                return false;
            return keywords.Contains(keyword);
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
            if (define is THHCardDefine thhCardDefine)
            {
                return thhCardDefine.isToken;
            }
            else
            {
                return define.getProp<bool>(DEFINE.ISTOKEN);
            }
        }
        /// <summary>
        /// 卡牌Id的符号位为负表示它是外部卡牌
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public static bool isExternalCard(int cardId)
        {
            return cardId < 0;
        }
        public static bool isExternalCard(this CardDefine card)
        {
            return isExternalCard(card.id);
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
    }
}