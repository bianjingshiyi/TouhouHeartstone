using System;

namespace TouhouHeartstone.Frontend.View.Animation
{
    public class CardDisappear : CardAnimationComponent
    {
        public override string AnimationName => "Disappear";

        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            Card.gameObject.SetActive(false);
            callback?.Invoke(gameObject, new EventArgs());
        }
    }
}
