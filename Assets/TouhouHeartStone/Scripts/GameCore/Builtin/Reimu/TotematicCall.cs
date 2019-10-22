using TouhouCardEngine;

namespace TouhouHeartstone.Backend.Builtin
{
    public class TotematicCall : SkillCardDefine
    {
        public override int id { get; set; } = 1001;
        public override int cost
        {
            get { return 2; }
        }
        public override Effect[] effects => new Effect[0];
    }
}
