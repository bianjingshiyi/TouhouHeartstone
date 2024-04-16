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
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using UnityEditor;
using System.Reflection;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using Game.Replays;
using Controllers;
using MessagePack;

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
        [Test]
        public void serializeInterfaceTest()
        {
            SerializableReplayData serializableReplayData = new(new ReplayData()
            {
                option = HearthstoneOption.Default,
                version = 0,
                playerAId = 1,
                playerBId = 2,
                players = new ReplayPlayerData[0],
                responses = new()
                {
                    new UseResponse() {  playerId = 1, cardId = 100 },
                    new AttackResponse(){ playerId = 1, cardId = 100, targetId = 200 }
                }
            });
            byte[] bytes = JsonHelper.toBytes(serializableReplayData);
            serializableReplayData = JsonHelper.fromBytes<SerializableReplayData>(bytes);
            Assert.IsInstanceOf<UseResponse>(serializableReplayData.responseList[0]);
            Assert.IsInstanceOf<AttackResponse>(serializableReplayData.responseList[1]);
        }
        [Test]
        public void serializeAbstractTest()
        {
            SerializableBuffDefine serializableBuffDefine = new()
            {
                propModifierList = new()
                {
                    new SerializableAttackModifier()
                    {
                        defaultValue = 1,
                        isSet  =true,
                        propertyName = "attack"
                    }
                }
            };
            byte[] bytes = JsonHelper.toBytes(serializableBuffDefine);
            serializableBuffDefine = JsonHelper.fromBytes<SerializableBuffDefine>(bytes);
            Assert.IsInstanceOf<SerializableAttackModifier>(serializableBuffDefine.propModifierList[0]);
        }
        [Test]
        public void readSerializedStreamHeaderTest()
        {
            SerializableContent content = new()
            {
                header = new() { title = "Title" },
                body = new() { bytes = new byte[] { 0, 1, 1, 1, 1, 1, 1 } }
            };
            using MemoryStream stream = new();
            MessagePackSerializer.Serialize(stream, content);
            stream.Position = 0;
            stream.ReadByte();
            SerializableHeader header = MessagePackSerializer.Deserialize<SerializableHeader>(stream);
            Assert.AreEqual("Title", header.title);
        }
        [MessagePackObject]
        public struct SerializableContent
        {
            [Key(0)]
            public SerializableHeader header;
            [Key(1)]
            public SerializableBody body;
        }
        [MessagePackObject]
        public class SerializableHeader
        {
            [Key(0)]
            public string title;
        }
        [MessagePackObject]
        public class SerializableBody
        {
            [Key(0)]
            public byte[] bytes;
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
            foreach (var type in typeof(RoomData).Assembly.GetTypes())
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