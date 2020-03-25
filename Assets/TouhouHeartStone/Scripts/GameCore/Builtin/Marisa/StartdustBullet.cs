using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
namespace TouhouHeartstone.Builtin
{
    public class StartdustBullet : SkillCardDefine
    {
        public override int id { get; set; } = 2001;
        public override int cost
        {
            get { return 2; }
        }
        public override IEffect[] effects { get; } = new IEffect[0];
    }
}