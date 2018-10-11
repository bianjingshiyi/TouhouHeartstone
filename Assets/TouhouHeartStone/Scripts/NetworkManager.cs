using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TouhouHeartstone
{
    public abstract class NetworkManager : THManager
    {
        public abstract bool isClient { get; }
        public abstract int id { get; }
        public NetworkManager[] connections
        {
            get
            {
                if (_connections == null || _connections.Length <= 0)
                {
                    List<NetworkManager> connectionList = new List<NetworkManager>();
                    for (int i = 0; i < SceneManager.sceneCount; i++)
                    {
                        Scene scene = SceneManager.GetSceneAt(i);
                        foreach (GameObject obj in scene.GetRootGameObjects())
                        {
                            NetworkManager connection = obj.GetComponentInChildren<NetworkManager>();
                            if (connection != null)
                                connectionList.Add(connection);
                        }
                    }
                    _connections = connectionList.ToArray();
                }
                return _connections;
            }
        }
        [SerializeField]
        NetworkManager[] _connections;
    }
}