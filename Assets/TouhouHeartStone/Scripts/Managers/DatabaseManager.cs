using System.IO;
using UnityEngine;
using BJSYGameCore;
using IGensoukyo.Utilities;

namespace Game
{
    public class DatabaseManager : Manager
    {
        [SerializeField]
        string dbFileName = "data.db";

        SQLiteDB db = new SQLiteDB();

        public SqlDatabase Database => db;

        public void init()
        {
            if (!db.IsConnected)
                db.Connect(Path.Combine(Application.persistentDataPath, dbFileName));
        }

        /// <summary>
        /// 连接到内存数据库
        /// </summary>
        public void ConnectToMemory()
        {
            db.Connect(":memory:");
        }
    }
}
