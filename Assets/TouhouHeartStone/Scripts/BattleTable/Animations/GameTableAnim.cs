using System.Collections;
using Game.BattleTable;
using TouhouCardEngine.Interfaces;
using UnityEngine;
using Card = TouhouCardEngine.Card;

namespace Game.Animations
{
    public class GameTableAnim : TableAnimation
    {
        public GameTableAnim(AnimationDefine define, IEventArg eventArg, Card source, Card target)
        {
            this.define = define;
            this.source = source;
            this.target = target;
            this.eventArg = eventArg;
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
                var info = new VisualEffectInfo()
                {
                    srcTransform = srcTransform,
                    destTransform = targetTransform,
                };
                effect = table.createEffect(define, info);
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
            if (nextAnim is PropChangeAnim)
                return false;
            IEventArg nextEventArg = null;
            if (nextAnim is GameTableAnim gameTableAnim)
                nextEventArg = gameTableAnim.eventArg;
            else if (nextAnim is EventAnimation eventAnimation)
                nextEventArg = eventAnimation.eventArg;
            if (nextEventArg != null)
            {
                if (eventArg.parent == nextEventArg.parent && eventArg.GetType() == nextEventArg.GetType())
                    return false;
            }
            var blockTime = effect.blockTime;
            return _duration <= blockTime || blockTime < 0;
        }
        private AnimationDefine define;
        private IEventArg eventArg;
        private Card source;
        private Card target;
        private TableEffect effect;
    }
}
