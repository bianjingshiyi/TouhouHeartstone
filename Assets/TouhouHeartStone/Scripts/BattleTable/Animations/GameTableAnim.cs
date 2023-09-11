using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.BattleTable;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using UI;
using UnityEngine;

namespace Game.Animations
{
    public class GameTableAnim : TableAnimation
    {
        public GameTableAnim(AnimationDefine define, IEventArg eventArg, EffectTriggerEventArg effectArg, SkinAnimationRef animRef)
        {
            this.define = define;
            this.eventArg = eventArg;
            this.effectArg = effectArg;
            this.animRef = animRef;
        }
        public override IEnumerator update(BattleTableManager table)
        {
            var sourceCard = table.getEventSkinAnimCard(effectArg, eventArg, animRef.sourcePosition);
            var targetCard = table.getEventSkinAnimCard(effectArg, eventArg, animRef.targetPosition);
            if (!effect)
            {
                Transform srcTransform = table.getCardTransform(sourceCard);
                Transform targetTransform = null;
                if (targetCard != null)
                {
                    targetTransform = table.getCardTransform(targetCard);
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
                // 这里不用Unity.Object的boolean隐式转换，因为Transform被销毁后会返回false，但它不是null
                // 防止类似飞刀的弹射物特效在击杀目标后又突然插在其他卡牌上之类的
                if (effect.sourceTransform == null && sourceCard != null)
                {
                    effect.sourceTransform = table.getCardTransform(sourceCard);
                }
                if (effect.destTransform == null && targetCard != null)
                {
                    effect.destTransform = table.getCardTransform(targetCard);
                }
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
        private EffectTriggerEventArg effectArg;
        private SkinAnimationRef animRef;
        private TableEffect effect;
    }
}
