using System.Collections;
using System.Collections.Generic;
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
        public TableEffectAnim(AnimationDefine define, TableArea area)
        {
            this.define = define;
            this.area = area;
        }
        public override IEnumerator update(BattleTableManager table)
        {
            if (effect == null) // 检查是否创建特效
            {
                // 创建特效
                Transform srcTransform = initSourceTransform(table);
                Transform targetTransform = initDestTransform(table);

                var info = new VisualEffectInfo()
                {
                    srcTransform = srcTransform,
                    destTransform = targetTransform,
                };
                effect = table.createEffect(define, info);
                onEffectCreated(effect);
            }
            while (!effect.isFinished() && // 检查特效是否播放完毕
                !(effect.state == EffectState.None && _time >= 10f)) // 十秒没播放算超时
            {
                // 防止类似飞刀的弹射物特效在击杀目标后又突然插在其他卡牌上之类的
                if (!_sourceConfirmed && !effect.sourceTransform)
                {
                    effect.sourceTransform = initSourceTransform(table);
                }
                if (!_destConfirmed && !effect.destTransform)
                {
                    effect.destTransform = initDestTransform(table);
                }
                effect.update(_deltaTime);
                yield return null;
            }
            finish(table);
            table.removeEffect(effect);
            effect = null;
        }
        public override bool continueNextGroup(IEnumerable<TableAnimation> nextGroup)
        {
            if (!effect)
                return true;
            if (effect.state == EffectState.None || effect.state == EffectState.Dormant)
                return true;
            return _time > effect.blockTime && effect.blockTime >= 0;
        }
        protected virtual void onEffectCreated(TableEffect effect)
        {

        }
        private Transform initSourceTransform(BattleTableManager table)
        {
            Transform srcTransform = null;
            if (sourceCardId >= 0)
            {
                srcTransform = table.getCardTransform(sourceCardId);
            }
            else if (area != TableArea.None)
            {
                srcTransform = table.getAreaTransform(area);
            }
            else if (define.type == AnimationType.Camera)
            {
                srcTransform = table.table.CenterDisplayTransform;
            }
            _sourceConfirmed = srcTransform;
            return srcTransform;
        }
        private Transform initDestTransform(BattleTableManager table)
        {
            Transform targetTransform = null;
            if (targetCardId >= 0)
            {
                targetTransform = table.getCardTransform(targetCardId);
                _destConfirmed = targetTransform;
            }
            _destConfirmed = targetTransform;
            return targetTransform;
        }
        protected AnimationDefine define;
        protected int sourceCardId;
        protected int targetCardId;
        protected TableArea area;
        protected TableEffect effect;
        private bool _sourceConfirmed;
        private bool _destConfirmed;
    }
}
