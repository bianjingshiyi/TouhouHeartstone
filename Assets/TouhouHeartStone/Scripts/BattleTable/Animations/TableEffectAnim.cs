using System.Collections;
using Game.BattleTable;
using UnityEngine;

namespace Game.Animations
{
    public class TableEffectAnim : TableAnimation
    {
        public TableEffectAnim(AnimationDefine define, int sourceCardId, int targetCardId)
        {
            this.define = define;
            this.sourceCardId = sourceCardId;
            this.targetCardId = targetCardId;
        }
        public override IEnumerator update(BattleTableManager table)
        {
            if (!effect)
            {
                Transform srcTransform = null;
                Transform targetTransform = null;
                if (sourceCardId >= 0)
                {
                    srcTransform = table.getCardTransform(sourceCardId);
                    _sourceConfirmed = srcTransform;
                }
                if (targetCardId >= 0)
                {
                    targetTransform = table.getCardTransform(targetCardId);
                    _destConfirmed = targetTransform;
                }
                var info = new VisualEffectInfo()
                {
                    srcTransform = srcTransform,
                    destTransform = targetTransform,
                };
                effect = table.createEffect(define, info);
            }
            while (!effect.isFinished() && !(effect.state == EffectState.None && _duration >= 10f)) // 十秒没播放算超时
            {
                // 防止类似飞刀的弹射物特效在击杀目标后又突然插在其他卡牌上之类的
                if (!_sourceConfirmed && !effect.sourceTransform && sourceCardId >= 0)
                {
                    effect.sourceTransform = table.getCardTransform(sourceCardId);
                    _sourceConfirmed = effect.sourceTransform;
                }
                if (!_destConfirmed && !effect.destTransform && targetCardId >= 0)
                {
                    effect.destTransform = table.getCardTransform(targetCardId);
                    _destConfirmed = effect.destTransform;
                }
                effect.update(_deltaTime);
                yield return null;
            }
            finish(table);
            table.removeEffect(effect);
            effect = null;
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            if (!effect)
                return true;
            if (effect.state == EffectState.None || effect.state == EffectState.Dormant)
                return true;
            var blockTime = effect.blockTime;
            return _duration > blockTime && blockTime >= 0;
        }
        protected AnimationDefine define;
        protected int sourceCardId;
        protected int targetCardId;
        protected TableEffect effect;
        private bool _sourceConfirmed;
        private bool _destConfirmed;
    }
}
