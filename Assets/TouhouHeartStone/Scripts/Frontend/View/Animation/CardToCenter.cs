using System;
using UnityEngine;

namespace TouhouHeartstone.Frontend.View.Animation
{
    public class CardToCenter : CardAnimationComponent
    {
        public override string AnimationName => "CardToCenter";

        GenericAction animateFinishCallback;

        PositionAnimation ani = null;

        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            var arg = Utilities.CheckType<CardPositionEventArgs>(args);
            animateFinishCallback = callback;

            var gv = GetComponentInParent<GlobalView>();
            var t = gv.CardPositionCalculator.GetCardCenter(arg.GroupID, arg.GroupCount);

            ani = new PositionAnimation(Time.time, transform)
            {
                Positions = new Vector3[2]{
                     transform.position,
                     t.Position
                }
            };
            gameObject.SetActive(true);
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
