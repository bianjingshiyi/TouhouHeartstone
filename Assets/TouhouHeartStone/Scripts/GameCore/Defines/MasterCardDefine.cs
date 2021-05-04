using System;
using TouhouCardEngine;

namespace TouhouHeartstone
{
    [Obsolete("请勿直接使用内置属性")]
    public abstract class MasterCardDefine : THHCardDefine
    {
        public override string type { get; set; } = DEFINE.MASTER;
        public abstract int skillID { get; set; }
        public override string isUsable(CardEngine engine, TouhouCardEngine.Player player, Card card)
        {
            return null;
        }
    }
}