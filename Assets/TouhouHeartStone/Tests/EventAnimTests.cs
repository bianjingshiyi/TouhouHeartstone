using BJSYGameCore;
using Game;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TouhouCardEngine.Interfaces;
using UI;
using UnityEngine.TestTools;

namespace Tests
{
    public class EventAnimTests
    {
        const string ANIM_NAME_ATOM = "Atom";
        const string ANIM_NAME_COMP = "Comp";
        /// <summary>
        /// 为事件设置动画，并且当每一次事件发生的时候，都可以获取到设置动画的复制实例。
        /// </summary>
        [Test]
        public void setAndGetAnimTest()
        {
            EventAnimManager manager = new EventAnimManager();
            AtomAnim atomOrigin = new AtomAnim();
            manager.setAnim4Event(ANIM_NAME_ATOM, atomOrigin);
            CompAnim compOrigin = new CompAnim();
            manager.setAnim4Event(ANIM_NAME_COMP, compOrigin);
            Assert.IsInstanceOf<AtomAnim>(manager.getAnim4Event(ANIM_NAME_ATOM));
            Assert.IsInstanceOf<CompAnim>(manager.getAnim4Event(ANIM_NAME_COMP));
            Assert.AreNotEqual(atomOrigin, manager.getAnim4Event(ANIM_NAME_ATOM));
            Assert.AreNotEqual(compOrigin, manager.getAnim4Event(ANIM_NAME_COMP));
            Assert.AreNotEqual(manager.getAnim4Event(ANIM_NAME_ATOM), manager.getAnim4Event(ANIM_NAME_ATOM));
            Assert.AreNotEqual(manager.getAnim4Event(ANIM_NAME_COMP), manager.getAnim4Event(ANIM_NAME_COMP));
        }
        /// <summary>
        /// 将动画加入队列
        /// </summary>
        [Test]
        public void addAndGetAnimInQueueTest()
        {
            EventAnimManager manager = createManager();
            EventAnimation atomAnim = manager.getAnim4Event(ANIM_NAME_ATOM);
            manager.addAnimToQueue(atomAnim);
            EventAnimation compAnim = manager.getAnim4Event(ANIM_NAME_COMP);
            manager.addAnimToQueue(compAnim);
            var anims = manager.getAnimsInQueue();
            Assert.AreEqual(atomAnim, anims[0]);
            Assert.AreEqual(compAnim, anims[1]);
        }

        private static EventAnimManager createManager()
        {
            EventAnimManager manager = new EventAnimManager();
            manager.setAnim4Event(ANIM_NAME_ATOM, new AtomAnim());
            manager.setAnim4Event(ANIM_NAME_COMP, new CompAnim());
            return manager;
        }
        [Test]
        public void onEventTest()
        {
            EventAnimManager manager = createManager();
            manager.onEvent(ANIM_NAME_ATOM);
            manager.onEvent(ANIM_NAME_COMP);
            var anims = manager.getAnimsInQueue();
            Assert.IsInstanceOf<AtomAnim>(anims[0]);
            Assert.IsInstanceOf<CompAnim>(anims[1]);
        }
        [Test]
        public void updateTest()
        {
            EventAnimManager manager = createManager();
            manager.onEvent(ANIM_NAME_ATOM);
            Assert.IsInstanceOf<AtomAnim>(manager.getAnimsInQueue()[0]);
            manager.update(.25f);
            Assert.AreEqual(1, manager.getAnimsInQueue().Length);
            manager.update(.3f);
            Assert.AreEqual(0, manager.getAnimsInQueue().Length);
        }
        /// <summary>
        /// 触发3个原子动画，预期在3个动画播放完毕之后，触发onAllAnimFinished事件。
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator onAllAnimFinishedTest()
        {
            EventAnimManager manager = createManager();
            yield return TestHelper.waitUntilEventTrig(manager,
                (m, a) => m.onAllAnimFinished += a,
                () =>
                {
                    manager.onEvent(ANIM_NAME_ATOM);
                    manager.onEvent(ANIM_NAME_ATOM);
                    manager.onEvent(ANIM_NAME_ATOM);
                    manager.update(1.55f);
                    return null;
                });
        }
        class AtomAnim : EventAnimation
        {
            public override string eventName { get; } = ANIM_NAME_ATOM;
            public override IEventArg eventArg { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
            Timer _timer = new Timer() { duration = .5f };
            public override bool update(TableManager table, float time)
            {
                _timer.update(time);
                if (!_timer.isExpired())
                    return false;
                return true;
            }
        }
        class CompAnim : EventAnimation
        {
            public override string eventName { get; } = ANIM_NAME_COMP;
            public override IEventArg eventArg { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
            public override bool update(TableManager table, float time)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}