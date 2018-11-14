using UnityEngine;


namespace TouhouHeartstone.Frontend.Manager
{
    public class FrontendInstance : MonoBehaviour
    {
        [SerializeField]
        FrontendManager frontendManager;

        public FrontendManager Manager => frontendManager;

        FrontendInstanceManager _manager;

        public FrontendInstanceManager InstanceManager => _manager;

        public void SetFrontendInstanceManager(FrontendInstanceManager fim)
        {
            _manager = fim;
        }

        public void Init()
        {
            Manager.Init();
        }
    }
}
