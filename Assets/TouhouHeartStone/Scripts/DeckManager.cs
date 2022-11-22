using BJSYGameCore;
using IGensoukyo.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using System;
using System.IO.Compression;
using System.IO;
using TouhouCardEngine;

namespace Game
{
    public partial class DeckManager : Manager
    {
        #region DB
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
            if (!db.Read<bool>($@"SELECT sql FROM sqlite_master WHERE type='table' AND name='{tableDeck}';", r =>
            {
                if (r.HasRows && r.Read())
                {
                    return r.Get<string>("sql").Contains("masterID");
                }
                return false;
            }))
            {
                db.Execute($@"ALTER TABLE {tableDeck} ADD COLUMN masterID INTEGER;", null);
            }
            if (!db.Read<bool>($@"SELECT sql FROM sqlite_master WHERE type='table' AND name='{tableDeck}';", r =>
            {
                if (r.HasRows && r.Read())
                {
                    return r.Get<string>("sql").Contains("masterCardPoolID");
                }
                return false;
            }))
            {
                db.Execute($@"ALTER TABLE {tableDeck} ADD COLUMN masterCardPoolID INTEGER;", null);
            }

            db.Execute($@"
            CREATE TABLE IF NOT EXISTS {tableCards}(
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                deckID INTEGER REFERENCES deck(deckID) ,
                cardID INTEGER
            );", null);

            if (!db.Read<bool>($@"SELECT sql FROM sqlite_master WHERE type='table' AND name='{tableCards}';", r =>
            {
                if (r.HasRows && r.Read())
                {
                    return r.Get<string>("sql").Contains("cardPoolID");
                }
                return false;
            }))
            {
                db.Execute($@"ALTER TABLE {tableCards} ADD COLUMN cardPoolID INTEGER;", null);
            }
        }

        /// <summary>
        /// 创建一个牌组
        /// </summary>
        /// <param name="name">牌组名称</param>
        /// <returns>新的牌组的ID</returns>
        public long CreateDeck(string name, DefineReference masterCardRef)
        {
            db.Execute($@"INSERT INTO {tableDeck} (name, masterID, masterCardPoolID) VALUES (?,?,?)", null,
                name,
                masterCardRef.id,
                masterCardRef.cardPoolId);
            return db.GetLastInsertID(tableDeck);
        }

        /// <summary>
        /// 创建一个牌组
        /// </summary>
        /// <param name="name">牌组名称</param>
        /// <returns>新的牌组的ID</returns>
        public long CreateDeck(long deckID, string name, DefineReference masterCardRef)
        {
            db.Execute($@"INSERT INTO {tableDeck} (deckID, name, masterID, masterCardPoolID) VALUES (?, ?, ?, ?)", null,
                deckID,
                name,
                masterCardRef.id,
                masterCardRef.cardPoolId);
            return db.GetLastInsertID(tableDeck);
        }

        /// <summary>
        /// 获取牌组的MasterCardID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DefineReference GetDeckMasterID(long id)
        {
            return db.Read<DefineReference>($@"SELECT masterID, masterCardPoolID FROM {tableDeck} WHERE deckID = ?", r =>
            {
                if (r.HasRows)
                {
                    r.Read();
                    return new DefineReference(r.Get<long>("masterCardPoolID"), (int)r.Get<long>("masterID"));
                }
                return null;
            }, id);
        }

        /// <summary>
        /// 设置牌组的MasterCardID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="masterCardRef"></param>
        public void SetDeckMasterID(long id, DefineReference masterCardRef)
        {
            db.Execute($@"UPDATE {tableDeck} SET masterID = ? WHERE deckID = ?", null, masterCardRef.id, id);
            db.Execute($@"UPDATE {tableDeck} SET masterCardPoolID = ? WHERE deckID = ?", null, masterCardRef.cardPoolId, id);
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
            //清理运行时卡组
            _deckList.RemoveAll(d => d.id == id);
        }

        /// <summary>
        /// 删除一个牌组里面的所有的牌
        /// </summary>
        /// <param name="id"></param>
        public void ClearDeck(long id)
        {
            // 先清理卡定义
            db.Execute($@"DELETE FROM {tableCards} WHERE deckID = ?", null, args: id);
        }

        /// <summary>
        /// 查询是否有指定ID的牌组
        /// </summary>
        /// <param name="id"></param>
        public bool HasDeck(long id)
        {
            return db.Read<bool>($@"SELECT name FROM {tableDeck} WHERE deckID = ?", (r) =>
            {
                return r.HasRows;
            }, id);
        }

        /// <summary>
        /// 获取所有卡组定义
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
                            MasterCardRef = new DefineReference(r.Get<long>("masterCardPoolID"), (int)r.Get<long>("masterID")),
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
                    MasterCardRef = new DefineReference(r.Get<long>("masterCardPoolID"), (int)r.Get<long>("masterID")),
                    Name = r.Get<string>("name")
                };
            }, id);
        }

        /// <summary>
        /// 将指定的卡插入卡组中
        /// </summary>
        /// <param name="deckID">卡组ID</param>
        /// <param name="cardRef">卡片ID</param>
        /// <returns>卡片引用ID</returns>
        public long AddCard(long deckID, DefineReference cardRef)
        {
            string query = $@"INSERT INTO {tableCards} (deckID, cardID, cardPoolID) VALUES (?, ?, ?)";

            if (tr == null)
            {
                db.Execute(query, null, deckID, cardRef.id, cardRef.cardPoolId);
            }
            else
            {
                using (var cmd = db.CreateCommand(query, tr, deckID, cardRef.id, cardRef.cardPoolId))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            long insertId = db.GetLastInsertID(tableCards);
            return insertId;
        }

        /// <summary>
        /// 获取卡片定义ID
        /// </summary>
        /// <param name="id">卡片引用ID</param>
        /// <returns>卡片定义ID</returns>
        public DefineReference GetCardRef(long id)
        {
            return db.Read<DefineReference>($@"SELECT cardPoolID, cardID FROM {tableCards} WHERE ID = ?", r =>
            {
                if (r.HasRows)
                {
                    r.Read();
                    return new DefineReference(r.Get<long>("cardPoolID"), (int)r.Get<long>("cardID"));
                }
                return null;
            }, id);
        }

        /// <summary>
        /// 获取卡组内所有的卡片
        /// </summary>
        /// <param name="deckID">卡组ID</param>
        /// <returns>引用ID-定义ID 键值对</returns>
        public Dictionary<long, DefineReference> GetDeckCards(long deckID)
        {
            return db.Read<Dictionary<long, DefineReference>>($@"SELECT cardID, ID FROM {tableCards} WHERE deckID = ?", (r) =>
            {
                Dictionary<long, DefineReference> dict = new Dictionary<long, DefineReference>();
                if (r.HasRows)
                {
                    while (r.Read())
                        dict.Add(r.Get<long>("ID"), new DefineReference(r.Get<long>("cardPoolID"), (int)r.Get<long>("cardID")));
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
        #endregion
        #region DeckCode

        const byte GzipHeaderByte1 = 0x1f;
        const byte GzipHeaderByte2 = 0x8b;

        const byte ZMHeaderByte1 = (byte)'z';
        const byte ZMHeaderByte2 = (byte)'c';

        /// <summary>
        /// Deck转String，V1版本
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        public string DeckToStringV1(int[] deck)
        {
            byte[] encByte = Deck2ByteV1(deck);
            return Convert.ToBase64String(encByte);
        }

        /// <summary>
        /// Deck转String
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        public string DeckToString(int[] deck)
        {
            return Convert.ToBase64String(Deck2ByteV2(deck));
        }
        public string DeckToString(DefineReference[] deck)
        {
            //TODO:实现该方法
            throw new NotImplementedException();
        }
        /// <summary>
        /// Deck转Byte，V1版本
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        private byte[] Deck2ByteV1(int[] deck)
        {
            byte[] dataBytes = new byte[deck.Length * sizeof(int)];

            int offset = 0;
            foreach (var item in deck)
            {
                BitConverter.GetBytes(item).CopyTo(dataBytes, offset);
                offset += 4;
            }
            byte[] encByte = gzipCompress(dataBytes);
            return encByte;
        }

        /// <summary>
        /// Deck转Byte，V2版本
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        private byte[] Deck2ByteV2(int[] deck)
        {
            var bytes = Deck2ByteV1(deck);
            var srcLen = bytes.Length;
            var dstLen = srcLen - 10;
            // 去除10byte的文件头和4byte的尾部，加上3byte的新文件头
            byte[] result = new byte[dstLen];

            result[0] = ZMHeaderByte1;
            result[1] = ZMHeaderByte2;
            result[2] = 2; // Version
            result[3] = bytes[srcLen - 4]; // Len的MSB。因为它不会大于256，所以直接使用

            Array.Copy(bytes, 10, result, 4, srcLen - 10 - 4);

            return result;
        }

        /// <summary>
        /// String转Deck，V1版本
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <exception cref="FormatException">不是完整的Base64字符串</exception>
        /// <exception cref="IOException">无法以Gzip方式解压</exception>
        public int[] StringToDeckV1(string str)
        {
            var compressdData = Convert.FromBase64String(str);
            return Byte2DeckV1(compressdData);
        }

        /// <summary>
        /// String转Deck
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <exception cref="FormatException">不是完整的Base64字符串</exception>
        /// <exception cref="IOException">无法以Gzip方式解压</exception>
        public int[] StringToDeck(string str)
        {
            var compressdData = Convert.FromBase64String(str);
            return Byte2Deck(compressdData);
        }
        public DefineReference[] StringToDeckWithCardPoolID(string str)
        {
            //TODO:实现该方法
            throw new NotImplementedException();
        }
        /// <summary>
        /// Byte转Deck
        /// </summary>
        /// <param name="compressdData"></param>
        /// <returns></returns>
        private int[] Byte2Deck(byte[] compressdData)
        {
            if (compressdData[0] == GzipHeaderByte1 && compressdData[1] == GzipHeaderByte2)
            {
                return Byte2DeckV1(compressdData);
            }
            if (compressdData[0] == ZMHeaderByte1 && compressdData[1] == ZMHeaderByte2)
            {
                // 版本号
                if (compressdData[2] == 2)
                {
                    return Byte2DeckV2(compressdData);
                }
                throw new Exception("无法识别的组卡代码版本");
            }
            throw new Exception("无法辨认的格式");
        }

        /// <summary>
        /// Byte转Deck，V1版本
        /// </summary>
        /// <param name="compressdData"></param>
        /// <exception cref="IOException">无法以Gzip方式解压</exception>
        /// <returns></returns>
        private int[] Byte2DeckV1(byte[] compressdData)
        {
            var decompressedData = gzipDecompress(compressdData);
            List<int> cards = new List<int>();
            for (int i = 0; i < decompressedData.Length; i += 4)
            {
                var card = BitConverter.ToInt32(decompressedData, i);
                cards.Add(card);
            }
            return cards.ToArray();
        }

        /// <summary>
        /// Byte转Deck，v2版本
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private int[] Byte2DeckV2(byte[] data)
        {
            var bytes = new byte[data.Length + 10];
            var i = 0;

            bytes[i++] = GzipHeaderByte1; // header, 2 bytes
            bytes[i++] = GzipHeaderByte2;
            bytes[i++] = 0x08; // CM, 8 = Deflate
            bytes[i++] = 0x00; // FLAG, disable all feature

            bytes[i++] = 0; // MTIME, 0
            bytes[i++] = 0;
            bytes[i++] = 0;
            bytes[i++] = 0;

            bytes[i++] = 2; // XFL, 2 = maximum compression
            bytes[i++] = 0; // OS, 0 = FAT system.

            Array.Copy(data, 4, bytes, 10, data.Length - 4);
            i += data.Length - 4;

            bytes[i++] = data[3]; // Length
            bytes[i++] = 0;
            bytes[i++] = 0;
            bytes[i++] = 0;

            return Byte2DeckV1(bytes);
        }

        /// <summary>
        /// 压缩字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] gzipCompress(byte[] data)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var inputStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    inputStream.Write(data, 0, data.Length);
                }
                return outputStream.ToArray();
            }
        }

        /// <summary>
        /// 解压字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] gzipDecompress(byte[] data)
        {
            using (var originalStream = new MemoryStream(data))
            {
                using (var decompressedStream = new MemoryStream())
                {
                    using (var decompressionStream = new GZipStream(originalStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedStream);
                    }
                    return decompressedStream.ToArray();
                }
            }
        }

        #endregion
    }

    public struct DeckDefine
    {
        public long ID;
        public string Name;
        public DefineReference MasterCardRef;
    }
}
