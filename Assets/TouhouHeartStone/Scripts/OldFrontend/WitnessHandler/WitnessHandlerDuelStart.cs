using IGensoukyo.Utilities;
namespace TouhouHeartstone.OldFrontend.WitnessHandler
{
    [System.Obsolete]
    public class WitnessHandlerDuelStart : WitnessHandlerBase<DuelStartWitness>
    {
        public override bool HasAnimation => false;

        public override void Exec(DuelStartWitness witness)
        {
            DebugUtils.Debug("对局开始了.");
        }
    }
}
