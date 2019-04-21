namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnTurnStart : WitnessHandler
    {
        public override string Name => "onTurnStart";

        public override bool HandleWitness(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var player = witness.getVar<int>("playerIndex");
            var maxGem = witness.getVar<int>("maxGem");
            var gem = witness.getVar<int>("gem");

            deck.TurnStart(player, maxGem, gem);
            callback?.Invoke(this, null);

            return false;
        }
    }

    public class OnTurnEnd : WitnessHandler
    {
        public override string Name => "onTurnEnd";

        public override bool HandleWitness(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var player = witness.getVar<int>("playerIndex");

            deck.TurnEnd(player);
            callback?.Invoke(this, null);

            return false;
        }
    }
}
