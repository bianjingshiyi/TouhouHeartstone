using System;
using System.Collections.Generic;

using UnityEngine;

namespace TouhouHeartstone
{
    public class Game : MonoBehaviour
    {
        protected void Update()
        {
            //初始化

            //游戏阶段
            if (gamePhase == GamePhase.none)
            {
                gamePhase = GamePhase.start;
                if (!network.isClient)
                {
                    //初始化
                    UnityEngine.Random.InitState(DateTime.Now.GetHashCode());
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
            else if (gamePhase == GamePhase.start)
            {
                if (network.isClient)
                {
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
        public GamePhase gamePhase
        {
            get { return _gamePhase; }
            set { _gamePhase = value; }
        }
        [SerializeField]
        GamePhase _gamePhase;
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
    }
    public enum GamePhase
    {
        none,
        start
    }
}