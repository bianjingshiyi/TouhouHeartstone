using System.IO;
using System.Threading.Tasks;
using BJSYGameCore;
using Game;
using Game.Replays;
using MongoDB.Bson.Serialization;
using TouhouCardEngine;
using UI;

namespace Controllers
{
    class ReplaySelectPanelCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
        public async Task display()
        {
            //读取Replay文件头
            _paths = replayManager.getPaths();
            await updateView();
            _ui.gameObject.SetActive(true);
        }
        public void hide()
        {
            _ui.gameObject.SetActive(false);
        }
        #endregion

        #region 私有方法

        #region 生命周期
        private void Awake()
        {
            _ui.onPlayButtonClick += onPlayButtonClick;
            _ui.onReplayListClick += onReplayListClick;
            _ui.onBackButtonClick += onReturnButtonClick;
        }
        #endregion

        #region 事件回调
        private void onReturnButtonClick(ReplaySelectPanel ui)
        {
            hide();
            mainUI.display(mainUI.MainMenu);
        }
        private async void onPlayButtonClick(ReplaySelectPanel ui)
        {
            string path = _paths[_selectIndex];
            var content = await fileManager.readPersistentTextFile(path, 1, 1);
            string json = content[0];
            ReplayData replay = BsonSerializer.Deserialize<ReplayData>(json);
            hide();
            _ = gameManager.startReplayGame(replay);
        }
        private void onReplayListClick(ReplaySelectPanel ui, int index)
        {
            _selectIndex = index;
            _ui.updateView(new ReplaySelectViewData()
            {
                selectIndex = index
            });
        }
        #endregion
        private async Task updateView()
        {
            string[][] fileHeads = await main.fileManager.readPersistentTextFiles(_paths);
            //转化为显示数据并刷新显示
            ReplayItemViewData[] viewDatas = new ReplayItemViewData[fileHeads.Length];
            for (int i = 0; i < fileHeads.Length; i++)
            {
                if (BsonSerializer.Deserialize<int[]>(fileHeads[i][0]) is int[] masterIds)
                {
                    viewDatas[i] = new ReplayItemViewData()
                    {
                        name = Path.GetFileNameWithoutExtension(_paths[i]),
                        playerAMasterDefRef = new DefineReference(0, masterIds[0]),
                        playerBMasterDefRef = new DefineReference(0, masterIds[1])
                    };
                }
                else if (BsonSerializer.Deserialize<DefineReference[]>(fileHeads[i][0]) is DefineReference[] masterDefRefs)
                {
                    viewDatas[i] = new ReplayItemViewData()
                    {
                        name = Path.GetFileNameWithoutExtension(_paths[i]),
                        playerAMasterDefRef = masterDefRefs[0],
                        playerBMasterDefRef = masterDefRefs[1]
                    };
                }
            }
            _ui.updateView(new ReplaySelectViewData()
            {
                replayItemViewDatas = viewDatas
            });
        }
        #endregion
        #region 属性字段
        private ReplayManager replayManager => main.replays;
        private GameManager gameManager => game.game;
        private FileManager fileManager => game.fileManager;
        private Main mainUI => game.ui.getObject<Main>();
        private MainManager main => this.findInstance<MainManager>();
        string[] _paths;
        int _selectIndex = -1;
        ReplaySelectPanel _ui;
        #endregion
    }
}
