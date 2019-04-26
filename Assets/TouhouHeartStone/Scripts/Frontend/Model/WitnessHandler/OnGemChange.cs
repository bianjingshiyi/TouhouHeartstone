namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnGemChange : WitnessHandler
    {
        public override string Name => "onGemChange";

        public override bool HandleWitness(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var maxGem = witness.getVar<int>("value");
            var player = witness.getVar<int>("playerIndex");

            deck.SetCurrentGem(player, maxGem);
            callback?.Invoke(this, null);
            return false;
        }
    }
}
