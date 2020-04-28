using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
namespace TouhouHeartstone.Builtin
{
    public class Marisa : MasterCardDefine
    {
        public override int id { get; set; } = 2000;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = StartdustBullet.ID;
        public override IEffect[] effects { get; set; } = new Effect[0];
    }
    public class StartdustBullet : SkillCardDefine
    {
        public const int ID = 2001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
}
