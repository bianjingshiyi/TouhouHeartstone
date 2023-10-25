using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PVERoomPanel : MonoBehaviour
    {
        #region 公有方法
        #region 玩家选择界面
        public void setSelectPlayerPanelVisible(bool visible)
        {
            _selectPlayerPanelObj.SetActive(visible);
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
        #region 敌人信息页面
        public void setEnemyInfoPanelVisible(bool visible)
        {
            _enemyInfoPanelObj.SetActive(visible);
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
        #endregion
        #region 私有方法
        #region 生命周期
        void Awake()
        {
            _playerSelectPanel.onItemClicked += onPlayerSelectedCallback;
            _playerInfoPanel.onConfirmClicked += onGameStartClickedCallback;
            _playerInfoPanel.onBackgroundClicked += onCancelSelectPlayerCallback;
            _enemyInfoPanel.onConfirmClicked += onFightClickedCallback;
            _enemyInfoPanel.onGiveUpClicked += onGiveUpClickedCallback;
            _enemyInfoPanel.onQuitClicked += onEnemyQuitButtonClickedCallback;
            _quitButton.onClick.AddListener(onClickQuitButtonCallback);
        }
        #endregion
        #region 事件回调
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
        private void onFightClickedCallback()
        {
            onFightClicked?.Invoke();
        }
        private void onGiveUpClickedCallback()
        {
            onGiveUpClicked?.Invoke();
        }
        private void onEnemyQuitButtonClickedCallback()
        {
            onEnemyQuitButtonClicked?.Invoke();
        }
        private void onClickQuitButtonCallback()
        {
            onQuitButtonClicked?.Invoke();
        }
        #endregion

        #endregion
        #region 事件
        /// <summary>
        /// 选择人物后触发
        /// </summary>
        public event Action<int> onPlayerSelectItemClicked;
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
        /// 从敌人界面退出时触发
        /// </summary>
        public event Action onEnemyQuitButtonClicked;
        /// <summary>
        /// 取消选择人物后触发
        /// </summary>
        public event Action onCancelSelectPlayer;
        /// <summary>
        /// 点击退出房间后触发
        /// </summary>
        public event Action onQuitButtonClicked;
        #endregion
        #region 属性字段
        [Header("选择玩家页面")]
        [SerializeField]
        Button _quitButton;
        [SerializeField]
        GameObject _selectPlayerPanelObj;
        [SerializeField]
        CharacterSelectPanel _playerSelectPanel;
        [SerializeField]
        GameObject _playerInfoPanelObj;
        [SerializeField]
        PVEInfoPanel _playerInfoPanel;
        [SerializeField]
        GameObject _enemyInfoPanelObj;
        [SerializeField]
        PVEInfoPanel _enemyInfoPanel;
        #endregion
    }
}
