using System;

namespace TouhouHeartstone.Frontend.View.Animation
{
    /// <summary>
    /// 初始抽卡
    /// </summary>
    public class InitDrawCard : CardSerialAnimation
    {
        public override string AnimationName => "InitDrawCard";

        protected override CardAnimationEventArgs[] createEventArgInner(object sender, EventArgs args)
        {
            return new CardAnimationEventArgs[] {
                 new CardAnimationEventArgs(){ AnimationName = "DrawToCenter", EventArgs = args },
                 new CardAnimationEventArgs(){ AnimationName = "CardToHand", EventArgs = args }
            };
        }
    }
}
