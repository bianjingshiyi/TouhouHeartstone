using System;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public partial class RoomListItem : UIObject
    {
        #region 公有方法
        public void update(RoomItemViewData viewData)
        {
            _roomNameText.text = viewData.roomName;
            _joinButton.interactable = viewData.canJoin;
            _lockedIcon.SetActive(viewData.locked);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            _itemButton.onClick.AddListener(onItemClickedCallback);
            _joinButton.onClick.AddListener(onJoinButtonClickedCallback);
        }
        #endregion
        #region 事件回调
        private void onItemClickedCallback()
        {
            onItemClicked?.Invoke(this);
        }
        private void onJoinButtonClickedCallback()
        {
            onJoinButtonClicked?.Invoke(this);
        }
        #endregion
        #endregion
        #region 事件
        public event Action<RoomListItem> onItemClicked;
        public event Action<RoomListItem> onJoinButtonClicked;
        #endregion
        #region 属性字段
        [SerializeField]
        private Button _itemButton;
        [SerializeField]
        private Button _joinButton;
        [SerializeField]
        private Text _roomNameText;
        [SerializeField]
        private GameObject _lockedIcon;
        #endregion
    }
    public struct RoomItemViewData
    {
        public string roomName;
        public bool canJoin;
        public bool locked;
    }
}
