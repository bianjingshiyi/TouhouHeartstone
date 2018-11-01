using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TouhouHeartstone
{
    public class GameContainer : MonoBehaviour
    {
        protected void Awake()
        {
            int randomSeed = (int)DateTime.Now.ToBinary();
            game = new Game(randomSeed);
            Debug.Log("随机种子：" + randomSeed);
        }
        protected void Start()
        {
            if (!network.isClient)
            {
                List<int> playerList = new List<int>();
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    foreach (GameObject obj in scene.GetRootGameObjects())
                    {
                        NetworkManager network = obj.GetComponentInChildren<NetworkManager>(true);
                        if (network != null)
                            playerList.Add(network.id);
                    }
                }
                game.start(playerList.ToArray());
            }
        }
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
        internal Game game { get; set; } = null;
    }
}