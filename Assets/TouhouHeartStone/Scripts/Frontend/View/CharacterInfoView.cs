using System;

using TouhouHeartstone.Frontend.ViewModel;

namespace TouhouHeartstone.Frontend.View
{
    public class CharacterInfoView : AnimationPlayerBase
    {
        public CharacterInfoViewModel CharacterVM { get; private set; } = null;
        private void Awake()
        {
            CharacterVM = GetComponentInParent<CharacterInfoViewModel>();
            CharacterVM.OnRecvActionEvent += onRecvAction;
        }

        private void onRecvAction(object sender, EventArgs args, GenericAction callback = null)
        {
            if (args is CardAnimationEventArgs)
            {
                PlayAnimation(this, args, callback);
            }
        }
    }
}