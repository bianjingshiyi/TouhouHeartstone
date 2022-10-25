using BJSYGameCore;
using Game;
using System;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Deck = TouhouHeartstone.Deck;
using System.Linq;
using System.Collections.Generic;
using BJSYGameCore.UI;

namespace Controllers
{
    class RoomCtrl : GameObjectCtrl<MainSceneCtrl>, IUICtrl
    {
        #region 公有方法
        public Task display(UIDisplayInfo displayInfo)
        {
            if (displayInfo is DisplayInfo info)
            {
                _currentRoomData = info.roomData;
                _isLocalGame = info.isLocalGame;

                _isDisplay = true;
                _ui.gameObject.SetActive(true);
                //_ui.ResetEvents();
                //_ui.SetMode(RoomPanel.Mode.Local);
                //_ui.SetSelfID(scene.game.network.logic.localPlayer.id);
                //_ui.SetRightText("");
                //_ui.SetInfo(roomData, true);
                updatePlayerListUI();
                if (_isLocalGame)
                {
                    _ui.displayIPText(false);
                }
                _ui.setRandomSeed(_currentRoomData.getRandomSeed());
                _ui.setShuffle(_currentRoomData.getShuffle());
                _ui.setInitReplaceTime(_currentRoomData.getTimeoutForInitReplace());
                _ui.setTurnTime(_currentRoomData.getTimeoutForTurn());
                _ui.setDisableRandomPlayerActOrder(_currentRoomData.getSortedPlayers() != null);
                updateUIStateForHost();
                updateStartAndReadyButton();
            }
            return Task.CompletedTask;
        }

        public UIDisplayInfo getCurrentDisplayInfo()
        {
            return new DisplayInfo(_currentRoomData, _isLocalGame);
        }

        public Task hide()
        {
            _isDisplay = false;
            _ui.gameObject.SetActive(false);
            return Task.CompletedTask;
        }
        public void display(RoomData roomData, bool isLocalGame)
        {
            display(new DisplayInfo(roomData, isLocalGame));
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            if (_ui == null)
            {
                _ui = GetComponentInChildren<RoomPanel>();
            }
            //_ui.onQuitClick += onQuitButtonClick;
            //_ui.onRoomPropUpdate += onRoomPropUpdate;
            _ui.onRandomSeedInputFieldValueChanged += onRandomSeedInputFieldValueChanged;
            _ui.onShuffleToggleValueChanged += onShuffleToggleValueChanged;
            _ui.onInitReplaceTimeInputFieldValueChanged += onInitReplaceTimeInputFieldValueChanged;
            _ui.onTurnTimeInputFieldValueChanged += onTurnTimeInputFieldValueChanged;
            _ui.onDisableRandomPlayerActOrderToggleValueChanged += onDisableRandomPlayerActOrderToggleValueChanged;
            _ui.onClickPlayerItemViewDeckButton += onClickPlayerListItemViewDeckButton;
            _ui.onClickPlayerItemSetDeckButton += onClickPlayerListItemSetDeckButton;
            _ui.onClickPlayerItemRemoveButton += onClickPlayerListItemRemoveButton;
            _ui.onClickAddAIPlayerButton += onClickAddAIPlayerButton;
            _ui.onClickSelectDeckButton += onClickSelectDeckButton;
            _ui.onClickDeckSelectPanelItem += onClickDeckSelectPanelItem;
            _ui.onClickStartButton += onClickStartButton;
            _ui.onClickReadyButton += onClickReadyButton;
            _ui.onClickCancleReadyButton += onClickCancleReadyButton;
            _ui.onClickQuitButton += onClickQuitButton;
        }
        #endregion
        #region 事件回调
        private void onRandomSeedInputFieldValueChanged(int value)
        {
            _currentRoomData.setOption(RoomDataPropName.Option.RandomSeed, value);
        }
        private void onShuffleToggleValueChanged(bool value)
        {
            _currentRoomData.setOption(RoomDataPropName.Option.Shuffle, value);
        }
        private void onInitReplaceTimeInputFieldValueChanged(float value)
        {
            _currentRoomData.setOption(RoomDataPropName.Option.TimeoutForInitReplace, value);
        }
        private void onTurnTimeInputFieldValueChanged(float value)
        {
            _currentRoomData.setOption(RoomDataPropName.Option.TimeoutForTurn, value);
        }
        private void onDisableRandomPlayerActOrderToggleValueChanged(bool value)
        {
            if (value)
            {
                _currentRoomData.setOption(RoomDataPropName.Option.SortedPlayers, _currentRoomData.playerDataList.Select(p => p.id).ToArray());
            }
            else
            {
                _currentRoomData.setOption(RoomDataPropName.Option.SortedPlayers, null);
            }
        }
        private void onClickPlayerListItemViewDeckButton(int index)
        {
            Deck deck = _currentRoomData.playerDataList[index].getDeck();
            displayPlayerDeckList(deck);
        }
        private async void onClickPlayerListItemSetDeckButton(int index)
        {
            _selectDeckPlayerId = _currentRoomData.playerDataList[index].id;
            await displaySelectDeckPanel();
        }
        private void onClickPlayerListItemRemoveButton(int index)
        {
            _currentRoomData.playerDataList.RemoveAt(index);
            updatePlayerListUI();
            updateStartAndReadyButton();
        }
        private async void onClickAddAIPlayerButton()
        {
            await game.network.logic.AddAIPlayer();
            updatePlayerListUI();
            updateStartAndReadyButton();
        }
        /// <summary>
        /// 当点击选择卡组按钮
        /// </summary>
        private async void onClickSelectDeckButton()
        {
            _selectDeckPlayerId = scene.game.network.logic.localPlayer.id;
            await displaySelectDeckPanel();
        }
        private async void onClickDeckSelectPanelItem(int index)
        {
            //选择卡组
            var deck = game.deck.getOrLoadDeckList()[index];
            setPlayerDeck(_selectDeckPlayerId, deck.toIdArray());
            //隐藏选择卡组界面
            _ui.hideDeckSelectPanel();
            //更新界面
            updatePlayerListUI();
            updateStartAndReadyButton();
            if (_selectDeckPlayerId == game.network.logic.localPlayer.id)
            {
                _ui.updateLocalPlayerDeck(deck.name, await game.card.loadCardImage(deck.masterId));
            }
        }
        private async void onClickStartButton()
        {
            var players = _currentRoomData.getSortedPlayers();
            if (players != null &&
                players.Length != _currentRoomData.playerDataList.Count)
            {
                _currentRoomData.setOption(RoomDataPropName.Option.SortedPlayers, _currentRoomData.playerDataList.Select(p => p.id).ToArray());
            }
            await game.game.startLocalGame(_currentRoomData);
        }
        private void onClickReadyButton()
        {
            var localPlayer = getLocalPlayerData();
            localPlayer.setReady(true);
            updateStartAndReadyButton();
            //TODO:多人再说
        }
        private void onClickCancleReadyButton()
        {
            var localPlayer = getLocalPlayerData();
            localPlayer.setReady(false);
            updateStartAndReadyButton();
            //TODO:多人再说
        }
        private void onClickQuitButton()
        {
            _currentRoomData = null;
            game.network.quitRoom();

            _ui.gameObject.SetActive(false);
            scene.menu.display();
        }
        #endregion
        private void setPlayerDeck(int playerId, int[] cardsId)
        {
            for (int i = 0; i < _currentRoomData.playerDataList.Count; i++)
            {
                var player = _currentRoomData.playerDataList[i];
                if (player.id == playerId)
                {
                    Deck deck = player.getDeck();
                    if (deck == null)
                    {
                        deck = new Deck(cardsId[0], cardsId.Skip(1));
                    }
                    else
                    {
                        deck.masterId = cardsId[0];
                        deck.setCards(cardsId.Skip(1));
                    }
                    player.setDeck(deck);
                    if (player.type == RoomPlayerType.ai)
                    {
                        player.setReady(true);
                    }
                }
            }
        }
        private void updateUIStateForHost()
        {
            bool isHost = game.network.logic.localPlayer.id == _currentRoomData.ownerId;
            _ui.enableRandomSeed(isHost);
            _ui.enableShuffle(isHost);
            _ui.enableInitReplaceTime(isHost);
            _ui.enableTurnTime(isHost);
            _ui.enableDisableRandomPlayerActOrder(isHost);
            _ui.enableAllowCustomCards(isHost);
        }
        private void updateStartAndReadyButton()
        {
            if (game.network.logic.localPlayer.id == _currentRoomData.ownerId)
            {
                _ui.displayStartButton(true);
                _ui.displayReadyButton(false);
                _ui.displayCancleReadyButton(false);
                _ui.enableStartButton(isAbleToStartGame());
            }
            else
            {
                _ui.displayStartButton(false);
                RoomPlayerData localPlayer = getLocalPlayerData();
                _ui.displayReadyButton(isAbleToReady(localPlayer) && !localPlayer.getReady());
                _ui.displayCancleReadyButton(localPlayer.getReady());
            }
        }
        private bool isAbleToStartGame()
        {
            if (_currentRoomData.playerDataList.Count < 2)//必须有对手才能开始游戏
                return false;
            if (!isAbleToReady(getLocalPlayerData()))//房主自己必须准备好
                return false;
            return _currentRoomData.playerDataList.Where(p => p.id != _currentRoomData.ownerId).All(p => p.getReady());//其他玩家必须准备好
        }
        private bool isAbleToReady(RoomPlayerData player)
        {
            Deck deck = player.getDeck();
            if (deck != null)
            {
                bool deckValid;
                if (_isLocalGame)
                {
                    deckValid = game.deck.isValidDeck(deck);
                }
                else
                {
                    deckValid = game.deck.isValidDeckForOnlineGame(deck);
                }
                return deckValid;
            }
            return false;
        }
        private RoomPlayerData getLocalPlayerData()
        {
            return _currentRoomData.playerDataList.Find(p => p.id == game.network.logic.localPlayer.id);
        }
        private void updatePlayerListUI()
        {
            (string playerName, string characterName, bool canViewDeck, bool canSetDeck, bool canRemove)[] viewData =
                new (string playerName, string characterName, bool canViewDeck, bool canSetDeck, bool canRemove)[_currentRoomData.playerDataList.Count];
            for (int i = 0; i < viewData.Length; i++)
            {
                RoomPlayerData player = _currentRoomData.playerDataList[i];
                viewData[i].playerName = player.name;
                Deck deck = player.getDeck();
                if (deck == null)
                {
                    viewData[i].characterName = string.Empty;
                }
                else if (game.card.tryGetCardSkin(deck.masterId, out CardSkinData skin))
                {
                    viewData[i].characterName = skin.name;
                }
                else
                {
                    viewData[i].characterName = game.langManager.getString(StringTable.BUILD_UNKNOW_CARD_NAME);
                }
                viewData[i].canViewDeck = player.type == RoomPlayerType.human && deck != null;
                viewData[i].canSetDeck = player.type == RoomPlayerType.ai;
                viewData[i].canRemove = game.network.logic.localPlayer.id == _currentRoomData.ownerId && player.id != _currentRoomData.ownerId;
            }
            _ui.updatePlayerList(viewData);
            _ui.displayAddAIButton(_currentRoomData.playerDataList.Count < _currentRoomData.maxPlayerCount && _isLocalGame);
        }
        private void displayPlayerDeckList(Deck deck)
        {
            //TODO:把编辑器界面里面右边那个挪过来
        }
        private async Task displaySelectDeckPanel()
        {
            List<Deck> deckList = game.deck.getOrLoadDeckList();
            (string name, Sprite image, bool isWarning)[] viewData = new (string name, Sprite image, bool isWarning)[deckList.Count];
            for (int i = 0; i < viewData.Length; i++)
            {
                viewData[i].name = deckList[i].name;
                viewData[i].image = await game.card.loadCardImage(deckList[i].masterId);
                viewData[i].isWarning = _isLocalGame ? game.deck.isValidDeck(deckList[i]) : game.deck.isValidDeckForOnlineGame(deckList[i]);
            }
            _ui.displayDeckSelectPanel(viewData);
        }
        #endregion
        #region 属性字段
        public bool isDisplay => _isDisplay;
        [SerializeField]
        RoomPanel _ui;
        [SerializeField]
        bool _isDisplay = false;
        [SerializeField]
        RoomData _currentRoomData;
        [SerializeField]
        bool _isLocalGame = true;
        [SerializeField]
        int _selectDeckPlayerId = 0;
        #endregion
        #region 内部类
        public class DisplayInfo : UIDisplayInfo
        {
            public DisplayInfo(RoomData roomData, bool isLocalGame)
            {
                this.roomData = roomData;
                this.isLocalGame = isLocalGame;
            }
            public RoomData roomData;
            public bool isLocalGame;
        }
        #endregion
    }
}
