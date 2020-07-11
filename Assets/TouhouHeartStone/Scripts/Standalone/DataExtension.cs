using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGensoukyo.Utilities
{
    public static class DataExtension
    {
        /// <summary>
        /// 读取指定名称的列数据，并将其转换为指定类型
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="row">行</param>
        /// <param name="name">列名</param>
        /// <exception cref="KeyNotFoundException">当列是空的时候抛出此异常</exception>
        /// <returns></returns>
        public static T ReadCol<T>(this DataRow row, string name)
        {
            if (row.IsNull(name))
                throw new KeyNotFoundException($"列[{name}]为空");

            T realVal = (T)Convert.ChangeType(row[name], typeof(T));
            return realVal;
        }
        /// <summary>
        /// 读取指定名称的列数据，并将其转换为指定类型。若此列数据不存在，则返回默认值。
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="row">行</param>
        /// <param name="name">列名</param>
        /// <param name="defaultVal">默认值</param>
        /// <returns></returns>
        public static T ReadCol<T>(this DataRow row, string name, T defaultVal)
        {
            if (row.IsNull(name)) return defaultVal;
            return (T)Convert.ChangeType(row[name], typeof(T));
        }
    }
}
