using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
namespace TouhouHeartstone.Builtin
{
    public class Reimu : MasterCardDefine
    {
        public const int ID = 0x00100000 | CardCategory.MASTER | 0x000;
        public override int id { get; set; } = 1000;
        public override int life { get; } = 30;
        public override int skillID { get; } = TotematicCall.ID;
        public override IEffect[] effects { get; } = new Effect[0];
    }
}