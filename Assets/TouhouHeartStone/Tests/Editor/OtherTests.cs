using NUnit.Framework;
using System.IO;
using UnityEngine;
using ExcelLibrary.SpreadSheet;
using Game;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Mono.Data.Sqlite;
using TouhouHeartstone;
using BJSYGameCore;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using UnityEditor;
using System.Reflection;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
namespace Tests
{
    public class OtherTests
    {
        [Test]
        public void funcDelegateTest()
        {
            DelegateObject obj = new DelegateObject();
            obj.func += () =>
            {
                Debug.Log("1=>true");
                return true;
            };
            obj.func += () =>
            {
                Debug.Log("2=>false");
                return false;
            };
            //obj.func += () =>
            //{
            //    Debug.Log("3=>true");
            //    return true;
            //};
            Assert.False(obj.invoke());
        }
        [Test]
        public void multiDelegateRegisterTest()
        {
            DelegateObject obj = new DelegateObject();
            int i = 0;
            obj.func += onFunc;
            obj.func += onFunc;
            bool onFunc()
            {
                i++;
                Debug.Log(i);
                return true;
            }
            obj.invoke();
            Assert.AreEqual(1, i);
        }
        class DelegateObject
        {
            List<Func<bool>> _list = new List<Func<bool>>();
            public event Func<bool> func
            {
                add
                {
                    if (value != null && !_list.Contains(value))
                        _list.Add(value);
                    else
                        Debug.Log("防止重复注册" + value.Method.Name);
                }
                remove
                {
                    _list.Remove(value);
                }
            }
            public bool invoke()
            {
                bool result = false;
                foreach (var func in _list)
                {
                    if (func != null)
                        result = func.Invoke();
                }
                return result;
            }
        }
        [Test]
        public void flagTest()
        {
            PileFlag flag = PileFlag.self | PileFlag.hand | PileFlag.field;
            Assert.True(flag.HasFlag(PileFlag.self));
            Assert.False(flag.HasFlag(PileFlag.oppo));
            Assert.True(flag.HasFlag(PileFlag.hand));
            Assert.True(flag.HasFlag(PileFlag.field));
            flag |= PileFlag.both;
            Assert.True(flag.HasFlag(PileFlag.oppo));
        }
        [Test]
        public void sqliteTest()
        {
            IGensoukyo.Utilities.SQLiteDB db = new IGensoukyo.Utilities.SQLiteDB();
            db.Connect(":memory:");

            using (var cmd = db.CreateCommand(@"
            CREATE TABLE IF NOT EXISTS circle(
                circleID INTEGER PRIMARY KEY AUTOINCREMENT,
                name VARCHAR(64),
                altname VARCHAR(64) NULL
            );"))
            {
                cmd.ExecuteNonQuery();
            }

            using (var cmd = db.CreateCommand(@"insert into circle(name, altname) values(?,?)", null, "test1", "test2"))
            {
                cmd.ExecuteNonQuery();
            }

            using (var cmd = db.CreateCommand(@"select name,altname from circle"))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.NextResult())
                    {
                        Assert.AreEqual("test1", reader.GetString(reader.GetOrdinal("name")));
                        Assert.AreEqual("test2", reader.GetString(reader.GetOrdinal("altname")));
                    }
                }
            }
        }
        [Test]
        public void skipUntilTest()
        {
            int[] array = new int[]
            {
                1,2,1,2,3,2,4,2
            };
            var e = array.skipUntil(i => i == 3);
            Assert.True(e.Contains(3));
        }
        [UnityTest]
        public IEnumerator cancleTest()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            int count = 0;
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    count++;
                    Debug.Log(count);
                }
            }, cts.Token);
            yield return new WaitForSeconds(5);
            cts.Cancel();
            yield return new WaitForSeconds(5);
            Debug.Log("结果：" + count);
        }
        [Test]
        public void ofTypeTest()
        {
            Parent[] parents = new Parent[]
            {
                new Parent(),new Parent(),new Child(),new Child()
            };
            Assert.AreEqual(2, parents.OfType<IStudent>().Count());
        }
        class Parent { }
        class Child : Parent, IStudent { }
        interface IStudent { }
        [Test]
        public void jsonTest()
        {
            JsonTarget target = new JsonTarget() { i = 1 };
            string json = target.ToJson();
            Debug.Log(json);
            //object obj = BsonSerializer.Deserialize(json);
            //Debug.Log(obj + ",Type:" + obj.GetType().Name);
            //Assert.True(obj is JsonTarget t && t.i == 1);
        }
        class JsonTarget
        {
            public int i;
        }
        [Test]
        public void regexReplaceTest()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("damage", 2);
            string input = "对你的英雄造成{dic:damage}点伤害";
            string result = Regex.Replace(input, @"{(?<obj>\w+):(?<name>.+)}", m =>
            {
                string obj = m.Groups["obj"].Value;
                string name = m.Groups["name"].Value;
                if (obj == "dic")
                    return dic[name].ToString();
                else
                    return "???";
            });
            Assert.AreEqual("对你的英雄造成2点伤害", result);
        }
        [Test]
        public void resourceSpriteLoadTest()
        {
            var sprites = Resources.LoadAll<Sprite>("Textures/Patchouli/淡黄阵风");
            Assert.AreEqual(2, sprites.Length);
            Assert.True(sprites.Any(s => s.name == "淡黄阵风_Token"));
        }
        [Test]
        public void arrayTest()
        {
            string[] array = new string[] { "1", "2", "3" };
            object obj = array;
            Assert.True(obj is IEnumerable);
            Assert.True(obj is IEnumerable<string>);
            Assert.True(obj is Array);
            IEnumerable c = obj as IEnumerable;
            foreach (var e in c)
            {
                Debug.Log(e);
            }
        }
        [Test]
        public void keyTest()
        {
            using (FileStream stream = new FileStream("Keys.txt", FileMode.OpenOrCreate))
            {
                StreamReader reader = new StreamReader(stream);
                HashSet<string> keySet = new HashSet<string>();
                string key = null;
                do
                {
                    key = reader.ReadLine()?.Trim();
                    if (!string.IsNullOrEmpty(key))
                    {
                        Debug.Log(key);
                        keySet.Add(key);
                    }
                    else
                        break;
                }
                while (true);
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine();
                foreach (var newKey in KeyGeneration.generateUniqueKeys(100, 6, existingKeys: keySet))
                {
                    writer.WriteLine(newKey);
                }
                writer.Flush();
            }
        }
        [Test]
        public void codeVersionTest()
        {
            Debug.Log(GetType().Assembly.GetName().Version);
        }
        [Test]
        public void getTypeTest()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Debug.Log("Assembly:" + assembly.FullName);
            }
            foreach (var type in typeof(Room).Assembly.GetTypes())
            {
                Debug.Log("Type:" + type.FullName);
            }
        }
        [Test]
        public async void asyncTest()
        {
            Debug.Log(Time.time);
            await Task.Delay(3000);
            Debug.Log(Time.time);
        }
        [Test]
        public void overwriteColTest()
        {
            Worksheet sheetA = new Worksheet("A");
            Worksheet sheetB = new Worksheet("B");
            sheetA.Cells[0, 0] = new Cell("ID");
            sheetA.Cells[0, 1] = new Cell("Name");
            sheetA.Cells[0, 2] = new Cell("Effect");
            sheetA.Cells[1, 0] = new Cell(1);
            sheetA.Cells[1, 1] = new Cell("CardA");
            sheetA.Cells[1, 2] = new Cell("XXX");
            sheetA.Cells[3, 0] = new Cell(3);
            sheetA.Cells[3, 1] = new Cell("CardB");
            sheetA.Cells[3, 2] = new Cell("YYY");

            sheetB.Cells[0, 0] = new Cell("ID");
            sheetB.Cells[1, 0] = new Cell(1);
            ExcelHelper.migrateCols(sheetA, sheetB, new string[]
            {
                "Name"
            });
            //旧的数据被删除
            Assert.AreEqual("Effect", sheetA.Cells[0, 1].Value);
            Assert.AreEqual("XXX", sheetA.Cells[1, 1].Value);
            Assert.AreEqual("YYY", sheetA.Cells[3, 1].Value);
            //新的数据增加
            Assert.AreEqual("Name", sheetB.Cells[0, 1].Value);
            Assert.AreEqual("CardA", sheetB.Cells[1, 1].Value);
            Assert.AreEqual(3, sheetB.Cells[2, 0].Value);
            Assert.AreEqual("CardB", sheetB.Cells[2, 1].Value);
        }
        [Test]
        public void saveJPGTest()
        {
            Texture2D texture = Resources.Load<Texture2D>("TestPicture");
            byte[] bytes = texture.EncodeToJPG();
            if (!Directory.Exists("Tests"))
            {
                Directory.CreateDirectory("Tests");
            }
            using (Stream stream = File.Create("Tests/TestPicture.jpg"))
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(bytes);
                writer.Flush();
            }
            using (Stream stream = File.Open("Tests/TestPicture.jpg", FileMode.Open))
            {
                BinaryReader reader = new BinaryReader(stream);
                byte[] savedBytes = reader.ReadBytes((int)stream.Length);
                texture.LoadImage(savedBytes);
                Assert.AreEqual(bytes.Length, savedBytes.Length);
                for (int i = 0; i < savedBytes.Length; i++)
                {
                    Assert.AreEqual(bytes[i], savedBytes[i]);
                }
            }
        }
    }
}