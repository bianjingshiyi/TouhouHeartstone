namespace TouhouHeartstone.OldFrontend.WitnessHandler
{
    [System.Obsolete]
    public class WitnessHandlerSetOrder : WitnessHandlerBase<SetOrderWitness>
    {
        public override bool HasAnimation => false;

        public override void Exec(SetOrderWitness witness)
        {
            frontend.PlayerOrder = witness.orderedPlayerId;
        }
    }
}
