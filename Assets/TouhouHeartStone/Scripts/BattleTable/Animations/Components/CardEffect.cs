using UnityEngine;

namespace Game.Animations
{
    public class CardEffect : TableEffect
    {
        protected override void updateAnim(float deltaTime)
        {
            if (sourceTransform)
            {
                transform.position = sourceTransform.position;
                if (_useSourceScale)
                {
                    transform.localScale = sourceTransform.lossyScale;
                }
            }
        }
        [Header("Card Effect")]
        [SerializeField]
        private bool _useSourceScale = false;
    }
}
