using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BJSYGameCore;
using BJSYGameCore.UI;
using Game.Animations;
using Game.BattleTable;
using Game.BattleTable.UI;
using MongoDB.Bson;
using MukioI18n;
using NitoriNetwork.Common;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Deck = TouhouHeartstone.Deck;

namespace Game
{
    public partial class GameManager : Manager
    {
        [SerializeField]
        bool _useTimeAsRandomSeed = true;
        [SerializeField]
        HearthstoneOption _option = new HearthstoneOption();
        [SerializeField]
        Main _mainUI;
        public CardEngine game { get; private set; } = null;
        public Player localPlayer { get; private set; } = null;
        Task gameTask { get; set; } = null;
        LangManager lang => getManager<LangManager>();

        protected override void onAwake()
        {
            base.onAwake();
            TaskExceptionHandler.register();
            _mainUI = this.findInstance<Main>();
            if (_mainUI)
            {
                _mainUI.game = this;
            }
        }
        [SerializeField]
        bool _useDefaultDeck = false;
        [SerializeField]
        Deck _defaultDeck;
        [SerializeField]
        bool _useTestDeck = false;
        [SerializeField]
        Deck _testDeck;
        [CardDefineID]
        [SerializeField]
        DefineReference[] _deck = new DefineReference[31];

        [SerializeField] bool _dummyAI = false;
        public DefineReference[] deck
        {
            get { return _deck; }
            set { _deck = value; }
        }
        public CardManager cards
        {
            get { return getManager<CardManager>(); }
        }
        [SerializeField]
        GameMode _mode = GameMode.none;
        public GameMode mode
        {
            get { return _mode; }
            private set { _mode = value; }
        }
        private void Start()
        {
            if (_useDefaultDeck && _defaultDeck != null)
                _deck = _defaultDeck.toDefRefArray();
            if (_useTestDeck && _testDeck != null)
                _deck = _testDeck.toDefRefArray();
        }
        private void Update()
        {
            if (gameTask != null)
            {
                //检查游戏是否结束
                if (gameTask.IsCanceled || gameTask.IsFaulted)
                {
                    if (gameTask.Exception != null)
                        UberDebug.LogErrorChannel("Game", "游戏因异常退出：" + gameTask.Exception);
                    else
                        UberDebug.LogErrorChannel("Game", "游戏因未知异常退出");
                    quitGame();
                    return;
                }
                if (_replayPlayer != null)
                {
                    //如果在Replay模式
                    onReplayGameUpdate();
                }
                //在Update中抛出游戏中发生的事件，避免线程冲突并隔离异常的发生。
                tableUI?.LogCtrl?.pollEvents();
            }
        }
        #region 外部接口
        public async Task startLocalGame(RoomData room)
        {
            //创建游戏
            CardEngine game = await createGame(room);
            mode = GameMode.local;
            //初始化本地玩家，AI，UI
            foreach (var player in game.players)
            {
                if (player.id == room.playerDataList[0].id) // 暂时的
                {
                    localPlayer = player;
                    await displayGameUI(game, localPlayer);
                }
                else
                {
#if UNITY_EDITOR
                    new AI(game, player, dummy: _dummyAI); //AI玩家用AI
#else
                    new AI(game, player);
#endif
                }
            }

            if (tableUI != null)
            {
                tableUI.showSurrenderButton(false);
                tableUI.GameResultDialog._ButtonGlassButtonBlack.asButton.onClick.set(quitGame);
                tableUI.GameResultDialog.onSaveReplayButtonClick += quitGame; // 保存并退出
            }
            //tableUI.GameResultDialog.onSaveReplayButtonClick += onSaveReplayButtonClick;
            //开始游戏
            await runGame(game);
        }

        [SerializeField]
        NetworkingLogic _nwLogic;

        public NetworkingLogic nwLogic
        {
            get { return _nwLogic; }
            set { _nwLogic = value; }
        }

        /// <summary>
        /// [RPC] 开始远程游戏
        /// </summary>
        /// <param name="room"></param>
        public void startRemoteGame(RoomData room)
        {
            bool headless = _mainUI == null;
            startRemoteGame(room, headless);
        }
        public Task startRemoteGame(RoomData room, bool headless)
        {
            return startRemoteGame(nwLogic, room, headless);
        }
        public async Task startRemoteGame(IClient client, RoomData room, bool headless = false)
        {
            //创建游戏
            CardEngine game = await createGame(room);
            mode = GameMode.remote;
            //将游戏与网络连接
            game.answers.client = client;
            //初始化本地玩家与UI
            foreach (var player in game.players)
            {
                if (client.id == player.id)
                {
                    localPlayer = player;
                    if (!headless)
                        await displayGameUI(game, localPlayer);
                }
            }
            if (tableUI != null)
            {
                tableUI.showSurrenderButton(true);
                tableUI.GameResultDialog._ButtonGlassButtonBlack.asButton.onClick.set(quitGame);
                tableUI.GameResultDialog.onSaveReplayButtonClick += quitGame; // 保存并退出
            }
            await runGame(game);
        }
        public async Task startReplayGame(Replay rep)
        {
            CardEngine game = await createGame(rep.option, rep.players);
            mode = GameMode.replay;
            new ReplayPlayer(rep, game);
            localPlayer = game.players.First();
            await displayGameUI(game, localPlayer);
            if (tableUI != null)
            {
                tableUI.showSurrenderButton(false);
                tableUI.GameResultDialog._ButtonGlassButtonBlack.asButton.onClick.set(quitGame);
                tableUI.GameResultDialog.onSaveReplayButtonClick += quitGame; // 保存并退出
            }
            await runGame(game);
        }

        public void surrender()
        {
            game.answers.answer(localPlayer.id, new SurrenderResponse());
        }
        public event Action onGameStart;
        #endregion
        #region 内部功能
        private Task<CardEngine> createGame(RoomData room)
        {
            return createGame(room.getOption(), room.playerDataList);
        }
        private async Task<CardEngine> createGame(HearthstoneOption option, IEnumerable<IRoomPlayer> playerList)
        {
            //加载外部卡牌
            Dictionary<long, Dictionary<int, CardDefine>> cardDict = new Dictionary<long, Dictionary<int, CardDefine>>();
            Dictionary<long, Dictionary<int, BuffDefine>> buffDict = new Dictionary<long, Dictionary<int, BuffDefine>>();
            Dictionary<long, Dictionary<int, ActionDefine>> actionDefines = new Dictionary<long, Dictionary<int, ActionDefine>>();
            try
            {
                await LoadExternalCardsAndBuffs(option, cardDict, buffDict, actionDefines);
            }
            catch (Exception e)
            {
                var title = main.lang._(StringTable.ERROR);
                var desc = main.lang._(GAME_CREATE_ERROR_LOAD_CARDS_FAILED);
                main.ui.dialog.ShowConfirm(title, desc);
                throw;
            }


            var answers = new GameObject(nameof(AnswerManager)).AddComponent<AnswerManager>();
            var triggerManager = new GameObject(nameof(TriggerManager)).AddComponent<TriggerManager>();
            CardEngine game = new CardEngine()
            {
                answers = answers,
                triggers = triggerManager,
                time = new GameObject(nameof(TimeManager)).AddComponent<TimeManager>(),
                snapshoter = new THHSnapshoter(),
                logger = new ULogger()
            };
            answers.game = game;
            triggerManager.game = game;
#if UNITY_EDITOR
            if (_useTestDeck)
            {
                foreach (var player in playerList)
                {
                    player.setDeck(_testDeck);
                }
            }
#endif

            foreach (var pair in actionDefines)
            {
                long cardPoolId = pair.Key;
                foreach (var actionPair in pair.Value)
                {
                    game.addActionDefine(new ActionReference(cardPoolId, actionPair.Key), actionPair.Value);
                }
            }

            Rule rule = new HearthStoneRule(cardDict, buffDict);
            await game.init(new Assembly[] { typeof(CardEngine).Assembly, typeof(HearthStoneRule).Assembly, typeof(EffectEditorLibrary).Assembly },
                rule,
                option,
                playerList.ToArray());
            return game;
        }

        Task runGame(CardEngine game, bool recordReplay = true)
        {
            this.game = game;
            if (recordReplay)
                _replaySystem.startRecord(game, game.players.Select(p =>
                {
                    RoomPlayerData playerData = new RoomPlayerData(p.id, p.name, RoomPlayerType.human);
                    playerData.setDeck(new Deck(
                        p.getMaster().define.getDefineRef(),
                        p.getDeck().Select(c => c.define.getDefineRef())));
                    return playerData;
                }).ToArray());
            onGameStart?.Invoke();
            // gameTask = THHGame.runGame(game);
            gameTask = game.run();
            return gameTask;
        }
        /// <summary>
        /// 关闭游戏
        /// </summary>
        public void endGame()
        {
            if (_replayPlayer != null)
                _replayPlayer = null;
            if (_replaySystem.isRecording)
                _replaySystem.stopRecord();
            if (game != null)
            {
                game.Dispose();
                game = null;
                gameTask = null;
                mode = GameMode.none;
                onGameEnd?.Invoke();
            }
        }

        /// <summary>
        /// 关闭游戏UI并回到主页面
        /// </summary>
        public void closeGameUI()
        {
            var op = SceneManager.UnloadSceneAsync("BattleUI");
            op.completed += (o) =>
            {
                _mainUI.setBackgroundVisible(true);
                _mainUI.display(_mainUI.MainMenu);
                main.camera.gameObject.SetActive(true);
                _battleTableManager = null;
                _tableUI = null;
                main.seManager.stopAllLoopingSE();
                main.bgm.PlayList("Menu");
            };
        }

        /// <summary>
        /// 关闭游戏和UI
        /// </summary>
        public void quitGame()
        {
            endGame();
            closeGameUI();
        }
        public event Action onGameEnd;
        #endregion
        #region 事件回调
        private void onQuitCallback()
        {
            main.ui.dialog?.ShowQuestion(StringTable.HINT_TITLE, QUIT_GAME_QUESTION, () =>
            {
                quitGame();
            });
        }
        private void onSurrenderCallback()
        {
            main.ui.dialog?.ShowQuestion(StringTable.HINT_TITLE, SURRENDER_QUESTION, () => {
                localPlayer.cmdSurrender(game);
            });
        }
        #endregion
        private async Task LoadExternalCardsAndBuffs(HearthstoneOption option,
            Dictionary<long, Dictionary<int, CardDefine>> cardDict,
            Dictionary<long, Dictionary<int, BuffDefine>> buffDict,
            Dictionary<long, Dictionary<int, ActionDefine>> actionDefines)
        {
            var cardManager = cards;
            animDict.Clear();
            // 尝试从本地获取卡组，否则读取缓存卡组。
            foreach (var pool in option.cardPools)
            {
                long poolId = pool.cardPoolId;
                var poolRef = pool.ToReference();

                if (cards.isBuiltinCardPool(poolId))
                {
                    cardDict.Add(poolId, cardManager.GetCardDefines().ToDictionary(c => c.id));
                    animDict.Add(poolId, main.anims.getBuiltinAnimationDefines().ToDictionary(a => a.id));
                    continue;
                }
                CardPool cardPool = null;
                var poolInfo = cards.getCardPoolInfo(poolRef);
                // 尝试获取本地卡池的卡牌定义（要求版本正确）。
                if (poolInfo != null && poolInfo.editTime == pool.version)
                {
                    cardPool = (await cards.getOrLoadCardPool(pool.GetReference())).cardPool;
                }
                // 如果本地卡地没有任何卡牌定义，或者版本不正确，尝试获取缓存内的该卡池。
                if (cardPool == null)
                {
                    cardPool = (await cards.loadCachedCardPoolAsync(poolId, pool.version)).cardPool;
                }
                if (cardPool != null)
                {
                    var cardDefines = cardPool.cardDefineList.ToArray();
                    var buffDefines = cardPool.buffDefineList.ToArray();
                    cardDict.Add(poolId, cardDefines.ToDictionary(c => c.id));
                    buffDict.Add(poolId, buffDefines.ToDictionary<BuffDefine, int>(c => c.id));
                    actionDefines.Add(poolId, cardPool.actionDefineList.ToDictionary<ActionDefine, int>(c => c.defineId));
                    animDict.Add(poolId, cardPool.animationDefineList.ToDictionary<AnimationDefine, int>(a => a.id));
                }
            }
        }
        private void checkDeckValid(CardEngine game, DefineReference[] deck)
        {
            if (deck == null)
                return;
            if (game.getDefine(deck[0].cardPoolId, deck[0].defineId) == null)
            {
                UberDebug.LogError("非法角色ID" + deck[0] + "被替换为灵梦");
                deck[0] = new DefineReference(0, Reimu.ID);
            }
            for (int i = 1; i < deck.Length; i++)
            {
                if (game.getDefine(deck[i].cardPoolId, deck[i].defineId) == null)
                {
                    UberDebug.LogError("非法随从ID" + deck[i] + "被替换为小野菊");
                    deck[i] = new DefineReference(0, RashFairy.ID);
                }
            }
        }
        /// <summary>
        /// 显示游戏UI，并指定本地玩家
        /// </summary>
        /// <param name="localPlayer"></param>
        private async Task displayGameUI(CardEngine game, Player localPlayer)
        {
            //本地玩家用UI
            var tcs = new TaskCompletionSource<AsyncOperation>();
            var operation = SceneManager.LoadSceneAsync("BattleUI", LoadSceneMode.Additive);
            operation.completed += op => { tcs.SetResult(op); };

            await tcs.Task;

            var scene = SceneManager.GetSceneByName("BattleUI");
            _battleTableManager = scene.findInstance<BattleTableManager>();
            _tableUI = scene.findInstance<TableUI>();
            _mainUI.setBackgroundVisible(false);
            _mainUI.hidePages();
            main.camera.gameObject.SetActive(false);
            main.bgm.PlayList("Game");
            if (battleTableManager)
            {
                battleTableManager.setGame(main, game, localPlayer);
            }

            if (tableUI)
            {
                tableUI.onQuit += onQuitCallback;
                tableUI.onSurrender += onSurrenderCallback;
                tableUI.LogCtrl.setGame(game, main, battleTableManager, localPlayer);
                tableUI.LogCtrl.reset();
            }
        }
        private async void onSaveReplayButtonClick()
        {
            await getManager<MainManager>().fileManager.savePersistentFile(
                _replaySystem.getSaveDir() + "/" + _replaySystem.getSaveFileName(_replaySystem.replay) + "." + _replaySystem.getSaveFileExtension(),
                _replaySystem.replay.players.Select(p => p.getDeck().masterCardRef).ToArray().ToJson() + "\r\n" +
                _replaySystem.replay.ToJson());
        }
        protected void OnGUI()
        {
#if UNITY_EDITOR
            Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0, 3, GUILayout.Width(300));
#endif
        }
        #region 属性字段
        [TranslateMsg]
        const string QUIT_GAME_QUESTION = "是否要结束本场游戏？";
        [TranslateMsg("尝试认输时的提醒")]
        const string SURRENDER_QUESTION = "确定要向对手认输吗？";
        [TranslateMsg("开始游戏时，读取卡池失败时的报错")]
        public const string GAME_CREATE_ERROR_LOAD_CARDS_FAILED = "开始游戏失败，无法读取游戏所需的卡池。";
        public MainManager main => getManager<MainManager>();
        private BattleTableManager _battleTableManager;
        private BattleTableManager battleTableManager 
        { 
            get
            {
                return _battleTableManager;
            }
        }
        private TableUI _tableUI;
        private TableUI tableUI
        {
            get
            {
                return _tableUI;
            }
        }

        #endregion
    }
    public class LogItem
    {
        public IEventArg arg;
        public int level;
    }
    public enum GameMode
    {
        none,
        local,
        remote,
        replay
    }
}
