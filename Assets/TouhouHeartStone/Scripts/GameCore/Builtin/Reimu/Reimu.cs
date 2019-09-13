namespace TouhouHeartstone.Backend.Builtin
{
    public class Reimu : MasterCardDefine
    {
        public override int id
        {
            get { return 1000; }
        }
        public override int skillID
        {
            get { return 1001; }
        }
        public override int category
        {
            get { return 1000; }
        }
        public override Effect[] effects { get; } = new Effect[0];
    }
}
