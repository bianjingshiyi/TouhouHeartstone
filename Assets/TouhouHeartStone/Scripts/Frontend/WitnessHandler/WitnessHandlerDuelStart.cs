namespace TouhouHeartstone.Frontend.WitnessHandler
{
    public class WitnessHandlerDuelStart : WitnessHandlerBase<DuelStartWitness>
    {
        public override bool HasAnimation => false;

        public override void Exec(DuelStartWitness witness)
        {
            DebugUtils.LogDebug("对局开始了.");
        }
    }
}
