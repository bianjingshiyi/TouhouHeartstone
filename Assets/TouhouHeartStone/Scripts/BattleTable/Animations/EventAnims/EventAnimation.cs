using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace Game.BattleTable
{
    public abstract class EventAnimation : TableAnimation
    {
        #region 公有方法
        public sealed override IEnumerator update(BattleTableManager table)
        {
            do
            {
                _varChanged = false;
                animState = EventAnimState.Started;
                yield return startUpdate(table);
                animState = EventAnimState.StartFinished;
                while (shouldPauseAfterStart())
                {
                    yield return null;
                }
            }
            while (_varChanged);

            if (!eventArg.isCanceled)
            {
                animState = EventAnimState.Logic;
                yield return logicUpdate(table);
            }
            else
            {
                animState = EventAnimState.Canceled;
                yield return canceledUpdate(table);
            }
            animState = EventAnimState.Finished;
        }
        public virtual void postEvent(BattleTableManager table)
        {
        }
        public virtual EventAnimation clone()
        {
            return MemberwiseClone() as EventAnimation;
        }
        public virtual IEnumerator startUpdate(BattleTableManager table)
        {
            yield break;
        }
        public virtual IEnumerator logicUpdate(BattleTableManager table)
        {
            yield break;
        }
        public virtual IEnumerator canceledUpdate(BattleTableManager table)
        {
            yield break;
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            if (animState == EventAnimState.StartFinished)
            {
                // 事件没有完成，并且该动画不是通过请求动画强行播放的，则继续后续动画。
                if (isEventPausedAfterStart())
                    return true;
                if (shouldPauseAfterStart() && isAnimGroupInsertedAnimOfThis(nextGroup))
                    return true;
            }
            return base.continueNextGroup(nextGroup);
        }
        public void setVar(string name, object value)
        {
            startData.setVar(name, value);
            _varChanged = true;
        }
        public bool isVarChanged()
        {
            return _varChanged;
        }
        #endregion

        #region 私有方法
        protected IEnumerator pauseWhileNotCompleted()
        {
            while (isEventPausedAfterStart())
                yield return null;
            yield break;
        }
        protected void stopWhenCanceled(BattleTableManager table)
        {
            if (eventArg.isCanceled)
                finish(table);
        }
        protected CardSnapshot snapshotCard(BattleTableManager table, Card card)
        {
            return table.game.snapshotCard(card);
        }
        protected CardSnapshot[] snapshotCardArray(BattleTableManager table, IEnumerable<Card> cards)
        {
            return cards.Select(c => table.game.snapshotCard(c)).ToArray();
        }
        private bool shouldPauseAfterStart()
        {
            // 事件在事件前被停住了；（泰坦，XX时选牌。）
            // 或者存在事件前插入的动画；（陷阱）
            // 并且当没有事件变量发生改变时；（古明地恋多次修改目标）
            // 暂停该动画。
            bool notCompleted = isEventPausedAfterStart();
            bool hasInsertAnim = insertedAnims != null && insertedAnims.Count > 0 && insertedAnims.Any(a => !a.isFinished());
            return (notCompleted || hasInsertAnim) && !_varChanged;
        }
        private bool isAnimGroupInsertedAnimOfThis(AnimationGroup nextGroup)
        {
            if (insertedAnims == null || insertedAnims.Count <= 0)
                return false;
            return nextGroup.Any(a => insertedAnims.Contains(a));
        }
        private bool isEventPausedAfterStart()
        {
            return eventArg.state == EventState.Before || eventArg.state == EventState.Logic;
        }
        #endregion

        #region 属性字段
        public IEventArg eventArg { get; set; }
        public List<TableAnimation> insertedAnims { get; } = new List<TableAnimation>();
        public EventAnimStartData startData { get; set; }
        public EventAnimState animState;
        private bool _varChanged;
        #endregion
    }
    public abstract class EventAnimation<T> : EventAnimation where T : EventDefine
    {
    }
    public static class AnimationHelper
    {
        public static bool isInstantAnimation(this TableAnimation anim)
        {
            return anim is PropChangeAnim || anim is AddBuffAnim || anim is RemoveBuffAnim || anim is ChangeCardVisibilityAnim;
        }
    }
    public enum EventAnimState
    {
        NotStarted,
        Started,
        StartFinished,
        Logic,
        Canceled,
        Finished
    }
}
