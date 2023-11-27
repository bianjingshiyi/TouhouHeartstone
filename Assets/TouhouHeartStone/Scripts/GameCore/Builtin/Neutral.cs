using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone.Builtin
{
    public class LuckyCoin : SpellCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SPELL | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override bool isToken { get; set; } = true;

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect((game, card) => { return card.getOwner().setGem(game, card.getOwner().getGem() + 1); })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
}