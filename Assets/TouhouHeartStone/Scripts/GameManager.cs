using System.IO;
using System.Reflection;
using System.Collections.Generic;
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
namespace Game
{
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
        private void Start()
        {
#if !UNITY_EDITOR
            tryLoadDeckFromPrefs();
#endif
        }
        private void Update()
        {
            if (gameTask != null)
            {
                if (!game.isRunning || gameTask.IsCompleted || gameTask.IsCanceled || gameTask.IsFaulted)
                {
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

            THHPlayer localPlayer = game.createPlayer(1, "本地玩家", game.getCardDefine(deck[0]) as MasterCardDefine,
                deck.Skip(1).Select(id => game.getCardDefine(id)));
            THHPlayer aiPlayer = game.createPlayer(2, "AI", game.getCardDefine(deck[0]) as MasterCardDefine,
                deck.Skip(1).Select(id => game.getCardDefine(id)));
            displayGameUI(localPlayer);
            //AI玩家用AI
            new AI(game, aiPlayer);

            game.triggers.onEventAfter += onEventAfter;
            gameTask = game.run();
        }
        public void startRemoteGame(ClientManager client, GameOption option, THHRoomPlayerInfo[] players)
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
                checkDeckValid(info.deck);

                THHPlayer player = game.createPlayer(info.id, info.name, game.getCardDefine(info.deck[0]) as MasterCardDefine,
                    info.deck.Skip(1).Select(id => game.getCardDefine(id)));
                if (client.id == info.id)
                    displayGameUI(player);
            }
            game.triggers.onEventAfter += onEventAfter;
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
        private void onEventAfter(TouhouCardEngine.Interfaces.IEventArg obj)
        {
            if (obj is THHGame.GameEndEventArg)
            {
                quitGame();
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
            Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0, 1, GUILayout.Width(200));
#endif
        }
    }
}
