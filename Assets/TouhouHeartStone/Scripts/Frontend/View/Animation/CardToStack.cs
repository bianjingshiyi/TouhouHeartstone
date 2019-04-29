using System;
using UnityEngine;

namespace TouhouHeartstone.Frontend.View.Animation
{
    /// <summary>
    /// 卡到卡堆
    /// </summary>
    public class CardToStack : CardAnimationComponent
    {
        public override string AnimationName => "CardToStack";

        GenericAction animateFinishCallback;

        PositionAnimation ani = null;

        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            var arg = Utilities.CheckType<CardPositionEventArgs>(args);
            animateFinishCallback = callback;

            var gv = GetComponentInParent<GlobalView>();

            transform.SetSiblingIndex(arg.GroupID);

            ani = new PositionAnimation(Time.time, transform, new Vector3[2] {
                    transform.localPosition,
                    gv.CardPositionCalculator.StackPosition
                }, new Vector3[2]
                {
                    transform.localRotation.eulerAngles,
                    Vector3.zero
                });
        }

        private void Update()
        {
            if (ani != null)
            {
                if (ani.Update(Time.time))
                {
                    ani = null;
                    animateFinishCallback?.Invoke(this.gameObject, new EventArgs());
                    // Destroy(this);
                }
            }
        }
    }
}
