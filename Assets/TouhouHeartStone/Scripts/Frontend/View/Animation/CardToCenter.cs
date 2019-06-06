using System;
using UnityEngine;
using IGensoukyo.Utilities;

namespace TouhouHeartstone.Frontend.View.Animation
{
    public class CardToCenter : CardAnimation
    {
        public override string AnimationName => "CardToCenter";

        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            var arg = Utilities.CheckType<CardPositionEventArgs>(args);

            var gv = Card.GetComponentInParent<GlobalView>();
            var t = gv.CardPositionCalculator.GetThrowCardPosition(arg.GroupID, arg.GroupCount);

            Vector3 newLo = Card.transform.parent.InverseTransformPoint(t.Position);

            Card.GetOrAddComponent<PositionAnimation>().Play(new Vector3[2]{
                     Card.transform.localPosition,
                     newLo
                }, new Vector3[2]
                {
                    Card.transform.localRotation.eulerAngles,
                    Vector3.zero
                }, callback);
            Card.SetActive(true);
        }
    }
}
