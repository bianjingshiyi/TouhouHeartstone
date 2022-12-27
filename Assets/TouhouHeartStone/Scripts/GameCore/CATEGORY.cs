using System;
using TouhouCardEngine;
using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    /// <summary>
    /// 关于ID格式，目前是8位十六进制，前3位为人物ID，中间2位是卡牌种类，后3位是卡片自己的ID。
    /// </summary>
    public static class CATEGORY
    {
        public const int CHARACTER_MASK = 0x0ff00000;
        public const int CATEGORY_MASK = 0x000ff000;
        public const int CARDID_MASK = 0x00000fff;

        public const int NONE = 0x00000000;
        public const int MASTER = 0x00000000;
        public const int SKILL = 0x00001000;
        public const int SERVANT = 0x00002000;
        public const int SPELL = 0x00003000;
        public const int ITEM = 0x00004000;
        public const int CHARACTER_NEUTRAL = 0x00000000;
        public const int CHARACTER_SANAE = 0x00300000;
        public const int CHARACTER_PATCHOULI = 0x00400000;
        public const int CHARACTER_YOUMU = 0x00500000;
        public const int CHARACTER_REISEN = 0x00600000;
        public const int CHARACTER_RIN = 0x00700000;
        public const int CHARACTER_NAZRIN = 0x00800000;


        public static int getCharacterID(int id)
        {
            return id & CHARACTER_MASK;
        }

        /// <summary>
        /// 获取卡片的所属角色ID
        /// </summary>
        /// <param name="define"></param>
        /// <returns></returns>
        public static DefineReference getCharacterRef(this CardDefine define)
        {
            if (define.hasProp(DEFINE.CHARACTER_ID))
            {
                if (define.getProp<DefineReference>(DEFINE.CHARACTER_ID) is DefineReference characterRef)
                    return characterRef;
                else
                {
                    // 旧版角色ID。
                    int id = define.getProp<int>(DEFINE.CHARACTER_ID);
                    return new DefineReference(id << 1 >> 17, id);
                }
            }
            if (define.cardPoolId == 0) //内置卡牌
            {
                return new DefineReference(0, getCharacterID(define.id));
            }
            // 中立卡牌。
            return new DefineReference(0, 0);
        }
        public static void setCharacterRef(this CardDefine define, DefineReference characterRef)
        {
            define.setProp(DEFINE.CHARACTER_ID, characterRef);
        }
        /// <summary>
        /// 获取给出的所有卡片定义的所属角色ID
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public static DefineReference[] getCharactersID(this IEnumerable<CardDefine> cards)
        {
            return cards.GroupBy(c => c.getCharacterRef()).Select(g => g.Key).ToArray();
        }

        public static int getCategory(int id)
        {
            return id & CATEGORY_MASK;
        }

        public static int getCategory(this CardDefine define)
        {
            return getCategory(define.id);
        }

        public static CardDefine[] getDefinesOf(this CardEngine game, params int[] characterID)
        {
            return game.getDefines().Where(define => characterID.Contains(getCharacterID(define.id))).ToArray();
        }

        public static int[] CHARACTER_ALL = new[]
        {
            CHARACTER_NEUTRAL, CHARACTER_RIN, CHARACTER_SANAE, CHARACTER_YOUMU, CHARACTER_NAZRIN, CHARACTER_REISEN,
            CHARACTER_PATCHOULI
        };
    }
}