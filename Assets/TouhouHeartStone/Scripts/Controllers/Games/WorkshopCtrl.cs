using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Game;
using MukioI18n;
using NitoriNetwork.Common;
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
            _viewingCardPool = null;
            _ui.hideDetails();
            _ui.setSelectUploadPoolPanelVisible(false);
            _ui.setUploadPoolPanelVisible(false);
            _ui.setDetailsStateVisible(false);
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
            _ui.onViewMineClicked += onViewMineClickedCallback;
            _ui.onHomeClicked += onHomeClickedCallback;
            _ui.onCreatorFilterChanged += onCreatorFilterChangedCallback;
            _ui.onSearchFilterChanged += onSearchFilterChangedCallback;
            _ui.onRefreshClicked += onRefreshClickedCallback;

            _ui.onCardPoolItemClick += onClickCardPoolItemCallback;
            _ui.onPageButtonClicked += onPageButtonClickCallback;

            _ui.onCreatorClicked += onCreatorClickedCallback;

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
        private void onHomeClickedCallback()
        {
            viewUser(-1);
        }
        private void onViewMineClickedCallback()
        {
            if (game.network.logic.UserInfo == null)
                return;
            viewUser(game.network.logic.UserInfo.UID);
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
        private async void onClickCardPoolItemCallback(int index)
        {
            var pool = _currentPageCardPools[index];
            _viewingCardPool = pool;

            var viewData = await getCardPoolViewData(pool);
            var creator = getUserName((int)pool.Author);
            var creationTime = DateTime.UnixEpoch.AddSeconds(pool.CreatedAt);
            creationTime += TimeZoneInfo.Local.GetUtcOffset(creationTime);
            var dateTime = game.lang._(TASK_DATETIME_TEMPLATE, creationTime.Year, creationTime.Month, creationTime.Day, creationTime.Hour, creationTime.Minute);
            var cardCount = pool.CardCount.ToString();
            var desc = pool.Description;
            _ui.showDetails(viewData, creator, dateTime, cardCount, desc);

            var selfUserInfo = game.network.logic.UserInfo;
            if (selfUserInfo != null)
            {
                bool stateVisible = _viewingUser == selfUserInfo.UID;
                _ui.setDetailsStateVisible(stateVisible);
                if (stateVisible)
                {
                    var stateColor = getStateColor(pool.State);
                    var stateText = getStateText(pool.State);
                    _ui.setDetailsState(stateColor, stateText);
                }
            }
        }
        private async void onRefreshClickedCallback()
        {
            await updatePage();
        }
        #endregion
        private async void onPageButtonClickCallback(bool next)
        {
            _page += next ? 1 : -1;
            await updatePage();
        }

        #region 详细信息
        private void onCreatorClickedCallback()
        {
            if (_viewingCardPool == null)
                return;
            viewUser((int)_viewingCardPool.Author);
        }
        #endregion

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

                // 获取卡池依赖
                var dependencies = info.dependencyRefs;
                string dependencyString;
                if (dependencies != null && dependencies.Length > 0)
                {
                    var dependencyNames = new string[dependencies.Length];
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        var dep = dependencies[i];
                        var localRef = game.card.getLocalCardPoolReference(dep.cardPoolId);
                        var (depInfo, depPool) = await game.card.getOrLoadCardPool(localRef);
                        if (depInfo == null)
                        {
                            // 本地没有该卡池。
                            var poolName = game.lang._(StringTable.EDITOR_UNKNOWN_CARD_POOL);
                            var str = game.lang._(StringTable.EDITOR_CARDPOOL_NAME_VERSION_TEMPLATE_DOT, poolName, dep.version);
                            dependencyNames[i] = $"<color=red>{str}</color>";
                        }
                        else if (depInfo.editTime > dep.version)
                        {
                            // 本地有该卡池的旧版本。
                            var poolName = game.card.getCardPoolName(depInfo);
                            var str = game.lang._(StringTable.EDITOR_CARDPOOL_NAME_VERSION_TEMPLATE_DOT, poolName, dep.version);
                            dependencyNames[i] = $"<color=yellow>{str}</color>";
                        }
                        else
                        {
                            // 本地有该卡池。
                            var poolName = game.card.getCardPoolName(depInfo);
                            dependencyNames[i] = game.lang._(StringTable.EDITOR_CARDPOOL_NAME_VERSION_TEMPLATE_DOT, poolName, dep.version);
                        }
                    }
                    dependencyString = string.Join("\n", dependencyNames);
                }
                else
                {
                    dependencyString = game.lang._(StringTable.NONE);
                }
                _ui.setUploadingPool(viewData, cardCount, version, dependencyString);
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
;
            try
            {
                var errorMessage = await validateCardPoolUpload();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    var title = game.lang._(StringTable.ERROR);
                    game.ui.dialog.ShowConfirm(title, errorMessage);
                    return;
                }

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
        private async Task<string> validateCardPoolUpload()
        {
            var info = game.card.getLocalCardPoolInfo(_uploadingPoolId);
            var dependencies = info.dependencyRefs.Where(dep => !game.card.isInnateCardPool(dep));
            List<CardPoolReference> notUploadedDependencies = new List<CardPoolReference>();
            foreach (var dep in dependencies)
            {
                var localRef = game.card.getLocalCardPoolReference(dep.cardPoolId);
                if (!game.card.isLocalCardPoolRefExists(localRef))
                    // 有丢失的依赖。
                    return game.lang._(ERROR_UPLOADING_CARDPOOL_HAS_MISSING_DEPENDENCY);

                if (dep.version < localRef.CardPoolVersion)
                    // 有过时的依赖。
                    return game.lang._(ERROR_UPLOADING_CARDPOOL_HAS_OUTDATED_DEPENDENCY);

                var depInfo = await getWorkshopCardPoolFile(dep);
                if (depInfo == null)
                    // 有未上传的依赖。
                    notUploadedDependencies.Add(dep);
            }
            if (notUploadedDependencies.Count > 0)
            {
                const int maxCount = 5;
                var depNames = string.Join(',', notUploadedDependencies.Take(maxCount).Select(dep => 
                {
                    var poolInfo = game.card.getCardPoolInfo(dep);
                    return game.card.getCardPoolName(poolInfo);
                }));
                if (notUploadedDependencies.Count > maxCount)
                {
                    depNames = game.lang._(StringTable.TRIMMED_COTENTS, depNames);
                }
                return game.lang._(ERROR_UPLOADING_CARDPOOL_HAS_NOT_UPLOADED_DEPENDENCY, depNames);
            }
            return null;
        }
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
        private async Task<Sprite> loadCardPoolCoverImage(string id)
        {
            if (id == null)
                return null;
            if (!game.cache.FileExists(ResourceType.Picture, id))
            {
                try
                {
                    byte[] bytes;
                    try
                    {
                        bytes = await game.network.getWorkshopResource("png", id);
                    }
                    catch (Exception)
                    {
                        bytes = await game.network.getWorkshopResource("jpg", id);
                    }
                    await game.cache.SaveCache(ResourceType.Picture, id, bytes);
                    return game.card.parseResource<Sprite>(ResourceType.Picture, bytes);
                }
                catch (Exception e)
                {
                    Debug.Log($"未找到缓存中的卡池封面{id}：{e}");
                }
            }

            if (!game.cache.FileExists(ResourceType.Picture, id))
                return game.card.getDefaultSprite();

            var spriteBytes = await game.cache.ReadCache(ResourceType.Picture, id);
            return game.card.parseResource<Sprite>(ResourceType.Picture, spriteBytes);
        }
        #endregion

        private Color getStateColor(WorkshopCardPoolState state)
        {
            switch (state)
            {
                case WorkshopCardPoolState.Deleted:
                case WorkshopCardPoolState.Pending:
                    return Color.gray;
                case WorkshopCardPoolState.NoPass:
                    return Color.red;
                case WorkshopCardPoolState.OldVer:
                    return Color.yellow;
            }
            return Color.white;
        }
        private string getStateText(WorkshopCardPoolState state)
        {
            if (_stateTexts.TryGetValue(state, out var text))
            {
                return game.lang._(text);
            }
            return game.lang._(StringTable.UNKNOWN);
        }
        private void viewUser(int uid)
        {
            if (_viewingUser == uid)
                return;
            _viewingUser = uid;
            _page = 0;
            _ = updatePage();
        }
        private async Task updatePage()
        {
            _ui.setLoadingState(WorkshopPanel.LoadingState.Loading);

            _ui.switchViewMineOrHome(_viewingUser < 0);
            _ui.switchViewUserLabelOrSearch(_viewingUser >= 0);
            if (_viewingUser >= 0)
            {
                var label = game.lang._(VIEWING_USER_NAME_TEMPLATE, getUserName(_viewingUser));
                _ui.setViewingUserLabel(label);
            }
            try
            {
                _ui.updateCardPools(Array.Empty<DeckListItemViewData>());
                await loadPage(_page);
                var pageViewDatas = await getCurrentPageViewData();
                _ui.updateCardPools(pageViewDatas);

                _ui.setPageText(game.lang._(StringTable.PAGE_NUMBER_TEMPLATE, _page + 1));
                _ui.setPageButtonInteractable(_page > 0, _currentPageCardPools.Length >= _poolCountPerPage);

                _ui.setLoadingState(WorkshopPanel.LoadingState.Normal);
            }
            catch (Exception e)
            {
                Debug.LogError($"加载创意工坊页面{_page}失败：{e}");
                _ui.setLoadingState(WorkshopPanel.LoadingState.Warning);
            }
        }
        private void updateDownload()
        {
            _ui.setDownloadButtonText(game.lang._(TASK_DOWNLOAD));
            _ui.setDownloadProgress(0);
        }
        private async Task loadPage(int page)
        {
            if (_viewingUser >= 0)
                _currentPageCardPools = await game.network.getWorkshopUserCardPools(_viewingUser, page, _poolCountPerPage);
            else
                _currentPageCardPools = await game.network.searchWorkshopPageCardPools(page, _poolCountPerPage, _searchFilter);
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
        private async Task<DeckListItemViewData[]> getCurrentPageViewData()
        {
            var count = _currentPageCardPools.Length;
            var viewDatas = new DeckListItemViewData[count];
            for (int i = 0; i < count; i++)
            {
                viewDatas[i] = await getCardPoolViewData(_currentPageCardPools[i]);
            }
            return viewDatas;
        }
        private async Task<DeckListItemViewData> getCardPoolViewData(WorkshopCardPoolInfo info)
        {
            Sprite sprite = await loadCardPoolCoverImage(info.CoverImage);
            return new DeckListItemViewData()
            {
                image = sprite,
                mode = DeckListItem.Mode.Default,
                name = info.Name
            };
        }
        private string getUserName(int author)
        {
            try
            {
                var userInfo = game.network.getUserInfo(author);
                return userInfo.Name;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"无法找到用户{author}：{e}");
                return game.lang._(StringTable.UNKNOWN_USER);
            }
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
        [TranslateMsg("尝试上传拥有丢失卡池依赖的卡池时出现的错误描述")]
        private const string ERROR_UPLOADING_CARDPOOL_HAS_MISSING_DEPENDENCY = "该卡池拥有丢失的卡池依赖，无法上传。";
        [TranslateMsg("尝试上传拥有过时卡池依赖的卡池时出现的错误描述")]
        private const string ERROR_UPLOADING_CARDPOOL_HAS_OUTDATED_DEPENDENCY = "该卡池拥有过时的卡池依赖，无法上传。";
        [TranslateMsg("尝试上传拥有未上传卡池依赖的卡池时出现的错误描述，{0}为未上传卡池")]
        private const string ERROR_UPLOADING_CARDPOOL_HAS_NOT_UPLOADED_DEPENDENCY = "以下依赖卡池没有被上传至创意工坊，请先将其上传：\n{0}";
        [TranslateMsg("尝试不勾选同意创意工坊规则上传卡池时出现的提示描述")]
        private const string HINT_AGREE_RULE_FIRST = "请先同意<b>创意工坊使用规则</b>。";
        [TranslateMsg("上传卡池成功后的提示描述")]
        private const string HINT_UPLOAD_CARDPOOL_SUCCESS = "卡池上传成功，请等待工作人员进行审核。";
        [TranslateMsg("上传卡池资源失败后的提示描述，{0}为未上传的资源数量")]
        private const string HINT_UPLOAD_RESOURCE_FAILED = "卡池上传失败，有{0}个资源未上传。";
        [TranslateMsg("尝试上传已经上传的卡池后的提示描述")]
        private const string HINT_CARD_POOL_ALREADY_UPLOADED = "该卡池已被上传至创意工坊，无需再次上传。";
        [TranslateMsg("上传进度-上传卡池文件")]
        private const string UPLOAD_PROGRESS_UPLOADING_POOL_FILE = "上传卡池文件中……";
        [TranslateMsg("上传进度-上传卡池资源")]
        private const string UPLOAD_PROGRESS_UPLOADING_RESOURCES = "上传卡池资源中……";
        [TranslateMsg("查看其他用户的卡池时的说明")]
        private const string VIEWING_USER_NAME_TEMPLATE = "正在查看：\n{0}上传的卡池";


        [TranslateMsg("创意工坊卡池状态")]
        private const string STATE_PENDING = "等待上传资源";
        [TranslateMsg("创意工坊卡池状态")]
        private const string STATE_REVIEWING = "审核中";
        [TranslateMsg("创意工坊卡池状态")]
        private const string STATE_PASS = "已通过";
        [TranslateMsg("创意工坊卡池状态")]
        private const string STATE_NOT_PASS = "未通过";
        [TranslateMsg("创意工坊卡池状态")]
        private const string STATE_OLD_VERSION = "旧版本";
        [TranslateMsg("创意工坊卡池状态")]
        private const string STATE_DELETED = "已删除";

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
        private static Dictionary<WorkshopCardPoolState, string> _stateTexts = new Dictionary<WorkshopCardPoolState, string>()
        {
            { WorkshopCardPoolState.Pending, STATE_PENDING },
            { WorkshopCardPoolState.Reviewing, STATE_REVIEWING },
            { WorkshopCardPoolState.Pass, STATE_PASS },
            { WorkshopCardPoolState.NoPass, STATE_NOT_PASS },
            { WorkshopCardPoolState.OldVer, STATE_OLD_VERSION },
            { WorkshopCardPoolState.Deleted, STATE_DELETED },
        };

        private WorkshopCardPoolInfo[] _currentPageCardPools;
        private WorkshopCardPoolInfo _viewingCardPool;
        private int _page;


        private Sorting _sorting;
        private int _viewingUser = -1;
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

}
