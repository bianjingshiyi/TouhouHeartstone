using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using Game;
using Game.Replays;
using MukioI18n;
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
            await updateSelected();
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
            _ui.onDeleteItemClick += onDeleteItemClick;
            _ui.onClearReplayClick += onClearReplayClick;
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
            if (_selectIndex < 0 || _selectIndex >= _metas.Count)
                return;
            var meta = _metas[_selectIndex];
            if (!isReplayCanPlay(meta, out string msg))
            {
                var title = game.lang._(StringTable.ERROR);
                game.ui.dialog.ShowConfirm(title, msg);
                return;
            }
            string path = meta.path;
            ReplayData replay = await replayManager.loadReplay(path);
            if (replay == null)
            {
                var title = game.lang._(StringTable.ERROR);
                var content = game.lang._(REPLAY_CANNOT_PLAY_REPLAY_LOAD_FAILED);
                game.ui.dialog.ShowConfirm(title, content);
                return;
            }
            hide();
            _ = gameManager.startReplayGame(replay);
        }
        private async void onReplayListClick(int index)
        {
            _selectIndex = index;
            await updateSelected();
        }
        private void onDeleteItemClick(int index)
        {
            var title = game.lang._(StringTable.WARNING);
            var content = game.lang._(REPLAY_CONFIRM_DELETE_REPLAY);
            game.ui.dialog.ShowQuestion(title, content, async () =>
            {
                await deleteReplayItem(index);
            });
        }
        private void onClearReplayClick()
        {
            var title = game.lang._(StringTable.WARNING);
            var content = game.lang._(REPLAY_CONFIRM_CLEAR_REPLAY);
            game.ui.dialog.ShowQuestion(title, content, async () =>
            {
                await deleteReplays();
            });
        }
        #endregion

        private async Task loadMetas()
        {
            var paths = replayManager.getPaths();
            _metas.Clear();
            _metas.AddRange(await replayManager.loadMetas(paths));
        }
        private async Task updateView()
        {
            //转化为显示数据并刷新显示
            ReplayItemViewData[] viewDatas = new ReplayItemViewData[_metas.Count];
            for (int i = 0; i < viewDatas.Length; i++)
            {
                var meta = _metas[i];
                viewDatas[i] = await getMetaViewData(meta);
            }
            _ui.updateView(viewDatas);
            _ui.setClearButtonInteractable(_metas.Count > 0);
            await updateSelected();
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
        private async Task<string> getMasterRefName(DefineReference defRef, ICardPoolReference[] pools)
        {
            var cardManager = game.card;
            var poolRef = pools.FirstOrDefault(p => p.CardPoolID == defRef.cardPoolId);
            if (poolRef != null)
            {
                await cardManager.getOrLoadCardPool(poolRef, true);
                return main.card.getCardName(poolRef, defRef.defineId);
            }
            return game.lang._(StringTable.BUILD_UNKNOWN_CARD_NAME);
        }
        private async Task deleteReplayItem(int index)
        {
            if (index == _selectIndex)
            {
                _selectIndex = -1;
                await updateSelected();
            }
            var path = _metas[index].path;
            game.replays.deleteReplay(path);
            _metas.RemoveAt(index);
            await updateView();
        }
        private async Task deleteReplays()
        {
            foreach (var meta in _metas)
            {
                game.replays.deleteReplay(meta.path);
            }
            _metas.Clear();
            await updateView();
            _selectIndex = -1;
            await updateSelected();
        }
        private async Task updateSelected()
        {
            _ui.setSelectedItem(_selectIndex);
            _ui.setPlayButtonInteractable(_selectIndex >= 0 && _selectIndex < _metas.Count);
            _ui.setDescText(await getDescText(_selectIndex));
        }
        private async Task<string> getDescText(int index)
        {
            if (index < 0 || index >= _metas.Count)
                return string.Empty;
            var meta = _metas[index];
            if (meta is InvalidReplayMeta)
                return game.lang._(REPLAY_DESC_REPLAY_CORRUPTED);
            var pools = meta.requiredCardPools;
            var cardManager = game.card;
            var selfMaster = await getMasterRefName(meta.playerAMasterDefRef, pools);
            var enemyMaster = await getMasterRefName(meta.playerBMasterDefRef, pools);
            var poolStrings = pools.Select(p => game.lang._(StringTable.EDITOR_CARDPOOL_NAME_VERSION_TEMPLATE_DOT, game.card.getCardPoolName(p), p.version));
            var cardPools = string.Join("\n", poolStrings);
            return game.lang._(REPLAY_DESC_TEMPLATE, selfMaster, enemyMaster, cardPools);
        }
        private bool isReplayCanPlay(ReplayMeta meta, out string msg)
        {
            if (meta is InvalidReplayMeta)
            {
                msg = game.lang._(REPLAY_CANNOT_PLAY_REPLAY_CORRUPTED);
                return false;
            }
            var cardManager = game.card;
            foreach (var poolRef in meta.requiredCardPools)
            {
                if (!cardManager.isCardPoolInfoLoaded(poolRef, true))
                {
                    msg = getMissingPoolString(meta);
                    return false;
                }
            }
            msg = null;
            return true;
        }
        private string getMissingPoolString(ReplayMeta meta)
        {
            const int maxDisplayPoolCount = 5;
            var pools = meta.requiredCardPools;
            var cardManager = game.card;
            var poolStrings = pools
                .Where(p => !cardManager.isCardPoolInfoLoaded(p))
                .Take(maxDisplayPoolCount)
                .Select(p => game.lang._(StringTable.EDITOR_CARDPOOL_NAME_VERSION_TEMPLATE, game.card.getCardPoolName(p), p.version));
            var cardPools = string.Join(",", poolStrings);
            if (pools.Length > 5)
            {
                cardPools = game.lang._(StringTable.TRIMMED_COTENTS, cardPools);
            }
            return game.lang._(REPLAY_CANNOT_PLAY_REPLAY_MISSING_CARDPOOL, cardPools);
        }
        #endregion
        #region 属性字段
        [TranslateMsg("删除单个回放时的警告")]
        public const string REPLAY_CONFIRM_DELETE_REPLAY = "确认要删除这条回放？";
        [TranslateMsg("删除所有回放时的警告")]
        public const string REPLAY_CONFIRM_CLEAR_REPLAY = "确认要清除所有回放？";
        [TranslateMsg("无法播放回放时的错误，0为缺失的卡池")]
        public const string REPLAY_CANNOT_PLAY_REPLAY_MISSING_CARDPOOL = "无法播放该回放，以下卡池缺失：\n{0}";
        [TranslateMsg("无法播放回放时的回放描述")]
        public const string REPLAY_CANNOT_PLAY_REPLAY_CORRUPTED = "无法播放该回放，该回放已损坏。";
        [TranslateMsg("无法播放回放时的回放描述")]
        public const string REPLAY_CANNOT_PLAY_REPLAY_LOAD_FAILED = "无法播放该回放，该回放的内容加载失败。";
        [TranslateMsg("无法播放回放时的回放描述")]
        public const string REPLAY_DESC_REPLAY_CORRUPTED = "回放已损坏";
        [TranslateMsg("回放描述模板，0为己方英雄的卡牌名，1为对方英雄的卡牌名，2为所需的卡池列表")]
        public const string REPLAY_DESC_TEMPLATE = "己方英雄：{0}\n对方英雄：{1}\n所需卡池：\n{2}";
        private ReplayManager replayManager => main.replays;
        private GameManager gameManager => game.game;
        private FileManager fileManager => game.fileManager;
        private Main mainUI => game.ui.getObject<Main>();
        private MainManager main => this.findInstance<MainManager>();
        private List<ReplayMeta> _metas = new List<ReplayMeta>();
        private int _selectIndex = -1;
        [SerializeField]
        private ReplaySelectPanel _ui;
        #endregion
    }
}
