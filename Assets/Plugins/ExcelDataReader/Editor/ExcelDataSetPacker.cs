using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomBuilder;
using UnityEngine;
using ExcelDataReader;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;
using UnityEditor;

namespace IGensoukyo
{
    public class ExcelDataSetPacker
    {
        static public void PackExcelToDataSet(string pathIn, string pathOut)
        {
            using (var stream = File.Open(Path.Combine(Application.streamingAssetsPath, pathIn), FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            // 使用第一行的内容作为列索引
                            // 其他选项的说明见Github的ReadMe
                            UseHeaderRow = true,
                        }
                    });
                    using (var writeStream = File.Open(Path.Combine(Application.streamingAssetsPath, pathOut), FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(writeStream, result);
                    }
                }
            }
        }

        [ExecuteBeforeBuild]
        [MenuItem("Custom/Data/转换所有Excel")]
        static public void PackAll()
        {
            var configs = ReadConfigs();
            foreach (var item in configs)
            {
                PackExcelToDataSet(item.ExcelFile, item.DataSetFile);
            }
        }

        static public DataSet ReadDataset(string path)
        {
            using (var stream = File.Open(Path.Combine(Application.streamingAssetsPath, path), FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return bf.Deserialize(stream) as DataSet;
            }
        }

        const string ConfigPath = "ExcelDataSetPacker.txt";
        static public PackerConfig[] ReadConfigs()
        {
            List<PackerConfig> configs = new List<PackerConfig>();
            if (File.Exists(ConfigPath))
            {
                var lines = File.ReadAllLines(ConfigPath);
                foreach (var line in lines)
                {
                    if (line.Length > 0 && !line.StartsWith("#"))
                    {
                        try
                        {
                            var strs = line.Split('|');
                            if (strs.Length == 2)
                            {
                                configs.Add(new PackerConfig() { ExcelFile = strs[0].Trim(), DataSetFile = strs[1].Trim() });
                            }
                        }
                        catch { }
                    }
                }
            }
            return configs.ToArray();
        }

        static public void WriteConfigs(PackerConfig[] configs)
        {
            string result = "# ExcelDataSetPacker 打包配置文件 \n\n";
            foreach (var config in configs)
            {
                result += $"{config.ExcelFile} | {config.DataSetFile}\n";
            }
            File.WriteAllText(ConfigPath, result);
        }
    }

    public class PackerConfig
    {
        public string ExcelFile;
        public string DataSetFile;
    }
}
