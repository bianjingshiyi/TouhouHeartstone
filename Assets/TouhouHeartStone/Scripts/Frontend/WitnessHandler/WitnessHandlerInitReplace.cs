using TouhouHeartstone.Frontend.Manager;

namespace TouhouHeartstone.Frontend.WitnessHandler
{
    public class WitnessHandlerInitReplace : WitnessHandlerBase<InitReplaceWitness>
    {
        public override bool HasAnimation => true;

        public override void Exec(InitReplaceWitness witness)
        {
            if (witness.playerId == frontend.PlayerID)
            {
                frontend.GetSubManager<FrontendCardManager>().NormalDrawCard(witness.replaceCards);
            }
            else
            {
                DebugUtils.LogNoImpl($"非本地玩家{witness.playerId}替换卡{witness.replaceCards}");
            }
        }
    }
}
