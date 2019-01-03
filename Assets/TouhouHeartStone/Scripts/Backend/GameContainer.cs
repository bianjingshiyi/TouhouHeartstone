
using UnityEngine;

using TouhouHeartstone.Frontend.Manager;

namespace TouhouHeartstone.Backend
{
    public abstract class GameContainer : MonoBehaviour
    {
        protected void Awake()
        {
            onAwake();
        }
        protected virtual void onAwake()
        {
            network.onReceiveObject += onReceiveObject;
        }
        protected void Start()
        {
            onStart();
        }
        protected virtual void onStart()
        {
            if (frontendEvents != null)
            {
                frontendEvents.ReplaceInitDrawAction += onInitReplace;
                frontendEvents.UseCardEventAction += onUse;
                frontendEvents.EndRoundEventAction += onTurnEnd;
            }
        }
        protected abstract void onInitReplace(int[] cards);
        protected abstract void onUse(int instance, int position, int target);
        protected abstract void onTurnEnd();
        protected abstract void onReceiveObject(int senderId, object obj);
        public abstract int localPlayerIndex { get; }
        FrontendWitnessEventDispatcher frontendEvents
        {
            get
            {
                if (_frontendEvents == null)
                {
                    foreach (GameObject obj in gameObject.scene.GetRootGameObjects())
                    {
                        _frontendEvents = obj.GetComponentInChildren<FrontendWitnessEventDispatcher>();
                        if (_frontendEvents != null)
                            break;
                    }
                }
                return _frontendEvents;
            }
        }
        [SerializeField]
        FrontendWitnessEventDispatcher _frontendEvents;
        public NetworkManager network
        {
            get
            {
                if (_network == null)
                    _network = GetComponentInChildren<NetworkManager>();
                return _network;
            }
        }
        [SerializeField]
        NetworkManager _network;
        public WitnessManager witness
        {
            get
            {
                if (_witness == null)
                    _witness = GetComponentInChildren<WitnessManager>();
                return _witness;
            }
        }
        [SerializeField]
        WitnessManager _witness;
    }
}