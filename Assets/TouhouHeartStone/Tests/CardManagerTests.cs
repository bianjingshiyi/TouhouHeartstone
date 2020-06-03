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

using System.IO;
using IGensoukyo;
using TouhouCardEngine;

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
            var go = new GameObject(nameof(CardManager));
            go.AddComponent<ResourceManager>();
            CardManager cm = go.AddComponent<CardManager>();
            await cm.Load(new string[] { "Cards/Cards.xls" });
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

        /// <summary>
        /// 在Test/Resources文件夹中有测试用的表格和图片，在测试开始的时候把它们复制到streamingAssets中，然后测试能够从中加载出卡片，再删掉测试文件。
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator loadCardsTest()
        {
            var go = new GameObject(nameof(CardManager));
            go.AddComponent<ResourceManager>();
            CardManager cm = go.AddComponent<CardManager>();

            string xlsFile = Path.Combine(Application.streamingAssetsPath, "Cards", "Test.xls");

            safeCopy("Assets\\TouhouHeartStone\\Tests\\Resources\\Cards.xls", xlsFile);

            var task = cm.loadCards(xlsFile);
            yield return new WaitUntil(() => task.IsCompleted);
            var cards = task.Result;

            Assert.NotNull(cards);
            Assert.True(cards.Where(c => c.id == ExistsCardID).Count() > 0);

            File.Delete(xlsFile);
        }

        void safeCopy(string src, string dst)
        {
            if (File.Exists(dst))
                File.Delete(dst);

            File.Copy(src, dst);
        }

        /// <summary>
        /// 同上，只不过是加载图片
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator loadSkinsTest()
        {
            var go = new GameObject(nameof(CardManager));
            go.AddComponent<ResourceManager>();
            CardManager cm = go.AddComponent<CardManager>();

            string xlsFile = Path.Combine(Application.streamingAssetsPath, "Cards", "Test.xls");
            string pictureFile = Path.Combine(Application.streamingAssetsPath, "测试图片.jpg");

            safeCopy("Assets\\TouhouHeartStone\\Tests\\Resources\\Cards.xls", xlsFile);
            safeCopy("Assets\\TouhouHeartStone\\Tests\\Resources\\测试图片.jpg", pictureFile);

            var task = cm.loadCards(xlsFile);
            yield return new WaitUntil(() => task.IsCompleted);
            var cards = task.Result;

            Assert.NotNull(cards);
            Assert.True(cards.Where(c => c.id == ExistsCardID).Count() > 0);

            File.Delete(xlsFile);
            File.Delete(pictureFile);
        }
        /// <summary>
        /// 同上，只不过是打包成安卓，然后再加载
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator loadCardsTest_Android()
        {
            var go = new GameObject(nameof(CardManager));
            go.AddComponent<ResourceManager>();
            CardManager cm = go.AddComponent<CardManager>();

            string xlsFile = Path.Combine(Application.streamingAssetsPath, "Cards", "Test.xls");
            string dataSetFileName = Path.Combine("Cards", "Test.xls.dataset");
            string dataSetFile = Path.Combine(Application.streamingAssetsPath, dataSetFileName);

            safeCopy("Assets\\TouhouHeartStone\\Tests\\Resources\\Cards.xls", xlsFile);
            ExcelDataSetPacker.PackExcelToDataSet(xlsFile, dataSetFile);

            var task = cm.loadCards(dataSetFileName, RuntimePlatform.Android);
            yield return new WaitUntil(() => task.IsCompleted);
            var cards = task.Result;

            Assert.NotNull(cards);
            Assert.True(cards.Where(c => c.id == ExistsCardID).Count() > 0);

            File.Delete(xlsFile);
            File.Delete(dataSetFile);
        }
        /// <summary>
        /// 同上
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator loadSkinsTest_Android()
        {
            var go = new GameObject(nameof(CardManager));
            go.AddComponent<ResourceManager>();
            CardManager cm = go.AddComponent<CardManager>();

            string xlsFile = Path.Combine(Application.streamingAssetsPath, "Cards", "Test.xls");
            string dataSetFileName = Path.Combine("Cards", "Test.xls.dataset");
            string dataSetFile = Path.Combine(Application.streamingAssetsPath, dataSetFileName);
            string pictureFile = Path.Combine(Application.streamingAssetsPath, "测试图片.jpg");

            safeCopy("Assets\\TouhouHeartStone\\Tests\\Resources\\Cards.xls", xlsFile);
            safeCopy("Assets\\TouhouHeartStone\\Tests\\Resources\\测试图片.jpg", pictureFile);
            ExcelDataSetPacker.PackExcelToDataSet(xlsFile, dataSetFile);

            var task = cm.loadCards(dataSetFileName, RuntimePlatform.Android);
            yield return new WaitUntil(() => task.IsCompleted);
            var cards = task.Result;

            Assert.NotNull(cards);
            Assert.True(cards.Where(c => c.id == ExistsCardID).Count() > 0);

            File.Delete(xlsFile);
            File.Delete(pictureFile);
            File.Delete(dataSetFile);
        }
    }
}
