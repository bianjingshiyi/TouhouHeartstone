using TouhouCardEngine;
using System;
namespace TouhouHeartstone
{
    [Obsolete("请勿直接使用内置属性")]
    public abstract class SpellCardDefine : THHCardDefine
    {
        public override string type { get; set; } = DEFINE.SPELL;
        public override string isUsable(CardEngine engine, TouhouCardEngine.Player player, Card card)
        {
            return null;
        }
    }
}