using TouhouHeartstone.Backend;
namespace TouhouHeartstone.Frontend.Model.Witness
{
    class OnAttackHandler : WitnessHandler
    {
        public override string Name => "onAttack";

        protected override bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            AttackWitness attackWitness = Utilities.CheckType<AttackWitness>(witness);
            var arg = new OnAttackEventArgs() { CardRID = attackWitness.cardRID, TargetRID = attackWitness.targetCardRID, PlayerID = attackWitness.playerIndex };
            deck.RecvEvent(arg, callback);

            return true;
        }
    }
}
