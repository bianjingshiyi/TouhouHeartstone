using System.Threading;

using TouhouCardEngine;

namespace TouhouHeartstone.Backend.Builtin
{
    /// <summary>
    /// 关于ID格式，目前是8位十六进制，前3位为人物ID，中间2位是卡牌种类，后3位是卡片自己的ID。
    /// </summary>
    class CardCategory
    {
        public const int MASTER = 0x00000000;
        public const int SKILL = 0x00001000;
        public const int SERVANT = 0x00002000;
        public const int SPELL = 0x00003000;
    }
    public class FairyTwins : ServantCardDefine
    {
        public FairyTwins(IGameEnvironment env)
        {
            effects = new Effect[]
            {
                new GeneratedEffect("Field","onUse",
                "return true",
                "engine.createToken(player, card.define, player[\"Field\"].indexOf(card) + 1);")
            };
        }
        public override int id { get; set; } = 2;
        public override int cost
        {
            get { return 1; }
        }
        public override int attack
        {
            get { return 1; }
        }
        public override int life
        {
            get { return 1; }
        }
        public override Effect[] effects { get; }
    }
}