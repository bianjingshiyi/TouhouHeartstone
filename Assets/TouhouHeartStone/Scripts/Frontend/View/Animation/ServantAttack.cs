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

            var posA = gv.CardPositionCalculator.GetRetinuePosition(arg.SelfServant.GroupID, arg.SelfServant.GroupCount);
            var posB = gv.CardPositionCalculator.GetOppositeRetinuePosition(arg.TargetServant.GroupID, arg.TargetServant.GroupCount);
            Card.GetOrAddComponent<PositionAnimation>().Play(new Vector3[]
            {
                Card.transform.localPosition,
                posB.Position,
                posA.Position
            }, new Vector3[] {
                Card.transform.localRotation.eulerAngles,
                posB.Rotation,
                posA.Rotation
            }, callback);
        }
    }
}
