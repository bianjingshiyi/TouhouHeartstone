using System;

namespace TouhouHeartstone.Frontend.View.Animation
{
    public class DestroyCardFace : CardAnimation
    {
        public override string AnimationName => "DestroyCardFace";

        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            var cv = Card.GetComponent<CardView>();
            cv.Destroy();
            cv.cardVM.Board.RemoveHandCard(cv.cardVM.RuntimeID);
            callback?.Invoke(this, null);
        }
    }
}
