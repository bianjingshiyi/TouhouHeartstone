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

namespace Controllers
{
    class RoomCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
        public void display(RoomData roomData, bool isLocalGame)
        {
            _currentRoomData = roomData;

            _ui.gameObject.SetActive(true);
            //_ui.ResetEvents();
            //_ui.SetMode(RoomPanel.Mode.Local);
            //_ui.SetSelfID(scene.game.network.logic.localPlayer.id);
            //_ui.SetRightText("");
            //_ui.SetInfo(roomData, true);
            updatePlayerListUI();
            if (isLocalGame)
            {
                _ui.displayIPText(false);
            }
            _ui.setRandomSeed(roomData.getOption().randomSeed);
            _ui.setShuffle(roomData.getOption().shuffle);
            _ui.setInitReplaceTime(roomData.getOption().timeoutForInitReplace);
            _ui.setTurnTime(roomData.getOption().timeoutForTurn);
            _ui.setDisableRandomPlayerActOrder(roomData.getOption().sortedPlayers != null);
            updateUIStateForHost();
            updateStartAndReadyButton();
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
            _currentRoomData.getOption().randomSeed = value;
        }
        private void onShuffleToggleValueChanged(bool value)
        {
            _currentRoomData.getOption().shuffle = value;
        }
        private void onInitReplaceTimeInputFieldValueChanged(float value)
        {
            _currentRoomData.getOption().timeoutForInitReplace = value;
        }
        private void onTurnTimeInputFieldValueChanged(float value)
        {
            _currentRoomData.getOption().timeoutForTurn = value;
        }
        private void onDisableRandomPlayerActOrderToggleValueChanged(bool value)
        {
            if (value)
            {
                _currentRoomData.getOption().sortedPlayers = _currentRoomData.playerDataList.Select(p => p.id).ToArray();
            }
            else
                _currentRoomData.getOption().sortedPlayers = null;
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
            if (_currentRoomData.getOption().sortedPlayers != null &&
                _currentRoomData.getOption().sortedPlayers.Length != _currentRoomData.playerDataList.Count)
            {
                _currentRoomData.getOption().sortedPlayers = _currentRoomData.playerDataList.Select(p => p.id).ToArray();
            }
            await game.game.startLocalGame(_currentRoomData);
        }
        private void onClickReadyButton()
        {
            //TODO:多人再说
        }
        private void onClickCancleReadyButton()
        {
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
                    Deck deck = _currentRoomData.playerDataList[i].getDeck();
                    if (deck == null)
                    {
                        deck = new Deck(cardsId[0], cardsId.Skip(1));
                    }
                    else
                    {
                        deck.masterId = cardsId[0];
                        deck.setCards(cardsId.Skip(1));
                    }
                    _currentRoomData.playerDataList[i].setDeck(deck);
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
            if (deck == null || game.deck.isValidDeck(deck))
                return false;
            return true;
        }
        private RoomPlayerData getLocalPlayerData()
        {
            return _currentRoomData.playerDataList.Find(p => p.id == _currentRoomData.ownerId);
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
                else if (game.card.tryGetSkin(deck.masterId, out CardSkinData skin))
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
            _ui.displayAddAIButton(_currentRoomData.playerDataList.Count < _currentRoomData.maxPlayerCount);
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
                viewData[i].isWarning = game.deck.isValidDeck(deckList[i]);
            }
            _ui.displayDeckSelectPanel(viewData);
        }
        #endregion
        [SerializeField]
        RoomPanel _ui;
        [SerializeField]
        RoomData _currentRoomData;
        [SerializeField]
        int _selectDeckPlayerId = 0;
    }
}
