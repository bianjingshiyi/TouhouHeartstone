using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.BattleTable;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using UI;

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
            sourceCardId = getEventSkinAnimCardId(table, true);
            targetCardId = getEventSkinAnimCardId(table, false);
            yield return base.update(table);
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
