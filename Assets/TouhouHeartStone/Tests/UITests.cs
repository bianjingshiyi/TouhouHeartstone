using NUnit.Framework;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Game;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace Tests
{
    public class UITests
    {
        [Test]
        public void setAndGetEventAnimTest()
        {
            TableManager manager = createTable();

            manager.setEventAnim<TestEvent, TestAnim>();
            Assert.IsInstanceOf<TestAnim>(manager.getEventAnim(new TestEvent()));
        }
        [Test]
        public void loadAnimTest()
        {
            var table = createTable();
            table.loadAnim(typeof(UITests).Assembly);
            Assert.IsInstanceOf<TestAnim>(table.getEventAnim(new TestEvent()));
        }
        [Test]
        public void addAnimToQueueTest()
        {
            var table = createTable();
            TestAnim anim = new TestAnim(new TestEvent());
            table.addAnim(anim);
            var queue = table.getAnimQueue();
            Assert.AreEqual(1, queue.Length);
            Assert.AreEqual(anim, queue[0]);
        }
        [Test]
        public void updateAnimTest()
        {
            var table = createTable();
            int i = 3;
            TestAnim anim = new TestAnim(() =>
            {
                i--;
                return i < 1;
            });
            table.addAnim(anim);
            table.updateAnim();//2
            Assert.AreEqual(1, table.getAnimQueue().Length);
            table.updateAnim();//1
            Assert.AreEqual(1, table.getAnimQueue().Length);
            table.updateAnim();//0
            Assert.AreEqual(0, table.getAnimQueue().Length);
        }
        [Test]
        public void updateAnimTest_Block()
        {
            var table = createTable();
            int i = 3, j = 3;
            TestAnim anim1 = new TestAnim(() =>
            {
                i--;
                return i < 1;
            });
            TestAnim anim2 = new TestAnim(() =>
            {
                j--;
                return j < 1;
            });
            table.addAnim(anim1);
            table.addAnim(anim2);
            table.updateAnim();//2
            Assert.AreEqual(2, table.getAnimQueue().Length);
            table.updateAnim();//1
            Assert.AreEqual(2, table.getAnimQueue().Length);
            table.updateAnim();//0 2
            Assert.AreEqual(1, table.getAnimQueue().Length);
            table.updateAnim();//1
            Assert.AreEqual(1, table.getAnimQueue().Length);
            table.updateAnim();//0
            Assert.AreEqual(0, table.getAnimQueue().Length);
        }
        [Test]
        public void updateAnimTest_Unblock()
        {
            var table = createTable();
            int i = 3, j = 3;
            TestAnim anim1 = new TestAnim(() =>
            {
                i--;
                return i < 1;
            }, anim => false);
            TestAnim anim2 = new TestAnim(() =>
            {
                j--;
                return j < 1;
            }, anim => false);
            table.addAnim(anim1);
            table.addAnim(anim2);
            table.updateAnim();//2 2
            Assert.AreEqual(2, table.getAnimQueue().Length);
            table.updateAnim();//1 1
            Assert.AreEqual(2, table.getAnimQueue().Length);
            table.updateAnim();//0 0
            Assert.AreEqual(0, table.getAnimQueue().Length);
        }
        private static TableManager createTable()
        {
            return new GameObject(nameof(TableManager)).AddComponent<TableManager>();
        }
        class TestEvent : EventArg
        {
        }
        class TestAnim : UIAnimation<TestEvent>
        {
            Func<bool> func { get; } = null;
            Func<UIAnimation, bool> onBlockAnim { get; } = null;
            public TestAnim(Func<bool> func = null, Func<UIAnimation, bool> onBlockAnim = null) : base(null)
            {
                this.func = func;
                this.onBlockAnim = onBlockAnim;
            }
            public TestAnim(TestEvent eventArg) : base(eventArg)
            {
            }
            public override bool update(Table table)
            {
                if (func != null)
                    return func();
                else
                    return true;
            }
            public override bool blockAnim(UIAnimation nextAnim)
            {
                if (onBlockAnim != null)
                    return onBlockAnim(nextAnim);
                return base.blockAnim(nextAnim);
            }
        }
    }
}