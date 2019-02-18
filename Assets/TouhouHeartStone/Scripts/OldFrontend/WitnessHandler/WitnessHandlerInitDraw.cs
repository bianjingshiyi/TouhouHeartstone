using TouhouHeartstone.OldFrontend.Manager;

namespace TouhouHeartstone.OldFrontend.WitnessHandler
{
    public class WitnessHandlerInitDraw : WitnessHandlerBase<InitDrawWitness>
    {
        public override bool HasAnimation => true;

        public override void Exec(InitDrawWitness witness)
        {
            if (witness.playerId == frontend.PlayerID)
            {
                frontend.GetSubManager<FrontendCardManager>().InitDrawCard(witness.cards);
                frontend.GetSubManager<FrontendWitnessEventDispatcher>().ReplaceInitDrawAction += onFinishCallback;
            }
            else
            {
                DebugUtils.LogNoImpl($"非本地玩家{witness.playerId}抽卡{witness.cards}");
                frontend.GetSubManager<FrontendWitnessEventDispatcher>().OnWitnessFinish();
            }
        }

        void onFinishCallback(int[] arg)
        {
            var dispatcher = frontend.GetSubManager<FrontendWitnessEventDispatcher>();
            dispatcher.ReplaceInitDrawAction -= onFinishCallback;

            dispatcher.OnWitnessFinish();
        }
    }
}
