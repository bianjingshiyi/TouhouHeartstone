using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
namespace TouhouHeartstone.Builtin
{
    public class Marisa : MasterCardDefine
    {
        public override int id { get; set; } = 2000;
        public override int life { get; } = 30;
        public override int skillID
        {
            get { return 2001; }
        }
        public override IEffect[] effects { get; set; } = new Effect[0];
    }
    public class StartdustBullet : SkillCardDefine
    {
        public override int id { get; set; } = 2001;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
}
