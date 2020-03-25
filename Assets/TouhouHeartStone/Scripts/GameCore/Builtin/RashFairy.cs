using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
namespace TouhouHeartstone.Builtin
{
    /// <summary>
    /// 鲁莽的妖精，111，冲锋
    /// </summary>
    public class RashFairy : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x001;
        public override int id { get; set; } = ID;
        public override int cost { get; } = 1;
        public override int attack { get; } = 1;
        public override int life { get; } = 1;
        public override IEffect[] effects { get; } = new Effect[0];
    }
}