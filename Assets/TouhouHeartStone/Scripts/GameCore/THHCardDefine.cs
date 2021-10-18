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
    }
}