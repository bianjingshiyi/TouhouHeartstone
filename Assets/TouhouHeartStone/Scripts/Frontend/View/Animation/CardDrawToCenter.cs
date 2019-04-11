using System;
using UnityEngine;

namespace TouhouHeartstone.Frontend.View.Animation
{
    public class CardDrawToCenter : CardAnimationComponent
    {
        public override string AnimationName => "DrawToCenter";

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
                     new Vector3(1920, 0, 0),
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
                    animateFinishCallback?.Invoke(this, new EventArgs());
                    Destroy(this);
                }
            }
        }
    }
}
