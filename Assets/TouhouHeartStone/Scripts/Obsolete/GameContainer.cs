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
                //决定先攻顺序，并发给Client
                List<Player> unorderedPlayers = new List<Player>(players);
                int[] orderedPlayerId = new int[unorderedPlayers.Count];
                for (int i = 0; i < orderedPlayerId.Length; i++)
                {
                    int index = UnityEngine.Random.Range(0, unorderedPlayers.Count);
                    orderedPlayerId[i] = unorderedPlayers[index].id;
                    unorderedPlayers.RemoveAt(index);
                }
                SetOrderRecord setOrder = new SetOrderRecord(orderedPlayerId);
                records.addRecord(setOrder);
                //初始化卡组，抽初始卡牌，并保留或者替换
                for (int i = 0; i < orderedPlayers.Length; i++)
                {
                    int[] presetDeck = new int[30];//预设卡组是空的。
                    AddCardRecord setDeck = new AddCardRecord(orderedPlayers[i].id, RegionType.deck, cards.createInstances(presetDeck));
                    records.addRecord(setDeck);
                    InitDrawRecord initDraw = new InitDrawRecord(orderedPlayers[i].id, i == 0 ? 3 : 4);
                    records.addRecord(initDraw);
                }
            }
        }
        public Player[] orderedPlayers
        {
            get { return _orderedPlayers; }
            set { _orderedPlayers = value; }
        }
        [SerializeField]
        Player[] _orderedPlayers = null;
        public PlayerManager players
        {
            get
            {
                if (_players == null)
                    _players = GetComponentInChildren<PlayerManager>();
                return _players;
            }
        }
        [SerializeField]
        PlayerManager _players = null;
        public LogManager log
        {
            get
            {
                if (_log == null)
                    _log = GetComponentInChildren<LogManager>();
                return _log;
            }
        }
        [SerializeField]
        LogManager _log;
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
        public RecordManager records
        {
            get
            {
                if (_records == null)
                    _records = GetComponentInChildren<RecordManager>();
                return _records;
            }
        }
        [SerializeField]
        RecordManager _records;
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
        public CardManager cards
        {
            get
            {
                if (_cards == null)
                    _cards = GetComponentInChildren<CardManager>();
                return _cards;
            }
        }
        [SerializeField]
        CardManager _cards;
        GameLogic game
        {
            get { return _logic; }
            set
            {
                _logic = value;
            }
        }
        [SerializeField]
        GameLogic _logic = null;
    }
    [Serializable]
    class GameLogic
    {
        public GameLogic(int randomSeed)
        {
            random = new System.Random(randomSeed);
        }
        System.Random random { get; set; }
        /// <summary>
        /// 开始游戏，需要提供游戏中玩家的id。
        /// </summary>
        /// <param name="playerId">玩家id数组</param>
        public void start(int[] playerId)
        {
            //初始化玩家
            players = playerId.Select(e => { return new PlayerLogic(e); }).ToArray();
            //决定回合顺序
            List<int> unorderedPlayers = new List<int>(players.Select(e => { return e.id; }));
            int[] orderedPlayerId = new int[unorderedPlayers.Count];
            for (int i = 0; i < orderedPlayerId.Length; i++)
            {
                int index = random.Next(0, unorderedPlayers.Count);
                orderedPlayerId[i] = unorderedPlayers[index];
                unorderedPlayers.RemoveAt(index);
            }
        }
        PlayerLogic[] players { get; set; }
    }
    class PlayerLogic
    {
        public PlayerLogic(int id)
        {
            this.id = id;
        }
        public int id { get; private set; }
        public override int GetHashCode()
        {
            return id;
        }
        public override bool Equals(object obj)
        {
            return obj is PlayerLogic && (obj as PlayerLogic).id == id;
        }
    }
}