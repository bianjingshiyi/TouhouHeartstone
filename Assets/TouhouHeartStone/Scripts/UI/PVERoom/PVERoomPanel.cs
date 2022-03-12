
using BJSYGameCore;
using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
using BJSYGameCore.UI;
using NitoriNetwork.Common;
using MukioI18n;
using UnityEngine.UI;

namespace UI
{   class PVERoomPanel : MonoBehaviour
    {
        #region 公有方法
        
        #endregion
        // DeckManager deckManager => this.findInstance<DeckManager>();
        // CardManager cardManager => this.findInstance<CardManager>();
        //
        // LangManager lang => this.findInstance<LangManager>();
        
        void initEvents()
        {
            
        }
        
        [TranslateMsg]
        const string STR_START_GAME = "开始游戏";
        [TranslateMsg]
        const string STR_PREPARE = "准备";

        
        #region 公有方法

        /// <summary>
        /// 更新显示当前可以选择所有卡组UI
        /// </summary>
        /// <param name="viewData">卡组显示数据</param>
        public void displayDeckSelectPanel((string name, Sprite image, bool isWarning)[] viewData)
        {
            //TODO:遍历刷新控件数据即可
        }
        
        #endregion
        #region 私有方法
        #region 生命周期
        
        private void OnEnable()
        {
           
        }
        
        void Awake()
        {
            initEvents();
            
            // DeckPanelMasterCardSelection.onClickItem += onClickDeckSelectItemCallback;
            //_randomSeedInputField.onValueChanged.AddListener(onRandomSeedInputFieldValueChangedCallback);
            // _cancleReadyButton.onClick.AddListener(onClickCancleReadyButtonCallback);
            _quitButton.onClick.AddListener(onClickQuitButtonCallback);
        }
        #endregion
        #endregion
        
        #region 事件回调
        private void onClickQuitButtonCallback()
        {
            onClickQuitButton?.Invoke();
        }
        #endregion
        
        #region 事件

        /// <summary>
        /// 当点击玩家设置卡组按钮，卡组是用id?
        /// </summary>
        public event Action<int> onClickAnyoneDeck;
        /// <summary>
        /// 玩家点击开始按钮
        /// </summary>
        public event Action onClickStartButton;
        /// <summary>
        /// 玩家点击准备按钮
        /// </summary>
        public event Action onClickReadyButton;
        /// <summary>
        /// 玩家点击取消按钮？这个功能有没有不清楚 TODO:产品侧待讨论
        /// </summary>
        public event Action onClickCancleReadyButton;
        /// <summary>
        /// 玩家点击退出按钮，回到主界面
        /// </summary>
        public event Action onClickQuitButton;
        #endregion
        #region 属性字段
        // [SerializeField]
        // RectTransform _playerRoot;
        // [SerializeField]
        // RectTransform _playerTemplate;
        // [SerializeField]
        // List<RectTransform> _playerList = new List<RectTransform>();
        // [SerializeField]
        // Button _addAIPlayerButton;
        // [SerializeField]
        // Text _ipText;
        // [SerializeField]
        // InputField _randomSeedInputField;
        // [SerializeField]
        // Toggle _shuffleToggle;
        // [SerializeField]
        // InputField _initReplaceTimeInputField;
        // [SerializeField]
        // InputField _turnTimeInputField;
        // [SerializeField]
        // Toggle _disableRandomPlayerActOrderToggle;
        // [SerializeField]
        // Text _localPlayerDeckNameText;
        // [SerializeField]
        // Image _localPlayerDeckImage;
        [SerializeField]
        Button _deckViewItem;
        [SerializeField]
        Button _startButton;
        [SerializeField]
        Button _readyButton;
        [SerializeField]
        Button _cancleReadyButton;
        [SerializeField]
        Button _quitButton;
        #endregion
        

        [TranslateMsg]
        private const String STR_REQUEST_PREPRAE = "取消准备";

        /// <summary>
        /// 点击开始游戏后触发
        /// </summary>
        public event Action onStartClick;
        /// <summary>
        /// 点击退出房间后触发
        /// </summary>
        public event Action onQuitClick;
        /// <summary>
        /// 玩家准备
        /// </summary>
        public event Action<int> onPlayerReady;
        
        /// <summary>
        /// 重置所有事件
        /// </summary>
        public void ResetEvents()
        {
            // onStartClick = null;
            // onQuitClick = null;
            // onRoomPropUpdate = null;
            // onPlayerRemove = null;
            // onAIPlayerAdd = null;
            // onSetPlayerDeck = null;
            // onPlayerReadyChange = null;
        }
    }
}
