using TouhouCardEngine;
using System;
namespace TouhouHeartstone
{
    public abstract class SpellCardDefine : THHCardDefine
    {
        public override string type { get; set; } = DEFINE.SPELL;
        public override string isUsable(CardEngine engine, TouhouCardEngine.Player player, Card card)
        {
            return null;
        }
    }
}