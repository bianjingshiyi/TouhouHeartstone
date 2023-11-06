using System;
using System.Collections.Generic;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    partial class ReplaySelectPanel : UIObject
    {
        #region 公有方法
        public void updateView(ReplayItemViewData[] viewData)
        {
            if (viewData == null)
                return;
            UIHelper.updateList(_replayListRoot, _replayListTemplate.transform as RectTransform, _replayList, viewData.Length, (i, item) =>
            {
                ReplayItem replayItem = item.GetComponent<ReplayItem>();
                replayItem.updateView(viewData[i]);
            },
            (item) =>
            {
                ReplayItem replayItem = item.GetComponent<ReplayItem>();
                replayItem.onClick += onReplayListClickCallback;
            },
            (item) =>
            {
                ReplayItem replayItem = item.GetComponent<ReplayItem>();
                replayItem.onClick -= onReplayListClickCallback;
            });
        }
        public void setSelectedItem(int index)
        {
            for (int i = 0; i < _replayList.Count; i++)
            {
                var replayItem = _replayList[i].GetComponent<ReplayItem>();
                replayItem.setSelected(i == index);
            }
        }
        public void setPlayButtonInteractable(bool interactable)
        {
            _playButton.interactable = interactable;
        }
        #endregion

        #region 私有方法

        #region 生命周期
        protected override void Awake()
        {
            _replayListTemplate.gameObject.SetActive(false);
            _playButton.onClick.AddListener(() => onPlayButtonClick?.Invoke());
            _backButton.onClick.AddListener(() => onBackButtonClick?.Invoke());
        }
        #endregion

        #region 事件回调
        private void onReplayListClickCallback(ReplayItem item)
        {
            onReplayListClick?.Invoke(_replayList.IndexOf(item.transform as RectTransform));
        }
        #endregion

        #endregion

        #region 事件
        public event Action onPlayButtonClick;
        public event Action onBackButtonClick;
        public event Action<int> onReplayListClick;
        #endregion

        #region 属性字段
        [SerializeField]
        private Button _playButton;
        [SerializeField]
        private Button _backButton;
        [SerializeField]
        private RectTransform _replayListRoot;
        [SerializeField]
        private GameObject _replayListTemplate;
        [SerializeField]
        private readonly List<RectTransform> _replayList = new List<RectTransform>();
        #endregion
    }
}
