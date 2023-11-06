using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using Game;
using Game.Replays;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using UI;
using UnityEngine;

namespace Controllers
{
    class ReplaySelectPanelCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
        public async Task display()
        {
            //读取Replay文件头
            await loadMetas();
            await updateView();
            _ui.gameObject.SetActive(true);
            _selectIndex = -1;
            updateSelected();
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
        private void onReturnButtonClick()
        {
            hide();
            mainUI.display(mainUI.MainMenu);
        }
        private async void onPlayButtonClick()
        {
            string path = _metas[_selectIndex].path;
            var content = await fileManager.readPersistentTextFile(path, 1, 1);
            string json = content[0];
            ReplayData replay = JsonHelper.fromJson<ReplayData>(json);
            hide();
            _ = gameManager.startReplayGame(replay);
        }
        private void onReplayListClick(int index)
        {
            _selectIndex = index;
            updateSelected();
        }
        #endregion

        private async Task loadMetas()
        {
            var paths = replayManager.getPaths();
            _metas = new ReplayMeta[paths.Length];
            for (int i = 0; i < _metas.Length; i++)
            {
                var path = paths[i];
                string[] fileHeads = await fileManager.readPersistentTextFile(path, 0, 1);
                string fileHead = fileHeads[0];
                var seri = JsonHelper.fromJson<SerializableReplayMeta>(fileHead);
                var meta = seri.toMeta();
                meta.path = path;
                _metas[i] = meta;
            }
        }
        private async Task updateView()
        {
            //转化为显示数据并刷新显示
            ReplayItemViewData[] viewDatas = new ReplayItemViewData[_metas.Length];
            for (int i = 0; i < viewDatas.Length; i++)
            {
                var meta = _metas[i];
                viewDatas[i] = await getMetaViewData(meta);
            }
            _ui.updateView(viewDatas);
            updateSelected();
        }
        private async Task<ReplayItemViewData> getMetaViewData(ReplayMeta meta)
        {
            var cardPools = meta.requiredCardPools;
            var spriteA = await getMasterRefSprite(meta.playerAMasterDefRef, cardPools);
            var spriteB = await getMasterRefSprite(meta.playerBMasterDefRef, cardPools);
            return new ReplayItemViewData()
            {
                name = meta.name,
                playerAMasterSprite = spriteA,
                playerBMasterSprite = spriteB
            };
        }
        private async Task<Sprite> getMasterRefSprite(DefineReference defRef, ICardPoolReference[] pools)
        {
            var cardManager = game.card;
            var poolRef = pools.FirstOrDefault(p => p.CardPoolID == defRef.cardPoolId);
            if (poolRef == null)
                return cardManager.getDefaultSprite();
            await cardManager.getOrLoadCardPool(poolRef, true);
            var define = game.card.getCardDefine(poolRef, defRef.defineId);
            if (define == null)
                return cardManager.getDefaultSprite();
            return await main.card.loadCardImage(define);
        }
        private void updateSelected()
        {
            _ui.setSelectedItem(_selectIndex);
            _ui.setPlayButtonInteractable(isReplayCanPlay(_selectIndex));
        }
        private bool isReplayCanPlay(int index)
        {
            if (index < 0 || index >= _metas.Length)
                return false;
            var meta = _metas[index];
            return isReplayCanPlay(meta);
        }
        private bool isReplayCanPlay(ReplayMeta meta)
        {
            var cardManager = game.card;
            foreach (var poolRef in meta.requiredCardPools)
            {
                if (!cardManager.hasCardPool(poolRef, true))
                    return false;
            }
            return true;
        }
        #endregion
        #region 属性字段
        private ReplayManager replayManager => main.replays;
        private GameManager gameManager => game.game;
        private FileManager fileManager => game.fileManager;
        private Main mainUI => game.ui.getObject<Main>();
        private MainManager main => this.findInstance<MainManager>();
        private ReplayMeta[] _metas;
        private int _selectIndex = -1;
        [SerializeField]
        private ReplaySelectPanel _ui;
        #endregion
    }
}
