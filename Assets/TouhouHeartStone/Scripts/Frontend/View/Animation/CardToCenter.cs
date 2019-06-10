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

            // todo: 需要做坐标转换
            Vector3 loWorld = Card.transform.parent.localToWorldMatrix * Card.transform.localPosition;
            Vector3 offset = loWorld - Card.transform.position;
            Vector3 newWo = t.Position + offset;
            Vector3 newLo = Card.transform.parent.worldToLocalMatrix * newWo;

            DebugUtils.Debug($"CalcPos: {t.Position}\nCurrentLo: {Card.transform.localPosition}\n CurrentWo: {loWorld}\nWo: {Card.transform.position}\n" +
                $"Offset:{offset}\nCalcWo:{newWo}\nCalclo:{newLo}");

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
