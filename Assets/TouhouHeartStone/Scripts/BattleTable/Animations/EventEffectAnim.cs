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
        public EventEffectAnim(AnimationDefine define, IEventArg eventArg, EventArg effectArg, SkinAnimationRef animRef) : base(define, null, null)
        {
            this.define = define;
            this.eventArg = eventArg;
            this.effectArg = effectArg;
            this.animRef = animRef;
        }
        public override IEnumerator update(BattleTableManager table)
        {
            source = table.getEventSkinAnimCard(effectArg, eventArg, animRef.sourcePosition);
            target = table.getEventSkinAnimCard(effectArg, eventArg, animRef.targetPosition);
            yield return base.update(table);
        }
        public override IEnumerable<TableAnimation> getGroupedAnimations(IEnumerable<TableAnimation> following)
        {
            return following.OfType<EventEffectAnim>().Where(a => a.define == define && a.eventArg.parent == eventArg.parent && a.eventArg.GetType() == eventArg.GetType());
        }
        private IEventArg eventArg;
        private EventArg effectArg;
        private SkinAnimationRef animRef;
    }
}
