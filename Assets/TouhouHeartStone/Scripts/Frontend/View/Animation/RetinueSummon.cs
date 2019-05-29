using System;
using UnityEngine;
using IGensoukyo.Utilities;

namespace TouhouHeartstone.Frontend.View.Animation
{
    public class RetinueSummon : CardAnimation
    {
        public override string AnimationName => "RetinueSummon";

        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            var arg = Utilities.CheckType<CardPositionEventArgs>(args);
            var gv = Card.GetComponentInParent<GlobalView>();

            Card.gameObject.SetActive(true);
            var pos = gv.CardPositionCalculator.GetRetinuePosition(arg.GroupID, arg.GroupCount);
            Card.GetOrAddComponent<PositionAnimation>().Play(new Vector3[2]
            {
                pos.Position,
                pos.Position
            }, new Vector3[2] {
                pos.Rotation,
                pos.Rotation
            }, callback);
        }
    }

    public class RetinueMove : CardAnimation
    {
        public override string AnimationName => "RetinueMove";

        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            var arg = Utilities.CheckType<CardPositionEventArgs>(args);
            var gv = Card.GetComponentInParent<GlobalView>();

            var pos = gv.CardPositionCalculator.GetRetinuePosition(arg.GroupID, arg.GroupCount);
            Card.GetOrAddComponent<PositionAnimation>().Play(new Vector3[2]
            {
                Card.transform.localPosition,
                pos.Position
            }, new Vector3[2] {
                Card.transform.localRotation.eulerAngles,
                pos.Rotation
            }, callback);
        }
    }

    public class ServantAttack : CardAnimation
    {
        public override string AnimationName => "ServantAttack";
        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            var arg = Utilities.CheckType<ServantAttackEventArgs>(args);
            var gv = Card.GetComponentInParent<GlobalView>();

            var posA = gv.CardPositionCalculator.GetRetinuePosition(arg.SelfServant.GroupID, arg.SelfServant.GroupCount);
            var posB = gv.CardPositionCalculator.GetOppositeRetinuePosition(arg.TargetServant.GroupID, arg.TargetServant.GroupCount);
            Card.GetOrAddComponent<PositionAnimation>().Play(new Vector3[2]
            {
                Card.transform.localPosition,
                posB.Position
            }, new Vector3[2] {
                Card.transform.localRotation.eulerAngles,
                posB.Rotation
            }, (a,b)=> {
            Card.GetOrAddComponent<PositionAnimation>().Play(new Vector3[2]
            {
                Card.transform.localPosition,
                posA.Position
            }, new Vector3[2] {
                Card.transform.localRotation.eulerAngles,
                posA.Rotation
            },callback);
            });
        }
    }
}
