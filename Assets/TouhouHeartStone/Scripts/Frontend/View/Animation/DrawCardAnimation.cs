using System;

namespace TouhouHeartstone.Frontend.View.Animation
{
    /// <summary>
    /// 通常抽卡
    /// </summary>
    public class DrawCardAnimation : CardSerialAnimation
    {
        public override string AnimationName => "DrawCard";

        protected override CardAnimationEventArgs[] createEventArgInner(object sender, EventArgs args)
        {
            return new CardAnimationEventArgs[2] {
                new CardAnimationEventArgs(){ AnimationName = "DrawToCenter", EventArgs = new CardPositionEventArgs(1, 0) },
                new CardAnimationEventArgs(){ AnimationName = "CardToHand", EventArgs = args }
            };
        }
    }
}
