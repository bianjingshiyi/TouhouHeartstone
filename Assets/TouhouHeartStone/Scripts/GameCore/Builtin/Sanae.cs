﻿using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
namespace TouhouHeartstone.Builtin
{
    /// <summary>
    /// 东风谷早苗
    /// </summary>
    public class Sanae : MasterCardDefine
    {
        public const int ID = 0x00300000 | CardCategory.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = WindStopMiracle.ID;
        public override IEffect[] effects { get; set; } = new Effect[0];
    }
    /// <summary>
    /// 风止奇迹：治愈2点生命值。
    /// </summary>
    public class WindStopMiracle : SkillCardDefine
    {
        public const int ID = Sanae.ID | CardCategory.SKILL | 0x000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect(new On<THHPlayer.ActiveEventArg>(),PileName.SKILL,(game,card,vars)=>
            {
                return true;
            },(game,card,targets)=>
            {
                if(targets[0] is Card target && target.getCurrentLife(game)<target.getLife(game))
                    return true;
                return false;

            },async (game,card,vars,targets)=>
            {
                if(targets[0] is Card target)
                    await target.heal(game, 2);
            })
        };
    }
}