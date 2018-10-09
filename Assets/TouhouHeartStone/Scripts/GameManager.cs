using System;

using UnityEngine;

namespace TouhouHeartstone
{
    public class GameManager : MonoBehaviour
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
                    //决定先攻顺序
                    Player firstPlayer = players[UnityEngine.Random.Range(0, players.Length)];//这个随机居然不包括最大值，我真是艹了
                    log.msg(firstPlayer + "获得了先手");
                    //抽初始卡牌，并保留或者替换

                    //开始选择
                }
            }
        }
        public GamePhase gamePhase
        {
            get { return _gamePhase; }
            set { _gamePhase = value; }
        }
        [SerializeField]
        GamePhase _gamePhase;
        public Player[] players
        {
            get
            {
                if (_players == null || _players.Length <= 0)
                    _players = GetComponentsInChildren<Player>();
                return _players;
            }
        }
        [SerializeField]
        Player[] _players;
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