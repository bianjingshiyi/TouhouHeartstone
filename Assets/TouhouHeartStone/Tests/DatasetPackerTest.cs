using NUnit.Framework;
using IGensoukyo;
using System.Data;
using System.IO;
using UnityEngine;

namespace Tests
{
    class DatasetPackerTest
    {
        [Test]
        public void PackTest()
        {
            string xlsFileName = Path.Combine("Cards", "Test.xls");
            string xlsFile = Path.Combine(Application.streamingAssetsPath, xlsFileName);
            string dataSetFileName = Path.Combine("Cards", "Test.xls.dataset");
            string dataSetFile = Path.Combine(Application.streamingAssetsPath, dataSetFileName);

            if (!File.Exists(xlsFile))
                File.Copy("Assets\\TouhouHeartStone\\Tests\\Resources\\Cards.xls", xlsFile);

            ExcelDataSetPacker.PackExcelToDataSet(xlsFile, dataSetFile);
            DataSet set = ExcelDataSetPacker.ReadDataset(dataSetFile);

            Assert.NotNull(set);
            Assert.True(set.Tables.Count > 0);

            File.Delete(dataSetFile);
            File.Delete(xlsFile);
        }
    }
}
