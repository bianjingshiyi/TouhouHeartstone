using System;
using Game;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PVERoomPanel : MonoBehaviour
    {
        #region 公有方法
        #region 玩家选择界面
        public void setPreStartPanelVisible(bool visible)
        {
            _preStartPanelObj.SetActive(visible);
        }
        public void updatePlayerSelectPanel(CharacterViewData[] viewDatas)
        {
            _playerSelectPanel.update(viewDatas);
        }
        #endregion
        #region 玩家信息页面
        public void setPlayerInfoPanelVisible(bool visible)
        {
            _playerInfoPanelObj.SetActive(visible);
        }
        public void updatePlayerInfoPanel(string playerName, Sprite playerSprite, string desc)
        {
            _playerInfoPanel.update(playerName, playerSprite, desc);
        }
        public void updatePlayerInfoPanelDeck((string name, Sprite image, int count)[] viewDatas)
        {
            _playerInfoPanel.updateDeck(viewDatas);
        }
        #endregion
        #region 游戏内容页面
        public void setInRunPanelVisible(bool visible)
        {
            _inRunPanelObj.SetActive(visible);
        }
        public void openInRunGamePage()
        {
            switchInRunPage(_gamePageObj);
        }
        #region 敌人信息面板
        public void switchToEnemyInfoPanel()
        {
            switchGamePanel(_enemyInfoPanelObj);
        }
        public void updateEnemyInfoPanel(string name, Sprite sprite, string desc)
        {
            _enemyInfoPanel.update(name, sprite, desc);
        }
        public void updateEnemyInfoPanelDeck((string name, Sprite image, int count)[] viewDatas)
        {
            _enemyInfoPanel.updateDeck(viewDatas);
        }
        #endregion
        #region 选择卡牌面板
        public void switchToSelectCardPanel()
        {
            switchGamePanel(_selectCardPanelObj);
        }
        public void updateSelectCardPanel(CardDefine[] defines, CardManager cardManager)
        {
            _cardSelectPanel.updateCards(defines, cardManager);
        }
        #endregion
        #region 编辑器面板
        public void openCardEditorPage()
        {
            switchInRunPage(_cardEditorPageObj);
        }
        #endregion
        public void updateYourDeck((string name, Sprite image, int count)[] viewDatas)
        {
            _yourDeckView.update(viewDatas);
        }
        #endregion
        #endregion
        #region 私有方法
        #region 生命周期
        void Awake()
        {
            _playerSelectPanel.onItemClicked += onPlayerSelectedCallback;
            _playerInfoPanel.onConfirmClicked += onGameStartClickedCallback;
            _cancelPlayerSelectButton.onClick.AddListener(onCancelSelectPlayerCallback);
            _enemyInfoPanel.onConfirmClicked += onFightClickedCallback;
            _enemyInfoPanel.onGiveUpClicked += onGiveUpClickedCallback;
            _enemyInfoPanel.onQuitClicked += onInRunQuitButtonClickedCallback;
            _enemyInfoPanel.onInventoryClicked += onInRunInventoryButtonClickedCallback;
            _cardSelectPanel.onChoiceClicked += onSelectCardChoiceClickedCallback;
            _cardSelectPanel.onConfirmClicked += onConfirmSelectCardClickedCallback;
            _cardSelectPanel.onSkipClicked += onSkipSelectCardClickedCallback;
            _cardSelectPanel.onQuitClicked += onInRunQuitButtonClickedCallback;
            _quitButton.onClick.AddListener(onClickQuitButtonCallback);
        }
        #endregion
        #region 事件回调
        #region 进入游戏前界面
        private void onPlayerSelectedCallback(int index)
        {
            onPlayerSelectItemClicked?.Invoke(index);
        }
        private void onCancelSelectPlayerCallback()
        {
            onCancelSelectPlayer?.Invoke();
        }
        private void onGameStartClickedCallback()
        {
            onGameStartClicked?.Invoke();
        }
        private void onClickQuitButtonCallback()
        {
            onQuitButtonClicked?.Invoke();
        }
        #endregion
        #region 游戏内界面
        private void onFightClickedCallback()
        {
            onFightClicked?.Invoke();
        }
        private void onGiveUpClickedCallback()
        {
            onGiveUpClicked?.Invoke();
        }
        private void onInRunQuitButtonClickedCallback()
        {
            onInRunQuitButtonClicked?.Invoke();
        }
        private void onInRunInventoryButtonClickedCallback()
        {
            onInventoryButtonClicked?.Invoke();
        }
        private void onSelectCardChoiceClickedCallback(int index)
        {
            onSelectCardChoiceClicked?.Invoke(index);
        }
        private void onConfirmSelectCardClickedCallback()
        {
            onConfirmSelectCardClicked?.Invoke();
        }
        private void onSkipSelectCardClickedCallback()
        {
            onSkipSelectCardClicked?.Invoke();
        }
        #endregion
        #endregion
        private void switchGamePanel(GameObject switchTo)
        {
            setInRunPage(_enemyInfoPanelObj, switchTo);
            setInRunPage(_selectCardPanelObj, switchTo);
        }
        private void switchInRunPage(GameObject switchTo)
        {
            setInRunPage(_gamePageObj, switchTo);
            setInRunPage(_cardEditorPageObj, switchTo);
        }
        private void setInRunPage(GameObject page, GameObject switchTo)
        {
            page.SetActive(page == switchTo);
        }
        #endregion
        #region 事件
        /// <summary>
        /// 选择人物后触发
        /// </summary>
        public event Action<int> onPlayerSelectItemClicked;
        /// <summary>
        /// 取消选择人物后触发
        /// </summary>
        public event Action onCancelSelectPlayer;
        /// <summary>
        /// 点击退出房间后触发
        /// </summary>
        public event Action onQuitButtonClicked;
        /// <summary>
        /// 选择人物且游戏开始后触发
        /// </summary>
        public event Action onGameStartClicked;
        /// <summary>
        /// 游戏开始且与敌人战斗时触发
        /// </summary>
        public event Action onFightClicked;
        /// <summary>
        /// 放弃一局游戏时触发
        /// </summary>
        public event Action onGiveUpClicked;
        /// <summary>
        /// 从游戏内退出时触发
        /// </summary>
        public event Action onInRunQuitButtonClicked;
        /// <summary>
        /// 打开背包时触发
        /// </summary>
        public event Action onInventoryButtonClicked;
        /// <summary>
        /// 选择一个选卡选项后触发
        /// </summary>
        public event Action<int> onSelectCardChoiceClicked;
        /// <summary>
        /// 确认选择选卡选项后触发
        /// </summary>
        public event Action onConfirmSelectCardClicked;
        /// <summary>
        /// 跳过选择选卡选项后触发
        /// </summary>
        public event Action onSkipSelectCardClicked;
        #endregion
        #region 属性字段
        [Header("选择玩家页面")]
        [SerializeField]
        GameObject _preStartPanelObj;
        [SerializeField]
        Button _quitButton;
        [SerializeField]
        CharacterSelectPanel _playerSelectPanel;
        [SerializeField]
        GameObject _playerInfoPanelObj;
        [SerializeField]
        PVECharacterInfoPanel _playerInfoPanel;
        [SerializeField]
        Button _cancelPlayerSelectButton;

        [Header("游戏内页面")]
        [SerializeField]
        GameObject _inRunPanelObj;
        [SerializeField]
        GameObject _enemyInfoPanelObj;
        [SerializeField]
        GameObject _selectCardPanelObj;
        [SerializeField]
        GameObject _gamePageObj;
        [SerializeField]
        GameObject _cardEditorPageObj;
        [SerializeField]
        PVECharacterInfoPanel _enemyInfoPanel;
        [SerializeField]
        PVESelectCardPanel _cardSelectPanel;
        [SerializeField]
        DeckCardList _yourDeckView;
        #endregion
    }
}
