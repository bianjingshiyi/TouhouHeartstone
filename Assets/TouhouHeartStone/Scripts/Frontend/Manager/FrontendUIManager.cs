using UnityEngine;


namespace TouhouHeartstone.Frontend.Manager
{
    public class FrontendUIManager : FrontendSubManager
    {
        [SerializeField]
        UIRoundEndButton roundEndButton;

        [SerializeField]
        UIPopup gameSuccessPopup;

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
