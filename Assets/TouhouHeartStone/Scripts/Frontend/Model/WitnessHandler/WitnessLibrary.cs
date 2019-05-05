
using System;
using System.Collections.Generic;

namespace TouhouHeartstone.Frontend.Model.Witness
{
    /// <summary>
    /// Witness的库
    /// </summary>
    public static class WitnessLibrary
    {
        static Dictionary<string, Type> typeDict = new Dictionary<string, Type>();

        /// <summary>
        /// 重载这玩意
        /// </summary>
        public static void ReloadALibrary()
        {
            typeDict.Clear();

            var types = typeof(WitnessLibrary).Assembly.GetTypes();
            foreach (var item in types)
            {
                if (item.IsSubclassOf(typeof(WitnessHandler)) && !item.IsAbstract)
                {
                    string name = ((WitnessHandler)Activator.CreateInstance(item)).Name;
                    typeDict.Add(name, item);
                }
            }
        }

        /// <summary>
        /// 创建一个对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static WitnessHandler CreateHandler(string name)
        {
            if (typeDict.Count == 0)
                ReloadALibrary();

            if (!typeDict.ContainsKey(name))
                throw new KeyNotFoundException($"指定的WitnessHandler({name})未找到");

            return (WitnessHandler)Activator.CreateInstance(typeDict[name]);
        }
    }
}
