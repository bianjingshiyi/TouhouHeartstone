using System;
using System.Collections.Generic;
using UnityEngine;


namespace TouhouHeartstone.Frontend.Manager
{
    /// <summary>
    /// 前端管理器
    /// </summary>
    public class FrontendManager : MonoBehaviour
    {
        [SerializeField]
        GameContainer game;

        /// <summary>
        /// 游戏主要部分的引用
        /// </summary>
        public GameContainer Game => game;

        Dictionary<string, FrontendSubManager> submanagerInstance = new Dictionary<string, FrontendSubManager>();

        /// <summary>
        /// 获取对应的子管理器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSubManager<T>() where T : FrontendSubManager
        {
            var name = typeof(T).Name;
            if (submanagerInstance.ContainsKey(name))
                return submanagerInstance[name] as T;

            return GetComponentInChildren<T>();
        }

        private void preloadManagers()
        {
            submanagerInstance.Clear();

            var types = typeof(FrontendSubManager).Assembly.GetTypes();
            foreach (var item in types)
            {
                if (item.IsSubclassOf(typeof(FrontendSubManager)))
                {
                    var instance = GetComponentInChildren(item) as FrontendSubManager;
                    if (instance != null)
                        submanagerInstance.Add(item.Name, instance);
                }
            }
        }

        protected void Awake()
        {
            game = game ?? FindObjectOfType<GameContainer>();

            preloadManagers();
        }

    }
}
