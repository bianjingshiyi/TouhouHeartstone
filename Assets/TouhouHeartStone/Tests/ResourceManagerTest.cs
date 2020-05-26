using NUnit.Framework;
namespace Tests
{
    public class ResourceManagerTest
    {
        //Excel测试资源为StreamgingAssets/TestExcel.xls。
        //不过我觉得把它删了，每次测试动态新建一个测试Excel表格然后删除好像也是可以的。
        /// <summary>
        /// Win加载Excel，预期得到一个非空Workbook，可以得到位于Cells[1,1]的测试内容"TestContent"
        /// </summary>
        [Test]
        public void loadExcel()
        {
        }
        /// <summary>
        /// 当发生异常，抛出加载过程中发生的异常，此处测试尝试加载不存在的Excel，应当抛出FileNotFound
        /// </summary>
        [Test]
        public void loadExcel_Failed()
        {
        }
        /// <summary>
        /// Android使用WebRequest加载Excel
        /// </summary>
        [Test]
        public void loadExcel_Android()
        {
        }
        /// <summary>
        /// Android加载不存在的Excel抛出异常，需要研究一下WebRequest的异常机制。
        /// </summary>
        [Test]
        public void loadExcel_Android_Failed()
        {
        }
        /// <summary>
        /// 同上，测试用贴图建议放在Test/Resources中，然后在测试的时候在StreamingAssets中新建并复制贴图，用于测试，测完了删掉。
        /// </summary>
        [Test]
        public void loadTexture()
        {
        }
        [Test]
        public void loadTexture_Failed()
        {
        }
        [Test]
        public void loadTexture_Android()
        {
        }
        [Test]
        public void loadTexture_Android_Failed()
        {
        }
    }
}