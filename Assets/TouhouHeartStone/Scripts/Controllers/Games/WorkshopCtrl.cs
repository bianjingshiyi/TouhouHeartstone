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
using TouhouCardEngine.Interfaces;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
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

            updateBrowseComponents();
            _ui.setSelectUploadPoolPanelVisible(false);
            _ui.setUploadPoolPanelVisible(false);
            _ui.setCardPoolDetailsStateVisible(false);
            _ui.setDownloadProgressVisible(false);
            _ui.hideHintMessage();
            quitFromCardPool();
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
            _sorting = _sortValues.FirstOrDefault();
            _ui.setSortItems(_sortNames.Select(n => game.lang._(n)).ToArray());
            _ui.onReturnClick += onReturnClickCallback;

            _ui.onSortChanged += onSortChangedCallback;
            _ui.onViewMineClicked += onViewMineClickedCallback;
            _ui.onHomeClicked += onHomeClickedCallback;
            _ui.onSearchFilterChanged += onSearchFilterChangedCallback;
            _ui.onRefreshClicked += onRefreshClickedCallback;
            _ui.onBrowseCardPoolItemClick += onClickCardPoolItemCallback;
            _ui.onPageButtonClicked += onPageButtonClickCallback;
            _ui.onUploadClick += onUploadClickCallback;

            _ui.onCardPoolDetailsDeleteClicked += onDeleteCardPoolClickedCallback;
            _ui.onCardPoolDetailsCreatorClicked += onCreatorClickedCallback;
            _ui.onCardPoolDetailsDownloadClick += onDownloadClickCallback;
            _ui.onCardPoolDetailsTabClick += onCardPoolDetailsTabClickCallback;
            _ui.onCardPoolDetailsCommentButtonClick += onCardPoolDetailsCommentButtonClickCallback;
            _ui.onCardPoolDetailsCommentPageButtonClick += onCardPoolDetailsCommentPageButtonClickCallback;

            _ui.onUploadPanelSelectCardPoolCloseButtonClick += onClickSelectUploadPoolCloseButtonCallback;
            _ui.onUploadPanelCardPoolItemClick += onClickSelectUploadPoolItemCallback;

            _ui.onClickWorkshopRuleViewButton += onClickWorkshopRuleViewButtonCallback;
            _ui.onUploadPanelConfirmButtonClick += onClickUploadConfirmButtonCallback;
            _ui.onUploadPanelCloseButtonClick += onClickUploadCloseButtonCallback;
            _ui.onCardDetailsSoundPlayClicked += onCardDetailsSoundPlayClickedCallback;
            _ui.onCardDetailsTabClick += onCardDetailsTabClickCallback;
            _ui.onCardDetailsCommentButtonClick += onCardDetailsCommentButtonClickCallback;
            _ui.onCardDetailsCommentPageButtonClick += onCardDetailsCommentPageButtonClickCallback;

            _ui.onCommentDialogClose += onCommentDialogCloseCallback;
            _ui.onSubmitComment += onSubmitCommentCallback;

            _cardPreviewList.enableCount(false);
            _cardPreviewList.onClickCard += onCardPreviewItemClickCallback;
        }
        #endregion

        #region 事件回调
        private async void onReturnClickCallback()
        {
            if (_viewingCard != null)
            {
                // 正在查看卡牌细节，返回卡池界面
                quitFromCardDetails();
                return;
            }
            if (_viewingCardPool != null)
            {
                // 正在查看卡池细节，返回主界面
                quitFromCardPool();
                return;
            }
            // 退出创意工坊。
            // 取消下载。
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
            // 取消预览缓存。
            cancelPreviewCaching();
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
            await viewCardPool(pool);
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
            quitFromCardPool();
        }
        private void onCardPoolDetailsTabClickCallback(WorkshopPanel.CardPoolDetailPage page)
        {
            _ui.setCardPoolDetailsPage(page);
        }
        private void onCardPoolDetailsCommentButtonClickCallback()
        {
            if (game.network.logic.isGuest)
            {
                var title = game.lang._(StringTable.HINT_TITLE);
                var desc = game.lang._(HINT_GUEST_NOT_VALID);
                game.ui.dialog.ShowConfirm(title, desc);
                return;
            }
            if (_viewingCardPool == null)
                return;

            _commentSource = new CommentSourceCardPool(this, _viewingCardPool);
            _ui.showCommentDialog();
        }
        private async void onCardPoolDetailsCommentPageButtonClickCallback(bool next)
        {
            if (_viewingCardPool == null)
                return;
            _cardPoolCommentPage += next ? 1 : -1;
            await updateCardPoolComments(_viewingCardPool.ID);
        }
        #endregion

        #region 卡牌详情
        private async void onCardDetailsSoundPlayClickedCallback(int index)
        {
            var cardPool = _viewingCardPool;
            if (cardPool == null)
                return;
            var cardDefine = _viewingCard?.cardDefine;
            if (cardDefine == null)
                return;
            var cardManager = game.card;
            var poolRef = new CardPoolReference(cardPool.ID, (int)cardPool.Version);
            var skin = cardManager.getCardSkin(poolRef, cardDefine.id);
            if (skin == null)
                return;
            var soundRefs = skin.getAllSoundRefs().Where(r => !string.IsNullOrEmpty(r.path)).ToArray();
            if (index < 0 || index >= soundRefs.Length)
                return;
            var soundRef = soundRefs[index];
            var sound = await cardManager.loadSound(poolRef, soundRef.path);
            if (sound == null)
                return;
            game.seManager.playSE(sound);
        }
        private void onCardDetailsTabClickCallback(WorkshopPanel.CardDetailPage page)
        {
            _ui.setCardDetailsPage(page);
        }
        private void onCardDetailsCommentButtonClickCallback()
        {
            if (game.network.logic.isGuest)
            {
                var title = game.lang._(StringTable.HINT_TITLE);
                var desc = game.lang._(HINT_GUEST_NOT_VALID);
                game.ui.dialog.ShowConfirm(title, desc);
                return;
            }
            if (_viewingCardPool == null)
                return;

            if (_viewingCard?.cardDefine == null)
                return;

            _commentSource = new CommentSourceCard(this, _viewingCardPool, _viewingCard.cardDefine.id);
            _ui.showCommentDialog();
        }
        private async void onCardDetailsCommentPageButtonClickCallback(bool next)
        {
            if (_viewingCardPool == null || _viewingCard?.cardDefine == null)
                return;
            _cardCommentPage += next ? 1 : -1;
            await updateCardComments(_viewingCardPool.ID, _viewingCard.cardDefine.id);
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
            if (game.network.logic.isGuest)
            {
                var title = game.lang._(StringTable.HINT_TITLE);
                var desc = game.lang._(HINT_GUEST_NOT_VALID);
                game.ui.dialog.ShowConfirm(title, desc);
                return;
            }

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
                Debug.LogError($"选择上传卡池时出现错误：{e}");
                onExceptionDialog(e, title, desc);
            }
            finally
            {
                stopWaiting();
            }
        }
        #endregion

        #region 上传卡池界面
        private async void onClickWorkshopRuleViewButtonCallback()
        {
            try
            {
                startWaiting();
                var content = await game.network.getWorkshopEULA();
                _ui.showWorkshopRule(content);
            }
            catch (Exception e)
            {
                var title = game.lang._(StringTable.ERROR);
                onExceptionDialog(e, title, e.Message);
            }
            finally
            {
                stopWaiting();
            }
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
            if (string.IsNullOrEmpty(desc))
            {
                var title = game.lang._(StringTable.HINT_TITLE);
                var quote = game.lang._(HINT_INPUT_DESCRIPTION);
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
                _ui.setUploadPoolPanelVisible(false);
                _ui.setSelectUploadPoolPanelVisible(false);
            }
            catch (Exception e)
            {
                var title = game.lang._(StringTable.ERROR);
                var quote = game.lang._(ERROR_UPLOADING_CARDPOOL, e.Message);
                Debug.LogError($"上传卡池时出现错误：{e}");
                onExceptionDialog(e, title, quote);
            }
            finally
            {
                stopWaiting();
            }
            await updatePage();
        }
        #endregion

        #region 卡牌预览列表
        private async void onCardPreviewItemClickCallback(CardDefine cardDefine)
        {
            try
            {
                _ui.setWorkshopPage(WorkshopPanel.WorkshopPage.Loading);

                _viewingCard = new CardDetailsData(this, cardDefine);
                _cardCommentPage = 0;

                await updateCardDetails(_viewingCardPool, _viewingCard);
                await updateCardComments(_viewingCardPool.ID, cardDefine.id);

                _ui.setWorkshopPage(WorkshopPanel.WorkshopPage.CardDetails);
                _ui.setCardDetailsPage(WorkshopPanel.CardDetailPage.Card);
            }
            catch (Exception e)
            {
                // 加载失败，弹出错误并退出预览
                var title = game.lang._(StringTable.ERROR);
                var quote = game.lang._(ERROR_PREVIEWING_CARD, e.Message);
                Debug.LogError($"预览卡牌时出现错误：{e}");
                onExceptionDialog(e, title, quote);

                quitFromCardDetails();
            }
        }
        #endregion

        #region 评论对话框
        private void onCommentDialogCloseCallback()
        {
            _commentSource = null;
            _ui.hideCommentDialog();
        }
        private async void onSubmitCommentCallback(string comment, bool positive)
        {
            if (_commentSource == null)
                return;
            if (await _commentSource.submit(comment))
            {
                scene.waitingBlocker.addWaitingTask(WaitingTask.LoadWorkshop);
                await _commentSource.updateComments();
                scene.waitingBlocker.removeWaitingTask(WaitingTask.LoadWorkshop);
                _commentSource = null;
                _ui.hideCommentDialog();
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

            if (info.versionNumber < CardManager.CURRENT_CARD_POOL_VERSION)
            {
                var msg = game.lang._(ERROR_CARD_POOL_VERSION_OUTDATED);
                throw new FormatException(msg);
            }

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
            if (resources == null)
                return Array.Empty<string>();
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
        private async Task viewCardPool(WorkshopCardPoolInfo coverInfo)
        {
            try
            {
                var workshopInfo = await getWorkshopCardPoolFile(coverInfo.ID, (int)coverInfo.Version);
                // 检验卡池能否被预览。
                var canPreview = canOpenCardPoolPreview(workshopInfo, out var errorMessage);
                if (!canPreview)
                {
                    var title = game.lang._(StringTable.ERROR);
                    game.ui.dialog.ShowConfirm(title, errorMessage);
                    return;
                }
                // 检验卡池依赖能否被下载。
                var allDeps = await getDownloadPoolAllDependencies(workshopInfo);
                var downloadableDeps = allDeps.Where(d => d.State != WorkshopCardPoolState.Deleted);
                errorMessage = validateCardPoolDependenciesDownload(workshopInfo, downloadableDeps);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    var title = game.lang._(StringTable.ERROR);
                    game.ui.dialog.ShowConfirm(title, errorMessage);
                    return;
                }

                _cardPoolCommentPage = 0;
                _viewingCardPool = workshopInfo;

                // 等待加载UI
                _ui.setWorkshopPage(WorkshopPanel.WorkshopPage.Loading);

                // 将卡池以及依赖卡池下载到缓存
                _previewCacheCts = new CancellationTokenSource();

                var group = downloadableDeps.Append(workshopInfo);
                _previewCacheTask = previewCacheCardPoolGroup(group, _previewCacheCts.Token);

                updateDownloadButton();
                await _previewCacheTask;
                clearPreviewCaching();

                // 更新页面。
                await updateCardPoolDetails(workshopInfo);
                // 加载评论。
                await updateCardPoolComments(workshopInfo.ID);

                updateDownloadButton();

                _ui.setWorkshopPage(WorkshopPanel.WorkshopPage.CardPoolDetails);
                _ui.setCardPoolDetailsPage(WorkshopPanel.CardPoolDetailPage.Info);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("预览卡池任务被取消。");
            }
            catch (Exception e)
            {
                // 加载失败，弹出错误并退出预览
                var title = game.lang._(StringTable.ERROR);
                var quote = game.lang._(ERROR_PREVIEWING_CARDPOOL, e.Message);
                Debug.LogError($"预览卡池时出现错误：{e}");
                onExceptionDialog(e, title, quote);

                quitFromCardPool();
            }
        }
        private void quitFromCardPool()
        {
            _viewingCardPool = null;
            _ui.setWorkshopPage(WorkshopPanel.WorkshopPage.Browse);

            cancelPreviewCaching();
            updateDownloadButton();
        }
        private async Task updateCardPoolDetails(WorkshopCardPoolInfo workshopInfo)
        {
            // 详细信息。
            var viewData = await getCardPoolHeaderViewData(workshopInfo);
            var creationTime = DateTime.UnixEpoch.AddSeconds(workshopInfo.CreatedAt);
            creationTime += TimeZoneInfo.Local.GetUtcOffset(creationTime);
            var dateTime = game.lang._(TASK_DATETIME_TEMPLATE, creationTime.Year, creationTime.Month, creationTime.Day, creationTime.Hour, creationTime.Minute);
            string dependencies = await getWorkshopCardPoolDependenciesName(workshopInfo);
            WorkshopCardPoolRating rating = await game.network.getCardPoolRating(workshopInfo.ID);
            var details = new WorkshopCardPoolDetailsViewData()
            {
                creator = getUserName((int)workshopInfo.Author),
                creationTime = dateTime,
                cardCount = workshopInfo.CardCount.ToString(),
                description = string.IsNullOrEmpty(workshopInfo.Description) ? game.lang._(NO_DESCRIPTION_PROVIDED) : workshopInfo.Description,
                version = workshopInfo.Version.ToString(),
                positiveRating = rating?.GoodCount.ToString(),
                negativeRating = rating?.BadCount.ToString(),
                dependencies = dependencies,
            };
            _ui.setCardPoolDetails(viewData, details);

            // 卡池状态。
            var selfUserInfo = game.network.logic.UserInfo;
            if (selfUserInfo != null)
            {
                bool stateVisible = _viewingUser == selfUserInfo.UID;
                bool reviewsVisible = stateVisible && workshopInfo.State == WorkshopCardPoolState.NoPass;
                _ui.setCardPoolDetailsStateVisible(stateVisible);
                _ui.setReviewsVisible(reviewsVisible);
                if (stateVisible)
                {
                    var stateColor = getStateColor(workshopInfo.State);
                    var stateText = getStateText(workshopInfo.State);
                    _ui.setCardPoolDetailsState(stateColor, stateText);
                }
                if (reviewsVisible)
                {
                    var reviews = await game.network.getWorkshopCardPoolReviews(workshopInfo.ID, (int)workshopInfo.Version);
                    var reviewString = getReviewsText(reviews);
                    _ui.setReviewsText(reviewString);
                }
            }

            // 卡牌列表。
            var poolRef = new CardPoolReference(workshopInfo.ID, (int)workshopInfo.Version);
            var (_, cardPool) = await game.card.getOrLoadCardPool(poolRef);
            if (cardPool != null)
            {
                _cardPreviewList.updateCards(cardPool.cardDefineList.OrderBy(d => d.getCost()).ThenBy(d => d.id));
            }
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
            bool viewingCardPool = _viewingCardPool != null;
            bool downloading = _downloadTask != null;
            bool canDownload = viewingCardPool && _viewingCardPool.State != WorkshopCardPoolState.Deleted && _previewCacheTask == null;
            bool canCancel = downloading;
            bool interactable = canDownload || canCancel;
            _ui.switchUploadOrDownloadButton(downloading || viewingCardPool);
            _ui.setDownloadButtonInteractable(interactable);
        }
        private async Task<WorkshopCommentViewData[]> getCardPoolCommentsViewData(long cardPoolId)
        {
            var comments = await game.network.getWorkshopCardPoolComments(cardPoolId, _cardPoolCommentPage * _cardPoolCommentPerPage, _cardPoolCommentPerPage);
            return comments.Select(c => getCardPoolCommentViewData(c)).ToArray();
        }
        private async Task updateCardPoolComments(long cardPoolId)
        {
            WorkshopCommentViewData[] comments;
            bool commentsError = false;
            try
            {
                _ui.setCardPoolCommentPageText(game.lang._(StringTable.PAGE_NUMBER_TEMPLATE, _cardPoolCommentPage + 1));
                _ui.setCardPoolCommentPageButtonInteractable(false, false);
                comments = await getCardPoolCommentsViewData(cardPoolId);
                _ui.setCardPoolCommentPageButtonInteractable(_cardPoolCommentPage > 0, comments.Length >= _cardPoolCommentPerPage);
            }
            catch (Exception ex)
            {
                comments = Array.Empty<WorkshopCommentViewData>();
                commentsError = true;
                _ui.setCardPoolCommentPageButtonInteractable(false, false);
                Debug.LogErrorFormat("加载卡池评论时出现错误：{0}", ex);
            }
            _ui.setCardPoolComments(comments, commentsError);
        }
        #endregion

        #region 卡牌详细信息
        private async Task updateCardDetails(WorkshopCardPoolInfo poolInfo, CardDetailsData cardDetails)
        {
            var authorName = getUserName((int)poolInfo.Author);
            _ui.setCardDetails(await cardDetails.getViewData(poolInfo, authorName));
        }
        private void quitFromCardDetails()
        {
            _viewingCard = null;
            _ui.setWorkshopPage(WorkshopPanel.WorkshopPage.CardPoolDetails);
        }
        private async Task<WorkshopCommentViewData[]> getCardCommentsViewData(long cardPoolId, int cardId)
        {
            var comments = await game.network.getWorkshopCardComments(cardPoolId, cardId, _cardCommentPage * _cardCommentPerPage, _cardCommentPerPage);
            return comments.Select(c => getCardCommentViewData(c)).ToArray();
        }
        private async Task updateCardComments(long cardPoolId, int cardId)
        {
            WorkshopCommentViewData[] comments;
            bool commentsError = false;
            try
            {
                _ui.setCardCommentPageText(game.lang._(StringTable.PAGE_NUMBER_TEMPLATE, _cardCommentPage + 1));
                _ui.setCardCommentPageButtonInteractable(false, false);
                comments = await getCardCommentsViewData(cardPoolId, cardId);
                _ui.setCardCommentPageButtonInteractable(_cardCommentPage > 0, comments.Length >= _cardCommentPerPage);
            }
            catch (Exception ex)
            {
                comments = Array.Empty<WorkshopCommentViewData>();
                commentsError = true;
                _ui.setCardCommentPageButtonInteractable(false, false);
                Debug.LogErrorFormat("加载卡牌评论时出现错误：{0}", ex);
            }
            _ui.setCardComments(comments, commentsError);
        }
        #endregion

        #region 预览，缓存卡池
        private bool canOpenCardPoolPreview(WorkshopCardPoolInfo poolInfo, out string errorMessage)
        {
            if (poolInfo.State == WorkshopCardPoolState.Deleted)
            {
                // 卡池已被删除。
                errorMessage = game.lang._(ERROR_DOWNLOAD_DELETED_CARDPOOL);
                return false;
            }
            errorMessage = null;
            return true;
        }
        private void clearPreviewCaching()
        {
            _previewCacheTask = null;
            _previewCacheCts = null;
        }
        private void cancelPreviewCaching()
        {
            if (_previewCacheCts != null)
            {
                _previewCacheCts.Cancel();
                clearPreviewCaching();
            }
        }
        private async Task previewCacheCardPoolGroup(IEnumerable<WorkshopCardPoolInfo> workshopInfos, CancellationToken token)
        {
            foreach (var info in workshopInfos)
            {
                await previewCacheCardPool(info, token);
                token.ThrowIfCancellationRequested();
            }
        }
        private async Task previewCacheCardPool(WorkshopCardPoolInfo workshopInfo, CancellationToken token)
        {
            var (poolInfo, cardPool) = await previewCacheCardPoolFile(workshopInfo);

            token.ThrowIfCancellationRequested();

            await previewCacheCardPoolResources(workshopInfo, poolInfo, cardPool, token);
        }
        private async Task<(CardPoolInfo info, CardPool pool)> previewCacheCardPoolFile(WorkshopCardPoolInfo workshopInfo)
        {
            var resType = ResourceType.CardDefine;
            var poolRef = new CardPoolReference(workshopInfo.ID, (int)workshopInfo.Version);
            var resId = CardPoolReference.GetResourceID(workshopInfo.ID, (int)workshopInfo.Version);
            if (game.cache.FileExists(resType, resId))
            {
                var (poolInfo, cardPool) = await game.card.loadCachedCardPool(workshopInfo.ID, (int)workshopInfo.Version, false);

                // 修复卡池文件。
                await game.card.fixCardPool(poolInfo, cardPool);

                return (poolInfo, cardPool);
            }
            else
            {
                // 下载卡池本体文件
                var poolFile = await game.network.getWorkshopResource("cardpool", workshopInfo.ContentID);
                var decompressed = Compression.tryDecompress(poolFile);
                if (!game.card.tryParseCardPoolBytes(decompressed, out var poolInfo, out var cardPool))
                    throw new FormatException("创意工坊卡池文件格式不正确");

                // 修复卡池文件。
                await game.card.fixCardPool(poolInfo, cardPool);

                var bytes = game.card.serializeCardPool(poolInfo, cardPool);
                await game.cache.SaveCache(resType, resId, bytes);

                // 将卡池信息读取到内存。
                await game.card.loadCachedCardPool(workshopInfo.ID, (int)workshopInfo.Version, false);

                return (poolInfo, cardPool);
            }
        }
        private async Task previewCacheCardPoolResources(WorkshopCardPoolInfo workshopInfo, CardPoolInfo poolInfo, CardPool cardPool, CancellationToken token)
        {
            // 下载资源
            var poolResources = CardManager.getCardPoolResources(poolInfo, cardPool);
            var workshopResources = workshopInfo.Resources.Prepend(workshopInfo.CoverImage);
            foreach (var resId in workshopResources)
            {
                if (string.IsNullOrEmpty(resId))
                    continue;

                var (resType, resPath) = poolResources.FirstOrDefault(r => r.path.StartsWith(resId));
                if (string.IsNullOrEmpty(resPath))
                    continue;

                if (game.cache.FileExists(resType, resId))
                    continue;

                // 创意工坊卡池需要这个资源，并且卡池文件也需要，下载
                var extension = Path.GetExtension(resPath).TrimStart('.');
                var resourceBytes = await game.network.getWorkshopResource(extension, resId);

                await game.cache.SaveCache(resType, resId, resourceBytes);

                token.ThrowIfCancellationRequested();
            }
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

                _ui.showHintMessage(game.lang._(CARD_POOL_DOWNLOADED));
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
                Debug.LogError($"下载卡池时出现错误：{e}");
                deleteDownloadedFiles();
                onExceptionDialog(e, title, quote);
            }
            finally
            {
                _downloadTask = null;
                _downloadCts = null;
                _downloadingCardPools.Clear();
                _downloadedCardPools.Clear();
                _downloadedResources.Clear();
                _ui.setDownloadButtonText(game.lang._(BUTTON_TEXT_DOWNLOAD));
                _ui.setDownloadProgressVisible(false);
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
            _ui.setDownloadProgressVisible(true);
            _ui.setDownloadProgress(0);

            // 下载卡池本体文件
            var (poolInfo, cardPool) = await downloadPoolFile(workshopInfo);

            _ui.setDownloadProgress(cardPoolFileProgress);

            token.ThrowIfCancellationRequested();

            // 下载资源
            await downloadPoolResources(workshopInfo, poolInfo, cardPool, (downloaded, total) =>
            {
                var downloadPercent = downloaded / (float)total;
                _ui.setDownloadProgress(cardPoolFileProgress + cardPoolResourceProgress * (downloadPercent));
                token.ThrowIfCancellationRequested();
            });

            _ui.setDownloadProgress(1);
        }
        private async Task<(CardPoolInfo info, CardPool pool)> downloadPoolFile(WorkshopCardPoolInfo workshopInfo)
        {
            var resType = ResourceType.CardDefine;
            var resId = CardPoolReference.GetResourceID(workshopInfo.ID, (int)workshopInfo.Version);
            CardPoolInfo poolInfo;
            CardPool cardPool;
            if (game.cache.FileExists(resType, resId))
            {
                // 存在缓存文件，直接搬运
                var (info, pool) = await game.card.loadCachedCardPool(workshopInfo.ID, (int)workshopInfo.Version);
                poolInfo = info;
                cardPool = pool;
            }
            else
            {
                // 从创意工坊下载卡池
                var poolFile = await game.network.getWorkshopResource("cardpool", workshopInfo.ContentID);
                var decompressed = Compression.tryDecompress(poolFile);
                if (!game.card.tryParseCardPoolBytes(decompressed, out poolInfo, out cardPool))
                    throw new FormatException("下载的创意工坊卡池文件格式不正确");
            }
            await game.card.fixCardPool(poolInfo, cardPool);

            var path = game.card.allocCardPoolPathById(poolInfo);
            await game.card.saveCardPoolFileOnly(poolInfo, cardPool, path);
            await game.card.loadLocalCardPoolInfoAsync(path);

            _downloadedCardPools.Add(poolInfo.ToReference());

            return (poolInfo, cardPool);
        }
        private async Task downloadPoolResources(WorkshopCardPoolInfo workshopInfo, CardPoolInfo poolInfo, CardPool cardPool, Action<int, int> onDownloaded = null)
        {
            var poolResources = CardManager.getCardPoolResources(poolInfo, cardPool);
            int downloadedCount = 0;
            int resourceCount = poolResources.Length;
            var workshopResources = workshopInfo.Resources.Prepend(workshopInfo.CoverImage);
            foreach (var resId in workshopResources)
            {
                if (!string.IsNullOrEmpty(resId))
                {
                    // 创意工坊卡池需要这个资源，并且卡池文件也需要，下载
                    var (resType, resPath) = poolResources.FirstOrDefault(r => r.path.StartsWith(resId));
                    if (!string.IsNullOrEmpty(resPath) && !game.card.isResourceExists(poolInfo, resType, resPath))
                    {
                        await downloadPoolResource(poolInfo, resType, resId, resPath);
                    }
                }
                downloadedCount++;
                onDownloaded?.Invoke(downloadedCount, resourceCount);
            }
        }
        private async Task downloadPoolResource(CardPoolInfo poolInfo, ResourceType resType, string resId, string resPath)
        {
            var extension = Path.GetExtension(resPath).TrimStart('.');
            byte[] bytes;
            if (game.cache.FileExists(resType, resId))
            {
                // 存在缓存文件，直接搬运。
                bytes = await game.cache.ReadCache(resType, resId);
            }
            else
            {
                bytes = await game.network.getWorkshopResource(extension, resId);
            }
            var savedFileName = await game.card.saveResourceBytes(poolInfo.cardPoolId, bytes, resType, resId, extension);
            _downloadedResources.Add(savedFileName);
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
                quitFromCardPool();
                await updatePage();
            }
            catch (Exception e)
            {
                var title = game.lang._(StringTable.ERROR);
                var desc = game.lang._(ERROR_DELETING_CARDPOOL, e.Message);
                Debug.LogError($"删除卡池时出现错误：{e}");
                onExceptionDialog(e, title, desc);
            }
            finally
            {
                stopWaiting();
            }
        }
        #endregion

        private bool checkUnauthorized(Exception e)
        {
            if (game.network.checkExceptionUnauthorized(e))
            {
                if (_downloadCts != null && !_downloadCts.IsCancellationRequested)
                {
                    _downloadCts.Cancel();
                }
                hide();
                scene.menu.display();
                scene.menu.clearUserInfo();
                scene.menu.showUnauthorizedDialog();
                return true;
            }
            return false;
        }
        private void onExceptionDialog(Exception e, string dialogTitle, string message = null)
        {
            if (!checkUnauthorized(e))
            {
                game.ui.dialog.ShowConfirm(dialogTitle, message ?? e.Message, null);
            }
        }
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

            _ui.setViewingUser(_viewingUser >= 0);
            if (_viewingUser >= 0)
            {
                var label = game.lang._(VIEWING_USER_NAME_TEMPLATE, getUserName(_viewingUser));
                _ui.setViewingUserLabel(label);
            }
            try
            {
                _loadingPage = true;
                updateBrowseComponents();

                _cardPoolCacheDict.Clear();
                _ui.updateBrowseCardPools(Array.Empty<WorkshopCardPoolItemViewData>());

                await loadPage(_page, _sorting);
                var pageViewDatas = await getCurrentPageViewData();
                bool hotPage = isHotBrowse(_sorting) && _page == 0;
                if (hotPage)
                {
                    _ui.updateHotCardPools(pageViewDatas);
                }
                else
                {
                    _ui.updateBrowseCardPools(pageViewDatas);
                }

                _ui.setPageText(game.lang._(StringTable.PAGE_NUMBER_TEMPLATE, _page + 1));

                _ui.setLoadingState(WorkshopPanel.LoadingState.Normal);
            }
            catch (Exception e)
            {
                Debug.LogError($"加载创意工坊页面{_page}失败：{e}");
                _ui.setLoadingState(WorkshopPanel.LoadingState.Warning);
                checkUnauthorized(e);
            }
            finally
            {
                _loadingPage = false;
                updateBrowseComponents();
            }
        }
        private async Task loadPage(int page, Sorting sorting)
        {
            var sortingString = sortingStrings.TryGetValue(sorting, out var str) ? str : "new";
            if (_viewingUser >= 0)
            {
                var limit = getMaxPoolCountOfPage(page, false);
                var offset = getPoolPageOffset(page, false);
                _currentPageCardPools = await game.network.getWorkshopUserCardPools(_viewingUser, offset, limit, sortingString);
            }
            else
            {
                bool hot = isHotBrowse(sorting);
                var limit = getMaxPoolCountOfPage(page, hot);
                var offset = getPoolPageOffset(page, hot);
                _currentPageCardPools = await game.network.searchWorkshopPageCardPools(offset, limit, _searchFilter, sortingString);
            }
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
        private async Task<WorkshopCardPoolItemViewData[]> getCurrentPageViewData()
        {
            var count = _currentPageCardPools.Length;
            var viewDatas = new WorkshopCardPoolItemViewData[count];
            for (int i = 0; i < count; i++)
            {
                viewDatas[i] = await getCardPoolItemViewData(_currentPageCardPools[i]);
            }
            return viewDatas;
        }
        private async Task<WorkshopCardPoolItemViewData> getCardPoolItemViewData(WorkshopCardPoolInfo info)
        {
            Sprite sprite = await loadCardPoolCoverImage(info.CoverImage);
            var authorName = getUserName((int)info.Author);
            var rating = await game.network.getCardPoolRating(info.ID);
            return new WorkshopCardPoolItemViewData()
            {
                image = sprite,
                name = info.Name,
                hasPrivatePools = info.State == WorkshopCardPoolState.Pending,
                isWarning = info.State == WorkshopCardPoolState.OldVer || info.State == WorkshopCardPoolState.NoPass || info.State == WorkshopCardPoolState.ReviewingOldVer,
                isError = info.State == WorkshopCardPoolState.Deleted,
                isWaiting = info.State == WorkshopCardPoolState.Reviewing || info.State == WorkshopCardPoolState.ReviewingOldVer,
                author = authorName,
                number = 0,
                rating = (int)rating.GoodCount - (int)rating.BadCount,
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
        private void updateBrowseComponents()
        {
            if (_loadingPage)
            {
                _ui.setBrowseComponentsInteractable(false);
                _ui.setPageButtonInteractable(false, false);
            }
            else
            {
                _ui.setBrowseComponentsInteractable(true);
                int currentPageCardPools = _currentPageCardPools?.Length ?? 0;
                int maxPoolCount = getMaxPoolCountOfPage(_page, isHotBrowse(_sorting));
                _ui.setPageButtonInteractable(_page > 0, currentPageCardPools >= maxPoolCount);
            }
        }
        private WorkshopCommentViewData getCardPoolCommentViewData(WorkshopCardPoolComment comment)
        {
            var creationTime = DateTime.UnixEpoch.AddSeconds(comment.CreatedAt);
            creationTime += TimeZoneInfo.Local.GetUtcOffset(creationTime);
            var dateTime = game.lang._(TASK_DATETIME_TEMPLATE, creationTime.Year, creationTime.Month, creationTime.Day, creationTime.Hour, creationTime.Minute);
            return new WorkshopCommentViewData()
            {
                comment = comment.Content,
                time = dateTime,
                rating = 0,
                userName = getUserName((int)comment.UID)
            };
        }
        private WorkshopCommentViewData getCardCommentViewData(WorkshopCardComment comment)
        {
            var creationTime = DateTime.UnixEpoch.AddSeconds(comment.CreatedAt);
            creationTime += TimeZoneInfo.Local.GetUtcOffset(creationTime);
            var dateTime = game.lang._(TASK_DATETIME_TEMPLATE, creationTime.Year, creationTime.Month, creationTime.Day, creationTime.Hour, creationTime.Minute);
            return new WorkshopCommentViewData()
            {
                comment = comment.Content,
                time = dateTime,
                rating = comment.Rating,
                userName = getUserName((int)comment.UID)
            };
        }
        private int getMaxPoolCountOfPage(int page, bool hot)
        {
            if (hot && page == 0)
            {
                return _hotPoolCount;
            }
            return _poolCountPerPage;
        }
        private int getPoolPageOffset(int page, bool hot)
        {
            if (hot)
            {
                if (page == 0)
                    return 0;
                else
                    return (page - 1) * getMaxPoolCountOfPage(page, hot) + _hotPoolCount;
            }
            else
            {
                return page * getMaxPoolCountOfPage(page, hot);
            }
        }
        private bool isHotBrowse(Sorting sorting)
        {
            return sorting == Sorting.Rating;
        }

        #endregion

        #region 属性字段
        [TranslateMsg("创意工坊-按钮文字，{0}为加载进度百分比")]
        private const string TASK_DOWNLOADING = "下载中（{0}%）";
        [TranslateMsg("创意工坊-描述文本模板，{0}为年，{1}为月，{2}为日，{3}为小时，{4}为分钟")]
        private const string TASK_DATETIME_TEMPLATE = "{0}年{1}月{2}日 {3}:{4}";
        [TranslateMsg("创意工坊-排序方法")]
        private const string SORTING_HOTTEST = "最热门";
        [TranslateMsg("创意工坊-排序方法")]
        private const string SORTING_NEWEST = "最新上传";
        [TranslateMsg("创意工坊-排序方法")]
        private const string SORTING_RATING = "最高评分"; 

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
        [TranslateMsg("创意工坊-查看卡池时出现的错误描述，{0}为错误信息")]
        private const string ERROR_PREVIEWING_CARDPOOL = "查看卡池时出现错误：{0}";
        [TranslateMsg("创意工坊-查看卡牌时出现的错误描述，{0}为错误信息")]
        private const string ERROR_PREVIEWING_CARD = "查看卡牌时出现错误：{0}";

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
        [TranslateMsg("创意工坊-尝试上传低格式版本卡池")]
        private const string ERROR_CARD_POOL_VERSION_OUTDATED = "卡池格式过旧，请尝试重新保存卡池以更新卡池格式。";

        [TranslateMsg("创意工坊-尝试在下载时离开的警告")]
        private const string WARNING_QUIT_WHILE_DOWNLOADING = "正在下载卡池，退出创意工坊会取消下载。\n您确认要退出吗？";
        [TranslateMsg("创意工坊-尝试删除卡池时出现的警告，{0}为卡池名字")]
        private const string WARNING_DELETE_CARD_POOL = "您确认要删除卡池“{0}”吗？";

        [TranslateMsg("创意工坊-尝试不输入描述时上传卡池时出现的提示描述")]
        private const string HINT_INPUT_DESCRIPTION = "请输入卡池描述。";
        [TranslateMsg("创意工坊-尝试不勾选同意创意工坊规则上传卡池时出现的提示描述")]
        private const string HINT_AGREE_RULE_FIRST = "请先同意<b>创意工坊使用规则</b>。";
        [TranslateMsg("创意工坊-上传卡池成功后的提示描述")]
        private const string HINT_UPLOAD_CARDPOOL_SUCCESS = "卡池上传成功，请等待工作人员进行审核。";
        [TranslateMsg("创意工坊-上传卡池资源失败后的提示描述，{0}为未上传的资源数量")]
        private const string HINT_UPLOAD_RESOURCE_FAILED = "卡池上传失败，有{0}个资源未上传。";
        [TranslateMsg("创意工坊-尝试上传已经上传的卡池后的提示描述")]
        private const string HINT_CARD_POOL_ALREADY_UPLOADED = "该卡池的同一版本已被上传至创意工坊，无需再次上传。\n请考虑提升卡池版本再试。";

        [TranslateMsg]
        private const string HINT_GUEST_NOT_VALID = "游客不能使用此功能";
        [TranslateMsg("创意工坊-上传进度-上传卡池文件")]
        private const string UPLOAD_PROGRESS_UPLOADING_POOL_FILE = "上传卡池文件中……";
        [TranslateMsg("创意工坊-上传进度-上传卡池资源")]
        private const string UPLOAD_PROGRESS_UPLOADING_RESOURCES = "上传卡池资源中……";
        [TranslateMsg("创意工坊-查看其他用户的卡池时的说明")]
        private const string VIEWING_USER_NAME_TEMPLATE = "正在查看：{0}上传的卡池";
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

        [TranslateMsg("创意工坊-卡池下载完毕提示文本")]
        private const string CARD_POOL_DOWNLOADED = "卡池下载完毕";

        const float cardPoolFileProgress = 0.25f;
        const float cardPoolResourceProgress = 1 - cardPoolFileProgress;

        private static string[] _sortNames =
        {
            SORTING_NEWEST,
            SORTING_RATING,
        };
        private static Sorting[] _sortValues =
        {
            Sorting.Newest,
            Sorting.Rating,
        };
        private static Dictionary<Sorting, string> sortingStrings = new Dictionary<Sorting, string>()
        {
            { Sorting.Newest, "new"},
            { Sorting.Hottest, "hot"},
            { Sorting.Rating, "rate"},
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

        private bool _loadingPage;
        private WorkshopCardPoolInfo[] _currentPageCardPools;
        private int _cardPoolCommentPage;
        private WorkshopCardPoolInfo _viewingCardPool;
        private int _cardCommentPage;
        private CardDetailsData _viewingCard;
        private Dictionary<string, WorkshopCardPoolInfo> _cardPoolCacheDict = new Dictionary<string, WorkshopCardPoolInfo>();

        private CancellationTokenSource _previewCacheCts;
        private Task _previewCacheTask;

        private CancellationTokenSource _downloadCts;
        private Task _downloadTask;
        private List<long> _downloadingCardPools = new List<long>();
        private List<ICardPoolReference> _downloadedCardPools = new List<ICardPoolReference>();
        private List<string> _downloadedResources = new List<string>();

        private CommentSource _commentSource;

        private int _page;


        private Sorting _sorting;
        private int _viewingUser = -1;
        private string _searchFilter;

        private CardPoolInfo[] _selectingUploadPools;
        private long _uploadingPoolId;

        [SerializeField]
        private int _poolCountPerPage = 16;
        [SerializeField]
        private int _hotPoolCount = 3;
        [SerializeField]
        private int _cardPoolCommentPerPage = 20;
        [SerializeField]
        private int _cardCommentPerPage = 20;
        [SerializeField]
        private WorkshopPanel _ui;
        [SerializeField]
        private CardListPanelCtrl _cardPreviewList;
        #endregion

        #region 内嵌类
        private enum Sorting
        {
            Newest,
            Hottest,
            Rating,
        }
        private class CardDetailsData
        {
            public CardDetailsData(WorkshopCtrl controller, CardDefine define)
            {
                _controller = controller;
                cardDefine = define;
            }
            public async Task<WorkshopCardDetailsViewData> getViewData(WorkshopCardPoolInfo workshopInfo, string creator)
            {
                var game = _controller.game;
                var cardManager = game.card;
                var poolRef = new CardPoolReference(workshopInfo.ID, (int)workshopInfo.Version);
                var skin = cardManager.getCardSkin(poolRef, cardDefine.id);
                var sounds = skin.getAllSoundRefs().Where(r => !string.IsNullOrEmpty(r.path)).Select(r => game.editor.translateCardSoundName(r.name)).ToArray();

                var (characterIcon, characterName) = await getCharacterData(game, workshopInfo);
                var rating = await game.network.getCardRating(workshopInfo.ID, cardDefine.id);
                return new WorkshopCardDetailsViewData()
                {
                    card = UIHelper.getCardDefineViewData(cardDefine, skin, cardManager),
                    cardImage = await cardManager.loadCardImage(poolRef, skin),
                    cardPoolName = workshopInfo.Name,
                    creator = creator,
                    characterIcon = characterIcon,
                    characterName = characterName,
                    flavor = string.Empty,
                    positiveRating = rating?.GoodCount.ToString(),
                    negativeRating = rating?.BadCount.ToString(),
                    sounds = sounds,
                };
            }
            private async Task<(Sprite sprite, string name)> getCharacterData(MainManager game, WorkshopCardPoolInfo workshopInfo)
            {
                var cardManager = game.card;
                var characterRef = cardDefine.getCharacterRef();
                // 中立
                if (characterRef == null || characterRef.cardPoolId == 0)
                    return (null, game.lang._(StringTable.CARDVIEW_CHARACTER_NEUTRAL));

                var env = workshopInfo.Dependencies.Select(d => new CardPoolReference(d.ID, (int)d.Version)).Prepend(new CardPoolReference(workshopInfo.ID, (int)workshopInfo.Version));
                var characterPoolRef = cardManager.getCardPoolReferenceInEnv(characterRef.cardPoolId, env);
                if (characterPoolRef != null)
                {
                    var (_, cardPool) = await cardManager.getOrLoadCardPool(characterPoolRef);
                    if (cardPool != null)
                    {
                        var characterDefine = cardPool.cardDefineList.FirstOrDefault(d => d.id == characterRef.defineId);
                        if (characterDefine != null)
                        {
                            var characterIcon = await cardManager.loadCardImage(characterDefine);
                            var characterName = cardManager.getCardName(characterPoolRef, characterDefine.id);
                            return (characterIcon, characterName);
                        }
                    }
                }
                return (null, game.lang._(StringTable.UNKNOWN_CHARACTER));
            }
            public CardDefine cardDefine;
            private WorkshopCtrl _controller;
        }
        private abstract class CommentSource
        {
            public CommentSource(WorkshopCtrl ctrl)
            {
                this.ctrl = ctrl;
            }
            public abstract Task<bool> submit(string comment);
            public abstract Task updateComments();
            protected WorkshopCtrl ctrl;
        }
        private class CommentSourceCardPool : CommentSource
        {
            public CommentSourceCardPool(WorkshopCtrl ctrl, WorkshopCardPoolInfo info) : base(ctrl)
            {
                this.info = info;
            }
            public override async Task<bool> submit(string comment)
            {
                var game = ctrl.game;
                var scene = ctrl.scene;
                try
                {
                    scene.waitingBlocker.addWaitingTask(WaitingTask.LoadWorkshop);

                    await game.network.postWorkshopCardPoolComment(info.ID, comment);
                    return true;
                }
                catch (Exception e)
                {
                    var title = game.lang._(StringTable.ERROR);
                    ctrl.onExceptionDialog(e, title);
                    return false;
                }
                finally
                {
                    scene.waitingBlocker.removeWaitingTask(WaitingTask.LoadWorkshop);
                }
            }
            public override Task updateComments()
            {
                return ctrl.updateCardPoolComments(info.ID);
            }
            public WorkshopCardPoolInfo info;

        }
        private class CommentSourceCard : CommentSource
        {
            public CommentSourceCard(WorkshopCtrl ctrl, WorkshopCardPoolInfo info, int cardId) : base(ctrl)
            {
                this.info = info;
                this.cardId = cardId;
            }
            public override async Task<bool> submit(string comment)
            {
                var game = ctrl.game;
                var scene = ctrl.scene;
                try
                {
                    scene.waitingBlocker.addWaitingTask(WaitingTask.LoadWorkshop);

                    await game.network.postWorkshopCardComment(info.ID, cardId, comment);
                    return true;
                }
                catch (Exception e)
                {
                    var title = game.lang._(StringTable.ERROR);
                    ctrl.onExceptionDialog(e, title);
                    return false;
                }
                finally
                {
                    scene.waitingBlocker.removeWaitingTask(WaitingTask.LoadWorkshop);
                }
            }
            public override Task updateComments()
            {
                return ctrl.updateCardComments(info.ID, cardId);
            }
            public WorkshopCardPoolInfo info;
            public int cardId;
        }
        #endregion
    }

}
