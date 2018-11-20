using TouhouHeartstone.Frontend.Manager;

namespace TouhouHeartstone.Frontend.WitnessHandler
{
    public class WitnessHandlerDraw : WitnessHandlerBase<DrawWitness>
    {
        public override bool HasAnimation => true;

        public override void Exec(DrawWitness witness)
        {
            if (witness.playerId == frontend.PlayerID)
            {
                frontend.GetSubManager<FrontendCardManager>().NormalDrawCard(witness.cards);
            }
            else
            {
                DebugUtils.LogNoImpl($"非本地玩家{witness.playerId}抽卡{witness.cards}");
            }
        }
    }
}
