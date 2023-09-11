using UnityEngine;

namespace Game.Animations
{
    public class CardEffect : TableEffect
    {
        protected override void updateAnim(float deltaTime)
        {
            if (sourceTransform)
            {
                var sourcePos = sourceTransform.position;
                transform.position = sourcePos;
            }
        }
    }
}
