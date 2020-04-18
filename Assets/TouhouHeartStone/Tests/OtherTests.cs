using NUnit.Framework;
using System.IO;
using UnityEngine;
using ExcelLibrary.SpreadSheet;
namespace Tests
{
    public class OtherTests
    {
        [Test]
        public void excelTest()
        {
            using (FileStream stream = new FileStream("Assets/TouhouHeartStone/Tests/Test.xls", FileMode.Open))
            {
                Workbook workbook = Workbook.Load(stream);

                Debug.Log(workbook.Worksheets[0].Cells[0, 0].Value.GetType().FullName);
                Assert.AreEqual(1, workbook.Worksheets[0].Cells[0, 0].Value);
                Debug.Log(workbook.Worksheets[0].Cells[0, 1].Value.GetType().FullName);
                Assert.AreEqual(1.2f, (float)(double)workbook.Worksheets[0].Cells[0, 1].Value);
                Debug.Log(workbook.Worksheets[0].Cells[0, 2].Value.GetType().FullName);
                Assert.AreEqual(true, workbook.Worksheets[0].Cells[0, 2].Value);
                Debug.Log(workbook.Worksheets[0].Cells[0, 3].Value.GetType().FullName);
                Assert.AreEqual("string", workbook.Worksheets[0].Cells[0, 3].Value);
            }
        }
    }
}