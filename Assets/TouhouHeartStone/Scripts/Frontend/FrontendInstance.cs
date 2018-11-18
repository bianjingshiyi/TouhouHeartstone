using UnityEngine;

using TouhouHeartstone.Frontend.Manager;

namespace TouhouHeartstone.Frontend
{
    public class FrontendInstance : MonoBehaviour
    {
        [SerializeField]
        FrontendManager frontendManager;

        public FrontendManager Manager => frontendManager;

        FrontendInstanceManager _manager;

        public int ID
        {
            get;
            set;
        }

        public FrontendInstanceManager InstanceManager => _manager;

        public void SetFrontendInstanceManager(FrontendInstanceManager fim)
        {
            _manager = fim;
        }

        public void Init()
        {
            gameObject.SetActive(true);
            Manager.Init();
        }
    }
}
