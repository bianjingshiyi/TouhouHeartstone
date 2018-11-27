namespace TouhouHeartstone.Backend
{
    public class GameClient : GameContainer
    {
        protected override void onInitReplace(int[] cards)
        {
            network.sendObject(network.hostId, new InitReplaceRequest(cards));
        }
        protected override void onUse(int instance, int position, int target)
        {
            network.sendObject(network.hostId, new UseRequest(instance, position, target));
        }
        protected override void onTurnEnd()
        {
            network.sendObject(network.hostId, new TurnEndRequest());
        }
        protected override void onReceiveObject(int senderId, object obj)
        {
            if (obj is IWitness)
            {
                witness.add(obj as IWitness);
                if (witness.hungupCount > 0)
                {
                    int min, max;
                    witness.getMissingRange(out min, out max);
                    network.sendObject(senderId, new GetMissingWitnessRequest(min, max));
                }
            }
        }
    }
}