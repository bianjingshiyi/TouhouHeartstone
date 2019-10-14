using TouhouHeartstone.Backend;
using TouhouHeartstone.Frontend.View.Animation;

using TouhouCardEngine;

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
                card.RecvAction(new CardAnimationEventArgs("OnDamage", new IntEventArgs(-wit.amounts[i])));
                card.CardSpec.HP -= wit.amounts[i];
            }
            // 这个事件不block
            return false;
        }
    }

    public class OnDeath : WitnessHandler
    {
        public override string Name => "onDeath";

        protected override bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var wit = Utilities.CheckType<DeathWitness>(witness);
            for (int i = 0; i < wit.cardsRID.Length; i++)
            {
                var card = deck.GetCardByRID(wit.cardsRID[i]);
                card.RecvAction(new CardAnimationEventArgs("OnDeath"));
            }
            return false; // 不block
        }
    }
}
