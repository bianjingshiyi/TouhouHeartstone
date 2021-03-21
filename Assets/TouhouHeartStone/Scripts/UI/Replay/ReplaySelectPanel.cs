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
                    lobbySceneCtrl = new LobbySceneCtrl(this, scene);
            }
        }

        public ReplaySystem replays { get; } = new ReplaySystem();

        public LobbySceneCtrl lobbySceneCtrl { get; private set; }
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
            mainMenuCtrl = new MainMenuCtrl(this, scene.findInstanceAt<Transform>("Canvas/Main/MainMenu").gameObject.GetComponent<MainMenu>());
            replaySelectCtrl = new ReplaySelectPanelCtrl(this, scene.findInstanceAt<Transform>("Canvas/Main/Replay").gameObject.GetComponent<ReplaySelectPanel>());
        }

        public IController parent
        {
            get { return main; }
        }

        public IAppManager app => parent.app;

        public MainCtrl main { get; }
        public MainMenuCtrl mainMenuCtrl { get; }
        public ReplaySelectPanelCtrl replaySelectCtrl { get; }
    }
    public class MainMenuCtrl : IController<MainCtrl>
    {
        public MainMenuCtrl(LobbySceneCtrl parent, MainMenu ui)
        {
            _parent = parent;
            main = parent.main;
            _ui = ui;
            _ui.onReplayButtonClick += onReplayButtonClick;
        }
        public void display()
        {
            _ui.gameObject.SetActive(true);
        }
        private async void onReplayButtonClick()
        {
            _ui.gameObject.SetActive(false);
            await _parent.replaySelectCtrl.displayAsync();
        }

        public MainCtrl main { get; }
        public IController parent => _parent;
        public IAppManager app => parent.app;
        LobbySceneCtrl _parent;
        MainMenu _ui;
    }
    public partial class ReplaySelectPanelCtrl : IController<MainCtrl>
    {
        public ReplaySelectPanelCtrl(LobbySceneCtrl parent, ReplaySelectPanel ui)
        {
            _parent = parent;
            main = parent.main;
            _ui = ui;
            _ui.onAwake();
            _ui.onPlayButtonClick += onPlayButtonClick;
            _ui.onReplayListClick += onReplayListClick;
            _ui.onBackButtonClick += onReturnButtonClick;
        }

        private void onReturnButtonClick(ReplaySelectPanel ui)
        {
            ui.gameObject.SetActive(false);
            _parent.mainMenuCtrl.display();
        }

        public async Task displayAsync()
        {
            //读取Replay文件头
            _paths = app.fileManager.getFiles(main.replays.getSaveDir(), "*." + main.replays.getSaveFileExtension(), true);
            string[][] fileHeads = await app.fileManager.readTextFilesHead(_paths);
            //转化为显示数据并刷新显示
            ReplayItemViewData[] viewDatas = new ReplayItemViewData[fileHeads.Length];
            for (int i = 0; i < fileHeads.Length; i++)
            {
                int[] masterIds = BsonSerializer.Deserialize<int[]>(fileHeads[i][0]);
                viewDatas[i] = new ReplayItemViewData()
                {
                    name = Path.GetFileNameWithoutExtension(_paths[i]),
                    playerAMasterId = masterIds[0],
                    playerBMasterId = masterIds[1]
                };
            }
            _ui.updateView(new ReplaySelectViewData()
            {
                replayItemViewDatas = viewDatas
            });
            _ui.gameObject.SetActive(true);
        }
        public void hide()
        {
            _ui.gameObject.SetActive(false);
        }
        private async void onPlayButtonClick(ReplaySelectPanel ui)
        {
            string path = _paths[_selectIndex];
            var content = await app.fileManager.readTextFileHead(path, 1, 1);
            string json = content[0];
            ReplayData replay = BsonSerializer.Deserialize<ReplayData>(json);
            hide();
            _ = (app as MainManager).getManager<GameManager>().startReplayGame(replay);
        }

        private void onReplayListClick(ReplaySelectPanel ui, int index)
        {
            _selectIndex = index;
            _ui.updateView(new ReplaySelectViewData()
            {
                selectIndex = index
            });
        }

        public MainCtrl main { get; }

        public IController parent => _parent;

        public IAppManager app => parent.app;
        string[] _paths;
        int _selectIndex = -1;
        LobbySceneCtrl _parent;
        ReplaySelectPanel _ui;
    }
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
