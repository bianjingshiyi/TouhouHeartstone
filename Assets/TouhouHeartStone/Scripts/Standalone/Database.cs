using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace IGensoukyo.Utilities
{
    public abstract class SqlDatabase
    {
        protected string tablePrefix;
        /// <summary>
        /// 表前缀
        /// </summary>
        public string TablePrefix => tablePrefix;

        /// <summary>
        /// 连接到某个数据库
        /// </summary>
        /// <param name="uri"></param>
        public abstract void Connect(string uri);

        /// <summary>
        /// 设置表前缀并连接到某个数据库
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="prefix"></param>
        public virtual void Connect(string uri, string prefix)
        {
            tablePrefix = prefix;
            Connect(uri);
        }

        /// <summary>
        /// 创建一个查询
        /// </summary>
        /// <param name="sql">查询</param>
        /// <param name="tr">对应事务</param>
        /// <returns></returns>
        public abstract DbCommand CreateCommand(string sql, DbTransaction tr = null);

        /// <summary>
        /// 创建一个匿名参数查询
        /// </summary>
        /// <param name="sql">查询</param>
        /// <param name="tr">事务</param>
        /// <param name="objs">顺序参数</param>
        /// <returns></returns>
        public virtual DbCommand CreateCommand(string sql, DbTransaction tr = null, params object[] objs)
        {
            var cmd = CreateCommand(sql);
            for (int i = 0; i < objs.Length; i++)
            {
                var p = cmd.CreateParameter();
                p.SourceColumn = "?";
                p.SourceColumnNullMapping = false;
                p.Value = objs[i];
                cmd.Parameters.Add(p);
            }
            return cmd;
        }

        /// <summary>
        /// 创建一个参数查询
        /// </summary>
        /// <typeparam name="T">任意类型T</typeparam>
        /// <param name="sql">查询</param>
        /// <param name="data">类型T的数据</param>
        /// <param name="tr">事务</param>
        /// <param name="nullable">是否可空</param>
        /// <returns></returns>
        public virtual DbCommand CreateCommand<T>(string sql, T data, DbTransaction tr = null, bool nullable = true) where T : struct
        {
            var cmd = CreateCommand(sql, tr);
            var type = typeof(T);

            foreach (var item in type.GetFields())
            {
                var p = cmd.CreateParameter();
                p.ParameterName = $"@{item.Name}";
                p.Value = item.GetValue(data);
                cmd.Parameters.Add(p);
            }
            return cmd;
        }

        public abstract void Close();

        public abstract DbTransaction BeginTransation();

        public abstract Task<long> GetLastInsertIDAsync(string table);


        public abstract bool IsConnected { get; }

        public abstract long GetLastInsertID(string table);

        public delegate bool EffectRowCallback(int row);

        /// <summary>
        /// 执行数据库
        /// </summary>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task ExecuteAsync(string query, EffectRowCallback callback, params object[] args)
        {
            using (var tr = BeginTransation())
            {
                using (var cmd = CreateCommand(query, tr, args))
                {
                    var result = await cmd.ExecuteNonQueryAsync();
                    try
                    {
                        if (callback != null && !callback(result))
                        {
                            tr.Rollback();
                        }
                        else
                        {
                            tr.Commit();
                        }
                    }
                    catch
                    {
                        tr.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// 执行数据库
        /// </summary>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public void Execute(string query, EffectRowCallback callback, params object[] args)
        {
            using (var tr = BeginTransation())
            {
                using (var cmd = CreateCommand(query, tr, args))
                {
                    var result = cmd.ExecuteNonQuery();
                    try
                    {
                        if (callback != null && !callback(result))
                        {
                            tr.Rollback();
                        }
                        else
                        {
                            tr.Commit();
                        }
                    }
                    catch
                    {
                        tr.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// 执行数据库
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <param name="arg"></param>
        /// <param name="nullable"></param>
        /// <returns></returns>
        public async Task ExecuteAsync<T>(string query, EffectRowCallback callback, T arg, bool nullable = true) where T : struct
        {
            using (var tr = BeginTransation())
            {
                using (var cmd = CreateCommand(query, arg, tr, nullable))
                {
                    var result = await cmd.ExecuteNonQueryAsync();
                    try
                    {
                        if (callback != null && !callback(result))
                        {
                            tr.Rollback();
                        }
                        else
                        {
                            tr.Commit();
                        }
                    }
                    catch
                    {
                        tr.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// 执行数据库
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <param name="arg"></param>
        /// <param name="nullable"></param>
        /// <returns></returns>
        public void Execute<T>(string query, EffectRowCallback callback, T arg, bool nullable = true) where T : struct
        {
            using (var tr = BeginTransation())
            {
                using (var cmd = CreateCommand(query, arg, tr, nullable))
                {
                    var result = cmd.ExecuteNonQuery();
                    try
                    {
                        if (callback != null && !callback(result))
                        {
                            tr.Rollback();
                        }
                        else
                        {
                            tr.Commit();
                        }
                    }
                    catch
                    {
                        tr.Rollback();
                    }
                }
            }
        }

        public delegate Task<object> ReaderCallbackAsync(System.Data.Common.DbDataReader reader);
        public delegate object ReaderCallback(System.Data.Common.DbDataReader reader);

        /// <summary>
        /// 异步读取数据库
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<T> ReadAsync<T>(string query, ReaderCallbackAsync callback, params object[] args)
        {
            using (var cmd = CreateCommand(query, null, args))
            {
                using (var result = await cmd.ExecuteReaderAsync())
                {
                    return (T)await callback(result);
                }
            }
        }

        /// <summary>
        /// 同步读取数据库
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public T Read<T>(string query, ReaderCallback callback, params object[] args)
        {
            using (var cmd = CreateCommand(query, null, args))
            {
                using (var result = cmd.ExecuteReader())
                {
                    return (T)callback(result);
                }
            }
        }

        /// <summary>
        /// 读取数据库
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<T> ReadAsync<T>(string query, ReaderCallback callback, params object[] args)
        {
            using (var cmd = CreateCommand(query, null, args))
            {
                using (var result = await cmd.ExecuteReaderAsync())
                {
                    return (T)callback(result);
                }
            }
        }
    }

    public static class DBReaderHelper
    {
        /// <summary>
        /// 获取指定名字的列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(this System.Data.Common.DbDataReader reader, string key)
        {
            var ordinal = reader.GetOrdinal(key);
            return reader.GetFieldValue<T>(ordinal);
        }
        /// <summary>
        /// 获取指定名字的列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(this System.Data.Common.DbDataReader reader, string key)
        {
            var ordinal = reader.GetOrdinal(key);
            return await reader.GetFieldValueAsync<T>(ordinal);
        }
        /// <summary>
        /// 解析整行数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T GetRow<T>(this System.Data.Common.DbDataReader reader) where T : new()
        {
            var fields = typeof(T).GetFields();
            T t = new T();
            object boxT = t;

            foreach (var item in fields)
            {
                var name = item.Name;
                var attrs = item.GetCustomAttributes(typeof(ColNameAttribute), false);
                if (attrs.Length > 0)
                    name = (attrs[0] as ColNameAttribute).ColName;

                try
                {
                    var ordinal = reader.GetOrdinal(name);
                    var valObj = reader.GetValue(ordinal);
                    object valTo;

                    if (valObj is DBNull)
                    {
                        if (!item.FieldType.IsValueType)
                        {
                            item.SetValue(boxT, null);
                        }
                    }
                    else
                    {
                        if (item.FieldType.IsEnum)
                            valTo = Convert.ChangeType(valObj, typeof(int));
                        else
                            valTo = Convert.ChangeType(valObj, item.FieldType);

                        item.SetValue(boxT, valTo);
                    }
                }
                catch (IndexOutOfRangeException) { }
            }
            return (T)boxT;
        }
    }

    [System.AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    sealed class ColNameAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly string colName;

        // This is a positional argument
        public ColNameAttribute(string colName)
        {
            this.colName = colName;
        }

        public string ColName => colName;
    }
}
