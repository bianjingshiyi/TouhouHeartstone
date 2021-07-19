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
                BJSYGameCore.UI.UIHelper.updateList(_replayListRoot, _replayListTemplate.transform as RectTransform, _replayList, viewData.replayItemViewDatas.Length, (i, item) =>
                    {
                        ReplayItem replayItem = item.GetComponent<ReplayItem>();
                        replayItem.updateView(viewData.replayItemViewDatas[i]);
                        replayItem.setSelected(i == viewData.selectIndex);
                    }, (item) =>
                    {
                        ReplayItem replayItem = item.GetComponent<ReplayItem>();
                        replayItem.onAwake();
                        replayItem.onClick += onReplayListClickCallback;
                    }, (item) =>
                    {
                        item.GetComponent<ReplayItem>().onClick -= onReplayListClickCallback;
                    });
            }
        }
        private void onReplayListClickCallback(ReplayItem item)
        {
            if (onReplayListClick != null)
                onReplayListClick(this, _replayList.IndexOf(item.transform as RectTransform));
        }
        public event Action<ReplaySelectPanel, int> onReplayListClick;
        GameObject _replayListTemplate;
        readonly List<RectTransform> _replayList = new List<RectTransform>();
    }
    public class ReplaySelectViewData
    {
        public int selectIndex = -1;
        public ReplayItemViewData[] replayItemViewDatas = null;
    }
}
