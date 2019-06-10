using System;
using TouhouHeartstone.Frontend.ViewModel;
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

    public class OnDeath : CardAnimation
    {
        public override string AnimationName => "OnDeath";

        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            var vm = Card.GetComponentInParent<CardViewModel>();
            GameObject.Destroy(Card);
            if (vm != null)
                GameObject.Destroy(vm.gameObject);
            callback?.Invoke(this, null);
        }
    }
}
