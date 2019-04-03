using System;
using System.Collections.Generic;
using UnityEngine;

namespace TouhouHeartstone.Frontend.View.Animation
{
    public static class CardAnimationComponentLibrary
    {
        static Dictionary<string, Type> aniDict = new Dictionary<string, Type>();

        /// <summary>
        /// 重载这玩意
        /// </summary>
        public static void ReloadAniALibrary()
        {
            aniDict.Clear();

            // 会炸吧（
            GameObject go = new GameObject();

            var types = typeof(CardAnimationComponentLibrary).Assembly.GetTypes();
            foreach (var item in types)
            {
                if (item.IsSubclassOf(typeof(CardAnimationComponent)) && !item.IsAbstract)
                {
                    var c = go.AddComponent(item) as CardAnimationComponent;
                    string name = c.AnimationName;
                    aniDict.Add(name, item);

                    GameObject.Destroy(c);
                }
            }
            GameObject.Destroy(go);
        }

        /// <summary>
        /// 创建一个CardAnimationComponent对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static CardAnimationComponent CreateAnimation(string name, GameObject go)
        {
            if (aniDict.Count == 0) ReloadAniALibrary();

            return go.AddComponent(aniDict[name]) as CardAnimationComponent;
        }

        /// <summary>
        /// 检查是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool AnimationExists(string name)
        {
            if (aniDict.Count == 0) ReloadAniALibrary();
            return aniDict.ContainsKey(name);
        }
    }
}
