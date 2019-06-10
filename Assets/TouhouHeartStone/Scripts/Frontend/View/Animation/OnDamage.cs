using System;
using UnityEngine;

namespace TouhouHeartstone.Frontend.View.Animation
{
    public class OnDamage : CardAnimation
    {
        public override string AnimationName => "OnDamage";

        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            var arg = Utilities.CheckType<IntEventArgs>(args);
            var prefab = Card.GetComponentInParent<GlobalView>().damagePrefab;
            var instance = GameObject.Instantiate(prefab, Card.transform);
            instance.gameObject.SetActive(true);
            instance.Show(arg.Value);

            callback?.Invoke(this, null);
        }
    }
}
