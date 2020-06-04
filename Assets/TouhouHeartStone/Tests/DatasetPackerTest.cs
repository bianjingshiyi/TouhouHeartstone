using NUnit.Framework;
using IGensoukyo;
using System.Data;
using System.IO;

namespace Tests
{
    class DatasetPackerTest
    {
        [Test]
        public void PackTest()
        {
            const string excelFile = "Cards/Cards.xls";
            const string outFile = excelFile + ".dataset";

            ExcelDataSetPacker.PackExcelToDataSet(excelFile, outFile);
            DataSet set = ExcelDataSetPacker.ReadDataset(outFile);

            Assert.NotNull(set);
            Assert.True(set.Tables.Count > 0);

            File.Delete(outFile);
        }
    }
}
