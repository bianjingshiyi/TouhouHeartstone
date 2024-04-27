using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
            _ui.setSelectUploadPoolPanelVisible(false);
            _ui.setUploadPoolPanelVisible(false);
            _ui.setDetailsStateVisible(false);
            deselectCardPool();
            await viewUser(-1, true);
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

            _ui.onDeleteCardPoolClicked += onDeleteCardPoolClickedCallback;
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
        private async void onReturnClickCallback()
        {
            if (_downloadTask != null)
            {
                var title = game.lang._(StringTable.WARNING);
                var quote = game.lang._(WARNING_QUIT_WHILE_DOWNLOADING);
                var result = await game.ui.dialog.ShowQuestionAsync(title, quote);
                if (result)
                    _downloadCts.Cancel();
                else
                    return;
            }
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
            _ = viewUser(-1);
        }
        private void onViewMineClickedCallback()
        {
            if (game.network.logic.UserInfo == null)
                return;
            _ = viewUser(game.network.logic.UserInfo.UID);
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
            await selectCardPool(pool);
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
        private bool validateCardPoolDelete(WorkshopCardPoolInfo poolInfo, out string errorMessage)
        {
            if (_downloadingCardPools.Contains(poolInfo.ID))
            {
                errorMessage = game.lang._(ERROR_CANNOT_DELETE_DOWNLOADING_CARDPOOL);
                return false;
            }

            errorMessage = null;
            return true;
        }

        private async void onDeleteCardPoolClickedCallback()
        {
            var userInfo = game.network.logic.UserInfo;
            if (userInfo == null)
                return;
            if (_viewingCardPool == null || _viewingCardPool.Author != userInfo.UID)
                return;

            if (!validateCardPoolDelete(_viewingCardPool, out var errorMessage))
            {
                var validateTitle = game.lang._(StringTable.ERROR);
                game.ui.dialog.ShowConfirm(validateTitle, errorMessage);
            }

            var title = game.lang._(StringTable.WARNING);
            var desc = game.lang._(WARNING_DELETE_CARD_POOL, _viewingCardPool.Name);
            var result = await game.ui.dialog.ShowQuestionAsync(title, desc);
            if (result)
            {
                await deleteCardPool();
            }
        }
        private void onCreatorClickedCallback()
        {
            if (_viewingCardPool == null)
                return;
            _ = viewUser((int)_viewingCardPool.Author);
        }
        #endregion

        #region 任务栏
        private void onDownloadClickCallback()
        {
            if (_downloadTask != null && _downloadCts != null)
            {
                _downloadCts.Cancel();
                return;
            }

            _ = startDownload();
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
        #endregion

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
                await updatePage();
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
            if (info != null && info.dependencyRefs != null)
            {
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
                    if (depInfo == null || !(depInfo.State == WorkshopCardPoolState.Pass || depInfo.State == WorkshopCardPoolState.OldVer))
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
                    depNames = notUploadedDependencies.Count > maxCount ? depNames = game.lang._(StringTable.TRIMMED_COTENTS, depNames) : depNames;
                    return game.lang._(ERROR_UPLOADING_CARDPOOL_HAS_NOT_UPLOADED_DEPENDENCY, depNames);
                }
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
            setWaitingProgress(cardPoolFileProgress);

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
                if (string.IsNullOrEmpty(res))
                    continue;
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
                setWaitingProgress(cardPoolFileProgress + cardPoolResourceProgress * (uploadedResources / (float)totalResources));
            }
            return notFoundResources.ToArray();
        }
        #endregion

        #region 获取卡池信息
        private Task<WorkshopCardPoolInfo> getWorkshopCardPoolFile(ICardPoolReference poolRef)
        {
            if (poolRef == null)
                return Task.FromResult<WorkshopCardPoolInfo>(null);
            return getWorkshopCardPoolFile(poolRef.CardPoolID, poolRef.CardPoolVersion);
        }
        private async Task<WorkshopCardPoolInfo> getWorkshopCardPoolFile(long id, int version)
        {
            var cacheKey = $"{id}_{version}";
            if (_cardPoolCacheDict.TryGetValue(cacheKey, out var cache))
                return cache;

            try
            {
                var workshopInfo = await game.network.getWorkshopCardPool(id, version);
                _cardPoolCacheDict.Add(cacheKey, workshopInfo);
                return workshopInfo;
            }
            catch (Exception e)
            {
                Debug.Log($"未找到创意工坊卡池{id}的{version}版本：{e}");
            }
            return null;
        }
        private async Task<Sprite> loadCardPoolCoverImage(string id)
        {
            if (string.IsNullOrEmpty(id))
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

        #region 卡池详细信息
        private async Task selectCardPool(WorkshopCardPoolInfo info)
        {
            var workshopInfo = await getWorkshopCardPoolFile(info.ID, (int)info.Version);
            _viewingCardPool = workshopInfo;

            await updateCardPoolDetails(workshopInfo);
            updateDownloadButton();
        }
        private void deselectCardPool()
        {
            _viewingCardPool = null;
            _ui.setDetailsVisible(WorkshopPanel.DetailPage.Nothing);
            updateDownloadButton();
        }
        private async Task updateCardPoolDetails(WorkshopCardPoolInfo workshopInfo)
        {
            _ui.setDetailsVisible(WorkshopPanel.DetailPage.Loading);
            var viewData = await getCardPoolHeaderViewData(workshopInfo);

            var creationTime = DateTime.UnixEpoch.AddSeconds(workshopInfo.CreatedAt);
            creationTime += TimeZoneInfo.Local.GetUtcOffset(creationTime);
            var dateTime = game.lang._(TASK_DATETIME_TEMPLATE, creationTime.Year, creationTime.Month, creationTime.Day, creationTime.Hour, creationTime.Minute);
            string dependencies = await getWorkshopCardPoolDependenciesName(workshopInfo);
            var details = new WorkshopCardPoolDetailsViewData()
            {
                creator = getUserName((int)workshopInfo.Author),
                creationTime = dateTime,
                cardCount = workshopInfo.CardCount.ToString(),
                description = string.IsNullOrEmpty(workshopInfo.Description) ? game.lang._(NO_DESCRIPTION_PROVIDED) : workshopInfo.Description,
                version = workshopInfo.Version.ToString(),
                dependencies = dependencies
            };
            _ui.setDetails(viewData, details);

            // 卡池状态。
            var selfUserInfo = game.network.logic.UserInfo;
            if (selfUserInfo != null)
            {
                bool stateVisible = _viewingUser == selfUserInfo.UID;
                bool reviewsVisible = stateVisible && workshopInfo.State == WorkshopCardPoolState.NoPass;
                _ui.setDetailsStateVisible(stateVisible);
                _ui.setReviewsVisible(reviewsVisible);
                if (stateVisible)
                {
                    var stateColor = getStateColor(workshopInfo.State);
                    var stateText = getStateText(workshopInfo.State);
                    _ui.setDetailsState(stateColor, stateText);
                }
                if (reviewsVisible)
                {
                    var reviews = await game.network.getWorkshopCardPoolReviews(workshopInfo.ID, (int)workshopInfo.Version);
                    var reviewString = getReviewsText(reviews);
                    _ui.setReviewsText(reviewString);
                }
            }

            _ui.setDetailsVisible(WorkshopPanel.DetailPage.Preview);
        }
        private async Task<string> getWorkshopCardPoolDependenciesName(WorkshopCardPoolInfo workshopInfo)
        {
            if (workshopInfo.Dependencies == null || workshopInfo.Dependencies.Length <= 0)
                return game.lang._(StringTable.NONE);

            var dependencyNames = new List<string>();
            foreach (var dep in workshopInfo.Dependencies)
            {
                if (dep != null)
                {
                    if (game.card.isInnateCardPool(dep.ID))
                    {
                        var depInfo = game.card.getLocalCardPoolInfo(dep.ID);
                        dependencyNames.Add(game.lang._(StringTable.EDITOR_CARDPOOL_NAME_VERSION_TEMPLATE_DOT, game.card.getCardPoolName(depInfo), dep.Version));
                        continue;
                    }
                    else
                    {
                        var depInfo = await getWorkshopCardPoolFile(dep.ID, dep.Version);
                        if (depInfo != null)
                        {
                            var poolString = game.lang._(StringTable.EDITOR_CARDPOOL_NAME_VERSION_TEMPLATE_DOT, depInfo.Name, dep.Version);
                            if (depInfo.State == WorkshopCardPoolState.Deleted)
                            {
                                poolString = $"<color=grey><s>{poolString}</s></color>";
                            }
                            dependencyNames.Add(poolString);
                            continue;
                        }
                    }
                }
                dependencyNames.Add($"<color=red>{game.lang._(StringTable.EDITOR_UNKNOWN_CARD_POOL)}</color>");
            }
            return string.Join('\n', dependencyNames);
        }
        private void updateDownloadButton()
        {
            _ui.setDownloadButtonInteractable((_viewingCardPool != null && _viewingCardPool.State != WorkshopCardPoolState.Deleted) || _downloadTask != null);
        }
        #endregion

        #region 下载卡池
        private string validateCardPoolDownload(WorkshopCardPoolInfo poolInfo)
        {
            if (poolInfo.State == WorkshopCardPoolState.Deleted)
            {
                // 卡池已被删除。
                return game.lang._(ERROR_DOWNLOAD_DELETED_CARDPOOL);
            }
            var info = game.card.getLocalCardPoolInfo(poolInfo.ID);
            if (info != null)
            {
                // 本地已有该卡池。
                return game.lang._(ERROR_DOWNLOADING_CARDPOOL_ALREADY_EXISTS);
            }
            return null;
        }
        private string validateCardPoolDependenciesDownload(WorkshopCardPoolInfo poolInfo, IEnumerable<WorkshopCardPoolInfo> dependencies)
        {
            var differentVersionDeps = dependencies.Where(dep =>
            {
                if (dep == null)
                    return false;
                var depRef = game.card.getLocalCardPoolReference(dep.ID);
                if (depRef == null)
                    return false;
                return depRef.CardPoolVersion != dep.Version;
            });
            if (differentVersionDeps.Count() > 0)
            {
                // 本地有不同版本的卡池依赖。
                const int maxCount = 5;
                var depNames = string.Join(',', differentVersionDeps.Take(maxCount).Select(dep => dep.Name));
                depNames = differentVersionDeps.Count() > maxCount ? game.lang._(StringTable.TRIMMED_COTENTS, depNames) : depNames;
                return game.lang._(ERROR_DOWNLOADING_CARDPOOL_DEPENDENCY_CONFLICT, depNames);
            }
            return null;
        }
        private async Task startDownload()
        {
            try
            {
                // 检验卡池能否被下载。
                var errorMessage = validateCardPoolDownload(_viewingCardPool);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    var title = game.lang._(StringTable.ERROR);
                    game.ui.dialog.ShowConfirm(title, errorMessage);
                    return;
                }

                // 检验卡池依赖能否被下载。
                var allDeps = await getDownloadPoolAllDependencies(_viewingCardPool);
                var downloadableDeps = allDeps.Where(d => d.State != WorkshopCardPoolState.Deleted);
                errorMessage = validateCardPoolDependenciesDownload(_viewingCardPool, downloadableDeps);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    var title = game.lang._(StringTable.ERROR);
                    game.ui.dialog.ShowConfirm(title, errorMessage);
                    return;
                }

                // 开始下载。
                var downloading = downloadableDeps.Prepend(_viewingCardPool);
                _downloadingCardPools.Clear();
                _downloadingCardPools.AddRange(downloading.Select(d => d.ID));
                _downloadCts = new CancellationTokenSource();
                _downloadTask = downloadPoolGroup(downloading);
                _ui.setDownloadButtonText(game.lang._(BUTTON_TEXT_CANCEL_DOWNLOADING));
                updateDownloadButton();
                await _downloadTask;
            }
            catch (OperationCanceledException)
            {
                Debug.Log("下载任务被取消。");
                deleteDownloadedFiles();
            }
            catch (Exception e)
            {
                var title = game.lang._(StringTable.ERROR);
                var quote = game.lang._(ERROR_DOWNLOADING_CARDPOOL, e.Message);
                game.ui.dialog.ShowConfirm(title, quote);
                Debug.LogError($"下载卡池时出现错误：{e}");
                deleteDownloadedFiles();
            }
            finally
            {
                _downloadTask = null;
                _downloadCts = null;
                _downloadingCardPools.Clear();
                _downloadedCardPools.Clear();
                _downloadedResources.Clear();
                _ui.setDownloadButtonText(game.lang._(BUTTON_TEXT_DOWNLOAD));
                _ui.setDownloadProgress(0);
                updateDownloadButton();
            }
        }
        private async Task<WorkshopCardPoolInfo[]> getDownloadPoolAllDependencies(WorkshopCardPoolInfo workshopInfo)
        {
            var set = new List<WorkshopCardPoolInfo>();
            await getDownloadPoolDependenciesChain(workshopInfo, set);
            return set.ToArray();
        }
        private async Task getDownloadPoolDependenciesChain(WorkshopCardPoolInfo workshopInfo, List<WorkshopCardPoolInfo> downloadedSet)
        {
            var deps = await getCardPoolDependencies(workshopInfo);
            if (deps == null)
                return;
            foreach (var dep in deps)
            {
                if (downloadedSet.Exists(p => p.ID == dep.ID))
                    continue;
                downloadedSet.Add(dep);
                await getDownloadPoolDependenciesChain(workshopInfo, downloadedSet);
            }
        }
        private async Task<WorkshopCardPoolInfo[]> getCardPoolDependencies(WorkshopCardPoolInfo workshopInfo)
        {
            if (workshopInfo.Dependencies == null)
                return null;
            List<WorkshopCardPoolInfo> deps = new List<WorkshopCardPoolInfo>();
            foreach (var dep in workshopInfo.Dependencies)
            {
                var depInfo = await getWorkshopCardPoolFile(dep.ID, dep.Version);
                if (depInfo == null)
                    continue;
                deps.Add(depInfo);
            }
            return deps.ToArray();
        }
        private async Task downloadPoolGroup(IEnumerable<WorkshopCardPoolInfo> workshopInfos)
        {
            var token = _downloadCts.Token;
            foreach (var workshopInfo in workshopInfos)
            {
                if (game.card.getLocalCardPoolInfo(workshopInfo.ID) != null)
                    // 本地已有该卡池，不下载。
                    continue;
                await downloadPool(workshopInfo);
                token.ThrowIfCancellationRequested();
            }
        }
        private async Task downloadPool(WorkshopCardPoolInfo workshopInfo)
        {
            var token = _downloadCts.Token;
            // 下载卡池本体文件
            _ui.setDownloadProgress(0);
            var poolFile = await game.network.getWorkshopResource("cardpool", workshopInfo.ContentID);
            var decompressed = Compression.tryDecompress(poolFile);
            if (!game.card.tryParseCardPoolBytes(decompressed, out var poolInfo, out var cardPool))
                throw new FormatException("下载的创意工坊卡池文件格式不正确");

            var path = game.card.allocCardPoolPathById(poolInfo);
            await game.card.saveCardPoolFileOnly(poolInfo, cardPool, path);
            await game.card.loadLocalCardPoolInfoAsync(path);
            _downloadedCardPools.Add(poolInfo.ToReference());
            _ui.setDownloadProgress(cardPoolFileProgress);

            token.ThrowIfCancellationRequested();

            // 下载资源
            var poolResources = CardManager.getCardPoolResources(poolInfo, cardPool);
            int downloadedCount = 0;
            int resourceCount = poolResources.Length;
            var workshopResources = workshopInfo.Resources.Prepend(workshopInfo.CoverImage);
            foreach (var resId in workshopInfo.Resources)
            {
                if (!string.IsNullOrEmpty(resId))
                {
                    var (resType, resPath) = poolResources.FirstOrDefault(r => r.path.StartsWith(resId));
                    if (!string.IsNullOrEmpty(resPath) && !game.card.isResourceExists(poolInfo, resType, resPath))
                    {
                        // 创意工坊卡池需要这个资源，并且卡池文件也需要，下载
                        var extension = Path.GetExtension(resPath).TrimStart('.');
                        var resourceBytes = await game.network.getWorkshopResource(extension, resId);
                        var savedFileName = await game.card.saveResourceBytes(poolInfo.cardPoolId, resourceBytes, resType, resId, extension);
                        var savedPath = Path.Combine(game.card.getCardPoolResourceDirectory(poolInfo, resType), savedFileName);
                        _downloadedResources.Add(savedFileName);
                    }
                }
                downloadedCount++;
                var downloadPercent = downloadedCount / (float)resourceCount;
                _ui.setDownloadProgress(cardPoolFileProgress + cardPoolResourceProgress * (downloadPercent));

                token.ThrowIfCancellationRequested();
            }
            _ui.setDownloadProgress(1);
        }
        private void deleteDownloadedFiles()
        {
            try
            {
                foreach (var poolRef in _downloadedCardPools)
                {
                    game.card.removeCardPool(poolRef);
                }
                foreach (var resPath in _downloadedResources)
                {
                    game.fileManager.deleteFile(resPath);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"删除已下载卡池资源时出错：{e}");
            }
        }
        #endregion

        #region 删除卡池
        private async Task deleteCardPool()
        {
            try
            {
                startWaiting();
                await game.network.deleteWorkshopCardPool(_viewingCardPool.ID, (int)_viewingCardPool.Version);
                deselectCardPool();
                await updatePage();
            }
            catch (Exception e)
            {
                var title = game.lang._(StringTable.ERROR);
                var desc = game.lang._(ERROR_DELETING_CARDPOOL, e.Message);
                game.ui.dialog.ShowConfirm(title, desc);
                Debug.LogError($"删除卡池时出现错误：{e}");
            }
            finally
            {
                stopWaiting();
            }
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
                case WorkshopCardPoolState.ReviewingOldVer:
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
        private string getReviewsText(IEnumerable<WorkshopCardPoolReview> reviews)
        {
            if (reviews.Count() <= 0)
                return game.lang._(StringTable.NONE);
            var texts = reviews.Select(r =>
            {
                string resultText;
                switch (r.Result)
                {
                    case WorkshopReviewResult.Pass:
                        resultText = $"<color=green>{game.lang._(REVIEW_RESULT_PASS)}</color>";
                        break;
                    case WorkshopReviewResult.NotPass:
                        resultText = $"<color=red>{game.lang._(REVIEW_RESULT_NOT_PASS)}</color>";
                        break;
                    default:
                        resultText = game.lang._(REVIEW_RESULT_COMMENT);
                        break;
                }
                string messageText = string.IsNullOrEmpty(r.Message) ? game.lang._(NO_DESCRIPTION_PROVIDED) : r.Message;
                return game.lang._(REVIEW_TEMPLATE, resultText, messageText);
            });
            return string.Join('\n', texts);
        }
        private async Task viewUser(int uid, bool forceUpdate = false)
        {
            if (_viewingUser == uid && !forceUpdate)
                return;
            _viewingUser = uid;
            _page = 0;
            await updatePage();
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
                _ui.setPageButtonInteractable(false, false);
                _cardPoolCacheDict.Clear();
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
                viewDatas[i] = await getCardPoolItemViewData(_currentPageCardPools[i]);
            }
            return viewDatas;
        }
        private async Task<DeckListItemViewData> getCardPoolItemViewData(WorkshopCardPoolInfo info)
        {
            Sprite sprite = await loadCardPoolCoverImage(info.CoverImage);
            return new DeckListItemViewData()
            {
                image = sprite,
                mode = DeckListItem.Mode.Default,
                name = info.Name,
                hasPrivatePools = info.State == WorkshopCardPoolState.Pending,
                isWarning = info.State == WorkshopCardPoolState.OldVer || info.State == WorkshopCardPoolState.NoPass || info.State == WorkshopCardPoolState.ReviewingOldVer,
                isError = info.State == WorkshopCardPoolState.Deleted,
                isWaiting = info.State == WorkshopCardPoolState.Reviewing || info.State == WorkshopCardPoolState.ReviewingOldVer,
            };
        }
        private async Task<DeckListItemViewData> getCardPoolHeaderViewData(WorkshopCardPoolInfo info)
        {
            Sprite sprite = await loadCardPoolCoverImage(info.CoverImage);
            var mode = DeckListItem.Mode.Default;
            if (info.State != WorkshopCardPoolState.Deleted && game.network.logic.UserInfo != null && info.Author == game.network.logic.UserInfo.UID)
            {
                mode = DeckListItem.Mode.CanDelete;
            }
            return new DeckListItemViewData()
            {
                image = sprite,
                mode = mode,
                name = info.Name,
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
        [TranslateMsg("创意工坊-按钮文字，{0}为加载进度百分比")]
        private const string TASK_DOWNLOADING = "下载中（{0}%）";
        [TranslateMsg("创意工坊-描述文本模板，{0}为年，{1}为月，{2}为日，{3}为小时，{4}为分钟")]
        private const string TASK_DATETIME_TEMPLATE = "{0}年{1}月{2}日 {3}:{4}";
        [TranslateMsg("创意工坊-排序方法")]
        private const string SORTING_MOST_RELEVANT = "最相关";
        [TranslateMsg("创意工坊-排序方法")]
        private const string SORTING_NEWEST = "最新上传";
        [TranslateMsg("创意工坊-尝试上传卡池时出现的错误描述，{0}为错误信息")]
        private const string ERROR_UPLOADING_CARDPOOL = "尝试上传卡池时出现错误：{0}";
        [TranslateMsg("创意工坊-尝试上传拥有丢失卡池依赖的卡池时出现的错误描述")]
        private const string ERROR_UPLOADING_CARDPOOL_HAS_MISSING_DEPENDENCY = "该卡池拥有丢失的卡池依赖，无法上传。";
        [TranslateMsg("创意工坊-尝试上传拥有过时卡池依赖的卡池时出现的错误描述")]
        private const string ERROR_UPLOADING_CARDPOOL_HAS_OUTDATED_DEPENDENCY = "该卡池拥有过时的卡池依赖，无法上传。";
        [TranslateMsg("创意工坊-尝试上传拥有未上传卡池依赖的卡池时出现的错误描述，{0}为未上传卡池")]
        private const string ERROR_UPLOADING_CARDPOOL_HAS_NOT_UPLOADED_DEPENDENCY = "无法上传该卡池。\n以下依赖卡池没有被上传至创意工坊，或尚未通过审核：\n{0}";
        [TranslateMsg("创意工坊-下载卡池时出现的错误描述，{0}为错误信息")]
        private const string ERROR_DOWNLOADING_CARDPOOL = "下载卡池时出现错误：{0}";
        [TranslateMsg("创意工坊-尝试下载已经存在的卡池时出现的错误描述")]
        private const string ERROR_DOWNLOAD_DELETED_CARDPOOL = "该卡池已被删除，无法下载。";
        [TranslateMsg("创意工坊-尝试下载已经存在的卡池时出现的错误描述")]
        private const string ERROR_DOWNLOADING_CARDPOOL_ALREADY_EXISTS = "该卡池在本地已存在，请先将其删除。";
        [TranslateMsg("创意工坊-尝试下载已经存在本地依赖的卡池时出现的错误描述，{0}为冲突的卡池")]
        private const string ERROR_DOWNLOADING_CARDPOOL_DEPENDENCY_CONFLICT = "本地卡池与该卡池所依赖的卡池冲突，无法下载：\n{0}";
        [TranslateMsg("创意工坊-删除卡池时出现的错误描述，{0}为错误信息")]
        private const string ERROR_DELETING_CARDPOOL = "删除卡池时出现错误：{0}";
        [TranslateMsg("创意工坊-尝试删除正在下载的卡池时")]
        private const string ERROR_CANNOT_DELETE_DOWNLOADING_CARDPOOL = "你不能删除正在下载的卡池。";

        [TranslateMsg("创意工坊-尝试在下载时离开的警告")]
        private const string WARNING_QUIT_WHILE_DOWNLOADING = "正在下载卡池，退出创意工坊会取消下载。\n您确认要退出吗？";
        [TranslateMsg("创意工坊-尝试删除卡池时出现的警告，{0}为卡池名字")]
        private const string WARNING_DELETE_CARD_POOL = "您确认要删除卡池“{0}”吗？";

        [TranslateMsg("创意工坊-尝试不勾选同意创意工坊规则上传卡池时出现的提示描述")]
        private const string HINT_AGREE_RULE_FIRST = "请先同意<b>创意工坊使用规则</b>。";
        [TranslateMsg("创意工坊-上传卡池成功后的提示描述")]
        private const string HINT_UPLOAD_CARDPOOL_SUCCESS = "卡池上传成功，请等待工作人员进行审核。";
        [TranslateMsg("创意工坊-上传卡池资源失败后的提示描述，{0}为未上传的资源数量")]
        private const string HINT_UPLOAD_RESOURCE_FAILED = "卡池上传失败，有{0}个资源未上传。";
        [TranslateMsg("创意工坊-尝试上传已经上传的卡池后的提示描述")]
        private const string HINT_CARD_POOL_ALREADY_UPLOADED = "该卡池的同一版本已被上传至创意工坊，无需再次上传。\n请考虑提升卡池版本再试。";
        [TranslateMsg("创意工坊-上传进度-上传卡池文件")]
        private const string UPLOAD_PROGRESS_UPLOADING_POOL_FILE = "上传卡池文件中……";
        [TranslateMsg("创意工坊-上传进度-上传卡池资源")]
        private const string UPLOAD_PROGRESS_UPLOADING_RESOURCES = "上传卡池资源中……";
        [TranslateMsg("创意工坊-查看其他用户的卡池时的说明")]
        private const string VIEWING_USER_NAME_TEMPLATE = "正在查看：\n{0}上传的卡池";
        [TranslateMsg("创意工坊-下载按钮文本")]
        private const string BUTTON_TEXT_DOWNLOAD = "下载";
        [TranslateMsg("创意工坊-下载按钮文本")]
        private const string BUTTON_TEXT_CANCEL_DOWNLOADING = "取消下载";
        [TranslateMsg("创意工坊-空的卡池说明")]
        private const string NO_DESCRIPTION_PROVIDED = "未提供说明。";

        [TranslateMsg("创意工坊-审核意见结果模板，{0}为审核结果，{1}为意见")]
        private const string REVIEW_TEMPLATE = "{0}{1}";
        [TranslateMsg("创意工坊-审核意见结果")]
        private const string REVIEW_RESULT_PASS = "[同意]";
        [TranslateMsg("创意工坊-审核意见结果")]
        private const string REVIEW_RESULT_NOT_PASS = "[拒绝]";
        [TranslateMsg("创意工坊-审核意见结果")]
        private const string REVIEW_RESULT_COMMENT = "[讨论]";


        [TranslateMsg("创意工坊-卡池状态")]
        private const string STATE_PENDING = "等待上传资源";
        [TranslateMsg("创意工坊-卡池状态")]
        private const string STATE_REVIEWING = "审核中";
        [TranslateMsg("创意工坊-卡池状态")]
        private const string STATE_PASS = "已通过";
        [TranslateMsg("创意工坊-卡池状态")]
        private const string STATE_NOT_PASS = "未通过";
        [TranslateMsg("创意工坊-卡池状态")]
        private const string STATE_OLD_VERSION = "旧版本";
        [TranslateMsg("创意工坊-卡池状态")]
        private const string STATE_REVIEWING_OLD_VERSION = "旧版本（审核中）";
        [TranslateMsg("创意工坊-卡池状态")]
        private const string STATE_DELETED = "已删除";

        const float cardPoolFileProgress = 0.25f;
        const float cardPoolResourceProgress = 1 - cardPoolFileProgress;

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
            { WorkshopCardPoolState.ReviewingOldVer, STATE_REVIEWING_OLD_VERSION },
            { WorkshopCardPoolState.Pass, STATE_PASS },
            { WorkshopCardPoolState.NoPass, STATE_NOT_PASS },
            { WorkshopCardPoolState.OldVer, STATE_OLD_VERSION },
            { WorkshopCardPoolState.Deleted, STATE_DELETED },
        };

        private WorkshopCardPoolInfo[] _currentPageCardPools;
        private WorkshopCardPoolInfo _viewingCardPool;
        private Dictionary<string, WorkshopCardPoolInfo> _cardPoolCacheDict = new Dictionary<string, WorkshopCardPoolInfo>();

        private CancellationTokenSource _downloadCts;
        private List<long> _downloadingCardPools = new List<long>();
        private List<ICardPoolReference> _downloadedCardPools = new List<ICardPoolReference>();
        private List<string> _downloadedResources = new List<string>();

        private Task _downloadTask;
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
