using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public override IEnumerable<TableAnimation> getGroupedAnimations(IEnumerable<TableAnimation> following)
        {
            return following.OfType<GameTableAnim>().Where(a => a.define == define && a.eventArg.parent == eventArg.parent && a.eventArg.GetType() == eventArg.GetType());
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            if (!effect)
                return true;
            var blockTime = effect.blockTime;
            return _duration > blockTime && blockTime >= 0;
        }
        private AnimationDefine define;
        private IEventArg eventArg;
        private Card source;
        private Card target;
        private TableEffect effect;
    }
}
