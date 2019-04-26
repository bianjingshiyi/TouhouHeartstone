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

            arg.GroupID += arg.GroupOffset;
            arg.GroupCount += arg.GroupOffset;

            var gv = GetComponentInParent<GlobalView>();
            var t = gv.CardPositionCalculator.GetCardHand(arg.GroupID, arg.GroupCount);

            transform.SetSiblingIndex(arg.GroupID);

            ani = new PositionAnimation(Time.time, transform, new Vector3[2] {
                    transform.localPosition,
                    t.Position
                }, new Vector3[2]
                {
                    transform.localRotation.eulerAngles,
                    t.Rotation
                });
        }

        private void Update()
        {
            if (ani != null)
            {
                if (ani.Update(Time.time))
                {
                    ani = null;
                    animateFinishCallback?.Invoke(this, new EventArgs());
                    // Destroy(this);
                }
            }
        }
    }
}
