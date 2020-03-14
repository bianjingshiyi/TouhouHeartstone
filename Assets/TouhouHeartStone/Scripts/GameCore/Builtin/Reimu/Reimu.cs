using TouhouCardEngine;

namespace TouhouHeartstone.Builtin
{
    public class Reimu : MasterCardDefine
    {
        public override int id { get; set; } = 1000;
        public override int skillID
        {
            get { return 1001; }
        }
        public override Effect[] effects { get; } = new Effect[0];
    }
}
