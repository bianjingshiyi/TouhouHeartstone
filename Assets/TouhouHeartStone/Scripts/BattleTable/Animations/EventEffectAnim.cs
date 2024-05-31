using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.BattleTable;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using UI;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Game.Animations
{
    public class EventEffectAnim : TableEffectAnim
    {
        public EventEffectAnim(AnimationDefine define, IEventArg eventArg, EventArg effectArg, SkinAnimationRef animRef) : base(define, -1, -1)
        {
            this.define = define;
            this.eventArg = eventArg;
            this.effectArg = effectArg;
            this.animRef = animRef;
        }
        public override IEnumerator update(BattleTableManager table)
        {
            switch (define.type)
            {
                case AnimationType.Area:
                    area = table.getEventSkinAnimArea(effectArg, eventArg, 0); // TODO
                    break;
                default:
                    sourceCardId = getEventSkinAnimCardId(table, true);
                    targetCardId = getEventSkinAnimCardId(table, false);
                    break;
            }
            yield return base.update(table);
        }
        protected override void onEffectCreated(TableEffect effect)
        {
            if (animRef.colorModifies == null)
                return;
            var targetGroups = define.getColorTargetGroups();
            foreach (var modify in animRef.colorModifies)
            {
                if (modify.animationRef != define.getDefineRef())
                    continue;
                if (modify.targetIndex < 0 || modify.targetIndex >= targetGroups.Length)
                    continue;
                var targetGroup = targetGroups[modify.targetIndex];
                foreach (var target in targetGroup.targets)
                {
                    effect.modifyColorTarget(target, modify.getColor());
                }
            }
        }
        public override IEnumerable<TableAnimation> getGroupedAnimations(IEnumerable<TableAnimation> following)
        {
            return following.OfType<EventEffectAnim>().Where(a => a.define == define && a.eventArg.parent == eventArg.parent && a.eventArg.GetType() == eventArg.GetType());
        }
        private int getEventSkinAnimCardId(BattleTableManager table, bool isSource)
        {
            var position = isSource ? animRef.sourcePosition : animRef.targetPosition;
            if (position == VisualEffectPosition.TEMP_VAR)
            {
                return isSource ? sourceTempVarCardId : targetTempVarCardId;
            }
            var card = table.getEventSkinAnimCard(effectArg, eventArg, position);
            return card?.id ?? -1;
        }
        public int sourceTempVarCardId;
        public int targetTempVarCardId;
        private IEventArg eventArg;
        private EventArg effectArg;
        private SkinAnimationRef animRef;
    }
}
