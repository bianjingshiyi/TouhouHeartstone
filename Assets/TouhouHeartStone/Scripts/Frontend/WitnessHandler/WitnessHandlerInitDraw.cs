using TouhouHeartstone.Frontend.Manager;

namespace TouhouHeartstone.Frontend.WitnessHandler
{
    public class WitnessHandlerInitDraw : WitnessHandlerBase<InitDrawWitness>
    {
        public override bool HasAnimation => true;

        public override void Exec(InitDrawWitness witness)
        {
            if (witness.playerId == frontend.PlayerID)
            {
                frontend.GetSubManager<FrontendCardManager>().InitDrawCard(witness.cards);
            }
            else
            {
                DebugUtils.LogNoImpl($"非本地玩家{witness.playerId}抽卡{witness.cards}");
            }
        }
    }
}
