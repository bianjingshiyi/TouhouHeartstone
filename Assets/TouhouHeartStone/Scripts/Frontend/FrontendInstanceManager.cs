using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

using TouhouHeartstone.Backend;


namespace TouhouHeartstone.Frontend.Manager
{
    public class FrontendInstanceManager : MonoBehaviour
    {
        [SerializeField]
        FrontendInstance prefab;

        List<FrontendInstance> frontendInstances = new List<FrontendInstance>();

        private void Awake()
        {
            AddInstanceToScene(gameObject.scene);
        }

        private void AddInstance(GameContainer gc, Scene scene)
        {
            var instance = Instantiate(prefab);
            SceneManager.MoveGameObjectToScene(instance.gameObject, scene);

            instance.SetFrontendInstanceManager(this);
            instance.Manager.SetGameContainer(gc);
            instance.Init();

            frontendInstances.Add(instance);
        }

        /// <summary>
        /// 再加一个
        /// </summary>
        private void AddAnotherInstance()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                var result = AddInstanceToScene(scene);

                // 一次添加一个
                if (result) break;
            }
        }

        public bool AddInstanceToScene(Scene scene)
        {
            var gos = scene.GetRootGameObjects();

            GameContainer container = null;
            foreach (var item in gos)
            {
                container = GetComponentInTree<GameContainer>(item);
                if (container != null) break;
            }
            if (container == null) return false;

            bool hasInstance = false;
            foreach (var item in gos)
            {
                hasInstance = GetComponentInTree<FrontendInstance>(item) != null;
                if (hasInstance) break;
            }
            if (hasInstance) return false;

            AddInstance(container, scene);
            return true;
        }

        public T GetComponentInTree<T>(GameObject go) where T : class
        {
            var gc = go.GetComponent<T>();
            if (gc == null)
            {
                gc = go.GetComponentInChildren<T>();
            }
            return gc;
        }

    }
}
