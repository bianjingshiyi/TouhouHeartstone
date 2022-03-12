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
    class PVERoomCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
        public void display()
        {
            _ui.gameObject.SetActive(true);
            _isDisplay = true;
            updatePVEModeUI();
        }
        #endregion
        
        #region 私有方法

        
        #region 生命周期
        protected void Awake()
        {
            if (_ui == null)
            {
                _ui = GetComponentInChildren<PVERoomPanel>();
            }
            //TODO:添加监听UI控件事件 & 游戏逻辑事件
            // //_ui.onQuitClick += onQuitButtonClick;
            // //_ui.onRoomPropUpdate += onRoomPropUpdate;
            // _ui.onRandomSeedInputFieldValueChanged += onRandomSeedInputFieldValueChanged;
            // _ui.onShuffleToggleValueChanged += onShuffleToggleValueChanged;
            // _ui.onInitReplaceTimeInputFieldValueChanged += onInitReplaceTimeInputFieldValueChanged;
            // _ui.onTurnTimeInputFieldValueChanged += onTurnTimeInputFieldValueChanged;
            // _ui.onDisableRandomPlayerActOrderToggleValueChanged += onDisableRandomPlayerActOrderToggleValueChanged;
            // _ui.onClickPlayerItemViewDeckButton += onClickPlayerListItemViewDeckButton;
            // _ui.onClickPlayerItemSetDeckButton += onClickPlayerListItemSetDeckButton;
            // _ui.onClickPlayerItemRemoveButton += onClickPlayerListItemRemoveButton;
            // _ui.onClickAddAIPlayerButton += onClickAddAIPlayerButton;
            // _ui.onClickSelectDeckButton += onClickSelectDeckButton;
            // _ui.onClickDeckSelectPanelItem += onClickDeckSelectPanelItem;
            // _ui.onClickStartButton += onClickStartButton;
            // _ui.onClickReadyButton += onClickReadyButton;
            // _ui.onClickCancleReadyButton += onClickCancleReadyButton;
            _ui.onClickQuitButton += onClickQuitButton;
        }
        #endregion
        #region 事件回调
        // private void onRandomSeedInputFieldValueChanged(int value)
        // {
        //     _currentRoomData.getOption().randomSeed = value;
        // }
        // private void onShuffleToggleValueChanged(bool value)
        // {
        //     _currentRoomData.getOption().shuffle = value;
        // }
        // private void onInitReplaceTimeInputFieldValueChanged(float value)
        // {
        //     _currentRoomData.getOption().timeoutForInitReplace = value;
        // }
        // private void onTurnTimeInputFieldValueChanged(float value)
        // {
        //     _currentRoomData.getOption().timeoutForTurn = value;
        // }
        
        /// <summary>
        /// 当用户选中了第index个卡组
        /// </summary>
        /// <param name="index"></param>
        private async void onClickDeckSelectItem(int index)
        {
            //选择卡组
            var deck = game.deck.getOrLoadDeckList()[index];
            _selectedDeck = deck;
            updateGameReadyButton();
        }

        #endregion
        #region 实现方法
        
        /// <summary>
        /// 返回主界面
        /// </summary>
        private void onClickQuitButton()
        {
            _ui.gameObject.SetActive(false);
            scene.menu.display();
        }
        
        /// <summary>
        /// 是否已经选择卡组
        /// </summary>
        private bool isSelectedDeck()
        {
            //TODO:这里判断是否选择卡组了
            if (_selectedDeck == null) return false;

            displayPlayerDeckList(_selectedDeck);
            // Deck deck = player.getDeck();
            // if (deck == null || game.deck.isValidDeck(deck))
            //     return false;
            return true;
        }


        private bool isGameReady()
        {
            //TODO:这里判断是否准备下一步了
            return false;
        }
        
        /// <summary>
        /// 更新游戏准备按钮，是否允许用户进入下一步。
        /// 第一步：选择玩家卡组。
        /// 第二步：查看并确认是否要挑战关卡内容。
        /// </summary>
        private void updateGameReadyButton()
        {
            if (isSelectedDeck())
            {
                if (isGameReady())
                {
                    
                }
            }
            else
            {
                // 未选中卡组，不可以下一步
            }
        }
        
        /// <summary>
        /// 更新当前PVE模式的UI内容
        /// </summary>
        private void updatePVEModeUI()
        {
            updateCanSelectedDeckList();
            // (string playerName, string characterName, bool canViewDeck, bool canSetDeck, bool canRemove)[] viewData =
            //     new (string playerName, string characterName, bool canViewDeck, bool canSetDeck, bool canRemove)[_currentRoomData.playerDataList.Count];
            // for (int i = 0; i < viewData.Length; i++)
            // {
            //     RoomPlayerData player = _currentRoomData.playerDataList[i];
            //     viewData[i].playerName = player.name;
            //     Deck deck = player.getDeck();
            //     if (deck == null)
            //     {
            //         viewData[i].characterName = string.Empty;
            //     }
            //     else if (game.card.tryGetSkin(deck.masterId, out CardSkinData skin))
            //     {
            //         viewData[i].characterName = skin.name;
            //     }
            //     else
            //     {
            //         viewData[i].characterName = game.langManager.getString(StringTable.BUILD_UNKNOW_CARD_NAME);
            //     }
            //     viewData[i].canViewDeck = player.type == RoomPlayerType.human && deck != null;
            //     viewData[i].canSetDeck = player.type == RoomPlayerType.ai;
            //     viewData[i].canRemove = game.network.logic.localPlayer.id == _currentRoomData.ownerId && player.id != _currentRoomData.ownerId;
            // }
            // _ui.updatePlayerList(viewData);
            // _ui.displayAddAIButton(_currentRoomData.playerDataList.Count < _currentRoomData.maxPlayerCount);
        }
        private void displayPlayerDeckList(Deck deck)
        {
            //TODO:当用户选择卡组之后，要展示卡组的内容，把编辑器界面里面右边那个挪过来
        }
        
        /// <summary>
        /// 更新可选择的当前的卡组列表
        /// </summary>
        private async Task updateCanSelectedDeckList()
        {
            List<Deck> deckList = game.deck.getOrLoadDeckList();
            (string name, Sprite image, bool isWarning)[] viewData = new (string name, Sprite image, bool isWarning)[deckList.Count];
            for (int i = 0; i < viewData.Length; i++)
            {
                viewData[i].name = deckList[i].name;
                viewData[i].image = await game.card.loadCardImage(deckList[i].masterId);
                viewData[i].isWarning = game.deck.isValidDeck(deckList[i]);
            }
            
            // TODO:这里需要一个控件来传入deck数据，并且展示出来
            _ui.displayDeckSelectPanel(viewData);
        }
        #endregion
        
        #endregion
        
        #region 属性字段
        public bool isDisplay => _isDisplay;
        [SerializeField]
        PVERoomPanel _ui;
        [SerializeField]
        bool _isDisplay = false;
        /// <summary>
        /// 用户当前选择的卡组
        /// </summary>
        [SerializeField]
        Deck _selectedDeck = null;
        #endregion
        #region 内部类
        
        #endregion
    }
}
