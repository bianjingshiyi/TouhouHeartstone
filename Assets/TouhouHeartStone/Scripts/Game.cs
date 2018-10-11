using System;

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
                    firstPlayer = players[UnityEngine.Random.Range(0, players.count)];//这个随机居然不包括最大值，我真是艹了
                    (network as FakeHostManager).broadcastObject(new FirstPlayerDiff(firstPlayer.id));
                    log.msg(firstPlayer + "获得了先手");
                    //抽初始卡牌，并保留或者替换

                    //开始选择
                }
            }
            else if (gamePhase == GamePhase.start)
            {
                if (network.isClient)
                {
                    if (firstPlayer == null)
                    {
                        //决定先攻顺序，等待Host发送随机结果
                    }
                }
            }
        }
        public Player firstPlayer
        {
            get { return _firstPlayer; }
            set { _firstPlayer = value; }
        }
        [SerializeField]
        Player _firstPlayer = null;
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
    }
    public enum GamePhase
    {
        none,
        start
    }
}