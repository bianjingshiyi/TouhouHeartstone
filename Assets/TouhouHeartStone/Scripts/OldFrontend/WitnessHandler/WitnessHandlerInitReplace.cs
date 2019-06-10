using TouhouHeartstone.OldFrontend.Manager;
using IGensoukyo.Utilities;

namespace TouhouHeartstone.OldFrontend.WitnessHandler
{
    [System.Obsolete]
    public class WitnessHandlerInitReplace : WitnessHandlerBase<OldInitReplaceWitness>
    {
        public override bool HasAnimation => true;

        public override void Exec(OldInitReplaceWitness witness)
        {
            if (witness.playerId == frontend.PlayerID)
            {
                var fcm = frontend.GetSubManager<FrontendCardManager>();

                fcm.OnDrawCardFinish += DrawCardFinishCallback;
                fcm.NormalDrawCard(witness.replaceCards);
            }
            else
            {
                DebugUtils.LogNoImpl($"非本地玩家{witness.playerId}替换卡{witness.replaceCards}");
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
