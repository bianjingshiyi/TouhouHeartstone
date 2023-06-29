using System.Collections;
using Game.BattleTable;
using TouhouCardEngine;
using UnityEngine;

namespace Game.Animations
{
    public class GameTableAnim : TableAnimation
    {
        public GameTableAnim(AnimationDefine define, Card source, Card target)
        {
            this.define = define;
            this.source = source;
            this.target = target;
        }
        public override IEnumerator update(BattleTableManager table)
        {
            if (!effect)
            {
                Transform srcTransform = table.getCardTransform(source);
                Transform targetTransform = null;
                if (target != null)
                {
                    targetTransform = table.getCardTransform(target);
                }
                effect = table.createEffect(define, srcTransform, targetTransform);
            }
            while (!effect.isFinished())
            {
                effect.update(_deltaTime);
                yield return null;
            }
            finish();
            table.removeEffect(effect);
            effect = null;
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (!effect)
                return false;
            var blockTime = effect.blockTime;
            return _duration <= blockTime || blockTime < 0;
        }
        private AnimationDefine define;
        private Card source;
        private Card target;
        private TableEffect effect;
    }
}
