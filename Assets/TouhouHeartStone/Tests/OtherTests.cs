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
    }
}