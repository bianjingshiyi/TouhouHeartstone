namespace TouhouHeartstone.Backend.Builtin
{
    public class StartdustBullet : SkillCardDefine
    {
        public override int id
        {
            get { return 2001; }
        }
        public override int cost
        {
            get { return 2; }
        }
        public override Effect[] effects { get; } = new Effect[0];
    }
}
