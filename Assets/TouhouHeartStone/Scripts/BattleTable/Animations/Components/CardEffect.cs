using UnityEngine;

namespace Game.Animations
{
    public class CardEffect : TableEffect
    {
        public override void update(float deltaTime)
        {
            base.update(deltaTime);
            if (sourceTransform)
            {
                var sourcePos = sourceTransform.position;
                transform.position = sourcePos;
            }
        }
    }
}
