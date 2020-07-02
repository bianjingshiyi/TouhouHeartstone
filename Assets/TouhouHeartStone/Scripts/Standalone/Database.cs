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

        public abstract DbTransaction BeginTransition();
    }
}
