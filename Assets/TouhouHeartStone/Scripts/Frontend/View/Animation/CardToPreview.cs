using System;
using UnityEngine;

using IGensoukyo.Utilities;

namespace TouhouHeartstone.Frontend.View.Animation
{
    /// <summary>
    /// 卡到手牌
    /// </summary>
    public class CardToPreview : CardAnimation
    {
        public override string AnimationName => "CardToPreview";

        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            var arg = Utilities.CheckType<CardPositionEventArgs>(args);

            var gv = Card.GetComponentInParent<GlobalView>();
            var t = gv.CardPositionCalculator.GetCardFlow(arg.GroupID, arg.GroupCount);

            Card.transform.SetAsLastSibling();

            Card.GetOrAddComponent<PositionAnimation>().Play(new Vector3[2] {
                    Card.transform.localPosition,
                    t.Position
                }, new Vector3[2]
                {
                    Card.transform.localRotation.eulerAngles,
                    t.Rotation
                }, callback);
        }
    }
}
