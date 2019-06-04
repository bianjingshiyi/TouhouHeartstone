using System;

namespace TouhouHeartstone.Frontend.View.Animation
{
    /// <summary>
    /// 一堆动画序列
    /// </summary>
    public abstract class CardSerialAnimation : CardAnimation
    {
        CardAnimationEventArgs[] aniArgs = null;

        [Obsolete("oh fuck.")]
        CardView cardView => Card.GetComponent<CardView>();
        int index = 0;

        GenericAction parentCallback;
        object parentSender;
        AnimationPlayerBase player;

        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            index = 0;
            parentSender = sender;
            if (parentSender is AnimationPlayerBase)
            {
                player = sender as AnimationPlayerBase;
            }
            else if (sender is UnityEngine.MonoBehaviour)
            {
                player = (sender as UnityEngine.MonoBehaviour).GetComponent<AnimationPlayerBase>();
            }
            parentCallback = callback;

            aniArgs = createEventArgInner(sender, args);
            playNext(sender, args);
        }

        protected abstract CardAnimationEventArgs[] createEventArgInner(object sender, EventArgs args);

        void playNext(object sender, EventArgs args)
        {
            if (index >= aniArgs.Length)
            {
                parentCallback?.Invoke(this, new EventArgs());
            }
            else
            {
                player.PlayAnimation(parentSender, aniArgs[index++], playNext);
            }
        }
    }
}
