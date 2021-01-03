using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;



namespace TouhouHeartstone.Builtin.Proble
{
    public class Aya : MasterCardDefine
    {
        public const int ID = 0x00700000 | CATEGORY.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = SpareFilm.ID;//英雄技能的ID
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }

    /// <summary>
    /// 技能：备用胶卷
    /// 下一张牌灵力值消耗减少1点
    /// </summary>
    public class SpareFilm : SkillCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.SKILL | 0x000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1; //技能花费
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().master.addBuff(game, new GeneratedBuff(ID,
                new Halo(new GeneratedBuff(ID, new CostModifier(-1)), PileFlag.self | PileFlag.hand, PileFlag.self | PileFlag.master, (g1, c1) =>
                {
                    return !c1.isSkill();
                }),
                new RemoveBuffBefore<THHPlayer.UseEventArg>(PILE.MASTER, (g, c, a) =>
                {
                    return a.player == card.getOwner() && !card.isSkill();
                }, ID)));
        }


    }
}
