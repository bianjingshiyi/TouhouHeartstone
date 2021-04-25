using TouhouCardEngine;
using System;
namespace TouhouHeartstone
{
    public abstract class ItemCardDefine : THHCardDefine
    {
        public override string type { get; set; } = DEFINE.ITEM;
        public override string isUsable(CardEngine engine, TouhouCardEngine.Player player, Card card)
        {
            throw new NotImplementedException();
        }
    }
}