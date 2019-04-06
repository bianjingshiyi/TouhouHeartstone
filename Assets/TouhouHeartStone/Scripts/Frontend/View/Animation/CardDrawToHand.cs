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

            ani = new PositionAnimation(Time.time, transform)
            {
                Positions = new Vector3[2]{
                     transform.position,
                     new Vector3(960, 0,0)
                },
                Rotations = new Vector3[2]
                {
                    transform.rotation.eulerAngles,
                    new Vector3(0,0,0)
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
