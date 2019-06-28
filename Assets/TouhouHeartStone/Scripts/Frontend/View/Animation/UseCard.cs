using System;
using TouhouHeartstone.Frontend.Model;
using System.Collections.Generic;
using TouhouHeartstone.Frontend.ViewModel;

namespace TouhouHeartstone.Frontend.View.Animation
{
    public class UseCard : CardSerialAnimation
    {
        public override string AnimationName => "UseCard";

        protected override CardAnimationEventArgs[] createEventArgInner(object sender, EventArgs args)
        {
            List<CardAnimationEventArgs> cal = new List<CardAnimationEventArgs>();
            var arg = Utilities.CheckType<UseCardEventArgs>(args);
            var cv = sender as CardView;

            var cvm = Card.GetComponentInParent<CardViewModel>();

            if (arg is UseCardWithPositionArgs)
            {
                var parg = arg as UseCardWithPositionArgs;
                cal.Add(new CardAnimationEventArgs("CardToServant", new CardPositionEventArgs(
                    cv.cardVM.Board.ServantCount + 1, // 当前卡还没刷出来，先加1再说
                    parg.Position, cvm.Board.IsSelf)));
            }
            cal.Add(new CardAnimationEventArgs("DestroyCardFace"));
            return cal.ToArray();
        }
    }
}
