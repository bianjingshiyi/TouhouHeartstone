using TouhouHeartstone.OldFrontend.Manager;
using IGensoukyo.Utilities;

namespace TouhouHeartstone.OldFrontend.WitnessHandler
{
    public class WitnessHandlerDraw : WitnessHandlerBase<DrawWitness>
    {
        public override bool HasAnimation => true;

        public override void Exec(DrawWitness witness)
        {
            if (witness.playerId == frontend.PlayerID)
            {
                var fcm = frontend.GetSubManager<FrontendCardManager>();

                fcm.OnDrawCardFinish += DrawCardFinishCallback;
                fcm.NormalDrawCard(witness.cards);
            }
            else
            {
                DebugUtils.LogNoImpl($"非本地玩家{witness.playerId}抽卡{witness.cards}");
                frontend.GetSubManager<FrontendWitnessEventDispatcher>().OnWitnessFinish();
            }
        }

        void DrawCardFinishCallback()
        {
            var fcm = frontend.GetSubManager<FrontendCardManager>();
            fcm.OnDrawCardFinish -= DrawCardFinishCallback;

            frontend.GetSubManager<FrontendWitnessEventDispatcher>().OnWitnessFinish();
        }
    }
}
