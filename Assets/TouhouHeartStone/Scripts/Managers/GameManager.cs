using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BJSYGameCore;
using Controllers;
using Game.Animations;
using Game.BattleTable;
using Game.BattleTable.UI;
using MukioI18n;
using NitoriNetwork.Common;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Deck = TouhouHeartstone.Deck;

namespace Game
{
    public partial class GameManager : Manager
    {
        #region 公有方法
        public async Task startLocalGame(RoomData room)
        {
            //创建游戏
            CardEngine game = await createGame(room);
            var localId = room.playerDataList[0].id;
            initReplay(game, room, localId);
            await startGame(game, GameMode.local, localId, true);
        }
        public Task startPVEGame(RoomData room)
        {
            isPVE = true;
            return startLocalGame(room);
        }
        /// <summary>
        /// [RPC] 开始远程游戏
        /// </summary>
        /// <param name="room"></param>
        public Task startRemoteGame(RoomData room)
        {
            return startRemoteGame(nwLogic, room);
        }
        public async Task startRemoteGame(IClient client, RoomData room)
        {
            //创建游戏
            CardEngine game = await createGame(room);
            //将游戏与网络连接
            game.answers.client = client;
            //初始化本地玩家与UI
            var localId = client.id;
            initReplay(game, room, localId);
            await startGame(game, GameMode.remote, localId);
        }
        #endregion
        #region 私有方法
        #region 生命周期
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
        private void Update()
        {
            if (gameTask != null)
            {
                //检查游戏是否结束
                if (gameTask.IsCanceled || gameTask.IsFaulted)
                {
                    if (gameTask.Exception != null)
                        UberDebug.LogErrorChannel("Game", $"游戏因异常退出：{gameTask.Exception}");
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
        private void OnGUI()
        {
#if UNITY_EDITOR
            Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0, 3, GUILayout.Width(300));
#endif
        }
        #endregion
        #region 事件回调
        private void onGameEndCallback()
        {
            endGame();
        }
        private void onGameQuitButtonCallback()
        {
            var title = main.lang._(StringTable.HINT_TITLE);
            var desc = main.lang._(QUIT_GAME_QUESTION);
            main.ui.dialog?.ShowQuestion(title, desc, () =>
            {
                quitGame();
            });
        }
        private void onGameResultQuitGameButtonClicked()
        {
            quitGame();
        }
        private void onSurrenderCallback()
        {
            var title = main.lang._(StringTable.HINT_TITLE);
            var desc = main.lang._(SURRENDER_QUESTION);
            main.ui.dialog?.ShowQuestion(title, desc, () =>
            {
                localPlayer.cmdSurrender(game);
            });
        }
        private void onEventAfter(IEventArg eventArg)
        {
            if (eventArg.define is GameEndEventDefine)
            {
                onGameFinished?.Invoke(eventArg.getVar<Player[]>(GameEndEventDefine.VAR_WINNERS));
            }
        }
        #endregion
        private Task<CardEngine> createGame(RoomData room)
        {
            return createGame(room.getOption(), room.playerDataList);
        }
        private async Task<CardEngine> createGame(HearthstoneOption option, IEnumerable<IRoomPlayer> playerList)
        {
            //加载外部卡牌
            List<CardDefine> cards = new List<CardDefine>();
            List<BuffDefine> buffs = new List<BuffDefine>();
            List<ActionDefine> actionDefines = new List<ActionDefine>();
            List<Rule> rules = new List<Rule>();
            var eventDefines = main.card.getAllEventDefines(true, option.cardPools);
            try
            {
                await LoadExternalCardsAndBuffs(option, cards, buffs, actionDefines, rules);
            }
            catch (Exception)
            {
                var title = lang._(StringTable.ERROR);
                var desc = lang._(GAME_CREATE_ERROR_LOAD_CARDS_FAILED);
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
            foreach (var actionDefine in actionDefines)
            {
                game.addActionDefine(actionDefine);
            }

            foreach (var eventDefine in eventDefines)
            {
                game.addEventDefine(eventDefine);
            }
            Player[] players = new Player[playerList.Count()];
            for (int i = 0; i < players.Length; i++)
            {
                var playerInfo = playerList.ElementAt(i);
                var player = new Player(playerInfo.ID, playerInfo.Name);
                var startDeck = playerInfo.getDeck().toDefRefArray();
                var defineArray = startDeck.Select(d => cards.FirstOrDefault(c => c.cardPoolId == d.cardPoolId && c.id == d.defineId)).ToArray();
                player.setStartDeck(game, defineArray);
                players[i] = player;
            }
            await game.init(cards.ToArray(), buffs.ToArray(), rules.ToArray(), option, players);
            return game;
        }

        private Task runGame(CardEngine game)
        {
            onGameStart?.Invoke();
            gameTask = game.run();
            return gameTask;
        }
        private async Task LoadExternalCardsAndBuffs(HearthstoneOption option,
            List<CardDefine> cards,
            List<BuffDefine> buffs,
            List<ActionDefine> actions,
            List<Rule> rules)
        {
            var cardManager = this.cards;
            animDict.Clear();
            animDict.Add(0, main.anims.getBuiltinAnimationDefines().ToDictionary(a => a.id)); // 内置特效
            //内置动作定义
            var assemblies = new Assembly[] {
                typeof(CardEngine).Assembly,
                typeof(HearthStoneRule).Assembly,
                typeof(EffectEditorLibrary).Assembly
            };
            actions.AddRange(ActionDefine.loadDefinesFromAssemblies(assemblies));
            // 内置规则。
            rules.Add(new HearthStoneRule());
            // 尝试从本地获取卡组，否则读取缓存卡组。
            foreach (var poolRef in option.cardPools)
            {
                long poolId = poolRef.cardPoolId;
                CardPool cardPool = null;
                var poolInfo = this.cards.getCardPoolInfo(poolRef);
                // 尝试获取本地卡池的卡牌定义（要求版本正确）。
                if (poolInfo != null && poolInfo.editTime == poolRef.version)
                {
                    cardPool = (await this.cards.getOrLoadCardPool(poolRef)).cardPool;
                }
                // 如果本地卡地没有任何卡牌定义，或者版本不正确，尝试获取缓存内的该卡池。
                if (cardPool == null)
                {
                    cardPool = (await this.cards.loadCachedCardPool(poolId, poolRef.version)).cardPool;
                }
                if (cardPool != null)
                {
                    var cardDefines = cardPool.cardDefineList.ToArray();
                    var buffDefines = cardPool.buffDefineList.ToArray();
                    cards.AddRange(cardDefines);
                    buffs.AddRange(buffDefines);
                    actions.AddRange(cardPool.actionDefineList);
                    animDict.Add(poolId, cardPool.animationDefineList.ToDictionary<AnimationDefine, int>(a => a.id));
                    if (cardPool.ruleLogicList.Count > 0)
                        rules.Add(new GeneratedRule(cardPool.ruleLogicList));
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
                battleTableManager.canSeeAllCards = mode == GameMode.replay;
                battleTableManager.setCanControl(mode != GameMode.replay);
            }
            initTableUI();
        }
        /// <summary>
        /// 开始游戏
        /// </summary>
        /// <returns></returns>
        private async Task startGame(CardEngine game, GameMode mode, int localId, bool hasAI = false)
        {
            this.game = game;
            this.mode = mode;
            localPlayer = game.players.FirstOrDefault(p => p.id == localId);
            //初始化本地玩家，AI，UI
            if (hasAI)
            {
                foreach (var player in game.players.Where(p => p != localPlayer))
                {
#if UNITY_EDITOR
                    new AI(game, player, dummy: _dummyAI); //AI玩家用AI
#else
                    new AI(game, player);
#endif
                }
            }
            initGameEvents();
            await displayGameUI(game, localPlayer);
            await runGame(game);
        }
        /// <summary>
        /// 关闭游戏
        /// </summary>
        private void endGame()
        {
            if (_replayPlayer != null)
                _replayPlayer = null;
            if (_replayRecorder.isRecording)
                _replayRecorder.stopRecord();
            _recordingReplay = false;
            if (game != null)
            {
                game.Dispose();
                game = null;
                gameTask = null;
                onGameEnd?.Invoke();
            }
        }
        /// <summary>
        /// 关闭游戏UI并回到主页面
        /// </summary>
        private void closeGameUI()
        {
            disposeTableUI();
            var op = SceneManager.UnloadSceneAsync("BattleUI");
            op.completed += (o) =>
            {
                _mainUI.setBackgroundVisible(true);
                _mainUI.hidePages();
                scene.menu.display();
                main.camera.gameObject.SetActive(true);
                main.seManager.stopAllLoopingSE();
                main.bgm.PlayList("Menu");
                onGameQuit?.Invoke();
                reset();
            };
        }
        /// <summary>
        /// 关闭游戏和UI
        /// </summary>
        private void quitGame()
        {
            endGame();
            closeGameUI();
        }
        private void initGameEvents()
        {
            game.triggers.onEventAfter += onEventAfter;
        }
        private void initTableUI()
        {
            if (tableUI)
            {
                tableUI.showSurrenderButton(mode == GameMode.remote || isPVE);
                tableUI.LogCtrl.setGame(game, main, battleTableManager, localPlayer);
                tableUI.LogCtrl.reset();
                tableUI.onQuit += onGameQuitButtonCallback;
                tableUI.onSurrender += onSurrenderCallback;
                tableUI.GameResultDialog.onGameEnd += onGameEndCallback;
                tableUI.GameResultDialog.onQuitButtonClicked += onGameResultQuitGameButtonClicked;
                tableUI.GameResultDialog.setReplayButtonVisible(mode != GameMode.replay);
                tableUI.GameResultDialog.onSaveReplayButtonClicked += onSaveReplayButtonClickCallback; // 保存并退出
            }
        }
        private void disposeTableUI()
        {
            if (tableUI)
            {
                tableUI.onQuit -= onGameQuitButtonCallback;
                tableUI.onSurrender -= onSurrenderCallback;
                tableUI.GameResultDialog.onGameEnd -= onGameEndCallback;
                tableUI.GameResultDialog.onQuitButtonClicked -= onGameResultQuitGameButtonClicked;
                tableUI.GameResultDialog.onSaveReplayButtonClicked -= onSaveReplayButtonClickCallback;
            }
        }
        private void reset()
        {
            mode = GameMode.none;
            localPlayer = null;
            _battleTableManager = null;
            _tableUI = null;
            isPVE = false;
        }
        #endregion
        #region 事件
        /// <summary>
        /// 游戏开始后被调用
        /// </summary>
        public event Action onGameStart;
        /// <summary>
        /// 游戏结束事件出现后被调用，参数为：获胜的玩家
        /// </summary>
        public event Action<Player[]> onGameFinished;
        /// <summary>
        /// 游戏结束对话框出现后被调用
        /// </summary>
        public event Action onGameEnd;
        /// <summary>
        /// 退出游戏UI后被调用
        /// </summary>
        public event Action onGameQuit;
        #endregion
        #region 属性字段
        [TranslateMsg("游戏回放保存成功后的提醒")]
        public const string GAME_REPLAY_SAVED = "回放保存成功";
        [TranslateMsg("开始游戏时，读取卡池失败时的报错")]
        public const string GAME_CREATE_ERROR_LOAD_CARDS_FAILED = "开始游戏失败，无法读取游戏所需的卡池。";
        [TranslateMsg]
        public const string QUIT_GAME_QUESTION = "是否要结束本场游戏？";
        [TranslateMsg("尝试认输时的提醒")]
        public const string SURRENDER_QUESTION = "确定要向对手认输吗？";
        public CardEngine game { get; private set; } = null;
        public Player localPlayer { get; private set; } = null;
        public bool isPVE;
        public GameMode mode
        {
            get { return _mode; }
            private set { _mode = value; }
        }
        public NetworkingLogic nwLogic
        {
            get { return _nwLogic; }
            set { _nwLogic = value; }
        }
        public MainManager main => getManager<MainManager>();
        public CardManager cards => main.card;
        public LangManager lang => main.lang;
        private MainSceneCtrl scene => this.findInstance<MainSceneCtrl>();
        Task gameTask { get; set; } = null;
        private BattleTableManager battleTableManager => _battleTableManager;
        private TableUI tableUI => _tableUI;
        private BattleTableManager _battleTableManager;
        private TableUI _tableUI;
        [SerializeField]
        GameMode _mode = GameMode.none;
        [SerializeField]
        HearthstoneOption _option = new HearthstoneOption();
        [SerializeField]
        Main _mainUI;
        [SerializeField]
        bool _useTestDeck = false;
        [SerializeField]
        Deck _testDeck;
        [SerializeField]
        bool _dummyAI = false;
        [SerializeField]
        NetworkingLogic _nwLogic;
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
