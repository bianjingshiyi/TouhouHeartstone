namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnMaxGemChange : WitnessHandler
    {
        public override string Name => "onMaxGemChange";

        public override bool HandleWitness(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var maxGem = witness.getVar<int>("value");
            var player = witness.getVar<int>("playerIndex");

            deck.SetMaxGem(player, maxGem);
            callback?.Invoke(this, null);
            return false;
        }
    }
}
