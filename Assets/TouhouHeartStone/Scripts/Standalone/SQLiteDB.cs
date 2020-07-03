using System.Data.Common;
using Mono.Data.Sqlite;
using System.IO;
using System;
using System.Threading.Tasks;

namespace IGensoukyo.Utilities
{
    public class SQLiteDB : SqlDatabase
    {
        SqliteConnection connection;

        public override bool IsConnected => connection != null && connection.State == System.Data.ConnectionState.Open;

        public override void Close()
        {
            connection.Close();
        }

        public override void Connect(string path)
        {
            if (path != ":memory:" && !File.Exists(path))
            {
                SqliteConnection.CreateFile(path);
            }

            connection = new SqliteConnection($"Data source={path}");
            connection.Open();
        }

        public override DbCommand CreateCommand(string sql, DbTransaction tr = null)
        {
            if (tr != null)
                return new SqliteCommand(sql, connection, tr as SqliteTransaction);
            else
                return new SqliteCommand(sql, connection);
        }

        public override DbCommand CreateCommand(string sql, DbTransaction tr = null, params object[] objs)
        {
            SqliteCommand cmd = CreateCommand(sql, tr) as SqliteCommand;
            for (int i = 0; i < objs.Length; i++)
            {
                cmd.Parameters.AddWithValue($"?{i}", objs[i]);
            }
            return cmd;
        }

        public override DbTransaction BeginTransation()
        {
            return connection.BeginTransaction();
        }

        public override async Task<long> GetLastInsertIDAsync(string table)
        {
            return await ReadAsync<Int64>($"SELECT last_insert_rowid() as rowid FROM {table}", async (r) =>
            {
                await r.ReadAsync();
                return r.Get<Int64>("rowid");
            });
        }

        public override long GetLastInsertID(string table)
        {
            return Read<Int64>($"SELECT last_insert_rowid() as rowid FROM {table}", (r) =>
            {
                r.Read();
                return r.Get<Int64>("rowid");
            });
        }
    }
}
