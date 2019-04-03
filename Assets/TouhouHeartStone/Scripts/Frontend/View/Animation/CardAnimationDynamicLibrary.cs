using System;
using System.Collections.Generic;

namespace TouhouHeartstone.Frontend.View.Animation
{
    /// <summary>
    /// Animation的库
    /// </summary>
    public static class CardAnimationDynamicLibrary
    {
        static Dictionary<string, Type> aniDict = new Dictionary<string, Type>();

        /// <summary>
        /// 重载这玩意
        /// </summary>
        public static void ReloadAniALibrary()
        {
            aniDict.Clear();

            var types = typeof(CardAnimationDynamicLibrary).Assembly.GetTypes();
            foreach (var item in types)
            {
                if (item.IsSubclassOf(typeof(CardAnimation)) && !item.IsAbstract)
                {
                    string name = ((CardAnimation)Activator.CreateInstance(item)).AnimationName;
                    aniDict.Add(name, item);
                }
            }
        }

        /// <summary>
        /// 创建一个Animation对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static CardAnimation CreateAnimation(string name)
        {
            if (aniDict.Count == 0) ReloadAniALibrary();
            return (CardAnimation)Activator.CreateInstance(aniDict[name]);
        }
    }
}
