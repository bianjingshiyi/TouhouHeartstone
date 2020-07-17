using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TouhouHeartstone;
using TouhouCardEngine;
using TouhouHeartstone.Builtin;
using BJSYGameCore;
using UI;
using System;
using ExcelLibrary.SpreadSheet;
using System.Collections.Generic;

namespace Game
{
    [Serializable]
    public class Deck
    {
        public string name;
        [CardDefineID]
        public int master;
        [Serializable]
        public struct DeckItem
        {
            [CardDefineID]
            public int id;
            public int count;
        }
        public List<DeckItem> deckList = new List<DeckItem>();
        public int[] toIdArray()
        {
            List<int> list = new List<int>
            {
                master
            };
            foreach (var item in deckList)
            {
                if (item.id < 1)
                    continue;
                if (item.count < 1)
                    continue;
                list.AddRange(Enumerable.Repeat(item.id, item.count));
            }
            return list.ToArray();
        }
    }
    public class GameManager : Manager
    {
        [SerializeField]
        GameOption _option = new GameOption();
        [SerializeField]
        Main _ui;
        public THHGame game { get; private set; } = null;
        Task gameTask { get; set; } = null;
        protected override void onAwake()
        {
            base.onAwake();
            _ui = this.findInstance<Main>();
            _ui.game = this;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }
        [SerializeField]
        bool _useDefaultDeck = true;
        [SerializeField]
        Deck _defaultDeck;
        [CardDefineID]
        [SerializeField]
        int[] _deck = new int[31];
        public int[] deck
        {
            get { return _deck; }
            set { _deck = value; }
        }
        public CardManager cards
        {
            get { return getManager<CardManager>(); }
        }
        [SerializeField]
        bool _testRandomDeck = true;
        private void Start()
        {
#if !UNITY_EDITOR
            tryLoadDeckFromPrefs();
#else
            if (_testRandomDeck)
            {
                List<int> randomDeck = new List<int>();
                randomDeck.Add(Reimu.ID);
                System.Random random = new System.Random();
                for (int i = 0; i < 30; i++)
                {
                    randomDeck.Add(cards.GetCardDefines(c => cards.isStandardCard(c)).randomTake(random, 1).First().id);
                }
                _deck = randomDeck.ToArray();
            }
            if (_defaultDeck.deckList.Count > 0)
                _deck = _defaultDeck.toIdArray();
#endif
            if (_useDefaultDeck)
                _deck = _defaultDeck.toIdArray();
        }
        private void Update()
        {
            if (gameTask != null)
            {
                if (gameTask.IsCanceled || gameTask.IsFaulted)
                {
                    if (gameTask.Exception != null)
                        UberDebug.LogErrorChannel("Game", "游戏因异常退出：" + gameTask.Exception);
                    else
                        UberDebug.LogErrorChannel("Game", "游戏因未知异常退出");
                    quitGame();
                }
            }
        }
        /// <summary>
        /// 创建并开始本地游戏
        /// </summary>
        public void startLocalGame()
        {
            game = new THHGame(_option, getManager<CardManager>().GetCardDefines())
            {
                answers = new GameObject(nameof(AnswerManager)).AddComponent<AnswerManager>(),
                triggers = new GameObject(nameof(TriggerManager)).AddComponent<TriggerManager>(),
                time = new GameObject(nameof(TimeManager)).AddComponent<TimeManager>(),
                logger = new UnityLogger()
            };
            (game.answers as AnswerManager).game = game;

            //检查卡组合法性
            int[] deck = _deck;
            checkDeckValid(deck);

            CardDefine playerMaster = game.getCardDefine(deck[0]);
            CardDefine[] playerDeck = deck.Skip(1).Select(id => game.getCardDefine(id)).ToArray();
            THHPlayer localPlayer = game.createPlayer(1, "本地玩家", playerMaster as MasterCardDefine,
                playerDeck);
            THHPlayer aiPlayer = game.createPlayer(2, "AI", playerMaster as MasterCardDefine,
                deck.Skip(1).Select(id => game.getCardDefine(id)));
            displayGameUI(localPlayer);
            //AI玩家用AI
            new AI(game, aiPlayer);
            gameTask = game.run();
        }
        public void startRemoteGame(ClientManager client, GameOption option, RoomPlayerInfo[] players)
        {
            game = new THHGame(option, getManager<CardManager>().GetCardDefines())
            {
                answers = new GameObject(nameof(AnswerManager)).AddComponent<AnswerManager>(),
                triggers = new GameObject(nameof(TriggerManager)).AddComponent<TriggerManager>(),
                time = new GameObject(nameof(TimeManager)).AddComponent<TimeManager>(),
                logger = new ULogger()
            };
            (game.answers as AnswerManager).client = client;

            foreach (var info in players)
            {
                checkDeckValid(info.getDeck());

                THHPlayer player = game.createPlayer(info.id, info.name, game.getCardDefine(info.getDeck()[0]) as MasterCardDefine,
                    info.getDeck().Skip(1).Select(id => game.getCardDefine(id)));
                if (client.id == info.id)
                    displayGameUI(player);
            }
            gameTask = game.run();
        }
        private void checkDeckValid(int[] deck)
        {
            if (deck == null)
                return;
            if (game.getCardDefine(deck[0]) == null)
            {
                UberDebug.LogError("非法角色ID" + deck[0] + "被替换为灵梦");
                deck[0] = game.getCardDefine<Reimu>().id;
            }
            for (int i = 1; i < deck.Length; i++)
            {
                if (game.getCardDefine(deck[i]) == null)
                {
                    UberDebug.LogError("非法随从ID" + deck[i] + "被替换为小野菊");
                    deck[i] = game.getCardDefine<RashFairy>().id;
                }
            }
        }
        /// <summary>
        /// 显示游戏UI，并指定本地玩家
        /// </summary>
        /// <param name="localPlayer"></param>
        private void displayGameUI(THHPlayer localPlayer)
        {
            //本地玩家用UI
            _ui.display(_ui.Game);
            getManager<TableManager>().setGame(game, localPlayer);
            _ui.Game.Table.setGame(game, localPlayer);
        }
        public void quitGame()
        {
            if (game != null)
            {
                game.Dispose();
                game = null;
                _ui.display(_ui.MainMenu);
                gameTask = null;
                onGameEnd?.Invoke();
            }
        }
        public event Action onGameEnd;
        void tryLoadDeckFromPrefs()
        {
            if (!PlayerPrefs.HasKey("DeckCount"))
                return;
            int count = PlayerPrefs.GetInt("DeckCount");
            for (int i = 1; i < count; i++)
            {
                _deck[i] = PlayerPrefs.GetInt("Deck" + i);
            }
        }
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                if (e.Exception.InnerException != null)
                    Debug.LogError(e.Exception.InnerException);
                else if (e.Exception.InnerExceptions != null)
                {
                    foreach (var exception in e.Exception.InnerExceptions)
                    {
                        Debug.LogError(exception);
                    }
                }
                else
                    Debug.LogError(e.Exception);
            }
            else
                Debug.LogError(e);
        }
        protected void OnGUI()
        {
#if UNITY_EDITOR
            Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0, 3, GUILayout.Width(300));
#endif
        }
    }
}
