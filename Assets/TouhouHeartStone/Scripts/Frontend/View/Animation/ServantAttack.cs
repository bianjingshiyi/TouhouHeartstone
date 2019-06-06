using System;
using UnityEngine;
using IGensoukyo.Utilities;

namespace TouhouHeartstone.Frontend.View.Animation
{
    public class ServantAttack : CardAnimation
    {
        public override string AnimationName => "ServantAttack";
        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            var arg = Utilities.CheckType<ServantAttackEventArgs>(args);
            var gv = Card.GetComponentInParent<GlobalView>();

            var posA = gv.CardPositionCalculator.GetRetinuePosition(arg.SelfServant.GroupID, arg.SelfServant.GroupCount, arg.SelfServant.SelfSide);
            var posB = gv.CardPositionCalculator.GetRetinuePosition(arg.TargetServant.GroupID, arg.TargetServant.GroupCount, arg.SelfServant.SelfSide, true);

            posB.Position = Card.transform.parent.InverseTransformPoint(posB.Position);
            var current = new PositionWithRotation() { Position = Card.transform.localPosition, Rotation = Card.transform.localRotation.eulerAngles };

            Card.GetOrAddComponent<PositionAnimation>().Play(new PositionWithRotation[] { current, posA, posB }, callback);
        }
    }
}
