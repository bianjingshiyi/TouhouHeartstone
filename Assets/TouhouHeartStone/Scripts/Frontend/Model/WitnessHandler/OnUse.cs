namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnUse : WitnessHandler
    {
        public override string Name => "onUse";

        public override bool HandleWitness(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            int playerIndex = witness.getVar<int>("playerIndex");
            int cardRID = witness.getVar<int>("cardRID");
            int cardDID = witness.getVar<int>("cardDID");
            int targetPosition = witness.getVar<int>("targetPosition");
            int targetCardRID = witness.getVar<int>("targetCardRID");

            UseCardEventArgs args;
            if (targetPosition == -1)
            {
                if (targetCardRID == 0)
                {
                    args = new UseCardEventArgs();
                }
                else
                {
                    args = new UseCardWithTargetArgs(targetCardRID);
                }
            }
            else
            {
                if (targetCardRID == 0)
                {
                    args = new UseCardWithPositionArgs(targetPosition);
                }
                else
                {
                    args = new UseCardWithTargetPositionArgs(targetPosition, targetCardRID);
                }
            }

            deck.OnCardUse(playerIndex, new CardID(cardDID, cardRID), args, callback);
            return false;
        }
    }

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

            // todo: 增加随从
            callback?.Invoke(this, null);

            return false;
        }
    }
}
