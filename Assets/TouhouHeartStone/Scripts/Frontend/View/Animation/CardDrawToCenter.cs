using System;
using UnityEngine;

using IGensoukyo.Utilities;

namespace TouhouHeartstone.Frontend.View.Animation
{
    public class CardDrawToCenter : CardAnimation
    {
        public override string AnimationName => "DrawToCenter";

        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            var arg = Utilities.CheckType<CardPositionEventArgs>(args);

            var gv = Card.GetComponentInParent<GlobalView>();
            var t = gv.CardPositionCalculator.GetThrowCardPosition(arg.GroupID, arg.GroupCount);
            // todo: 需要做坐标转换

            Card.GetOrAddComponent<PositionAnimation>().Play(new Vector3[2]{
                     gv.CardPositionCalculator.StackPosition,
                     Card.transform.worldToLocalMatrix * t.Position
                },
                new Vector3[2]
                {
                    Card.transform.localRotation.eulerAngles,
                    Vector3.zero
                }, callback);
            Card.SetActive(true);
        }

    }
}
