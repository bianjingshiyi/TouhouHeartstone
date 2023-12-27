using System;
using BJSYGameCore.UI;
using Game;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RoomPlayerListItem : UIObject
    {
        #region 公有事件

        /// <summary>
        /// 设置当前的玩家信息
        /// </summary>
        /// <param name="data"></param>
        public void update(RoomPlayerItemViewData viewData)
        {
            _nameText.text = viewData.name;
            _characterText.text = viewData.characterName;
            PlayerLoadState loadState = viewData.loadState;
            _uploadIconImage.gameObject.SetActive(loadState == PlayerLoadState.Upload);
            _downloadIconImage.gameObject.SetActive(loadState == PlayerLoadState.Download);
            _checkingIconImage.gameObject.SetActive(loadState == PlayerLoadState.Checking);
            _progressBar.gameObject.SetActive(loadState != PlayerLoadState.None);
            _readyIconImage.gameObject.SetActive(loadState == PlayerLoadState.None && viewData.isReady);


            _viewDeckButtonObj.SetActive(!viewData.canSetDeck);
            _setDeckButtonObj.SetActive(viewData.canSetDeck);
            _viewDeckButton.interactable = viewData.canViewDeck;
            _viewDeckButtonObj.SetActive(!viewData.isAI);
            _setDeckButtonObj.SetActive(viewData.isAI);
            _removePlayerButton.interactable = viewData.canRemove;
        }

        /// <summary>
        /// 设置此玩家是否可以移除
        /// </summary>
        /// <param name="val"></param>
        public void SetRemoveable(bool val)
        {
            _removePlayerButton.interactable = val;
        }
        public void SetProgress(float progress)
        {
            _progressBar.transform.localScale = new Vector3(Mathf.Clamp01(progress), 1, 1);
        }
        #endregion

        #region 私有事件
        protected override void Awake()
        {
            base.Awake();
            _viewDeckButton.onClick.AddListener(onClickViewDeckButtonCallback);
            _setDeckButton.onClick.AddListener(onClickSetDeckButtonCallback);
            _removePlayerButton.onClick.AddListener(onClickRemoveButtonCallback);
        }
        #endregion

        #region 私有方法
        #region 事件回调
        private void onClickViewDeckButtonCallback()
        {
            onClickViewDeckButton?.Invoke(this);
        }
        private void onClickSetDeckButtonCallback()
        {
            onClickSetDeckButton?.Invoke(this);
        }
        private void onClickRemoveButtonCallback()
        {
            onClickRemoveButton?.Invoke(this);
        }
        #endregion
        #endregion
        #region 事件
        public event Action<RoomPlayerListItem> onClickViewDeckButton;
        public event Action<RoomPlayerListItem> onClickSetDeckButton;
        public event Action<RoomPlayerListItem> onClickRemoveButton;
        #endregion
        #region 属性字段
        [Header("Texts")]
        [SerializeField]
        private Text _nameText;
        [SerializeField]
        private Text _characterText;

        [Header("GameObjects")]
        [SerializeField]
        private GameObject _setDeckButtonObj;
        [SerializeField]
        private GameObject _viewDeckButtonObj;

        [Header("Buttons")]
        [SerializeField]
        private Button _setDeckButton;
        [SerializeField]
        private Button _viewDeckButton;
        [SerializeField]
        private Button _removePlayerButton;

        [Header("Images")]
        [SerializeField]
        private Image _readyIconImage;
        [SerializeField]
        private Image _uploadIconImage;
        [SerializeField]
        private Image _downloadIconImage;
        [SerializeField]
        private Image _checkingIconImage;
        [SerializeField]
        private Image _progressBar;
        #endregion

        #region 内嵌类
        public enum PlayerListMode
        {
            Player,
            AIPlayer
        }
        #endregion
    }

    public struct RoomPlayerItemViewData
    {
        public string name;
        public string characterName;
        public PlayerLoadState loadState;
        public bool isReady;
        public bool canSetDeck;
        public bool canViewDeck;
        public bool canRemove;
        public bool isAI;
    }
}
