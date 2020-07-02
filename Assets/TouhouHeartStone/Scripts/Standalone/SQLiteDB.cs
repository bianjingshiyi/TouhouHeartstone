using System.Data.Common;
using Mono.Data.Sqlite;
using System.IO;

namespace IGensoukyo.Utilities
{
    public class SQLiteDB : SqlDatabase
    {
        SqliteConnection connection;

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

        public override DbTransaction BeginTransition()
        {
            return connection.BeginTransaction();
        }
    }


}
