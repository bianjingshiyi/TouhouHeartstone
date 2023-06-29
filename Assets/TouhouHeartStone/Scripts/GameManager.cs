using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BJSYGameCore;
using BJSYGameCore.UI;
using Controllers;
using MongoDB.Bson;
using MukioI18n;
using NitoriNetwork.Common;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using TouhouHeartStone;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Card = TouhouCardEngine.Card;
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
        [SerializeField]
        UI.Game _ui;
        public UI.Game ui
        {
            get { return _ui; }
        }
        public CardEngine game { get; private set; } = null;
        public Player localPlayer { get; private set; } = null;
        Task gameTask { get; set; } = null;
        LangManager lang => getManager<LangManager>();

        protected override void onAwake()
        {
            base.onAwake();
            TaskExceptionHandler.register();
            _mainUI = this.findInstance<Main>();
            if (_mainUI != null)
            {
                _mainUI.game = this;
                _mainUI.Game.GameResultDialog._ButtonGlassButtonBlack.asButton.onClick.set(quitGame);
                _mainUI.Game.GameResultDialog.onSaveReplayButtonClick += quitGame; // 保存并退出
                _ui = _mainUI.Game;
            }
            ui.GameResultDialog.onSaveReplayButtonClick += onSaveReplayButtonClick;
            ui.LogScrollView.LogList.onEnableLogListItem += onEnableLogListItem;
            ui.LogScrollView.LogList.onDisableLogListItem += onDisableLogListItem;
            ui.LogScrollView.LogList.onUpdateLogListItem += onUpdateLogListItem;
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
                gameTriggerEventCache.poll();
            }
        }

        #region 事件回调
        private void onEnableLogListItem(int index, RectTransform rect)
        {
            var item = rect.GetComponent<LogListItem>();
            item.onPointerEnter.add(onLogListItemEnter);
            item.onPointerExit.add(onLogListItemExit);
        }
        private void onDisableLogListItem(int index, RectTransform rect)
        {
            var item = rect.GetComponent<LogListItem>();
            item.onPointerEnter.remove(onLogListItemEnter);
            item.onPointerExit.remove(onLogListItemExit);
        }
        private void onUpdateLogListItem(int index, RectTransform rect)
        {
            var item = rect.GetComponent<LogListItem>();
            var logItem = logItems[index];
            var obj = logItem.arg;
            item.SetLevel(logItem.level);
            switch (obj)
            {
                //case THHPlayer.SetGemEventArg setGem:
                //    item.IconImage.sprite = item.gemIcon;
                //    item.Text.text = "=>" + setGem.value;
                //    item.Image.sprite = getManager<TableManager>().getSkin(game.getDefine<LuckyCoin>()).image;
                //    item.BorderImage.color = setGem.player == localPlayer ? item.selfColor : item.oppoColor;
                //    break;
                //case THHPlayer.SetMaxGemEventArg setMaxGem:
                //    item.IconImage.sprite = item.gemIcon;
                //    item.Text.text = "=>" + setMaxGem.value;
                //    item.Image.sprite = getManager<TableManager>().getSkin(game.getDefine<LuckyCoin>()).image;
                //    item.BorderImage.color = setMaxGem.player == localPlayer ? item.selfColor : item.oppoColor;
                //    break;
                case HearthStoneRule.DrawEventArg draw:
                    item.IconImage.sprite = item.drawIcon;
                    item.Text.text = lang._(GAME_MANAGER_DRAW);
                    item.Image.sprite = getManager<TableManager>().getSkin(draw.player == localPlayer ? draw.card : draw.player.getMaster()).image;
                    item.BorderImage.sprite = draw.player == localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case HearthStoneRule.BurnEventArg burn:

                    item.IconImage.sprite = item.burnIcon;
                    item.Text.text = lang._(GAME_MANAGER_BURN);
                    item.Image.sprite = getManager<TableManager>().getSkin(burn.card).image;
                    item.BorderImage.sprite = burn.player == localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case HearthStoneRule.FatigueEventArg fatigue:
                    item.IconImage.sprite = item.fatigueIcon;
                    item.Text.text = lang._(GAME_MANAGER_FATIGUE);
                    item.Image.sprite = getManager<TableManager>().getSkin(fatigue.player.getMaster()).image;
                    item.BorderImage.sprite = fatigue.player == localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case THHCard.DiscardEventArg discard:
                    item.IconImage.sprite = item.discardIcon;
                    item.Text.text = lang._(GAME_MANAGER_DISCARD);
                    item.Image.sprite = getManager<TableManager>().getSkin(discard.card).image;
                    item.BorderImage.sprite = discard.player == localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case HearthStoneRule.UseEventArg use:
                    item.IconImage.sprite = item.useIcon;
                    item.Text.text = lang._(GAME_MANAGER_USE);
                    item.Image.sprite = use.card.hasKeyword(game, KEYWORD.SECRET) ?
                        getManager<TableManager>().getSkin(use.card.getOwner().getMaster()).image :
                        getManager<TableManager>().getSkin(use.card).image;
                    item.BorderImage.sprite = use.player == localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case HearthStoneRule.CreateTokenEventArg token:
                    item.IconImage.sprite = item.rebornIcon;
                    item.Text.text = lang._(GAME_MANAGER_TOKEN);
                    item.Image.sprite = getManager<TableManager>().getSkin(token.card).image;
                    item.BorderImage.sprite = token.player == localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case HearthStoneRule.RebornEventArg reborn:
                    item.IconImage.sprite = item.rebornIcon;
                    item.Text.text = lang._(GAME_MANAGER_REBORN);
                    item.Image.sprite = getManager<TableManager>().getSkin(reborn.card).image;
                    item.BorderImage.sprite = reborn.player == localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case THHCard.AttackEventArg attack:
                    item.IconImage.sprite = item.attackIcon;
                    item.Text.text = lang._(GAME_MANAGER_ATTACK);
                    item.Image.sprite = getManager<TableManager>().getSkin(attack.card).image;
                    item.BorderImage.sprite = attack.card.getOwner() == localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case THHCard.DamageEventArg damage:
                    item.IconImage.sprite = item.damageIcon;
                    item.Text.text = "-" + damage.damagedValue;
                    item.Image.sprite = getManager<TableManager>().getSkin(damage.target).image;
                    item.BorderImage.sprite = damage.target.getOwner() == localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case THHCard.SetArmorEventArg armor:
                    item.IconImage.sprite = item.armorIcon;
                    item.Text.text = "=>" + armor.value;
                    item.Image.sprite = getManager<TableManager>().getSkin(armor.card).image;
                    item.BorderImage.sprite = armor.card.getOwner() == localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case THHCard.HealEventArg heal:
                    if (heal.infoDic.Count > 0)
                    {
                        var firstHeal = heal.infoDic.First();
                        var card = firstHeal.Key;
                        var healInfo = firstHeal.Value;
                        item.IconImage.sprite = item.healIcon;
                        item.Text.text = "+" + healInfo.healedValue;
                        item.Image.sprite = getManager<TableManager>().getSkin(card).image;
                        item.BorderImage.sprite = card.getOwner() == localPlayer ? item.selfsprite : item.opposprite;
                    }
                    else
                    {
                        item.IconImage.sprite = item.healIcon;
                        item.Text.text = "+0";
                        item.Image.sprite = null;
                        item.BorderImage.sprite = null;
                    }
                    break;
                case THHCard.SilenceEventArg silence:
                    item.IconImage.sprite = item.silenceIcon;
                    item.Text.text = lang._(GAME_MANAGER_SILENCE);
                    item.Image.sprite = getManager<TableManager>().getSkin(silence.cards[0]).image;
                    item.BorderImage.color = silence.cards[0].getOwner() == localPlayer ? item.selfColor : item.oppoColor;
                    break;
                case THHCard.SecretTriggerEventArg secret:
                    item.IconImage.sprite = item.secretIcon;
                    item.Text.text = lang._(GAME_MANAGER_SECRET);
                    item.Image.sprite = getManager<TableManager>().getSkin(secret.card).image;
                    item.BorderImage.sprite = secret.card.getOwner() == localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case THHCard.DeathEventArg death:
                    item.IconImage.sprite = item.deathIcon;
                    item.Text.text = lang._(GAME_MANAGER_DEATH);
                    item.Image.sprite = getManager<TableManager>().getSkin(death.card).image;
                    item.BorderImage.sprite = death.player == localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case THHPlayer.DiscoverEventArg discover:
                    item.Text.text = lang._(GAME_MANAGER_DISCOVER);
                    break;
                case THHPlayer.RevealDuelEventArg revealDuel:
                    item.Text.text = lang._(GAME_MANAGER_REVEAL_DUEL);
                    item.BorderImage.sprite = revealDuel.player == localPlayer ? item.selfsprite : item.opposprite;
                    break;
                default:
                    break;
            }
        }
        #endregion
        #region 外部接口
        public async Task startLocalGame(RoomData room)
        {
            //创建游戏
            CardEngine game = await createGame(room);
            mode = GameMode.local;
            ui.showSurrenderButton(false);
            //初始化本地玩家，AI，UI
            foreach (var player in game.players)
            {
                if (player.id == room.playerDataList[0].id) // 暂时的
                {
                    localPlayer = player;
                    displayGameUI(game, localPlayer);
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
            ui.showSurrenderButton(true);
            //将游戏与网络连接
            game.answers.client = client;
            //初始化本地玩家与UI
            foreach (var player in game.players)
            {
                if (client.id == player.id)
                {
                    localPlayer = player;
                    if (!headless)
                        displayGameUI(game, localPlayer);
                }
            }
            await runGame(game);
        }
        public async Task startReplayGame(Replay rep)
        {
            CardEngine game = await createGame(rep.option, rep.players);
            mode = GameMode.replay;
            new ReplayPlayer(rep, game);
            localPlayer = game.players.First();
            displayGameUI(game, localPlayer);
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
            List<ActionDefine> actionDefines = new List<ActionDefine>();
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

            foreach (var define in actionDefines)
            {
                game.addActionDefine(define.defineName, define);
            }

            Rule rule = new HearthStoneRule(cardDict,buffDict);
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
            if (_mainUI != null)
            {
                getManager<TableManager>().setGame(null, null);
                _mainUI.display(_mainUI.MainMenu);
            }
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
        private void addLogItem(IEventArg arg)
        {
            var item = new LogItem()
            {
                arg = arg
            };
            logItems.Add(item);
            ui.LogScrollView.LogList.updateLogListCount(logItems.Count);
        }
        private async Task LoadExternalCardsAndBuffs(HearthstoneOption option,
            Dictionary<long, Dictionary<int, CardDefine>> cardDict,
            Dictionary<long, Dictionary<int, BuffDefine>> buffDict,
            List<ActionDefine> actionDefines)
        {
            var cardManager = getManager<CardManager>();
            // 尝试从本地获取卡组，否则读取缓存卡组。
            foreach (var pool in option.cardPools)
            {
                long poolId = pool.cardPoolId;

                if (cards.isBuiltinCardPool(poolId))
                {
                    cardDict.Add(poolId, cards.GetCardDefines().ToDictionary(c => c.id));
                    continue;
                }
                CardPool cardPool = null;
                var poolInfo = cards.getCardPoolInfo(poolId);
                // 尝试获取本地卡池的卡牌定义（要求版本正确）。
                if (poolInfo != null && poolInfo.editTime == pool.version)
                {
                    if (!cards.tryGetCardPool(pool.GetReference(), out cardPool))
                    {
                        if (cards.getCardPoolPath(pool.GetReference()) is string path)
                            cardPool = (await cards.loadCardPool(path)).cardPool;
                    }
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
                    // 加载自定义动作定义。
                    foreach (var define in cardPool.actionDefineList)
                    {
                        actionDefines.Add(define);
                    }
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
        private void displayGameUI(CardEngine game, Player localPlayer)
        {
            _mainUI.Game.GameResultDialog.hide();
            //本地玩家用UI
            _mainUI.display(_mainUI.Game);
            getManager<TableManager>().setGame(game, localPlayer);
            _mainUI.Game.Table.setGame(game, localPlayer);

            game.triggers.onEventBefore += t1 => gameTriggerEventCache.cache(onEventBefore, t1);
            game.triggers.onEventAfter += t1 => gameTriggerEventCache.cache(onEventAfter, t1);
            ui.logPanel.shrink();

            logItems.Clear();
            ui.LogScrollView.LogList.updateLogListCount(0);
        }
        ActionEventCache gameTriggerEventCache { get; } = new ActionEventCache();
        void onEventBefore(IEventArg obj)
        {
            switch (obj)
            {
                case HearthStoneRule.UseEventArg _:
                //case THHPlayer.SetGemEventArg _:
                //case THHPlayer.SetMaxGemEventArg _:
                case HearthStoneRule.BurnEventArg _:
                case HearthStoneRule.FatigueEventArg _:
                case THHCard.DiscardEventArg _:
                case HearthStoneRule.CreateTokenEventArg _:
                case HearthStoneRule.RebornEventArg _:
                case THHCard.AttackEventArg _:
                case THHCard.DamageEventArg _:
                case THHCard.SetArmorEventArg _:
                case THHCard.HealEventArg _:
                case THHCard.SilenceEventArg _:
                case THHCard.DeathEventArg _:
                case THHPlayer.DiscoverEventArg _:
                case THHPlayer.RevealDuelEventArg _:
                case THHCard.SecretTriggerEventArg _:
                    addLogItem(obj);
                    break;
                case HearthStoneRule.DrawEventArg _:
                    if (obj is IDescribableEventArg describable)
                    {
                        // string info = describable.toString(game, localPlayer);
                        string info = lang._(
                            describable.localizeTemplateString(game, localPlayer),
                            describable.localizeStringArgs(game, localPlayer)
                        );
                        if (info != null)
                        {
                            addLogItem(obj);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        void onEventAfter(IEventArg obj)
        {
            var item = logItems.FirstOrDefault(i => i.arg == obj);
            if (item == null)
                return;
            foreach (var parentItem in logItems)
            {
                int depth = 0;
                if (isChildOf(parentItem.arg, item.arg, ref depth))
                {
                    item.level = depth;
                    break;
                }
            }
            bool isChildOf(IEventArg parent, IEventArg child, ref int depth)
            {
                if (depth > 100)
                    throw new StackOverflowException();
                if (child == null)
                    return false;
                if (parent == null)
                    return false;
                if (child.parent == null)
                    return false;
                if (child.parent == parent)
                {
                    depth++;
                    return true;
                }
                else
                {
                    depth++;
                    return isChildOf(parent, child.parent, ref depth);
                }
            }
            ui.LogScrollView.asScrollRect.verticalNormalizedPosition = 0;
        }
        [TranslateMsg]
        private const string GAME_MANAGER_DEATH = "死亡";
        [TranslateMsg]
        private const string GAME_MANAGER_SECRET = "陷阱";
        [TranslateMsg]
        private const string GAME_MANAGER_SILENCE = "封印";
        [TranslateMsg]
        private const string GAME_MANAGER_ATTACK = "攻击";
        [TranslateMsg]
        private const string GAME_MANAGER_REBORN = "复活";
        [TranslateMsg]
        private const string GAME_MANAGER_TOKEN = "召唤";
        [TranslateMsg]
        private const string GAME_MANAGER_USE = "使用";
        [TranslateMsg]
        private const string GAME_MANAGER_DISCARD = "弃牌";
        [TranslateMsg]
        private const string GAME_MANAGER_FATIGUE = "疲劳";
        [TranslateMsg]
        private const string GAME_MANAGER_BURN = "爆牌";
        [TranslateMsg]
        private const string GAME_MANAGER_DRAW = "抽牌";
        [TranslateMsg]
        private const string GAME_MANAGER_DISCOVER = "发现";
        [TranslateMsg]
        private const string GAME_MANAGER_REVEAL_DUEL = "信仰对决";
        private async void onSaveReplayButtonClick()
        {
            await getManager<MainManager>().fileManager.savePersistentFile(
                _replaySystem.getSaveDir() + "/" + _replaySystem.getSaveFileName(_replaySystem.replay) + "." + _replaySystem.getSaveFileExtension(),
                _replaySystem.replay.players.Select(p => p.getDeck().masterCardRef).ToArray().ToJson() + "\r\n" +
                _replaySystem.replay.ToJson());
        }
        #region Events
        private async void onLogListItemEnter(LogListItem item)
        {
            int itemIndex = ui.LogScrollView.LogList.getLogItemIndex(item.rectTransform);
            var logItem = logItems[itemIndex];
            var arg = logItem.arg;
            if (arg is IDescribableEventArg describable)
            {
                ui.LogScrollView.LogInfoHorizantal.display();
                string info = lang._(describable.localizeTemplateString(game, localPlayer), describable.localizeStringArgs(game, localPlayer));
                info = Regex.Replace(info, @"{card:(?<poolId>-?\d+),(?<id>-?\d+)}",
                    m =>
                    {
                        if (int.TryParse(m.Groups["id"].Value, out var id) && long.TryParse(m.Groups["poolId"].Value, out var poolId))
                        {
                            var cardPoolRef = cards.getCardPoolReferenceInSet(poolId, game?.getOption()?.cardPools);
                            return getManager<CardManager>().getDefaultSkin(cardPoolRef, id).name;
                        }
                        return "未知卡牌";
                    });
                ui.LogScrollView.LogInfoHorizantal.Text.text = info;
                if (describable.getCard(game, localPlayer) != null)
                {
                    ui.LogScrollView.LogInfoHorizantal.InfoCard.display();
                    CardDefine card = describable.getCard(game, localPlayer).define;
                    await ui.LogScrollView.LogInfoHorizantal.InfoCard.Card.setCardDefine(card, main.card);
                }
                else
                    ui.LogScrollView.LogInfoHorizantal.InfoCard.hide();
                ui.LogScrollView.LogInfoHorizantal.TargetGridList.clearItems();
                if (describable.getTargets(game, localPlayer) != null && describable.getTargets(game, localPlayer).Length > 0)
                {
                    ui.LogScrollView.LogInfoHorizantal.TargetGridList.gameObject.SetActive(true);
                    foreach (var target in describable.getTargets(game, localPlayer))
                    {
                        if (target == null)
                            continue;
                        var targetItem = ui.LogScrollView.LogInfoHorizantal.TargetGridList.addItem();
                        CardDefine card = target.define;
                        await targetItem.Card.setCardDefine(card, main.card);
                    }
                }
                else
                {
                    ui.LogScrollView.LogInfoHorizantal.TargetGridList.gameObject.SetActive(false);
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(ui.LogScrollView.LogInfoHorizantal.TargetGridList.rectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(ui.LogScrollView.LogInfoHorizantal.LogInfoVerticalVerticalLayoutGroup.GetComponent<RectTransform>());
                LayoutRebuilder.ForceRebuildLayoutImmediate(ui.LogScrollView.LogInfoHorizantal.rectTransform);
            }
        }
        void onLogListItemExit(LogListItem item)
        {
            ui.LogScrollView.LogInfoHorizantal.hide();
        }
        #endregion
        protected void OnGUI()
        {
#if UNITY_EDITOR
            Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0, 3, GUILayout.Width(300));
#endif
        }
        #region 属性字段
        [TranslateMsg("开始游戏时，读取卡池失败时的报错")]
        public const string GAME_CREATE_ERROR_LOAD_CARDS_FAILED = "开始游戏失败，无法读取游戏所需的卡池。";
        public MainManager main => getManager<MainManager>();
        private List<LogItem> logItems = new List<LogItem>();
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
