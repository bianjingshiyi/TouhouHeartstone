using BJSYGameCore.UI;
using System.Collections.Generic;
using BJSYGameCore;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Game.Replays;
using System.Linq;
using System.IO;
using MongoDB.Bson.Serialization;
using Game;
using Controllers;

namespace UI
{
    partial class ReplaySelectPanel : UIObject
    {
        public void onAwake()
        {
            autoInit();
            _replayListTemplate = _replayListRoot.transform.Find("ReplayItem").gameObject;
            _replayListTemplate.GetComponent<ReplayItem>().onAwake();
            _replayListTemplate.gameObject.SetActive(false);
        }
        public void updateView(ReplaySelectViewData viewData)
        {
            _playButton.interactable = viewData.selectIndex >= 0;
            if (viewData.replayItemViewDatas != null)
            {
                _replayList.updateList(_replayListTemplate, _replayListRoot, viewData.replayItemViewDatas,
                    onReplayListCreate, onReplayListUpdate, onReplayListRecycle);
            }
            for (int i = 0; i < _replayList.Count; i++)
            {
                _replayList[i].setSelected(viewData.selectIndex == i);
            }
        }
        ReplayItem onReplayListCreate(GameObject gameObject)
        {
            ReplayItem item = gameObject.GetComponent<ReplayItem>();
            item.onAwake();
            item.onClick += onReplayListClickCallback;
            return item;
        }
        void onReplayListUpdate(ReplayItem item, ReplayItemViewData viewData)
        {
            item.updateView(viewData);
        }
        void onReplayListRecycle(ReplayItem item)
        {
            item.onClick -= onReplayListClickCallback;
        }
        private void onReplayListClickCallback(ReplayItem item)
        {
            if (onReplayListClick != null)
                onReplayListClick(this, _replayList.IndexOf(item));
        }
        public event Action<ReplaySelectPanel, int> onReplayListClick;
        GameObject _replayListTemplate;
        readonly List<ReplayItem> _replayList = new List<ReplayItem>();
    }
    public class ReplaySelectViewData
    {
        public int selectIndex = -1;
        public ReplayItemViewData[] replayItemViewDatas = null;
    }
}
