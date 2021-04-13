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

        public const int MASTER = 0x00000000;
        public const int SKILL = 0x00001000;
        public const int SERVANT = 0x00002000;
        public const int SPELL = 0x00003000;
        public const int ITEM = 0x00004000;
        public const int TOKEN = 0x00000800;
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
        public static int getCharacterID(this CardDefine define)
        {
            return getCharacterID(define.id);
        }
        /// <summary>
        /// 获取给出的所有卡片定义的所属角色ID
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public static int[] getCharactersID(this IEnumerable<CardDefine> cards)
        {
            return cards.GroupBy(c => c.getCharacterID()).Select(g => g.Key).ToArray();
        }
        public static int getCategory(int id)
        {
            return id & CATEGORY_MASK;
        }
        public static int getCategory(this CardDefine define)
        {
            return getCategory(define.id);
        }

        public static IEnumerable<CardDefine> getDefines(this CategoryFlag flag, CardEngine game)
        {
            return game.getDefines().Where(define => flag.contains(define.id));
        }

        private static bool contains(this CategoryFlag flag, int id)
        {
            int category = getCategory(id);
            int character = getCharacterID(id);
            bool catInRange = false;
            bool charInRange = false;
            switch (category)
            {
                case MASTER:
                    if (flag.HasFlag(CategoryFlag.master))
                        catInRange = true;
                    break;
                case SKILL:
                    if (flag.HasFlag(CategoryFlag.skill))
                        catInRange = true;
                    break;
                case SPELL:
                    if (flag.HasFlag(CategoryFlag.spell))
                        catInRange = true;
                    break;
                case SERVANT:
                    if (flag.HasFlag(CategoryFlag.servant))
                        catInRange = true;
                    break;
                case ITEM:
                    if (flag.HasFlag(CategoryFlag.item))
                        catInRange = true;
                    break;
                case TOKEN:
                    if (flag.HasFlag(CategoryFlag.token))
                        catInRange = true;
                    break;
            }

            switch (character)
            {
                case CHARACTER_NEUTRAL:
                    if (flag.HasFlag(CategoryFlag.neutral))
                        charInRange = true;
                    break;
                case CHARACTER_SANAE:
                    if (flag.HasFlag(CategoryFlag.sanae))
                        charInRange = true;
                    break;
                case CHARACTER_PATCHOULI:
                    if (flag.HasFlag(CategoryFlag.patchouli))
                        charInRange = true;
                    break;
                case CHARACTER_YOUMU:
                    if (flag.HasFlag(CategoryFlag.youmu))
                        charInRange = true;
                    break;
                case CHARACTER_RIN:
                    if (flag.HasFlag(CategoryFlag.rin))
                        charInRange = true;
                    break;
                case CHARACTER_NAZRIN:
                    if (flag.HasFlag(CategoryFlag.nazrin))
                        charInRange = true;
                    break;
                case CHARACTER_REISEN:
                    if (flag.HasFlag(CategoryFlag.reisen))
                    {
                        charInRange = true;
                    }
                    break;
            }

            return catInRange && charInRange;
        }
    }
    
    [Flags]
    public enum CategoryFlag
    {
        none = 0b_000000000,
        master = 0b_0000001,
        skill = 0b_000000010,
        servant = 0b_00000100,
        spell = 0b_00000001000,
        item = 0b_0000000010000,
        token = 0b_0000000100000,
        neutral = 0b_000001000000,
        sanae = 0b_000000010000000,
        patchouli = 0b_000100000000,
        youmu = 0b_00000001000000000,
        reisen = 0b_00000010000000000,
        rin = 0b_000000000100000000000,
        nazrin = 0b_0000001000000000000,
        allCharacters =  neutral | sanae | patchouli | youmu | reisen | nazrin,
        allTypes = master | skill | servant | spell | item,
        all = allCharacters | allTypes | token,
    }
}