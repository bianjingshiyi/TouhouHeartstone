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
using BJSYGameCore.UI;
using TouhouCardEngine.Interfaces;
using UnityEngine.UI;
using Card = TouhouCardEngine.Card;
using System.Text.RegularExpressions;
using NitoriNetwork.Common;
using MukioI18n;
using MongoDB.Bson;

namespace Game
{
    public partial class GameManager : Manager
    {
        [SerializeField]
        bool _useTimeAsRandomSeed = true;
        [SerializeField]
        GameOption _option = new GameOption();
        [SerializeField]
        Main _mainUI;
        [SerializeField]
        UI.Game _ui;
        public UI.Game ui
        {
            get { return _ui; }
        }
        public THHGame game { get; private set; } = null;
        public THHPlayer localPlayer { get; private set; } = null;
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
        GameMode _mode = GameMode.none;
        public GameMode mode
        {
            get { return _mode; }
            private set { _mode = value; }
        }
        private void Start()
        {
            if (_useDefaultDeck && _defaultDeck != null)
                _deck = _defaultDeck.toIdArray();
            if (_useTestDeck && _testDeck != null)
                _deck = _testDeck.toIdArray();
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
        #region 外部接口
        public Task startLocalGame(RoomInfo room)
        {
            //创建游戏
            THHGame game = createGame(room);
            //初始化本地玩家，AI，UI
            foreach (var player in game.players)
            {
                if (player.id == room.playerList[0].RoomID) // 暂时的
                {
                    localPlayer = player;
                    displayGameUI(game, localPlayer);
                }
                else
                    new AI(game, player); //AI玩家用AI
            }
            mode = GameMode.local;
            //开始游戏
            return runGame(game);
        }
        [SerializeField]
        ClientManager _client;
        public ClientManager client
        {
            get { return _client; }
            set { _client = value; }
        }
        /// <summary>
        /// [RPC] 开始远程游戏
        /// </summary>
        /// <param name="room"></param>
        public void startRemoteGame(RoomInfo room)
        {
            bool headless = _mainUI == null;
            startRemoteGame(room, headless);
        }
        public Task startRemoteGame(RoomInfo room, bool headless)
        {
            return startRemoteGame(client, room, headless);
        }
        public Task startRemoteGame(ClientManager client, RoomInfo room, bool headless = false)
        {
            //创建游戏
            THHGame game = createGame(room);
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
            mode = GameMode.remote;
            return runGame(game);
        }
        public Task startReplayGame(Replay rep)
        {
            THHGame game = createGame(rep.option, rep.players);
            new ReplayPlayer(rep, game);
            localPlayer = game.players.First();
            displayGameUI(game, localPlayer);
            mode = GameMode.replay;
            return runGame(game);
        }
        public event Action onGameStart;
        #endregion
        #region 内部功能
        THHGame createGame(RoomInfo room)
        {
            if (room._persistDataList.Count > room.runtimeDic.Count)
                room.deserialize();
            return createGame(room.getOption(), room.playerList);
        }
        THHGame createGame(GameOption option, IEnumerable<RoomPlayerInfo> playerList)
        {
            THHGame game = new THHGame(option, getManager<CardManager>().GetCardDefines())
            {
                answers = new GameObject(nameof(AnswerManager)).AddComponent<AnswerManager>(),
                triggers = new GameObject(nameof(TriggerManager)).AddComponent<TriggerManager>(),
                time = new GameObject(nameof(TimeManager)).AddComponent<TimeManager>(),
                logger = new ULogger()
            };
            game.answers.game = game;
            foreach (var info in playerList)
            {
                var deck = info.getDeck();
                checkDeckValid(game, deck);
#if UNITY_EDITOR
                //检查卡组合法性
                if (_useTestDeck)
                    deck = _testDeck.toIdArray();
#endif
                THHPlayer player = game.createPlayer(info.RoomID, info.name, game.getDefine(deck[0]) as MasterCardDefine,
                    deck.Skip(1).Select(id => game.getDefine(id)));
            }
            return game;
        }
        Task runGame(THHGame game, bool recordReplay = true)
        {
            this.game = game;
            if (recordReplay)
                _replaySystem.startRecord(game, game.players.Select(p =>
                {
                    RoomPlayerData playerData = new RoomPlayerData(p.id, p.name, RoomPlayerType.human);
                    playerData.setDeck(new int[] { p.master.define.id }.Concat(p.deck.Select(c => c.define.id)).ToArray());
                    return playerData;
                }).ToArray());
            onGameStart?.Invoke();
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
        private void checkDeckValid(THHGame game, int[] deck)
        {
            if (deck == null)
                return;
            if (game.getDefine(deck[0]) == null)
            {
                UberDebug.LogError("非法角色ID" + deck[0] + "被替换为灵梦");
                deck[0] = game.getDefine<Reimu>().id;
            }
            for (int i = 1; i < deck.Length; i++)
            {
                if (game.getDefine(deck[i]) == null)
                {
                    UberDebug.LogError("非法随从ID" + deck[i] + "被替换为小野菊");
                    deck[i] = game.getDefine<RashFairy>().id;
                }
            }
        }
        /// <summary>
        /// 显示游戏UI，并指定本地玩家
        /// </summary>
        /// <param name="localPlayer"></param>
        private void displayGameUI(THHGame game, THHPlayer localPlayer)
        {
            _mainUI.Game.GameResultDialog.hide();
            //本地玩家用UI
            _mainUI.display(_mainUI.Game);
            getManager<TableManager>().setGame(game, localPlayer);
            _mainUI.Game.Table.setGame(game, localPlayer);

            game.triggers.onEventBefore += t1 => gameTriggerEventCache.cache(onEventBefore, t1);
            game.triggers.onEventAfter += t1 => gameTriggerEventCache.cache(onEventAfter, t1);
            ui.LogScrollView.shrink();
            ui.LogScrollView.LogList.clearItems();
        }
        ActionEventCache gameTriggerEventCache { get; } = new ActionEventCache();
        void onEventBefore(IEventArg obj)
        {
            switch (obj)
            {
                case THHPlayer.UseEventArg _:
                //case THHPlayer.SetGemEventArg _:
                //case THHPlayer.SetMaxGemEventArg _:
                case THHPlayer.BurnEventArg _:
                case THHPlayer.FatigueEventArg _:
                case THHPlayer.DiscardEventArg _:
                case THHPlayer.CreateTokenEventArg _:
                case THHPlayer.RebornEventArg _:
                case THHCard.AttackEventArg _:
                //case THHCard.DamageEventArg _:
                //case THHCard.SetArmorEventArg _:
                //case THHCard.HealEventArg _:
                //case THHCard.SilenceEventArg _:
                case THHCard.DeathEventArg _:
                case THHCard.SecretTriggerEventArg _:
                    var item = ui.LogScrollView.LogList.addItem();
                    item.setProp("event", obj);
                    item.onPointerEnter.set(onLogListItemEnter);
                    item.onPointerExit.set(onLogListItemExit);
                    break;
                case THHPlayer.DrawEventArg _:
                    if (obj is IDescribableEventArg describable)
                    {
                        // string info = describable.toString(game, localPlayer);
                        string info = lang._(
                            describable.localizeTemplateString(game, localPlayer), 
                            describable.localizeStringArgs(game, localPlayer)
                        );
                        if (info != null)
                        {
                            var DrawItem = ui.LogScrollView.LogList.addItem();
                            DrawItem.setProp("event", obj);
                            DrawItem.onPointerEnter.set(onLogListItemEnter);
                            DrawItem.onPointerExit.set(onLogListItemExit);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        void onEventAfter(IEventArg obj)
        {
            
                var item = ui.LogScrollView.LogList.getItems().FirstOrDefault(i => i.getProp("event") == obj);
                if (item == null)
                    return;
                foreach (var parentItem in ui.LogScrollView.LogList.getItems())
                {
                    int depth = 0;
                    if (isChildOf(parentItem.getProp<IEventArg>("event"), item.getProp<IEventArg>("event"), ref depth))
                    {
                        item.rectTransform.setWidth(item.rectTransform.getWidth() + 20 * depth);
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
                LayoutRebuilder.ForceRebuildLayoutImmediate(ui.LogScrollView.LogList.rectTransform);
                ui.LogScrollView.asScrollRect.verticalNormalizedPosition = 0;
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
                    case THHPlayer.DrawEventArg draw:
                        item.IconImage.sprite = item.drawIcon;
                        item.Text.text = lang._(GAME_MANAGER_DRAW);
                        item.Image.sprite = getManager<TableManager>().getSkin(draw.player == localPlayer ? draw.card : draw.player.master).image;
                        item.BorderImage.color = draw.player == localPlayer ? item.selfColor : item.oppoColor;
                        break;
                    case THHPlayer.BurnEventArg burn:

                        item.IconImage.sprite = item.burnIcon;
                        item.Text.text = lang._(GAME_MANAGER_BURN);
                        item.Image.sprite = getManager<TableManager>().getSkin(burn.card).image;
                        item.BorderImage.color = burn.player == localPlayer ? item.selfColor : item.oppoColor;
                        break;
                    case THHPlayer.FatigueEventArg fatigue:
                        item.IconImage.sprite = item.fatigueIcon;
                        item.Text.text = lang._(GAME_MANAGER_FATIGUE);
                        item.Image.sprite = getManager<TableManager>().getSkin(fatigue.player.master).image;
                        item.BorderImage.color = fatigue.player == localPlayer ? item.selfColor : item.oppoColor;
                        break;
                    case THHPlayer.DiscardEventArg discard:
                        item.IconImage.sprite = item.discardIcon;
                        item.Text.text = lang._(GAME_MANAGER_DISCARD);
                        item.Image.sprite = getManager<TableManager>().getSkin(discard.cards[0]).image;
                        item.BorderImage.color = discard.player == localPlayer ? item.selfColor : item.oppoColor;
                        break;
                    case THHPlayer.UseEventArg use:
                        item.IconImage.sprite = item.useIcon;
                        item.Text.text = lang._(GAME_MANAGER_USE);
                        item.Image.sprite = use.card.hasKeyword(game, KEYWORD.SECRET) ?
                            getManager<TableManager>().getSkin(use.card.getOwner().master).image :
                            getManager<TableManager>().getSkin(use.card).image;
                        item.BorderImage.color = use.player == localPlayer ? item.selfColor : item.oppoColor;
                        break;
                    case THHPlayer.CreateTokenEventArg token:
                        item.IconImage.sprite = item.rebornIcon;
                        item.Text.text = lang._(GAME_MANAGER_TOKEN);
                        item.Image.sprite = getManager<TableManager>().getSkin(token.card).image;
                        item.BorderImage.color = token.player == localPlayer ? item.selfColor : item.oppoColor;
                        break;
                    case THHPlayer.RebornEventArg reborn:
                        item.IconImage.sprite = item.rebornIcon;
                        item.Text.text = lang._(GAME_MANAGER_REBORN);
                        item.Image.sprite = getManager<TableManager>().getSkin(reborn.card).image;
                        item.BorderImage.color = reborn.player == localPlayer ? item.selfColor : item.oppoColor;
                        break;
                    case THHCard.AttackEventArg attack:
                        item.IconImage.sprite = item.attackIcon;
                        item.Text.text = lang._(GAME_MANAGER_ATTACK);
                        item.Image.sprite = getManager<TableManager>().getSkin(attack.card).image;
                        item.BorderImage.color = attack.card.getOwner() == localPlayer ? item.selfColor : item.oppoColor;
                        break;
                case THHCard.DamageEventArg damage:
                    item.IconImage.sprite = item.damageIcon;
                    item.Text.text = "-" + damage.infoDic.First().Value.damagedValue;
                    item.Image.sprite = getManager<TableManager>().getSkin(damage.infoDic.First().Key).image;
                    item.BorderImage.color = damage.infoDic.First().Key.getOwner() == localPlayer ? item.selfColor : item.oppoColor;
                    break;
                case THHCard.SetArmorEventArg armor:
                    item.IconImage.sprite = item.armorIcon;
                    item.Text.text = "=>" + armor.value;
                    item.Image.sprite = getManager<TableManager>().getSkin(armor.card).image;
                    item.BorderImage.color = armor.card.getOwner() == localPlayer ? item.selfColor : item.oppoColor;
                    break;
                case THHCard.HealEventArg heal:
                    item.IconImage.sprite = item.healIcon;
                    item.Text.text = "+" + heal.infoDic.First().Value.healedValue;
                    item.Image.sprite = getManager<TableManager>().getSkin(heal.infoDic.First().Key).image;
                    item.BorderImage.color = heal.infoDic.First().Key.getOwner() == localPlayer ? item.selfColor : item.oppoColor;
                    break;
                //case THHCard.SilenceEventArg silence:
                //    item.IconImage.sprite = item.healIcon;
                //    item.Text.text = lang._(GAME_MANAGER_SILENCE);
                //    item.Image.sprite = getManager<TableManager>().getSkin(silence.cards[0]).image;
                //    item.BorderImage.color = silence.cards[0].getOwner() == localPlayer ? item.selfColor : item.oppoColor;
                //    break;
                case THHCard.SecretTriggerEventArg secret:
                        item.IconImage.sprite = item.secretIcon;
                        item.Text.text = lang._(GAME_MANAGER_SECRET);
                        item.Image.sprite = getManager<TableManager>().getSkin(secret.card).image;
                        item.BorderImage.color = secret.card.getOwner() == localPlayer ? item.selfColor : item.oppoColor;
                        break;
                    case THHCard.DeathEventArg death:
                        item.IconImage.sprite = item.deathIcon;
                        item.Text.text = lang._(GAME_MANAGER_DEATH);
                        item.Image.sprite = getManager<TableManager>().getSkin(death.infoDic.First().Key).image;
                        item.BorderImage.color = death.infoDic.First().Key.getOwner() == localPlayer ? item.selfColor : item.oppoColor;
                        break;
                    default:
                        break;
                }
            
            
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
        private void onSaveReplayButtonClick()
        {
            _ = getManager<MainManager>().fileManager.saveFile(
                _replaySystem.getSaveDir() + "/" + _replaySystem.getSaveFileName(_replaySystem.replay) + "." + _replaySystem.getSaveFileExtension(),
                _replaySystem.replay.players.Select(p => p.getDeck()[0]).ToArray().ToJson() + "\r\n" +
                _replaySystem.replay.ToJson());
        }
        #region Events
        void onLogListItemEnter(LogListItem item)
        {
            var arg = item.getProp<IEventArg>("event");
            if (arg is IDescribableEventArg describable)
            {
                ui.LogScrollView.LogInfoHorizantal.display();
                string info = lang._(describable.localizeTemplateString(game, localPlayer), describable.localizeStringArgs(game, localPlayer));
                info = Regex.Replace(info, @"{card:(?<id>\d+)}", m => int.TryParse(m.Groups["id"].Value, out var id) ?
                    getManager<CardManager>().getSkin(id).name : "未知卡牌");
                ui.LogScrollView.LogInfoHorizantal.Text.text = info;
                if (describable.getCard(game, localPlayer) != null)
                {
                    ui.LogScrollView.LogInfoHorizantal.InfoCard.display();
                    ui.LogScrollView.LogInfoHorizantal.InfoCard.Card.setCard(describable.getCard(game, localPlayer).define as CardDefine,
                        getManager<CardManager>().getSkin(describable.getCard(game, localPlayer) as Card));
                }
                else
                    ui.LogScrollView.LogInfoHorizantal.InfoCard.hide();
                ui.LogScrollView.LogInfoHorizantal.TargetGridList.clearItems();
                if (describable.getTargets(game, localPlayer) != null && describable.getTargets(game, localPlayer).Length > 0)
                {
                    foreach (var target in describable.getTargets(game, localPlayer))
                    {
                        var targetItem = ui.LogScrollView.LogInfoHorizantal.TargetGridList.addItem();
                        targetItem.Card.setCard(target.define as CardDefine, getManager<CardManager>().getSkin(target as Card));
                    }
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
    }
    public enum GameMode
    {
        none,
        local,
        remote,
        replay
    }
}
