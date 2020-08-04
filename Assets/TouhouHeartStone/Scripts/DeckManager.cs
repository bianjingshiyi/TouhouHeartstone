using BJSYGameCore;
using IGensoukyo.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using System;

namespace Game
{
    public class DeckManager : Manager
    {
        public SqlDatabase db => getManager<DatabaseManager>().Database;

        DbTransaction tr = null;

        const string tableDeck = "deck";
        const string tableCards = "deck_cards";


        protected override void onAwake()
        {
            base.onAwake();

            db.Execute($@"
            CREATE TABLE IF NOT EXISTS {tableDeck}(
                deckID INTEGER PRIMARY KEY AUTOINCREMENT,
                name VARCHAR(64),
                masterID INTEGER
            );", null);

            // 数据库更新定义
            var hasRow = db.Read<bool>($@"SELECT sql FROM sqlite_master WHERE type='table' AND name='{tableDeck}';",
            (r) =>
            {
                if (r.HasRows && r.Read())
                {
                    return r.Get<string>("sql").Contains("masterID");
                }
                return false;
            });

            if (!hasRow)
                db.Execute($@"ALTER TABLE {tableDeck} ADD COLUMN masterID INTEGER;", null);
            

            db.Execute($@"
            CREATE TABLE IF NOT EXISTS {tableCards}(
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                deckID INTEGER REFERENCES deck(deckID) ,
                cardID INTEGER
            );", null);
        }

        /// <summary>
        /// 创建一个牌组
        /// </summary>
        /// <param name="name">牌组名称</param>
        /// <returns>新的牌组的ID</returns>
        public long CreateDeck(string name, int masterID)
        {
            db.Execute($@"INSERT INTO {tableDeck} (name, masterID) VALUES (?,?)", null, name, masterID);
            return db.GetLastInsertID(tableDeck);
        }

        /// <summary>
        /// 获取牌组的MasterCardID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetDeckMasterID(long id)
        {
            return db.Read<int>($@"SELECT masterID FROM {tableDeck} WHERE deckID = ?", (r) =>
            {
                if (r.HasRows)
                {
                    r.Read();
                    return (int)r.Get<long>("masterID");
                }
                return null;
            }, id);
        }

        /// <summary>
        /// 设置牌组的MasterCardID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="masterID"></param>
        public void SetDeckMasterID(long id, int masterID)
        {
            db.Execute($@"UPDATE {tableDeck} SET masterID = ? WHERE deckID = ?", null, masterID, id);
        }

        /// <summary>
        /// 获取牌组名称
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetDeckName(long id)
        {
            return db.Read<string>($@"SELECT name FROM {tableDeck} WHERE deckID = ?", (r) =>
            {
                if (r.HasRows)
                {
                    r.Read();
                    return r.Get<string>("name");
                }

                return null;
            }, id);
        }

        /// <summary>
        /// 重命名一个牌组
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public void RenameDeck(long id, string name)
        {
            db.Execute($@"UPDATE {tableDeck} SET name = ? WHERE deckID = ?", null, name, id);
        }

        /// <summary>
        /// 移除一个牌组
        /// </summary>
        /// <param name="id"></param>
        public void RemoveDeck(long id)
        {
            ClearDeck(id);
            db.Execute($@"DELETE FROM {tableDeck} WHERE deckID = ?", null, args: id); // 然后清理卡组定义
        }

        /// <summary>
        /// 删除一个牌组里面的所有的牌
        /// </summary>
        /// <param name="id"></param>
        public void ClearDeck(long id)
        {
            db.Execute($@"DELETE FROM {tableCards} WHERE deckID = ?", null, args: id); // 先清理卡定义
        }

        /// <summary>
        /// 获取所有牌组定义
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DeckDefine> GetDecks()
        {
            return db.Read<IEnumerable<DeckDefine>>($@"SELECT * FROM {tableDeck}", (r) =>
            {
                List<DeckDefine> list = new List<DeckDefine>();
                if (r.HasRows)
                {
                    while (r.Read())
                    {
                        list.Add(new DeckDefine()
                        {
                            ID = r.Get<long>("deckID"),
                            MasterID = (int)r.Get<long>("masterID"),
                            Name = r.Get<string>("name")
                        });
                    }
                }
                return list;
            });
        }

        /// <summary>
        /// 获取牌组定义
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DeckDefine GetDeck(long id)
        {
            return db.Read<DeckDefine>($@"SELECT * FROM {tableDeck} WHERE deckID = ?", (r) =>
            {
                if (!r.HasRows) return null;
                r.Read();
                return new DeckDefine()
                {
                    ID = r.Get<long>("deckID"),
                    MasterID = (int)r.Get<long>("masterID"),
                    Name = r.Get<string>("name")
                };
            }, id);
        }

        /// <summary>
        /// 将指定的卡插入卡组中
        /// </summary>
        /// <param name="deckID">卡组ID</param>
        /// <param name="cardID">卡片ID</param>
        /// <returns>卡片引用ID</returns>
        public long AddCard(long deckID, long cardID)
        {
            string query = $@"INSERT INTO {tableCards} (deckID, cardID) VALUES (?,?)";

            if (tr == null)
            {
                db.Execute(query, null, deckID, cardID);
            }
            else
            {
                using (var cmd = db.CreateCommand(query, tr, deckID, cardID))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            return db.GetLastInsertID(tableCards);
        }

        /// <summary>
        /// 获取卡片定义ID
        /// </summary>
        /// <param name="id">卡片引用ID</param>
        /// <returns>卡片定义ID</returns>
        public long GetCardID(long id)
        {
            return db.Read<Int64>($@"SELECT cardID FROM {tableCards} WHERE ID = ?", (r) =>
            {
                if (r.HasRows)
                {
                    r.Read();
                    return r.Get<Int64>("cardID");
                }
                return (Int64)(-1);
            }, id);
        }

        /// <summary>
        /// 获取卡组内所有的卡片
        /// </summary>
        /// <param name="deckID">卡组ID</param>
        /// <returns>引用ID-定义ID 键值对</returns>
        public Dictionary<long, long> GetDeckCards(long deckID)
        {
            return db.Read<Dictionary<long, long>>($@"SELECT cardID, ID FROM {tableCards} WHERE deckID = ?", (r) =>
            {
                Dictionary<long, long> dict = new Dictionary<long, long>();
                if (r.HasRows)
                {
                    while (r.Read())
                        dict.Add(r.Get<long>("ID"), r.Get<long>("cardID"));
                }

                return dict;
            }, deckID);
        }

        /// <summary>
        /// 移除一张卡片
        /// </summary>
        /// <param name="id">卡片引用ID</param>
        public void RemoveCard(long id)
        {
            string query = $@"DELETE FROM {tableCards} WHERE ID = ?";

            if (tr == null)
            {
                db.Execute(query, null, args: id);
            }
            else
            {
                using (var cmd = db.CreateCommand(query, tr, id))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 进入事务。
        /// 进入事务后，对**卡片**的修改不会立即写入数据库，需要手工提交或回滚事务
        /// </summary>
        public void EnterTransaction()
        {
            if (tr != null)
            {
                tr.Rollback();
                tr = null;
            }

            tr = db.BeginTransation();
        }

        /// <summary>
        /// 回滚变更
        /// 回滚后会退出事务
        /// </summary>
        public void Rollback()
        {
            if (tr != null)
            {
                tr.Rollback();
                tr = null;
            }
        }

        /// <summary>
        /// 提交变更
        /// 提交变更后会退出事务
        /// </summary>
        public void Commit()
        {
            if (tr != null)
            {
                tr.Commit();
                tr = null;
            }
        }

        /// <summary>
        /// 是否处于事务中
        /// </summary>
        public bool IsInTransation => tr != null;
    }

    public struct DeckDefine
    {
        public long ID;
        public string Name;
        public int MasterID;
    }
}
