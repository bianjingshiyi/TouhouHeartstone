using System;
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

            var posA = gv.CardPositionCalculator.GetPOV(arg.SelfServant);
            var posB = gv.CardPositionCalculator.GetPOV(arg.TargetServant, true);

            var testPa = Card.transform.GlobalToLocal(gv.CardPositionCalculator.GetPOV(arg.SelfServant, true));
            DebugUtils.Log((posA.Position - testPa.Position).ToString());

            posB = Card.transform.GlobalToLocal(posB);
            var current = Card.transform.GetLocalPWR();

            Card.GetOrAddComponent<PositionAnimation>().Play(new PositionWithRotation[] { current, posB, posA }, callback);
        }
    }
}
