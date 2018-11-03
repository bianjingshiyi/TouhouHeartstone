using System;

using UnityEngine;

namespace TouhouHeartstone.Backend
{
    public abstract class NetworkManager : MonoBehaviour
    {
        public abstract bool isClient { get; }
        public abstract int localPlayerId { get; }
        public abstract int hostId { get; }
        public abstract int[] playersId { get; }
        public abstract event Action<int, object> onReceiveObject;
        public abstract void sendObject(int targetId, object obj);
        public abstract void broadcastObject(object obj);
        public GameContainer game
        {
            get
            {
                if (_game == null)
                    _game = GetComponentInParent<GameContainer>();
                return _game;
            }
        }
        [SerializeField]
        GameContainer _game;
    }
}