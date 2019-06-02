namespace TouhouHeartstone.Backend.Builtin
{
    public class FairyTwins : ServantCardDefine
    {
        public override int id
        {
            get { return 2; }
        }
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
        public override int category
        {
            get { return 2; }
        }
        public override Effect[] getEffects()
        {
            return new Effect[] { new OnUseEffect() };
        }
        class OnUseEffect : Effect
        {
            public override string pile
            {
                get { return "Field"; }
            }
            public override string trigger
            {
                get { return "onUse"; }
            }
            public override void execute(CardEngine engine, Player player, Card card, Card[] targetCards)
            {
                engine.createToken(player, card.define, player["Field"].indexOf(card) + 1);
            }
        }
    }
}