namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnInit : WitnessHandler
    {
        public override string Name => "onInit";

        public override bool HandleWitness(EventWitness witness, DeckController deck)
        {
            int[] charactersID = witness.getVar<int[]>("players_master_define_id");
            int[] playerOrder = witness.getVar<int[]>("sortedPlayers_id");
            int[] initHandCard = witness.getVar<int[]>("self_init_define_id");

            deck.SetPlayer(playerOrder, charactersID);
            deck.SetInitHandcard(initHandCard);

            return false;
        }
    }
}
