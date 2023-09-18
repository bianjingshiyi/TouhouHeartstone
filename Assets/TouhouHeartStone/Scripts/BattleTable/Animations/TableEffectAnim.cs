using System.Collections;
using Game.BattleTable;
using UnityEngine;
using Card = TouhouCardEngine.Card;

namespace Game.Animations
{
    public class TableEffectAnim : TableAnimation
    {
        public TableEffectAnim(AnimationDefine define, Card source, Card target)
        {
            this.define = define;
            this.source = source;
            this.target = target;
        }
        public override IEnumerator update(BattleTableManager table)
        {
            if (!effect)
            {
                Transform srcTransform = null;
                Transform targetTransform = null;
                if (source != null)
                {
                    srcTransform = table.getCardTransform(source);
                }
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
            while (!effect.isFinished() && !(!effect.isStarted && _duration >= 10f)) // 十秒没播放算超时
            {
                // 这里不用Unity.Object的boolean隐式转换，因为Transform被销毁后会返回false，但它不是null
                // 防止类似飞刀的弹射物特效在击杀目标后又突然插在其他卡牌上之类的
                if (effect.sourceTransform == null && source != null)
                {
                    effect.sourceTransform = table.getCardTransform(source);
                }
                if (effect.destTransform == null && target != null)
                {
                    effect.destTransform = table.getCardTransform(target);
                }
                effect.update(_deltaTime);
                yield return null;
            }
            finish();
            table.removeEffect(effect);
            effect = null;
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            if (!effect)
                return true;
            if (!effect.isStarted)
                return true;
            var blockTime = effect.blockTime;
            return _duration > blockTime && blockTime >= 0;
        }
        protected AnimationDefine define;
        protected Card source;
        protected Card target;
        protected TableEffect effect;
    }
}
