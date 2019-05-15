namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnSummon : WitnessHandler
    {
        public override string Name => "onSummon";

        public override bool HandleWitness(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            int playerIndex = witness.getVar<int>("playerIndex");
            int cardRID = witness.getVar<int>("cardRID");
            int cardDID = witness.getVar<int>("cardDID");

            // 随从位置
            int position = witness.getVar<int>("position");

            var arg = new RetinueSummonEventArgs()
            {
                CardDID = cardDID,
                CardRID = cardRID,
                PlayerID = playerIndex,
                Position = position
            };
            deck.RecvEvent(arg, callback);

            return false;
        }
    }
}
