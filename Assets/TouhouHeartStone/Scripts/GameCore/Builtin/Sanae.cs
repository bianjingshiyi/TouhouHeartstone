using TouhouCardEngine;
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
        public override int life { get; } = 30;
        public override int skillID { get; } = WindStopMiracle.ID;
        public override IEffect[] effects { get; } = new Effect[0];
    }
    /// <summary>
    /// 风止奇迹：治愈2点生命值。
    /// </summary>
    public class WindStopMiracle : SkillCardDefine
    {
        public const int ID = Sanae.ID | CardCategory.SKILL | 0x000;
        public override int id { get; set; } = ID;
        public override int cost
        {
            get { return 2; }
        }
        public override IEffect[] effects { get; } = new IEffect[]
        {
            new THHEffect(new On<THHPlayer.ActiveEventArg>(),PileName.SKILL,(game,player,card,vars)=>
            {
                return true;
            },(game,player,card,targets)=>
            {
                if(targets[0] is Card target && target.getCurrentLife()<target.getLife())
                    return true;
                return false;

            },async (game,player,card,vars,targets)=>
            {
                if(targets[0] is Card target)
                    await target.heal(game, 2);
            })
        };
    }
}