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
namespace Game
{
    public class GameManager : Manager
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
        protected override void onAwake()
        {
            base.onAwake();
            TaskExceptionHandler.register();
            _mainUI = this.findInstance<Main>();
            _mainUI.game = this;
            _mainUI.Game.GameResultDialog.ButtonGlassButtonBlack.asButton.onClick.set(quitGame);

            _ui = getManager<UIManager>().getObject<UI.Game>();
        }
        [SerializeField]
        bool _useDefaultDeck = true;
        [SerializeField]
        Deck _defaultDeck;
        [SerializeField]
        bool _useTestDeck = true;
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
        bool _testRandomDeck = true;
        private void Start()
        {
#if !UNITY_EDITOR
            tryLoadDeckFromPrefs();
            if (_useDefaultDeck)
                _deck = _defaultDeck.toIdArray();
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
            if (_useDefaultDeck)
                _deck = _defaultDeck.toIdArray();
            if (_useTestDeck)
                _deck = _testDeck.toIdArray();
#endif
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
                    return;
                }
                eventCache.poll();
            }
        }

        public event Action onGameStart;

        /// <summary>
        /// 创建并开始本地游戏
        /// </summary>
        public void startLocalGame()
        {
            _mainUI.Game.GameResultDialog.hide();
            if (_useTimeAsRandomSeed)
                _option.randomSeed = (int)DateTime.Now.ToBinary();
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
            localPlayer = game.createPlayer(1, "本地玩家", playerMaster as MasterCardDefine,
                playerDeck);
            THHPlayer aiPlayer = game.createPlayer(2, "AI", playerMaster as MasterCardDefine,
                deck.Skip(1).Select(id => game.getCardDefine(id)));
            displayGameUI();
            //AI玩家用AI
            new AI(game, aiPlayer);
            onGameStart?.Invoke();
            gameTask = game.run();
        }

        public void startLocalGame(RoomInfo room)
        {
            _mainUI.Game.GameResultDialog.hide();

            var option = room.getOption();
            if (option.randomSeed == 0)
            {
                var rand = new byte[4];
                System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(rand);
                option.randomSeed = BitConverter.ToInt32(rand, 0);
            }

            game = new THHGame(option, getManager<CardManager>().GetCardDefines())
            {
                answers = new GameObject(nameof(AnswerManager)).AddComponent<AnswerManager>(),
                triggers = new GameObject(nameof(TriggerManager)).AddComponent<TriggerManager>(),
                time = new GameObject(nameof(TimeManager)).AddComponent<TimeManager>(),
                logger = new UnityLogger()
            };
            (game.answers as AnswerManager).game = game;

            foreach (var info in room.playerList)
            {
                int[] deck = info.getDeck();
                checkDeckValid(deck);
#if UNITY_EDITOR
                //检查卡组合法性
                if (_useTestDeck)
                    deck = _testDeck.toIdArray();
#endif

                THHPlayer player = game.createPlayer(info.RoomID, info.name, game.getCardDefine(deck[0]) as MasterCardDefine,
                    deck.Skip(1).Select(id => game.getCardDefine(id)));
                if (info.RoomID == room.playerList[0].RoomID) // 暂时的
                {
                    localPlayer = player;
                    displayGameUI();
                }
                else
                    new AI(game, player); //AI玩家用AI
            }
            onGameStart?.Invoke();
            gameTask = game.run();
        }

        public void startRemoteGame(ClientManager client, GameOption option, RoomPlayerInfo[] players)
        {
            _mainUI.Game.GameResultDialog.hide();

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

                THHPlayer player = game.createPlayer(info.RoomID, info.name, game.getCardDefine(info.getDeck()[0]) as MasterCardDefine,
                    info.getDeck().Skip(1).Select(id => game.getCardDefine(id)));
                if (client.id == info.RoomID)
                {
                    localPlayer = player;
                    displayGameUI();
                }
            }
            onGameStart?.Invoke();
            gameTask = game.run();
        }

        public void startReplayGame(Replay rep)
        {
            _mainUI.Game.GameResultDialog.hide();

            game = new THHGame(rep.option, getManager<CardManager>().GetCardDefines())
            {
                answers = new GameObject(nameof(AnswerManager)).AddComponent<AnswerManager>(),
                triggers = new GameObject(nameof(TriggerManager)).AddComponent<TriggerManager>(),
                time = new GameObject(nameof(TimeManager)).AddComponent<TimeManager>(),
                logger = new ULogger()
            };
            (game.answers as AnswerManager).game = game;

            foreach (var player in rep.players)
            {
                checkDeckValid(player.Deck);
                checkDeckValid(new int[] { player.MasterCard });

                THHPlayer p = game.createPlayer(
                    player.ID,
                    player.Name,
                    game.getCardDefine(player.MasterCard) as MasterCardDefine,
                    player.Deck.Select(c => game.getCardDefine(c)));
            }

            new ReplayPlayer(rep, game);

            localPlayer = game.players.Last();
            displayGameUI();
            gameTask = game.run();
        }

        private void checkDeckValid(int[] deck)
        {
            if (deck == null)
                return;
            if (game.getCardDefine(deck[0]) == null)
            {
                UberDebug.LogError("非法角色ID" + deck[0] + "被替换为灵梦");
                deck[0] = game.getDefine<Reimu>().id;
            }
            for (int i = 1; i < deck.Length; i++)
            {
                if (game.getCardDefine(deck[i]) == null)
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
        private void displayGameUI()
        {
            //本地玩家用UI
            _mainUI.display(_mainUI.Game);
            getManager<TableManager>().setGame(game, localPlayer);
            _mainUI.Game.Table.setGame(game, localPlayer);

            game.triggers.onEventBefore += t1 => eventCache.cache(onEventBefore, t1);
            game.triggers.onEventAfter += t1 => eventCache.cache(onEventAfter, t1);
            ui.LogScrollView.shrink();
            ui.LogScrollView.LogList.clearItems();
        }
        ActionEventCache eventCache { get; } = new ActionEventCache();
        void onEventBefore(IEventArg obj)
        {
            switch (obj)
            {
                case THHPlayer.UseEventArg _:
                case THHPlayer.SetGemEventArg _:
                case THHPlayer.SetMaxGemEventArg _:
                case THHPlayer.DrawEventArg _:
                case THHPlayer.BurnEventArg _:
                case THHPlayer.FatigueEventArg _:
                case THHPlayer.DiscardEventArg _:
                case THHPlayer.CreateTokenEventArg _:
                case THHPlayer.RebornEventArg _:
                case THHCard.AttackEventArg _:
                case THHCard.DamageEventArg _:
                case THHCard.SetArmorEventArg _:
                case THHCard.HealEventArg _:
                case THHCard.SilenceEventArg _:
                case THHCard.DeathEventArg _:
                    var item = ui.LogScrollView.LogList.addItem();
                    item.setProp("event", obj);
                    item.onPointerEnter.set(onLogListItemEnter);
                    item.onPointerExit.set(onLogListItemExit);
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
                case THHPlayer.SetGemEventArg setGem:
                    item.IconImage.sprite = item.gemIcon;
                    item.Text.text = "=>" + setGem.value;
                    item.Image.sprite = getManager<TableManager>().getSkin(game.getDefine<LuckyCoin>()).image;
                    item.BorderImage.color = setGem.player == localPlayer ? item.selfColor : item.oppoColor;
                    break;
                case THHPlayer.SetMaxGemEventArg setMaxGem:
                    item.IconImage.sprite = item.gemIcon;
                    item.Text.text = "=>" + setMaxGem.value;
                    item.Image.sprite = getManager<TableManager>().getSkin(game.getDefine<LuckyCoin>()).image;
                    item.BorderImage.color = setMaxGem.player == localPlayer ? item.selfColor : item.oppoColor;
                    break;
                case THHPlayer.DrawEventArg draw:
                    item.IconImage.sprite = item.drawIcon;
                    item.Text.text = "抽牌";
                    item.Image.sprite = getManager<TableManager>().getSkin(draw.player == localPlayer ? draw.card : draw.player.master).image;
                    item.BorderImage.color = draw.player == localPlayer ? item.selfColor : item.oppoColor;
                    break;
                case THHPlayer.BurnEventArg burn:
                    item.IconImage.sprite = item.burnIcon;
                    item.Text.text = "爆牌";
                    item.Image.sprite = getManager<TableManager>().getSkin(burn.card).image;
                    item.BorderImage.color = burn.player == localPlayer ? item.selfColor : item.oppoColor;
                    break;
                case THHPlayer.FatigueEventArg fatigue:
                    item.IconImage.sprite = item.fatigueIcon;
                    item.Text.text = "疲劳";
                    item.Image.sprite = getManager<TableManager>().getSkin(fatigue.player.master).image;
                    item.BorderImage.color = fatigue.player == localPlayer ? item.selfColor : item.oppoColor;
                    break;
                case THHPlayer.DiscardEventArg discard:
                    item.IconImage.sprite = item.discardIcon;
                    item.Text.text = "弃牌";
                    item.Image.sprite = getManager<TableManager>().getSkin(discard.cards[0]).image;
                    item.BorderImage.color = discard.player == localPlayer ? item.selfColor : item.oppoColor;
                    break;
                case THHPlayer.UseEventArg use:
                    item.IconImage.sprite = item.useIcon;
                    item.Text.text = "使用";
                    item.Image.sprite = getManager<TableManager>().getSkin(use.card).image;
                    item.BorderImage.color = use.player == localPlayer ? item.selfColor : item.oppoColor;
                    break;
                case THHPlayer.CreateTokenEventArg token:
                    item.IconImage.sprite = item.rebornIcon;
                    item.Text.text = "召唤";
                    item.Image.sprite = getManager<TableManager>().getSkin(token.card).image;
                    item.BorderImage.color = token.player == localPlayer ? item.selfColor : item.oppoColor;
                    break;
                case THHPlayer.RebornEventArg reborn:
                    item.IconImage.sprite = item.rebornIcon;
                    item.Text.text = "复活";
                    item.Image.sprite = getManager<TableManager>().getSkin(reborn.card).image;
                    item.BorderImage.color = reborn.player == localPlayer ? item.selfColor : item.oppoColor;
                    break;
                case THHCard.AttackEventArg attack:
                    item.IconImage.sprite = item.attackIcon;
                    item.Text.text = "攻击";
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
                case THHCard.SilenceEventArg silence:
                    item.IconImage.sprite = item.healIcon;
                    item.Text.text = "沉默";
                    item.Image.sprite = getManager<TableManager>().getSkin(silence.cards[0]).image;
                    item.BorderImage.color = silence.cards[0].getOwner() == localPlayer ? item.selfColor : item.oppoColor;
                    break;
                case THHCard.DeathEventArg death:
                    item.IconImage.sprite = item.deathIcon;
                    item.Text.text = "死亡";
                    item.Image.sprite = getManager<TableManager>().getSkin(death.infoDic.First().Key).image;
                    item.BorderImage.color = death.infoDic.First().Key.getOwner() == localPlayer ? item.selfColor : item.oppoColor;
                    break;
                default:
                    break;
            }
        }
        public void quitGame()
        {
            if (game != null)
            {
                game.Dispose();
                game = null;
                getManager<TableManager>().setGame(null, null);
                _mainUI.display(_mainUI.MainMenu);
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
        #region Events
        void onLogListItemEnter(LogListItem item)
        {
            var arg = item.getProp<IEventArg>("event");
            if (arg is IDescribableEventArg describable)
            {
                ui.LogScrollView.LogInfoHorizantal.display();
                string info = describable.toString(game, localPlayer);
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
}
