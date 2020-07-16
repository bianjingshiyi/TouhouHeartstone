using System;
using System.Collections.Generic;
using TouhouCardEngine;
namespace TouhouHeartstone
{
    public static class PileName
    {
        public const string NONE = null;
        public const string MASTER = "Master";
        public const string HAND = "Hand";
        public const string SKILL = "Skill";
        public const string DECK = "Deck";
        public const string FIELD = "Field";
        public const string GRAVE = "Grave";
        public static string[] getPiles(this PileFlag flag)
        {
            List<string> pileList = new List<string>();
            if (flag.HasFlag(PileFlag.master))
                pileList.Add(MASTER);
            if (flag.HasFlag(PileFlag.hand))
                pileList.Add(HAND);
            if (flag.HasFlag(PileFlag.skill))
                pileList.Add(SKILL);
            if (flag.HasFlag(PileFlag.deck))
                pileList.Add(DECK);
            if (flag.HasFlag(PileFlag.field))
                pileList.Add(FIELD);
            if (flag.HasFlag(PileFlag.grave))
                pileList.Add(GRAVE);
            return pileList.ToArray();
        }
        public static Pile[] getPiles(this PileFlag flag, THHGame game, Player player)
        {
            List<Pile> pileList = new List<Pile>();
            if (flag.HasFlag(PileFlag.self))
            {
                if (flag.HasFlag(PileFlag.master))
                    pileList.Add(player[MASTER]);
                if (flag.HasFlag(PileFlag.hand))
                    pileList.Add(player[HAND]);
                if (flag.HasFlag(PileFlag.skill))
                    pileList.Add(player[SKILL]);
                if (flag.HasFlag(PileFlag.deck))
                    pileList.Add(player[DECK]);
                if (flag.HasFlag(PileFlag.field))
                    pileList.Add(player[FIELD]);
                if (flag.HasFlag(PileFlag.grave))
                    pileList.Add(player[GRAVE]);
            }
            if (flag.HasFlag(PileFlag.oppo))
            {
                player = game.getOpponent(player);
                if (flag.HasFlag(PileFlag.master))
                    pileList.Add(player[MASTER]);
                if (flag.HasFlag(PileFlag.hand))
                    pileList.Add(player[HAND]);
                if (flag.HasFlag(PileFlag.skill))
                    pileList.Add(player[SKILL]);
                if (flag.HasFlag(PileFlag.deck))
                    pileList.Add(player[DECK]);
                if (flag.HasFlag(PileFlag.field))
                    pileList.Add(player[FIELD]);
                if (flag.HasFlag(PileFlag.grave))
                    pileList.Add(player[GRAVE]);
            }
            return pileList.ToArray();
        }
    }
    [Flags]
    public enum PileFlag
    {
        none = 0b_000000,
        master = 0b_000001,
        hand = 0b_000010,
        skill = 0b_000100,
        deck = 0b_001000,
        field = 0b_010000,
        grave = 0b_100000,

        self = 0b_01000000,
        oppo = 0b_10000000,
        both = 0b_11000000,
    }
}