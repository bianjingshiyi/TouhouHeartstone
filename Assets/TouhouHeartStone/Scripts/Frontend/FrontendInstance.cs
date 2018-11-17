using UnityEngine;


namespace TouhouHeartstone.Frontend.Manager
{
    public class FrontendInstance : MonoBehaviour
    {
        [SerializeField]
        FrontendManager frontendManager;

        public FrontendManager Manager => frontendManager;

        FrontendInstanceManager _manager;

        public int ID
        {
            get { return id;}
            set { id = value; }
        }

        int id;

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
