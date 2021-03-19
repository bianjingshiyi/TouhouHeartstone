using BJSYGameCore.UI;
using System.Collections.Generic;
using BJSYGameCore;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Game.Replays;

namespace UI
{
    public interface IAppManager
    {
        FileManager fileManager { get; }
    }
    public interface IController
    {
        IAppManager app { get; }
    }
    public interface IController<TMainCtrl> : IController where TMainCtrl : IController
    {
        TMainCtrl main { get; }
        IController parent { get; }
    }
    public partial class MainCtrl : IController
    {
        public MainCtrl(IAppManager appManager)
        {
            app = appManager;
        }
        public void onAwake()
        {

        }
        public void onStart()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == "NewUI")
                    _lobbySceneCtrl = new LobbySceneCtrl(this, scene);
            }
        }

        public ReplaySystem replays { get; } = new ReplaySystem();

        LobbySceneCtrl _lobbySceneCtrl;
        public IController parent
        {
            get { return this; }
        }
        public IAppManager app { get; }
    }
    public class LobbySceneCtrl : IController<MainCtrl>
    {
        public LobbySceneCtrl(MainCtrl parent, Scene scene)
        {
            main = parent;
            _replayCtrl = new ReplaySelectPanelCtrl(this, scene.findInstanceAt<Transform>("Canvas/Main/Replay").gameObject.AddComponent<ReplaySelectPanel>());
        }

        public IController parent
        {
            get { return main; }
        }

        public IAppManager app => parent.app;

        public MainCtrl main { get; }

        ReplaySelectPanelCtrl _replayCtrl;
    }
    public partial class ReplaySelectPanelCtrl : IController<MainCtrl>
    {
        public ReplaySelectPanelCtrl(IController<MainCtrl> parent, ReplaySelectPanel ui)
        {
            this.parent = parent;
            this.main = parent.main;
            _ui = ui;
            _ui.onPlayButtonClick += onPlayButtonClick;
            _ui.onReplayListClick += onReplayListClick;
        }

        public async Task displayAsync()
        {
            //读取Replay文件
            app.fileManager.getFiles(main.replays.getSaveDir(), "*." + main.replays.getSaveFileExtension(), true);
            //_ui.updateView();
        }

        private void onPlayButtonClick(ReplaySelectPanel ui)
        {
            throw new NotImplementedException();
        }

        private void onReplayListClick(ReplaySelectPanel ui, int index)
        {
            throw new NotImplementedException();
        }

        public MainCtrl main { get; }

        public IController parent { get; }

        public IAppManager app => parent.app;

        ReplaySelectPanel _ui;
    }
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
        }
        ReplayItem onReplayListCreate(GameObject gameObject)
        {
            ReplayItem item = gameObject.GetComponent<ReplayItem>();
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
        List<ReplayItem> _replayList;
    }
}
