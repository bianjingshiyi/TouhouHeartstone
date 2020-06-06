using NUnit.Framework;
using System.IO;
using UnityEngine;
using UnityEngine.TestTools;
using ExcelLibrary.SpreadSheet;
using Game;
using System;
using System.Collections;
using System.Threading.Tasks;
using IGensoukyo;
using System.Linq;

namespace Tests
{
    public class ResourceManagerTest
    {
        //Excel测试资源为StreamgingAssets/TestExcel.xls。
        //不过我觉得把它删了，每次测试动态新建一个测试Excel表格然后删除好像也是可以的。
        /// <summary>
        /// Win加载Excel，预期得到一个非空Workbook，可以得到位于Cells[1,1]的测试内容"TestContent"
        /// </summary>
        [UnityTest]
        public IEnumerator loadExcel()
        {
            Workbook workbook = new Workbook();
            workbook.Worksheets.Add(new Worksheet("TestSheet"));
            workbook.Worksheets[0].Cells[0, 0] = new Cell("TestContent");
            const string fileName = "TestExcel.xls";
            string filePath = Application.streamingAssetsPath + "/" + fileName;
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                workbook.Save(stream);
            }

            ResourceManager manager = new GameObject(nameof(ResourceManager)).AddComponent<ResourceManager>();
            var task = manager.loadExcel(fileName, RuntimePlatform.WindowsEditor);
            yield return new WaitUntil(() => task.IsCompleted);
            workbook = task.Result;

            Assert.NotNull(workbook);
            Assert.AreEqual("TestContent", workbook.Worksheets[0].Cells[0, 0].Value);

            File.Delete(filePath);
        }
        /// <summary>
        /// 当发生异常，抛出加载过程中发生的异常，此处测试尝试加载不存在的Excel，应当抛出FileNotFound
        /// </summary>
        [UnityTest]
        public IEnumerator loadExcel_Failed()
        {
            const string fileName = "TestExcel.xls";

            ResourceManager manager = new GameObject(nameof(ResourceManager)).AddComponent<ResourceManager>();
            Task<Workbook> task = null;
            Assert.Throws<FileNotFoundException>(() =>
            {
                task = manager.loadExcel(fileName, RuntimePlatform.WindowsEditor);
            });
            yield break;
            //if (task == null)
            //    yield break;
            //yield return new WaitUntil(() => task.IsCompleted || task.IsFaulted);
            //Workbook workbook = task.Result;

            //Assert.IsTrue(task.IsFaulted);
            //Assert.IsInstanceOf<FileNotFoundException>(task.Exception.InnerException);
        }
        /// <summary>
        /// Android使用WebRequest加载Excel
        /// </summary>
        [UnityTest]
        public IEnumerator loadExcel_Android()
        {
            Workbook workbook = new Workbook();
            workbook.Worksheets.Add(new Worksheet("TestSheet"));
            workbook.Worksheets[0].Cells[0, 0] = new Cell("TestContent");
            const string fileName = "TestExcel.xls";
            string filePath = Application.streamingAssetsPath + "/" + fileName;
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                workbook.Save(stream);
            }

            ResourceManager manager = new GameObject(nameof(ResourceManager)).AddComponent<ResourceManager>();
            var task = manager.loadExcel(fileName, RuntimePlatform.Android);
            yield return new WaitUntil(() => task.IsCompleted);
            workbook = task.Result;

            Assert.NotNull(workbook);
            Assert.AreEqual("TestContent", workbook.Worksheets[0].Cells[0, 0].Value);

            File.Delete(filePath);
        }
        /// <summary>
        /// Android加载不存在的Excel抛出异常，需要研究一下WebRequest的异常机制。
        /// </summary>
        [UnityTest]
        public IEnumerator loadExcel_Android_Failed()
        {
            const string fileName = "SomeNonExistFileName";

            ResourceManager manager = new GameObject(nameof(ResourceManager)).AddComponent<ResourceManager>();
            Task<Workbook> task = manager.loadExcel(fileName, RuntimePlatform.Android);
            yield return new WaitUntil(() => task.IsCompleted);
            Assert.Throws<FileNotFoundException>(() =>
            {
                if (task.Exception != null)
                {
                    foreach (var ex in task.Exception.InnerExceptions)
                    {
                        throw ex;
                    }
                }
            });
        }
        /// <summary>
        /// 同上，测试用贴图建议放在Test/Resources中，然后在测试的时候在StreamingAssets中新建并复制贴图，用于测试，测完了删掉。
        /// </summary>
        [UnityTest]
        public IEnumerator loadTexture()
        {
            const string fileName = "TestFile.png";
            string filePath = Application.streamingAssetsPath + "/" + fileName;

            Texture2D tx = new Texture2D(512, 512);
            File.WriteAllBytes(filePath, tx.EncodeToPNG());

            ResourceManager manager = new GameObject(nameof(ResourceManager)).AddComponent<ResourceManager>();
            var task = manager.loadTexture(fileName, RuntimePlatform.WindowsPlayer);
            yield return new WaitUntil(() => task.IsCompleted);

            File.Delete(filePath);
        }
        [UnityTest]
        public IEnumerator loadTexture_Failed()
        {
            const string fileName = "SomeNonExistFile.png";
            ResourceManager manager = new GameObject(nameof(ResourceManager)).AddComponent<ResourceManager>();
            Task<Texture2D> task = manager.loadTexture(fileName, RuntimePlatform.WindowsEditor);
            yield return new WaitUntil(() => task.IsCompleted);
            Assert.Throws<FileNotFoundException>(() =>
            {
                if (task.Exception != null)
                {
                    foreach (var ex in task.Exception.InnerExceptions)
                    {
                        throw ex;
                    }
                }
            });
        }
        [UnityTest]
        public IEnumerator loadTexture_Android()
        {
            const string fileName = "TestFile.png";
            string filePath = Application.streamingAssetsPath + "/" + fileName;

            Texture2D tx = new Texture2D(512, 512);
            File.WriteAllBytes(filePath, tx.EncodeToPNG());

            ResourceManager manager = new GameObject(nameof(ResourceManager)).AddComponent<ResourceManager>();
            var task = manager.loadTexture(fileName, RuntimePlatform.Android);
            yield return new WaitUntil(() => task.IsCompleted);

            File.Delete(filePath);
        }
        [UnityTest]
        public IEnumerator loadTexture_Android_Failed()
        {
            const string fileName = "SomeNonExistFile.png";

            ResourceManager manager = new GameObject(nameof(ResourceManager)).AddComponent<ResourceManager>();
            Task<Texture2D> task = manager.loadTexture(fileName, RuntimePlatform.Android);
            yield return new WaitUntil(() => task.IsCompleted);
            Assert.Throws<FileNotFoundException>(() =>
            {
                if (task.Exception != null)
                {
                    foreach (var ex in task.Exception.InnerExceptions)
                    {
                        throw ex;
                    }
                }
            });
        }
        [UnityTest]
        public IEnumerator loadTexture_Android_Fallback()
        {
            const string fileName = "TestFile.png";
            const string fileNameFake = "TestFile.jpg";

            string filePath = Application.streamingAssetsPath + "/" + fileName;

            Texture2D tx = new Texture2D(512, 512);
            File.WriteAllBytes(filePath, tx.EncodeToPNG());

            ResourceManager manager = new GameObject(nameof(ResourceManager)).AddComponent<ResourceManager>();
            var task = manager.loadTexture(fileNameFake, RuntimePlatform.Android);
            yield return new WaitUntil(() => task.IsCompleted);

            File.Delete(filePath);
        }

        [UnityTest]
        public IEnumerator loadTexture_Fallback()
        {
            const string fileName = "TestFile.png";
            const string fileNameFake = "TestFile.jpg";
            string filePath = Application.streamingAssetsPath + "/" + fileName;

            Texture2D tx = new Texture2D(512, 512);
            File.WriteAllBytes(filePath, tx.EncodeToPNG());

            ResourceManager manager = new GameObject(nameof(ResourceManager)).AddComponent<ResourceManager>();
            var task = manager.loadTexture(fileNameFake, RuntimePlatform.WindowsPlayer);
            yield return new WaitUntil(() => task.IsCompleted);

            File.Delete(filePath);
        }
    }
}