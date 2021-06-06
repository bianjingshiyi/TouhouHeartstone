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
    public partial class MainCtrl : IController<MainCtrl>
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
                    lobbySceneCtrl = new LobbySceneCtrl(app, this, scene);
            }
        }

        public ReplaySystem replays { get; } = new ReplaySystem();

        public LobbySceneCtrl lobbySceneCtrl { get; private set; }
        public IAppManager app { get; }
        public MainCtrl main => this;
    }
    public class LobbySceneCtrl : IController<MainCtrl>
    {
        public LobbySceneCtrl(IAppManager app, MainCtrl main, Scene scene)
        {
            this.app = app;
            this.main = main;
            mainMenuCtrl = new MainMenuCtrl(app, main, scene.findInstanceAt<Transform>("Canvas/Main/MainMenu").gameObject.GetComponent<MainMenu>());
            replaySelectCtrl = new ReplaySelectPanelCtrl(app, main, scene.findInstanceAt<Transform>("Canvas/Main/Replay").gameObject.GetComponent<ReplaySelectPanel>());
            editorCtrl = new EditorCtrl(app, main, scene.findInstanceAt<Transform>("Canvas/Main/EditorPanel").gameObject.GetComponent<EditorPanel>());
            editorCtrl.onStart();
        }
        public IAppManager app { get; }
        public MainCtrl main { get; }
        public MainMenuCtrl mainMenuCtrl { get; }
        public ReplaySelectPanelCtrl replaySelectCtrl { get; }
        public EditorCtrl editorCtrl { get; }
    }
    public class MainMenuCtrl : IController<MainCtrl>
    {
        public MainMenuCtrl(IAppManager app, MainCtrl main, MainMenu ui)
        {
            this.app = app;
            this.main = main;
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
            await main.lobbySceneCtrl.replaySelectCtrl.displayAsync();
        }
        public MainCtrl main { get; }
        public IAppManager app { get; }
        MainMenu _ui;
    }
    public partial class ReplaySelectPanelCtrl : IController<MainCtrl>
    {
        public ReplaySelectPanelCtrl(IAppManager app, MainCtrl main, ReplaySelectPanel ui)
        {
            this.app = app;
            this.main = main;
            _ui = ui;
            _ui.onAwake();
            _ui.onPlayButtonClick += onPlayButtonClick;
            _ui.onReplayListClick += onReplayListClick;
            _ui.onBackButtonClick += onReturnButtonClick;
        }
        private void onReturnButtonClick(ReplaySelectPanel ui)
        {
            ui.gameObject.SetActive(false);
            main.lobbySceneCtrl.mainMenuCtrl.display();
        }
        public async Task displayAsync()
        {
            //读取Replay文件头
            _paths = app.fileManager.getFiles(main.replays.getSaveDir(), "*." + main.replays.getSaveFileExtension(), true);
            string[][] fileHeads = await app.fileManager.readTextFiles(_paths);
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
            var content = await app.fileManager.readTextFile(path, 1, 1);
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
        public IAppManager app { get; }
        string[] _paths;
        int _selectIndex = -1;
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
