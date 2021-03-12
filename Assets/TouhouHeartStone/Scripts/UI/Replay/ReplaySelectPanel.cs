using BJSYGameCore.UI;
using System.Collections.Generic;
using BJSYGameCore;
using UnityEngine;

namespace UI
{
    partial class ReplaySelectPanel : UIObject
    {
        public void updateView(ReplayItemViewData[] replayItemViewDatas)
        {
            _replayList.updateList(_replayListTemplate, _replayListRoot, replayItemViewDatas,
                onReplayListCreate, onReplayListUpdate, onReplayListRecycle);
        }
        protected override void Awake()
        {
            base.Awake();
            autoInit();
            _playButton.onClick.AddListener(onPlayerButtonClick);
        }
        void onPlayerButtonClick()
        {

        }
        ReplayItem onReplayListCreate(GameObject gameObject)
        {
            ReplayItem item = gameObject.GetComponent<ReplayItem>();
            return item;
        }
        void onReplayListUpdate(ReplayItem item, ReplayItemViewData viewData)
        {
            item.updateView(viewData);
        }
        void onReplayListRecycle(ReplayItem item)
        {

        }
        GameObject _replayListTemplate;
        List<ReplayItem> _replayList;
    }
}
