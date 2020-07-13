using TouhouCardEngine;

namespace TouhouHeartstone
{
    /// <summary>
    /// 关于ID格式，目前是8位十六进制，前3位为人物ID，中间2位是卡牌种类，后3位是卡片自己的ID。
    /// </summary>
    public static class CardCategory
    {
        public const int CHARACTER_MASK = 0x0ff00000;
        public const int CATEGORY_MASK = 0x000ff000;
        public const int CARDID_MASK = 0x00000fff;

        public const int MASTER = 0x00000000;
        public const int SKILL = 0x00001000;
        public const int SERVANT = 0x00002000;
        public const int SPELL = 0x00003000;
        public const int ITEM = 0x00004000;
        public const int CHARACTER_NEUTRAL = 0x00000000;

        public static int getCharacterID(int id)
        {
            return id & CHARACTER_MASK;
        }
        public static int getCharacterID(this CardDefine define)
        {
            return getCharacterID(define.id);
        }
        public static int getCategory(int id)
        {
            return id & CATEGORY_MASK;
        }
        public static int getCategory(this CardDefine define)
        {
            return getCategory(define.id);
        }
    }
}