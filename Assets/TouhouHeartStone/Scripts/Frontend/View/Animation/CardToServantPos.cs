using System;
using IGensoukyo.Utilities;

namespace TouhouHeartstone.Frontend.View.Animation
{
    public class CardToServantPos : CardAnimation
    {
        public override string AnimationName => "CardToServant";

        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            var arg = Utilities.CheckType<CardPositionEventArgs>(args);
            var gv = Card.GetComponentInParent<GlobalView>();

            var pg = gv.CardPositionCalculator.GetRetinuePosition(arg.GroupID, arg.GroupCount, arg.SelfSide, true);

            Card.GetOrAddComponent<PositionAnimation>().Play(new PositionWithRotation[] {
                Card.transform.GetLocalPWR(),
                Card.transform.GlobalToLocal(pg)}, callback);
        }
    }
}
