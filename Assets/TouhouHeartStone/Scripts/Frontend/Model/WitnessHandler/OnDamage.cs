using TouhouHeartstone.Backend;

namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnDamage : WitnessHandler
    {
        public override string Name => "onDamage";

        protected override bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var wit = Utilities.CheckType<DamageWitness>(witness);
            for (int i = 0; i < wit.cardsRID.Length; i++)
            {
                var card = deck.GetCardByRID(wit.cardsRID[i]);
                card.RecvAction(new OnDamageEventArgs(wit.amounts[i]));
            }
            // 这个事件不block
            return false;
        }
    }
}
