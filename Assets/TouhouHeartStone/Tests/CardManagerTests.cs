using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game;
using UnityEngine.SceneManagement;
using TouhouHeartstone;

namespace Tests
{
    public class CardManagerTests
    {
        /// <summary>
        /// 一定会有的卡ID
        /// 如果这张卡不见了，请改一下这个测试
        /// </summary>
        const int ExistsCardID = 1000;

        async Task<CardManager> loadCards()
        {
            CardManager cm = new GameObject(nameof(CardManager)).AddComponent<CardManager>();
            await cm.Load(new string[] { "Cards/cards.xls" });
            return cm;
        }

        /// <summary>
        /// 加载数据测试
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator LoadTest()
        {
            var task = loadCards();
            yield return new WaitUntil(() => task.IsCompleted);
            Assert.IsNull(task.Exception);
        }

        /// <summary>
        /// 获取指定ID的卡片测试
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator GetCardDefineTest()
        {
            var task = loadCards();
            yield return new WaitUntil(() => task.IsCompleted);
            var cm = task.Result;

            // 一定会有的卡
            var cd = cm.GetCardDefine(ExistsCardID);
            Assert.NotNull(cd);
            Assert.Equals(cd.id, ExistsCardID);
        }

        /// <summary>
        /// 使用过滤器获取多张卡片的测试
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator GetCardDefinesTest()
        {
            var task = loadCards();
            yield return new WaitUntil(() => task.IsCompleted);
            var cm = task.Result;

            var cards = cm.GetCardDefines((cd) => { return cd.id == ExistsCardID; });
            Assert.NotNull(cards);
            Assert.Equals(cards.Length, 1);
            Assert.Equals(cards[0].id, ExistsCardID);
        }

        [UnityTest]
        public IEnumerator GetDefaultSpriteTest()
        {
            var task = loadCards();
            yield return new WaitUntil(() => task.IsCompleted);
            var cm = task.Result;

            var task2 = cm.getDefaultSprite();
            yield return new WaitUntil(() => task2.IsCompleted);
            Assert.Null(task2.Exception);
            Assert.NotNull(task2.Result);
        }

        [UnityTest]
        public IEnumerator CardSkinTest()
        {
            var task = loadCards();
            yield return new WaitUntil(() => task.IsCompleted);
            var cm = task.Result;

            cm.AddCardSkinTemp(new UI.CardSkinData { id = ExistsCardID });
            var skin = cm.GetCardSkin(ExistsCardID);
            Assert.NotNull(skin);
            Assert.Equals(skin.id, ExistsCardID);
        }
    }
}
