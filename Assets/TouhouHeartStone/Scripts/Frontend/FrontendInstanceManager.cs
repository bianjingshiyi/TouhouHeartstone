using System.Collections.Generic;
using TouhouHeartstone.Backend;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TouhouHeartstone.Frontend
{
    public class FrontendInstanceManager : MonoBehaviour
    {
        [SerializeField]
        FrontendInstance prefab;

        List<FrontendInstance> frontendInstances = new List<FrontendInstance>();

        public FrontendInstance[] Instances => frontendInstances.ToArray();

        private void Start()
        {
            AddInstanceToScene(gameObject.scene);
            Instances[0].gameObject.SetActive(true);
        }

        public void AddInstance(GameContainer gc, Scene scene, int id)
        {
            var instance = Instantiate(prefab);
            instance.gameObject.SetActive(false);
            SceneManager.MoveGameObjectToScene(instance.gameObject, scene);

            instance.SetFrontendInstanceManager(this);
            instance.Manager.SetGameContainer(gc);
            instance.ID = id;
            instance.Init();

            frontendInstances.Add(instance);
        }

        /// <summary>
        /// 再加一个
        /// </summary>
        public void AddAnotherInstance()
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

            AddInstance(container, scene, frontendInstances.Count);
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

        public void SwitchInstance(int targetID)
        {
            for (int i = 0; i < frontendInstances.Count; i++)
            {
                var item = frontendInstances[i];
                item.gameObject.SetActive(item.ID == targetID);
            }
        }

        public void SetInstanceState(int targetID, bool state)
        {
            frontendInstances[targetID].gameObject.SetActive(state);
        }
    }
}
