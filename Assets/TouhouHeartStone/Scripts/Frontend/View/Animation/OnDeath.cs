using System;
using TouhouHeartstone.Frontend.ViewModel;
using UnityEngine;
using UnityEngine.UI;

namespace TouhouHeartstone.Frontend.View.Animation
{
    public class OnDeath : CardAnimationComponent
    {
        public override string AnimationName => "OnDeath";

        const float duration = 0.5f;
        readonly Color targetColor = new Color(0, 0, 0, 0);
        float t = 0;
        Image[] Images = null;
        GenericAction callback = null;

        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            var vm = Card.GetComponentInParent<CardViewModel>();
            Images = Card.GetComponentsInChildren<Image>();
            this.callback = callback;
        }

        private void Update()
        {
            if (Images != null)
            {
                foreach (var img in Images)
                {
                    img.color = Color.Lerp(img.color, targetColor, t / duration);
                }
                t += Time.deltaTime;
                if (t >= duration)
                {
                    Images = null;
                    Destroy(Card.GetComponentInParent<CardViewModel>()?.gameObject);
                    callback?.Invoke(this, null);
                }
            }
        }
    }
}
