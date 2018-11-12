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
    }
}
