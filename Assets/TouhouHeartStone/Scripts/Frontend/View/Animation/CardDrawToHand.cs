using System;
using UnityEngine;

namespace TouhouHeartstone.Frontend.View.Animation
{
    /// <summary>
    /// 卡到手牌
    /// </summary>
    public class CardDrawToHand : CardAnimationComponent
    {
        public override string AnimationName => "CardToHand";

        GenericAction animateFinishCallback;

        PositionAnimation ani = null;

        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            var arg = Utilities.CheckType<CardPositionEventArgs>(args);
            animateFinishCallback = callback;

            var gv = GetComponentInParent<GlobalView>();
            var t = gv.CardPositionCalculator.GetCardHand(arg.GroupID, arg.GroupCount);

            ani = new PositionAnimation(Time.time, transform)
            {
                Positions = new Vector3[2]{
                     transform.position,
                     t.Position
                },
                Rotations = new Vector3[2]
                {
                    transform.rotation.eulerAngles,
                    t.Rotation
                }
            };
        }

        private void Update()
        {
            if (ani != null)
            {
                if (ani.Update(Time.time))
                {
                    ani = null;
                    animateFinishCallback?.Invoke(this, new EventArgs());
                    Destroy(this);
                }
            }
        }
    }
}
