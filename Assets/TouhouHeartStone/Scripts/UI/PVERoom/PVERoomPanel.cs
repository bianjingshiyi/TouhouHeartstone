using System;
using System.Collections.Generic;
using BJSYGameCore.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PVERoomPanel : MonoBehaviour
    {
        #region 公有方法
        public void displayPlayerInfoPanel()
        {
            _playerInfoPanelObj.SetActive(true);
        }
        public void hidePlayerInfoPanel()
        {
            _playerInfoPanelObj.SetActive(false);
        }
        public void updatePlayerInfoPanel(string playerName, Sprite playerSprite, string desc)
        {
            _playerHeadName.text = playerName;
            _playerHeadImage.sprite = playerSprite;
            _playerDescText.text = desc;
        }
        public void updatePlayerInfoPanelDeck((string name, Sprite image, int count)[] viewDatas)
        {
            UIHelper.updateList(_playerDeckCardListRoot, _playerDeckCardItemTemplate, _playerDeckCardList, viewDatas.Length,
                (i, item) =>
                {
                    var listItem = item.GetComponent<CardViewDeckCardListItem>();
                    var viewData = viewDatas[i];
                    listItem.updateView(viewData.name, viewData.image, viewData.count);
                });
        }
        public void updatePlayerSelectPanel(CharacterViewData[] viewDatas)
        {
            _playerSelectPanel.update(viewDatas);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        void Awake()
        {
            _playerSelectPanel.onItemClicked += onPlayerSelectedCallback;
            _playerCancelButton.onClick.AddListener(onCancelSelectPlayerCallback);
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
        /// 取消选择人物后触发
        /// </summary>
        public event Action onCancelSelectPlayer;
        /// <summary>
        /// 点击退出房间后触发
        /// </summary>
        public event Action onQuitButtonClicked;
        #endregion
        #region 属性字段
        [SerializeField]
        CharacterSelectPanel _playerSelectPanel;
        [SerializeField]
        GameObject _playerInfoPanelObj;
        [SerializeField]
        TextMeshProUGUI _playerHeadName;
        [SerializeField]
        Image _playerHeadImage;
        [SerializeField]
        TextMeshProUGUI _playerDescText;
        [SerializeField]
        Button _playerConfirmButton;
        [SerializeField]
        Button _playerCancelButton;
        [SerializeField]
        Button _quitButton;

        [Header("玩家卡组显示")]
        [SerializeField]
        RectTransform _playerDeckCardListRoot;
        [SerializeField]
        RectTransform _playerDeckCardItemTemplate;
        [SerializeField]
        List<RectTransform> _playerDeckCardList;
        #endregion
    }
}
