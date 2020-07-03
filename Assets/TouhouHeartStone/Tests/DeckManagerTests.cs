
using NUnit.Framework;
using UnityEngine;
using Game;

namespace Tests
{
    public class DeckManagerTests
    {
        /// <summary>
        /// 加载数据测试
        /// </summary>
        /// <returns></returns>
        [Test]
        public void DeckCreate()
        {
            DeckManager dm = createDM();

            var id = dm.CreateDeck("测试卡组");
            var name = dm.GetDeckName(id);

            Assert.AreEqual("测试卡组", name);

            GameObject.Destroy(dm.gameObject);
        }

        [Test]
        public void CardsAdd()
        {
            DeckManager dm = createDM();

            var deckID = dm.CreateDeck("测试卡组");

            var instID1 = dm.AddCard(deckID, 1000);
            var instID2 = dm.AddCard(deckID, 1001);

            Assert.AreEqual(1000, dm.GetCardID(instID1));
            Assert.AreEqual(1001, dm.GetCardID(instID2));
        }

        [Test]
        public void CardRemove()
        {
            DeckManager dm = createDM();

            var deckID = dm.CreateDeck("测试卡组");

            var instID1 = dm.AddCard(deckID, 1000);
            var instID2 = dm.AddCard(deckID, 1001);

            dm.RemoveCard(instID2);

            System.Threading.Thread.Sleep(50);

            Assert.AreEqual(1000, dm.GetCardID(instID1));
            Assert.AreEqual(-1, dm.GetCardID(instID2));
        }

        [Test]
        public void DeckRemove()
        {
            DeckManager dm = createDM();

            var deckID = dm.CreateDeck("测试卡组");

            var instID1 = dm.AddCard(deckID, 1000);
            var instID2 = dm.AddCard(deckID, 1001);

            dm.RemoveDeck(deckID);

            System.Threading.Thread.Sleep(50);

            Assert.Null(dm.GetDeckName(deckID));
            Assert.AreEqual(-1, dm.GetCardID(instID1));
            Assert.AreEqual(-1, dm.GetCardID(instID2));
        }

        [Test]
        public void CardsTransation()
        {
            DeckManager dm = createDM();

            var deckID = dm.CreateDeck("测试卡组");

            var instID1 = dm.AddCard(deckID, 1000);
            var instID2 = dm.AddCard(deckID, 1001);

            // 进入事务
            dm.EnterTransaction();

            var instID3 = dm.AddCard(deckID, 1003);
            dm.RemoveCard(instID1);

            Assert.AreEqual(-1, dm.GetCardID(instID1));
            Assert.AreEqual(1003, dm.GetCardID(instID3));

            // 尝试回滚
            dm.Rollback();
            Assert.AreEqual(1000, dm.GetCardID(instID1));
            Assert.AreEqual(-1, dm.GetCardID(instID3));

            // 进入事务
            dm.EnterTransaction();
            instID3 = dm.AddCard(deckID, 1003);
            dm.RemoveCard(instID2);

            Assert.AreEqual(-1, dm.GetCardID(instID2));
            Assert.AreEqual(1003, dm.GetCardID(instID3));

            // 提交
            dm.Commit();
            Assert.AreEqual(-1, dm.GetCardID(instID2));
            Assert.AreEqual(1003, dm.GetCardID(instID3));
        }

        private static DeckManager createDM()
        {
            var go = new GameObject("DeckManager");
            var db = go.AddComponent<DatabaseManager>();
            db.ConnectToMemory();
            var dm = go.AddComponent<DeckManager>();
            return dm;
        }
    }
}
