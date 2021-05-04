using TouhouCardEngine;
using System;
namespace TouhouHeartstone
{
    [Obsolete("请勿直接使用内置属性")]
    public abstract class ItemCardDefine : THHCardDefine
    {
        public override string type { get; set; } = DEFINE.ITEM;
        public override string isUsable(CardEngine engine, TouhouCardEngine.Player player, Card card)
        {
            throw new NotImplementedException();
        }
    }
}