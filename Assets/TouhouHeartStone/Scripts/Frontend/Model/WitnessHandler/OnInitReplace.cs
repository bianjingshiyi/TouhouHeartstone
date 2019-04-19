namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnInitReplace : WitnessHandler
    {
        public override string Name => "onInitReplace";

        public override bool HandleWitness(EventWitness witness, DeckController deck, GenericAction callback)
        {
            int player = witness.getVar<int>("player_index");
            int[] cards = witness.getVar<int[]>("replaced_define_id");
            int cardCount = witness.getVar<int>("replaced_count");

            if (cards.Length == 0 && cardCount > 0)
                cards = new int[cardCount];

            deck.SetInitReplace(player, cards, callback);

            return false;
        }
    }
}
