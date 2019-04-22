namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnTurnEnd : WitnessHandler
    {
        public override string Name => "onTurnEnd";

        public override bool HandleWitness(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var player = witness.getVar<int>("playerIndex");

            deck.onTurnEnd(player);
            callback?.Invoke(this, null);

            return false;
        }
    }
}
