using System.Collections.Generic;
using Game.Animations;
using TouhouCardEngine;

namespace Game
{
    partial class GameManager
    {
        public AnimationDefine getAnimationDefine(long cardPoolId, int id)
        {
            if (animDict.TryGetValue(cardPoolId, out var anims))
            {
                if (anims.TryGetValue(id, out AnimationDefine animationDef))
                    return animationDef;
            }
            return null;
        }
        public AnimationDefine getAnimationDefine(DefineReference animRef)
        {
            if (animRef == null)
                return null;
            return getAnimationDefine(animRef.cardPoolId, animRef.defineId);
        }
        private Dictionary<long, Dictionary<int, AnimationDefine>> animDict = new Dictionary<long, Dictionary<int, AnimationDefine>>();
    }
}
