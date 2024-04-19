using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ExcelLibrary.BinaryFileFormat;
using Game;
using MukioI18n;
using NitoriNetwork.Common;
using Steamworks;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using UI;
using UnityEngine;

namespace Controllers
{
    class WorkshopCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
        public async Task display()
        {
            gameObject.SetActive(true);
            _page = 0;
            _ui.hideDetails();
            await updatePage();
            updateDownload();
        }
        public void hide()
        {
            gameObject.SetActive(false);
        }
        #endregion

        #region 私有方法

        #region 生命周期
        private void Awake()
        {
            _ui.setSortItems(_sortNames.Select(n => game.lang._(n)).ToArray());
            _ui.onReturnClick += onReturnClickCallback;

            _ui.onSortChanged += onSortChangedCallback;
            _ui.onOnlyMineChanged += onOnlyMeChangedCallback;
            _ui.onCreatorFilterChanged += onCreatorFilterChangedCallback;
            _ui.onSearchFilterChanged += onSearchFilterChangedCallback;

            _ui.onCardPoolItemClick += onClickCardPoolItemCallback;
            _ui.onPageButtonClicked += onPageButtonClickCallback;

            _ui.onDownloadClick += onDownloadClickCallback;
            _ui.onUploadClick += onUploadClickCallback;

            _ui.onClickSelectUploadPoolCloseButton += onClickSelectUploadPoolCloseButtonCallback;
            _ui.onClickSelectUploadPoolItem += onClickSelectUploadPoolItemCallback;

            _ui.onClickWorkshopRuleViewButton += onClickWorkshopRuleViewButtonCallback;
            _ui.onClickUploadConfirmButton += onClickUploadConfirmButtonCallback;
            _ui.onClickUploadCloseButton += onClickUploadCloseButtonCallback;
        }
        #endregion

        #region 事件回调
        private void onReturnClickCallback()
        {
            hide();
            scene.menu.display();
        }

        #region 筛选
        private void onSortChangedCallback(int index)
        {
            _sorting = _sortValues[index];
            _page = 0;
            _ = updatePage();
        }
        private void onOnlyMeChangedCallback(bool value)
        {
            _onlyMineFilter = value;
            _page = 0;
            _ = updatePage();
        }
        private void onCreatorFilterChangedCallback(string filter)
        {
            _creatorFilter = filter;
            _page = 0;
            _ = updatePage();
        }
        private void onSearchFilterChangedCallback(string filter)
        {
            _searchFilter = filter;
            _page = 0;
            _ = updatePage();
        }
        #endregion

        #region 卡池列表
        private void onClickCardPoolItemCallback(int index)
        {
            var pool = _currentPagePools[index];
            var viewData = getCardPoolViewData(index);
            var creator = pool.creator;
            var creationTime = pool.creationTime;
            var dateTime = game.lang._(TASK_DATETIME_TEMPLATE, creationTime.Year, creationTime.Month, creationTime.Day, creationTime.Hour, creationTime.Minute);
            var cardCount = pool.cardCount.ToString();
            var desc = pool.description;
            _ui.showDetails(viewData, creator, dateTime, cardCount, desc);
        }
        #endregion
        private async void onPageButtonClickCallback(bool next)
        {
            _page += next ? 1 : -1;
            await updatePage();
        }
        private void onDownloadClickCallback()
        {
        }
        private async void onUploadClickCallback()
        {
            startWaiting();
            try
            {
                var localPools = game.card.getLocalCardPoolInfos();
                _selectingUploadPools = localPools.Where(p => !game.card.isInnateCardPool(p.cardPoolId)).ToArray();

                var viewDatas = new DeckListItemViewData[_selectingUploadPools.Length];
                for (int i = 0; i < viewDatas.Length; i++)
                {
                    var pool = _selectingUploadPools[i];
                    string name = game.card.getCardPoolName(pool);
                    viewDatas[i] = new DeckListItemViewData()
                    {
                        name = name,
                        image = await game.card.loadCardPoolImage(pool)
                    };
                }
                _ui.setSelectingUploadPools(viewDatas);
                _ui.setSelectUploadPoolPanelVisible(true);
            }
            finally
            {
                stopWaiting();
            }
        }

        #region 选择上传卡池界面
        private void onClickSelectUploadPoolCloseButtonCallback()
        {
            _uploadingPoolId = 0;
            _selectingUploadPools = null;
            _ui.setSelectUploadPoolPanelVisible(false);
        }
        private async void onClickSelectUploadPoolItemCallback(int index)
        {
            startWaiting();
            try
            {
                var info = _selectingUploadPools[index];
                _uploadingPoolId = info.cardPoolId;
                var (_, pool) = await game.card.getOrLoadCardPool(info);
                var cardCount = pool.cardDefineList.Count;
                var version = info.editTime;
                var viewData = new DeckListItemViewData()
                {
                    image = await game.card.loadCardPoolImage(info),
                    name = game.card.getCardPoolName(info),
                };
                _ui.setUploadingPool(viewData, cardCount, version);
                _ui.setSelectUploadPoolPanelVisible(false);
                _ui.setUploadPoolPanelVisible(true);
            }
            catch (Exception e)
            {
                var title = game.lang._(StringTable.ERROR);
                var desc = game.lang._(ERROR_UPLOADING_CARDPOOL, e.Message);
                game.ui.dialog.ShowConfirm(title, desc);
                Debug.LogError($"选择上传卡池时出现错误：{e}");
            }
            finally
            {
                stopWaiting();
            }
        }
        #endregion

        #region 上传卡池界面
        private void onClickWorkshopRuleViewButtonCallback()
        {
            var content = string.Empty;
            _ui.showWorkshopRule(content);
        }
        private void onClickUploadCloseButtonCallback()
        {
            _ui.setUploadPoolPanelVisible(false);
            _ui.setSelectUploadPoolPanelVisible(true);
        }
        private async void onClickUploadConfirmButtonCallback(string desc, bool agreed)
        {
            if (!agreed)
            {
                var title = game.lang._(StringTable.HINT_TITLE);
                var quote = game.lang._(HINT_AGREE_RULE_FIRST);
                game.ui.dialog.ShowConfirm(title, quote);
                return;
            }

            startWaiting(true);
            try
            {
                await startUploadCardPool(desc);
            }
            catch (Exception e)
            {
                var title = game.lang._(StringTable.ERROR);
                var quote = game.lang._(ERROR_UPLOADING_CARDPOOL, e.Message);
                game.ui.dialog.ShowConfirm(title, quote);
                Debug.LogError($"上传卡池时出现错误：{e}");
            }
            finally
            {
                stopWaiting();
            }
        }
        #endregion

        #endregion

        #region 上传卡池
        private async Task startUploadCardPool(string desc)
        {

            // 上传卡池文件。
            setWaitingText(game.lang._(UPLOAD_PROGRESS_UPLOADING_POOL_FILE));
            setWaitingProgress(0);

            var info = game.card.getLocalCardPoolInfo(_uploadingPoolId);

            // 读取远端的卡池信息。
            var workshopInfo = await getWorkshopCardPoolFile(info);
            string[] requiredResources;
            if (workshopInfo == null)
            {
                // 创意工坊中不存在该卡池。
                requiredResources = await uploadCardPoolFile(info, desc);
            }
            else if (workshopInfo.State == WorkshopCardPoolState.Pending)
            {
                // 等待上传资源。
                requiredResources = workshopInfo.PendingResources;
            }
            else
            {
                // 已经上传完所有资源，直接结束
                var title = game.lang._(StringTable.HINT_TITLE);
                var quote = game.lang._(HINT_CARD_POOL_ALREADY_UPLOADED);
                game.ui.dialog.ShowConfirm(title, quote);
                return;
            }


            // 上传资源
            setWaitingText(game.lang._(UPLOAD_PROGRESS_UPLOADING_RESOURCES));
            setWaitingProgress(uploadCardPoolProgress);

            var notFoundResources = await uploadCardPoolResources(info, requiredResources);

            if (notFoundResources.Length > 0)
            {
                var title = game.lang._(StringTable.ERROR);
                var quote = game.lang._(HINT_UPLOAD_RESOURCE_FAILED, notFoundResources.Length);
                game.ui.dialog.ShowConfirm(title, quote);
                Debug.LogError($"上传卡池资源失败：\n{string.Join(",\n", notFoundResources)}");
            }
            else
            {
                var title = game.lang._(StringTable.HINT_TITLE);
                var quote = game.lang._(HINT_UPLOAD_CARDPOOL_SUCCESS);
                game.ui.dialog.ShowConfirm(title, quote);
            }
        }
        private async Task<string[]> uploadCardPoolFile(ICardPoolReference poolRef, string desc)
        {
            var path = game.card.getCardPoolPath(poolRef);
            var data = await game.fileManager.readBinaryFile(path);
            var compressed = Compression.compress(data);
            return await game.network.uploadWorkshopCardPool(desc, compressed);
        }
        private async Task<string[]> uploadCardPoolResources(CardPoolInfo poolInfo, string[] resources)
        {
            var (_, pool) = await game.card.getOrLoadCardPool(poolInfo);
            var cardPoolResources = CardManager.getCardPoolResources(poolInfo, pool);
            int totalResources = resources.Length;
            int uploadedResources = 0;
            List<string> notFoundResources = new List<string>();
            foreach (var res in resources)
            {
                var (type, resPath) = cardPoolResources.FirstOrDefault(r => r.path.StartsWith(res));
                if (string.IsNullOrEmpty(resPath))
                {
                    notFoundResources.Add(res);
                    continue;
                }
                var fullPath = game.card.getFullResourcePath(poolInfo, type, resPath);
                var resType = Path.GetExtension(resPath).TrimStart('.');
                var resData = await game.card.loadResourceBytes(type, fullPath);
                await game.network.uploadWorkshopResource(resType, res, resData);
                uploadedResources++;
                setWaitingProgress(uploadCardPoolProgress + uploadResourceProgress * (uploadedResources / (float)totalResources));
            }
            return notFoundResources.ToArray();
        }
        #endregion

        #region 获取卡池信息
        private async Task<WorkshopCardPoolInfo> getWorkshopCardPoolFile(ICardPoolReference poolRef)
        {
            try
            {
                return await game.network.getWorkshopCardPool(poolRef.CardPoolID.ToString(), poolRef.CardPoolVersion);
            }
            catch (Exception e)
            {
                Debug.Log($"未找到创意工坊卡池{poolRef.CardPoolID}的{poolRef.CardPoolVersion}版本：{e}");
            }
            return null;
        }
        #endregion
        private async Task updatePage()
        {
            startWaiting();
            await loadPage(_page);
            stopWaiting();

            var pageViewDatas = getCurrentPageViewData();
            _ui.updateCardPools(pageViewDatas);

            _ui.setPageText(game.lang._(StringTable.PAGE_NUMBER_TEMPLATE, _page + 1));
            _ui.setPageButtonInteractable(_page > 0, _currentPagePools.Length >= _poolCountPerPage);
        }
        private void updateDownload()
        {
            _ui.setDownloadButtonText(game.lang._(TASK_DOWNLOAD));
            _ui.setDownloadProgress(0);
        }
        private Task loadPage(int page)
        {
            _currentPagePools = new WorkshopPool[_poolCountPerPage];
            for (int i = 0; i < _currentPagePools.Length; i++)
            {
                _currentPagePools[i] = new WorkshopPool()
                {
                    sprite = game.card.getDefaultSprite(),
                    cardCount = 16,
                    creationTime = DateTime.Now,
                    creator = "Cuerzor58",
                    name = "未知卡池",
                    description = "描述",
                };
            }
            return Task.CompletedTask;
        }
        private void startWaiting(bool showProgress = false)
        {
            scene.waitingBlocker.addWaitingTask(WaitingTask.LoadWorkshop, showProgressBar: showProgress);
        }
        private void setWaitingText(string text)
        {
            scene.waitingBlocker.setText(text);
        }
        private void setWaitingProgress(float progress)
        {
            scene.waitingBlocker.setProgress(progress);
        }
        private void stopWaiting()
        {
            scene.waitingBlocker.removeWaitingTask(WaitingTask.LoadWorkshop);
        }
        private DeckListItemViewData[] getCurrentPageViewData()
        {
            var count = _currentPagePools.Length;
            var viewDatas = new DeckListItemViewData[count];
            for (int i = 0; i < count; i++)
            {
                viewDatas[i] = getCardPoolViewData(i);
            }
            return viewDatas;
        }
        private DeckListItemViewData getCardPoolViewData(int index)
        {
            var item = _currentPagePools[index];
            return new DeckListItemViewData()
            {
                image = item.sprite,
                mode = DeckListItem.Mode.Default,
                name = item.name
            };
        }

        #endregion

        #region 属性字段
        [TranslateMsg("创意工坊页面的按钮文字")]
        private const string TASK_DOWNLOAD = "下载";
        [TranslateMsg("创意工坊页面的按钮文字，{0}为加载进度百分比")]
        private const string TASK_DOWNLOADING = "下载中（{0}%）";
        [TranslateMsg("创意工坊页面的描述文本模板，{0}为年，{1}为月，{2}为日，{3}为小时，{4}为分钟")]
        private const string TASK_DATETIME_TEMPLATE = "{0}年{1}月{2}日 {3}:{4}";
        [TranslateMsg("创意工坊排序方法")]
        private const string SORTING_MOST_RELEVANT = "最相关";
        [TranslateMsg("创意工坊排序方法")]
        private const string SORTING_NEWEST = "最新上传";
        [TranslateMsg("尝试上传卡池时出现的错误描述，{0}为错误信息")]
        private const string ERROR_UPLOADING_CARDPOOL = "尝试上传卡池时出现错误：{0}";
        [TranslateMsg("尝试不勾选同意创意工坊规则上传卡池时出现的提示描述")]
        private const string HINT_AGREE_RULE_FIRST = "请先同意<b>创意工坊使用规则</b>。";
        [TranslateMsg("上传卡池成功后的提示描述")]
        private const string HINT_UPLOAD_CARDPOOL_SUCCESS = "卡池上传成功。";
        [TranslateMsg("上传卡池资源失败后的提示描述，{0}为未上传的资源数量")]
        private const string HINT_UPLOAD_RESOURCE_FAILED = "卡池上传失败，有{0}个资源未上传。";
        [TranslateMsg("尝试上传已经上传的卡池后的提示描述")]
        private const string HINT_CARD_POOL_ALREADY_UPLOADED = "该卡池已被上传至创意工坊，无需再次上传。";
        [TranslateMsg("上传进度-上传卡池文件")]
        private const string UPLOAD_PROGRESS_UPLOADING_POOL_FILE = "上传卡池文件中……";
        [TranslateMsg("上传进度-上传卡池资源")]
        private const string UPLOAD_PROGRESS_UPLOADING_RESOURCES = "上传卡池资源中……";

        const float uploadCardPoolProgress = 0.25f;
        const float uploadResourceProgress = 1 - uploadCardPoolProgress;

        private static string[] _sortNames =
        {
            SORTING_MOST_RELEVANT,
            SORTING_NEWEST,
        };
        private static Sorting[] _sortValues =
        {
            Sorting.MostRelevant,
            Sorting.Newest,
        };

        private WorkshopPool[] _currentPagePools;
        private int _page;


        private Sorting _sorting;
        private bool _onlyMineFilter;
        private string _creatorFilter;
        private string _searchFilter;

        private CardPoolInfo[] _selectingUploadPools;
        private long _uploadingPoolId;

        [SerializeField]
        private int _poolCountPerPage = 16;
        [SerializeField]
        private WorkshopPanel _ui;
        #endregion

        #region 内嵌类
        private enum Sorting
        {
            MostRelevant,
            Newest,
        }
        #endregion
    }

    public class WorkshopPool
    {
        public Sprite sprite;
        public string name;
        public string creator;
        public DateTime creationTime;
        public int cardCount;
        public string description;
    }
}
