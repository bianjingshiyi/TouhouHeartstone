using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Controllers;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine.Networking;
using ILogger = TouhouCardEngine.Shared.ILogger;

namespace Game
{
    /// <summary>
    /// 用于管理房间内卡池传输的管理器。
    /// </summary>
    public class RoomCardPoolManager
    {
        #region 公共方法
        /// <summary>
        /// 检查是否应进行卡池的更新。
        /// 如果应该更新卡池，会自动运行卡池计划。
        /// </summary>
        /// <param name="cardPools">房间允许的，或者要加入的卡池列表。</param>
        /// <returns></returns>
        public async Task CheckUpdate(CardPoolInfoPack[] cardPools)
        {
            // 空闲状态下才可检查更新。
            while (state != PlayerLoadState.None)
                await Task.Delay(retryCooldown);

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                ChangeLoadState(PlayerLoadState.Checking);
                List<CardPoolInfoPack> uploadPools = new List<CardPoolInfoPack>();
                List<CardPoolInfoPack> downloadPools = new List<CardPoolInfoPack>();
                bool startPlan = false;
                foreach (CardPoolInfoPack roomPool in cardPools)
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        var poolRef = roomPool.GetReference();
                        if (game.card.isBuiltinCardPool(roomPool.cardPoolId) || game.card.isStandardCardPool(roomPool.cardPoolId))
                        {
                            continue;
                        }
                        string id = roomPool.GetResourceID();
                        // 已经在计划中的卡池，或者已经检查为上传完毕的卡池直接跳过。
                        if (IsCardPoolInPlan(roomPool.cardPoolId, roomPool.version) || uploadedPools.Contains(id))
                        {
                            continue;
                        }
                        bool lanHost = game.network.logic.isLanHost;
                        var localPoolInfo = game.card.getCardPoolInfo(poolRef);

                        // 这个卡池是否是玩家本地可用的。
                        bool hasUsablePool = false;
                        string cardPoolPath = game.card.getCardPoolPath(poolRef);
                        if (localPoolInfo != null && localPoolInfo.editTime == roomPool.version)
                        {
                            if (!string.IsNullOrEmpty(cardPoolPath))
                            {
                                hasUsablePool = game.card.isLocalCardPool(poolRef) && game.fileManager.isFileExist(cardPoolPath);
                            }
                        }

                        // 这个卡池是否存在于玩家本地或者缓存中。
                        bool hasLocalPool = game.cache.FileExists(game.fileManager, ResourceType.CardDefine, id) || (localPoolInfo != null && localPoolInfo.editTime == roomPool.version);

                        // 远端是否有该卡池。
                        // 局域网房主的远端就是本地。
                        bool hasRemotePool = lanHost ? hasLocalPool : await game.network.ResourceExistsAsync(ResourceType.CardDefine, id);

                        // 如果没有读取卡池，尝试读取卡池对象。
                        CardPool localCardPool = null;
                        if (hasLocalPool && !game.card.tryGetCardPool(roomPool.GetReference(), out localCardPool) && !string.IsNullOrEmpty(cardPoolPath))
                        {
                            localCardPool = (await game.card.loadCardPool(cardPoolPath)).cardPool;
                        }

                        token.ThrowIfCancellationRequested();
                        // 上传卡池。
                        List<ResourcePlan> uploadResources = new List<ResourcePlan>();
                        // 本地存在该房间指定的卡池，且不是缓存。
                        if (hasUsablePool && !lanHost)
                        {
                            if (!hasRemotePool)
                            {
                                // 远端没有该卡池信息，上传该卡池文件。
                                uploadPools.Add(roomPool);
                                startPlan = true;
                                break;
                            }
                            // 检测卡图上传。
                            else if (localCardPool != null)
                            {
                                var resList = new List<Tuple<ResourceType, string>>();
                                var picturePaths = GetCardPoolPictures(poolRef);
                                // 遍历卡池内的所有图片。
                                foreach (var path in picturePaths)
                                {
                                    string picId = Path.GetFileNameWithoutExtension(path);
                                    if (string.IsNullOrEmpty(picId))
                                        continue;

                                    resList.Add(new Tuple<ResourceType, string>(ResourceType.Picture, picId));
                                }

                                // 如果有远端不存在的图片，上传该卡图
                                token.ThrowIfCancellationRequested();

                                bool[] resExists = await game.network.ResourceExistsBatchAsync(resList.ToArray());
                                if (resExists.Contains(false))
                                {
                                    uploadPools.Add(roomPool);
                                    startPlan = true;
                                }
                            }
                            // 该卡池无需上传，已就绪。
                            uploadedPools.Add(id);
                        }

                        token.ThrowIfCancellationRequested();
                        // 下载卡池。
                        List<ResourcePlan> downloadResources = new List<ResourcePlan>();
                        // 如果本地不存在该卡池，加入到下载列表中。
                        if (!hasLocalPool)
                        {
                            downloadPools.Add(roomPool);
                            startPlan = true;
                        }
                        // 如果本地存在该卡池，检测卡图下载。
                        else if (localCardPool != null)
                        {
                            // 遍历卡池内的所有卡牌皮肤，如果有不存在的卡图，下载该卡图。
                            var picturePaths = GetCardPoolPictures(poolRef);
                            foreach (var path in picturePaths)
                            {
                                token.ThrowIfCancellationRequested();
                                string picId = Path.GetFileNameWithoutExtension(path);
                                if (!string.IsNullOrEmpty(picId) &&
                                    !game.cache.FileExists(game.fileManager, ResourceType.Picture, picId) &&
                                    !game.card.isExternalImageExists(path))
                                {
                                    downloadPools.Add(roomPool);
                                    startPlan = true;
                                    break;
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException e)
                    {
                        // 取消事件丢给下面处理。
                        throw e;
                    }
                    catch (Exception e)
                    {
                        logger.logError($"检查房间卡池时出现异常：{e}");
                    }
                }
                AddUploadPools(uploadPools.ToArray());
                AddDownloadPools(downloadPools.ToArray());
                ChangeLoadState(PlayerLoadState.None);
                // 开始执行计划。
                if (startPlan)
                {
                    _cts = null;
                    await StartPlans();
                }
                LoadRoomCardPoolsFromCache(cardPools);
            }
            catch (OperationCanceledException)
            {
                logger.logError($"取消检查卡池。");
                _cts = null;
            }

        }
        /// <summary>
        /// 添加卡池上传计划。
        /// </summary>
        /// <param name="pools">要添加的卡池信息包。</param>
        public void AddUploadPools(params CardPoolInfoPack[] pools)
        {
            foreach (var pool in pools)
            {
                if (IsCardPoolInPlan(pool.cardPoolId, pool.version))
                {
                    continue;
                }
                var cardPoolPlan = new CardPoolFilePlan(pool);

                var plans = new List<ResourcePlan>
                {
                    cardPoolPlan
                };
                var picPlans = GetCardPoolPicturePlans(pool.GetReference());
                foreach (var p in picPlans)
                {
                    p.size = GetPlanFileSize(p);
                    plans.Add(p);
                }
                var plan = new CardPoolPlan(pool, plans);
                uploadPlans.Add(plan);
            }
        }

        /// <summary>
        /// 添加卡池下载计划。
        /// </summary>
        /// <param name="pools">要添加的卡池信息包。</param>
        public void AddDownloadPools(params CardPoolInfoPack[] pools)
        {
            foreach (var pool in pools)
            {
                var cardPoolPlan = new CardPoolFilePlan(pool);
                cardPoolPlan.size = GetPlanFileSize(cardPoolPlan);
                // 因为卡池还未下载，要下载的图片未知，所以为null。
                var plan = new CardPoolPlan(pool, cardPoolPlan);
                downloadPlans.Add(plan);
            }
        }
        /// <summary>
        /// 取消正在进行的传输计划。
        /// </summary>
        public void Cancel()
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }
        }
        /// <summary>
        /// 重置所有数据，包括所有传输计划。
        /// </summary>
        public void Reset()
        {
            Clear();
            uploadPlans.Clear();
            downloadPlans.Clear();
            uploadedPools.Clear();
        }
        /// <summary>
        /// 清除状态和进度。
        /// </summary>
        public void Clear()
        {
            ChangeLoadState(PlayerLoadState.None);
            ChangeProgress(0);
            _cts = null;
            running = false;
        }
        /// <summary>
        /// 开始所有计划。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="TaskNotCompleteException">重试次数过多，传输卡池失败。</exception>
        public async Task StartPlans()
        {
            if (state != PlayerLoadState.None)
                return;

            running = true;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                const int MAX_RETRY_COUNT = 16;
                int retryCounts = 0;
                var uploadList = uploadPlans.Where(p => !p.completed);
                if (uploadList.Count() > 0)
                {
                    int retryMaxTime = retryCooldown;
                    ChangeLoadState(PlayerLoadState.Upload);
                    do
                    {
                        bool allSuccess = true;

                        _totalSize = uploadList.Sum(plan => plan.resPlans.Where(p => !p.completed).Sum(p => p.size));
                        _transmitedSize = 0;

                        foreach (var plan in uploadList)
                        {
                            token.ThrowIfCancellationRequested();
                            bool success = await UploadCardPoolPlan(plan);
                            if (!success)
                            {
                                allSuccess = false;
                            }
                            else
                            {
                                onUploadCompleted?.Invoke(plan);
                            }
                        }
                        if (!allSuccess)
                        {
                            retryCounts++;
                            ChangeProgress(0);
                            if (retryCounts > MAX_RETRY_COUNT)
                            {
                                throw new TaskNotCompleteException("重试次数过多，上传卡池失败。");
                            }
                            await Task.Delay(retryMaxTime);
                        }
                        var newUploadList = uploadPlans.Where(p => !p.completed);
                        bool hasNewFileUploaded = newUploadList.Count() < uploadList.Count();
                        if (!hasNewFileUploaded)
                        {
                            retryMaxTime = Math.Min(16000, retryMaxTime * 2);
                        }
                        uploadList = newUploadList;
                    }
                    while (uploadList.Count() > 0);

                    ChangeLoadState(PlayerLoadState.None);
                }


                token.ThrowIfCancellationRequested();

                retryCounts = 0;
                var downloadList = downloadPlans.Where(p => !p.completed);
                if (downloadList.Count() > 0)
                {
                    int retryMaxTime = retryCooldown;
                    ChangeLoadState(PlayerLoadState.Download);
                    do
                    {
                        bool allSuccess = true;


                        // 首先下载并读取卡池文件。
                        foreach (var plan in downloadList)
                        {
                            token.ThrowIfCancellationRequested();

                            bool poolLoaded = await DownloadPlanCardPool(plan);
                        }
                        // 下载卡池的时候，卡图还没有下载到本地，没有大小
                        // 所以进度条只表示下载的资源数量。
                        _totalSize = downloadList.Sum(plan => plan.resPlans.Where(p => !p.completed).Count());
                        _transmitedSize = 0;

                        foreach (var plan in downloadList)
                        {
                            token.ThrowIfCancellationRequested();
                            // 如果没有卡池的缓存文件，则不读取卡图。
                            if (game.cache.FileExists(game.fileManager, ResourceType.CardDefine, plan.poolPack.GetResourceID()))
                            {
                                bool success = await DownloadPlan(plan);
                                if (success)
                                {
                                    onDownloadCompleted?.Invoke(plan);
                                }
                                else
                                {
                                    allSuccess = false;
                                }
                            }
                        }

                        if (!allSuccess)
                        {
                            retryCounts++;
                            ChangeProgress(0);
                            if (retryCounts > MAX_RETRY_COUNT)
                            {
                                throw new TaskNotCompleteException("重试次数过多，下载卡池失败。");
                            }
                            await Task.Delay(retryMaxTime);
                        }
                        var newDownloadList = downloadPlans.Where(p => !p.completed);
                        bool hasNewFileDownloaded = newDownloadList.Count() < downloadList.Count();
                        if (!hasNewFileDownloaded)
                        {
                            retryMaxTime = Math.Min(16000, retryMaxTime * 2);
                        }
                        downloadList = newDownloadList;
                    }
                    while (downloadList.Count() > 0);

                    ChangeLoadState(PlayerLoadState.None);
                }
            }
            catch (OperationCanceledException)
            {
                logger.logWarn("卡池传输已取消。");
            }
            catch (TaskNotCompleteException e)
            {
                logger.logError(e.ToString());
                onOutOfRetry?.Invoke(e);
            }
            finally
            {
                Clear();
            };
        }
        public bool IsCardPoolInPlan(long cardPoolId, int version)
        {
            return uploadPlans.Any(p => p.poolPack.cardPoolId == cardPoolId && p.poolPack.version == version) ||
                downloadPlans.Any(p => p.poolPack.cardPoolId == cardPoolId && p.poolPack.version == version);
        }
        public async void LoadRoomCardPoolsFromCache(CardPoolInfoPack[] pools)
        {
            foreach (var cardPool in pools)
            {
                CardPoolInfo info = game.card.getCardPoolInfo(cardPool);
                if (info == null)
                {
                    await game.card.loadCachedCardDefinesAsync(cardPool.cardPoolId, cardPool.version);
                }
            }
        }
        #endregion
        #region 构造器
        public RoomCardPoolManager(MainManager main)
        {
            game = main;
            logger = new ULogger("RoomCardPool");
        }
        #endregion
        #region 私有方法
        /// <summary>
        /// 获取卡池内的所有图片路径。
        /// </summary>
        /// <param name="cardPoolRef">卡池引用。</param>
        /// <returns>图片路径数组。</returns>
        private string[] GetCardPoolPictures(CardPoolReference cardPoolRef)
        {
            List<string> paths = new List<string>();
            if (game.card.tryGetCardPool(cardPoolRef, out var cardPool))
            {
                foreach (var skin in cardPool.cardSkinList)
                {
                    if (string.IsNullOrEmpty(skin.imagePath))
                    {
                        continue;
                    }
                    paths.Add(skin.imagePath);
                }
                foreach (var skin in cardPool.buffSkinList)
                {
                    if (string.IsNullOrEmpty(skin.imagePath))
                    {
                        continue;
                    }
                    paths.Add(skin.imagePath);
                }
            }
            return paths.ToArray();
        }
        /// <summary>
        /// 获取卡池内的所有图片，并转换为图片资源计划。
        /// </summary>
        /// <param name="cardPoolId">卡池引用。</param>
        /// <returns>图片资源计划数组。</returns>
        private PictureFilePlan[] GetCardPoolPicturePlans(CardPoolReference cardPoolRef)
        {
            var paths = GetCardPoolPictures(cardPoolRef);
            return paths.Select(p =>
            {
                var plan = new PictureFilePlan(p);
                plan.size = GetPlanFileSize(plan);
                return plan;
            }).ToArray();
        }
        /// <summary>
        /// 改变正在进行的传输状态。
        /// </summary>
        /// <param name="state">传输状态</param>
        private void ChangeLoadState(PlayerLoadState state)
        {
            if (this.state != state)
            {
                this.state = state;
                onLoadStateChanged?.Invoke(state);
            }
        }
        /// <summary>
        /// 改变正在进行的传输进度。
        /// </summary>
        /// <param name="value">传输进度</param>
        private void ChangeProgress(float value)
        {
            value = Math.Min(1, value);
            _progress = value;
            onProgressChanged?.Invoke(value);
        }
        /// <summary>
        /// 获取资源计划的文件大小。
        /// </summary>
        /// <param name="plan">资源计划。</param>
        /// <returns>文件大小。</returns>
        private long GetPlanFileSize(ResourcePlan plan)
        {
            if (game.cache.GetResourceInfo(plan.type, plan.id, out long size))
            {
                return size;
            }
            return 0;
        }
        /// <summary>
        /// 上传一个资源计划。
        /// </summary>
        /// <param name="plan">资源计划。</param>
        /// <returns></returns>
        private async Task UploadResourcePlan(ResourcePlan plan)
        {
            try
            {
                byte[] bytes = await plan.GetBytes(game);
                await game.network.UploadResourceAsync(plan.type, plan.id, bytes);
                plan.completed = true;
            }
            catch (FileNotFoundException)
            {
                logger.logWarn($"无法找到要上传的资源：{plan}");
            }
            catch (Exception e)
            {
                logger.logError($"上传资源{plan}时遇到异常：" + e.ToString());
            }
        }
        /// <summary>
        /// 下载一个资源计划。
        /// </summary>
        /// <param name="plan">资源计划。</param>
        /// <returns>下载得出的结果。</returns>
        private async Task<byte[]> DownloadResourcePlan(ResourcePlan plan)
        {
            try
            {
                byte[] bytes = await game.network.GetResourceAsync(plan.type, plan.id);
                plan.completed = true;
                return bytes;
            }
            catch (FileNotFoundException)
            {
                logger.logWarn($"无法找到要下载的资源：{plan}");
            }
            catch (Exception e)
            {
                logger.logError($"下载资源{plan}时遇到异常：" + e.ToString());
            }
            return null;
        }

        /// <summary>
        /// 上传一个卡池计划。
        /// </summary>
        /// <param name="plan">卡池计划。</param>
        /// <returns>是否上传成功。</returns>
        private async Task<bool> UploadCardPoolPlan(CardPoolPlan plan)
        {
            // 局域网房主不上传卡池。
            if (game.network.logic.isLanHost)
            {
                plan.completed = true;
                foreach (var resPlan in plan.resPlans)
                {
                    resPlan.completed = true;
                }
                return true;
            }
            // 获取资源的大小，以评估上传的进度。
            var notCompleted = plan.resPlans.Where(p => !p.completed);

            // 上传这些ResourcePlan。
            foreach (var resPlan in notCompleted)
            {
                _cts.Token.ThrowIfCancellationRequested();

                await UploadResourcePlan(resPlan);
                _transmitedSize += resPlan.size;
                if (_totalSize > 0)
                    ChangeProgress((float)((double)_transmitedSize / _totalSize));
            }

            // 如果所有ResourcePlan都已完成，将该卡池计划设置为完成。
            if (!notCompleted.Any(p => !p.completed))
            {
                plan.completed = true;
                return true;
            }
            return false;
        }

        private async Task<bool> DownloadPlanCardPool(CardPoolPlan plan)
        {
            var poolRef = plan.poolPack.GetReference();
            // 先下载计划中的卡池文件资源。
            if (plan.resPlans.Count <= 0)
                return false;
            var resPlan = plan.resPlans[0];
            _cts.Token.ThrowIfCancellationRequested();

            byte[] bytes = await DownloadResourcePlan(resPlan);
            // 字节数组为空，下载失败。
            if (bytes == null)
                return false;
            // 将资源保存为缓存。
            await game.cache.SaveCache(game.fileManager, resPlan.type, resPlan.id, bytes);

            // 如果有任何未完成的ResourcePlan，视为下载失败。
            //if (plan.resPlans.Any(p => !p.completed))
            //    return false;


            // 缓存保存后，读取该卡池。
            var cardPoolData = await game.card.loadCachedCardPoolAsync(poolRef.cardPoolId, poolRef.version);
            CardPool cardPool = cardPoolData.cardPool;
            // 如果获得的卡池对象是null，返回false，下载失败。
            if (cardPool == null)
            {
                return false;
            }
            // 获取该卡池的所有卡图和buff图片id，添加ResourcePlan。
            var plans = GetCardPoolPicturePlans(poolRef).Where(p => !plan.resPlans.Any(rp => rp.id == p.id));
            plan.resPlans.AddRange(plans);
            return true;
        }

        /// <summary>
        /// 下载一个卡池计划。
        /// </summary>
        /// <param name="plan">卡池计划。</param>
        /// <returns>是否下载成功。</returns>
        private async Task<bool> DownloadPlan(CardPoolPlan plan)
        {
            // 获取资源的大小，以评估下载的进度。
            var notCompleted = plan.resPlans.Where(p => !p.completed);

            // 下载这些ResourcePlan。
            foreach (var resPlan in notCompleted)
            {
                _cts.Token.ThrowIfCancellationRequested();

                byte[] downloaded = await DownloadResourcePlan(resPlan);
                if (downloaded != null)
                    await game.cache.SaveCache(game.fileManager, resPlan.type, resPlan.id, downloaded);
                // 进度条只表示下载的资源数量。
                _transmitedSize++;
                if (_totalSize > 0)
                    ChangeProgress((float)((double)_transmitedSize / _totalSize));
            }


            // 如果所有计划都已完成，将该卡池计划设置为完成。
            if (!notCompleted.Any(p => !p.completed))
            {
                plan.completed = true;
                return true;
            }
            return false;
        }
        #endregion
        #region 事件
        public event Action<float> onProgressChanged;
        public event Action<PlayerLoadState> onLoadStateChanged;
        public event Action<CardPoolPlan> onUploadCompleted;
        public event Action<CardPoolPlan> onDownloadCompleted;
        public event Action<Exception> onOutOfRetry;
        #endregion
        #region 字段属性
        private MainManager game;
        private ILogger logger;
        public bool running { get; private set; }
        /// <summary>
        /// 重试传输的冷却时间（毫秒）。
        /// </summary>
        public int retryCooldown = 1000;
        /// <summary>
        /// 所有卡池的上传计划。
        /// </summary>
        private List<CardPoolPlan> uploadPlans = new List<CardPoolPlan>();
        /// <summary>
        /// 所有卡池的下载计划。
        /// </summary>
        private List<CardPoolPlan> downloadPlans = new List<CardPoolPlan>();
        /// <summary>
        /// 上传完毕，且卡图也完全上传完毕的卡池列表。
        /// 用于防止再次进行大量远端资源的判断，浪费时间。
        /// </summary>
        private HashSet<string> uploadedPools = new HashSet<string>();
        /// <summary>
        /// 卡池传输的进度。
        /// </summary>
        private float _progress;
        public float progress => _progress;
        /// <summary>
        /// 当前传输中已传输的文件大小。
        /// </summary>
        private long _transmitedSize;
        /// <summary>
        /// 当前传输中需要传输的文件总大小。
        /// </summary>
        private long _totalSize;
        /// <summary>
        /// 取消计划所用的标记。
        /// </summary>
        private CancellationTokenSource _cts;
        /// <summary>
        /// 卡池传输的状态。
        /// </summary>
        public PlayerLoadState state { get; private set; }
        #endregion
    }

    /// <summary>
    /// 卡池计划类，负责整个卡池的上传/下载，包括卡池文件、卡图和buff图片。
    /// </summary>
    public class CardPoolPlan
    {
        public CardPoolInfoPack poolPack;
        public List<ResourcePlan> resPlans = new List<ResourcePlan>();
        public bool completed;
        public CardPoolPlan(CardPoolInfoPack pool, IEnumerable<ResourcePlan> plans)
        {
            poolPack = pool;
            resPlans.AddRange(plans);
        }
        public CardPoolPlan(CardPoolInfoPack pool, params ResourcePlan[] plans)
        {
            poolPack = pool;
            resPlans.AddRange(plans);
        }
    }
    /// <summary>
    /// 传输单个资源的计划类，负责单个资源的上传、下载。
    /// </summary>
    public abstract class ResourcePlan
    {
        public abstract ResourceType type { get; }
        public abstract string id { get; }
        public long size { get; set; }
        public bool completed { get; set; }
        public abstract Task<byte[]> GetBytes(MainManager game);
        public override string ToString()
        {
            return $"[{type}]{id}({size} bytes)";
        }
    }

    /// <summary>
    /// 传输卡池文件的计划类。
    /// </summary>
    public class CardPoolFilePlan : ResourcePlan
    {
        public CardPoolInfoPack poolPack;
        public override ResourceType type => ResourceType.CardDefine;
        public override string id => poolPack.GetResourceID();

        public CardPoolFilePlan(CardPoolInfoPack pool)
        {
            poolPack = pool;
        }
        public async override Task<byte[]> GetBytes(MainManager game)
        {
            return await game.fileManager.readBinaryFile(game.card.getCardPoolPath(poolPack.GetReference()));
        }
    }
    /// <summary>
    /// 传输卡池图片的计划类。
    /// </summary>
    public class PictureFilePlan : ResourcePlan
    {
        public string path;
        public override ResourceType type => ResourceType.Picture;
        public override string id => Path.GetFileNameWithoutExtension(path);

        public PictureFilePlan(string path)
        {
            this.path = path;
        }
        public async override Task<byte[]> GetBytes(MainManager game)
        {
            if (path == null)
                throw new FileNotFoundException();

            var fileManager = game.fileManager;
            var picPath = Path.Combine(game.card.textureDirName, path);
            bool isFullPath = Path.GetFullPath(path) == picPath;
            if (isFullPath && fileManager.isFileExist(picPath))
            {
                //如果直接给出完整路径就直接从完整路径上读
                return await fileManager.readBinaryFile(picPath);
            }
            else if (fileManager.isPersistentFileExist(picPath))
            {
                // 从外部图片路径读取图片。
                return await fileManager.readPersistentBinaryFile(picPath);
            }
            throw new FileNotFoundException();
        }
    }
}