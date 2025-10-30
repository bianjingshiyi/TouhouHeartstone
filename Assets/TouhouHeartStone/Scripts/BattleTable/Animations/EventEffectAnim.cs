using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.BattleTable;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Card = TouhouCardEngine.Card;

namespace Game.Animations
{
    public class EventEffectAnim : TableEffectAnim
    {
        public EventEffectAnim(AnimationDefine define, EventArg eventArg, EventArg effectArg, SkinAnimationRef animRef) : base(define, -1, -1)
        {
            this.define = define;
            this.eventArg = eventArg;
            this.effectArg = effectArg;
            this.animRef = animRef;
        }
        public EventEffectAnim(AnimationDefine define, EventArg eventArg, SkinAnimationRef animRef, Card card) : base(define, -1, -1)
        {
            this.define = define;
            this.eventArg = eventArg;
            this.animRef = animRef;
            this.card = card;
        }
        public override IEnumerator update(BattleTableManager table)
        {
            switch (define.type)
            {
                case AnimationType.Camera:
                    // 什么都不初始化。
                    break;
                case AnimationType.Area:
                    area = table.getEventSkinAnimArea(eventArg, 0); // TODO
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
            effect.modifyColors(animRef.colorModifies);
        }
        public override IEnumerable<TableAnimation> getGroupedAnimations(IEnumerable<TableAnimation> following)
        {
            return following.OfType<EventEffectAnim>().Where(a => a.define == define && a.eventArg.parent == eventArg.parent && a.eventArg.GetType() == eventArg.GetType());
        }
        private int getEventSkinAnimCardId(BattleTableManager table, bool isSource)
        {
            int position = isSource ? animRef.sourcePosition : animRef.targetPosition;
            string varName = isSource ? animRef.sourcePositionVarName : animRef.targetPositionVarName;
            switch (position)
            {
                case VisualEffectPosition.SELF_MASTER:
                    return card.getOwner().getMaster()?.id ?? -1;
                case VisualEffectPosition.ENEMY_MASTER:
                    return table.game.getOpponent(card.getOwner()).getMaster()?.id ?? -1;
                case VisualEffectPosition.THIS_CARD:
                    return card?.id ?? -1;
                case VisualEffectPosition.TARGET_CARD:
                    if (eventArg.define is ICardEventDefine targetEventArg)
                        return targetEventArg.getCard(eventArg)?.id ?? -1;
                    if (eventArg.define is EffectTriggerEventDefine targetEffectArg)
                    {
                        Card[] targets = eventArg.getVar<Card[]>(EffectTriggerEventDefine.VAR_TARGETS);
                        if (targets != null && targets.Length > 0)
                            return targets[0]?.id ?? -1;
                    }
                    break;
                case VisualEffectPosition.TEMP_VAR:
                    return isSource ? sourceTempVarCardId : targetTempVarCardId;
                case VisualEffectPosition.EVENT_VAR:
                    return eventArg.getVar<Card>(varName)?.id ?? -1;
            }
            return -1;
        }
        public int sourceTempVarCardId;
        public int targetTempVarCardId;
        /// <summary>
        /// 触发事件，用于节点
        /// </summary>
        private EventArg eventArg;
        /// <summary>
        /// 效果生效的事件，用于效果根节点特效
        /// </summary>
        private EventArg effectArg;
        private Card card;
        private SkinAnimationRef animRef;
    }
}
