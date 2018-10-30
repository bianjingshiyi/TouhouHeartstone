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
            game = new GameLogic(randomSeed);
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
        internal GameLogic game
        {
            get { return _logic; }
            set
            {
                _logic = value;
            }
        }
        GameLogic _logic = null;
    }
    [Serializable]
    class GameLogic
    {
        public GameLogic(int randomSeed)
        {
            random = new System.Random(randomSeed);
            cards = new CardsLogic();
            records = new RecordLogic(this);
        }
        System.Random random { get; set; }
        /// <summary>
        /// 开始游戏，需要提供游戏中玩家的id。
        /// </summary>
        /// <param name="playersId">玩家id数组</param>
        public void start(int[] playersId)
        {
            //初始化玩家
            players = new PlayersLogic(playersId);
            //决定回合顺序
            List<int> unorderedPlayers = new List<int>(players.Select(e => { return e.id; }));
            int[] orderedPlayerId = new int[unorderedPlayers.Count];
            for (int i = 0; i < orderedPlayerId.Length; i++)
            {
                int index = random.Next(0, unorderedPlayers.Count);
                orderedPlayerId[i] = unorderedPlayers[index];
                unorderedPlayers.RemoveAt(index);
            }
            SetOrderRecord setOrder = new SetOrderRecord(orderedPlayerId);
            records.addRecord(setOrder);
            //初始化卡组，抽初始卡牌，并保留或者替换
            for (int i = 0; i < players.orderedPlayers.Length; i++)
            {
                int[] presetDeck = new int[30];//预设卡组是空的。
                AddCardRecord setDeck = new AddCardRecord(players.orderedPlayers[i].id, RegionType.deck, cards.createInstances(presetDeck));
                records.addRecord(setDeck);
                InitDrawRecord initDraw = new InitDrawRecord(players.orderedPlayers[i].id, i == 0 ? 3 : 4);
                records.addRecord(initDraw);
            }
        }
        public CardsLogic cards { get; private set; }
        public PlayersLogic players { get; private set; }
        public RecordLogic records { get; set; }
    }
}