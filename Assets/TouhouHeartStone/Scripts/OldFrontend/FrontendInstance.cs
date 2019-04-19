using UnityEngine;

using TouhouHeartstone.OldFrontend.Manager;
using IGensoukyo.Utilities;

namespace TouhouHeartstone.OldFrontend
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
            DebugUtils.LogDebug("Instance Init.");
            Manager.Init();
        }
    }
}
