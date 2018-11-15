using UnityEngine;


namespace TouhouHeartstone.Frontend.Manager
{
    public class FrontendUIManager : FrontendSubManager
    {
        [SerializeField]
        UIRoundEndButton roundEndButton;

        [SerializeField]
        UIPopup gameSuccessPopup;

        [SerializeField]
        TargetSelector selector;

        [SerializeField]
        UIStoneBar stoneBar;

        public UIStoneBar StoneBar => stoneBar;

        public TargetSelector TargetSelector => selector;

        private new void Awake()
        {
            base.Awake();
            roundEndButton.EndRound += getSiblingManager<FrontendWitnessEventDispatcher>().InvokeEndRoundEvent;
            roundEndButton.SetState(true);
        }

        void endRoundTest()
        {
            gameSuccessPopup.Show();
        }

        public void RoundStart()
        {
            roundEndButton.SetState(true);
        }
    }
}
