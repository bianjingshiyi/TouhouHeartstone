using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouHeartstone;
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
        /// 添加卡池上传计划。
        /// </summary>
        /// <param name="pools">要添加的卡池信息包。</param>
        public void AddUploadPlans(params CardPoolInfoPack[] pools)
        {
            foreach (var pool in pools)
            {
                var cardPoolPlan = new CardPoolFilePlan(pool);
                cardPoolPlan.size = GetPlanFileSize(cardPoolPlan);

                var plans = new List<ResourcePlan>();
                plans.Add(cardPoolPlan);
                plans.AddRange(GetCardPoolPictures(pool.cardPoolId));
                var plan = new CardPoolPlan(plans.ToArray())
                {
                    poolPack = pool
                };
                uploadPlans.Add(plan);
            }
        }

        /// <summary>
        /// 添加卡池下载计划。
        /// </summary>
        /// <param name="pools">要添加的卡池信息包。</param>
        public void AddDownloadPlans(params CardPoolInfoPack[] pools)
        {
            foreach (var pool in pools)
            {
                var cardPoolPlan = new CardPoolFilePlan(pool);
                cardPoolPlan.size = GetPlanFileSize(cardPoolPlan);
                // 因为卡池还未下载，要下载的图片未知，所以为null。
                var plan = new CardPoolPlan(cardPoolPlan)
                {
                    poolPack = pool
                };
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
        /// 重置所有传输计划，状态和进度。
        /// </summary>
        public void Reset()
        {
            uploadPlans.Clear();
            downloadPlans.Clear();
            ChangeLoadState(PlayerLoadState.Upload);
            ChangeProgress(0);
            _cts = null;
        }
        /// <summary>
        /// 开始所有计划。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="TaskNotCompleteException">重试次数过多，传输卡池失败。</exception>
        public Task StartPlans()
        {
            if (state != PlayerLoadState.None)
            {
                return Task.CompletedTask;
            }
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            Task task = Task.Run(async () =>
            {
                try
                {
                    const int MAX_RETRY_COUNT = 16;
                    int retryCounts = 0;
                    do
                    {
                        foreach (var plan in uploadPlans)
                        {
                            token.ThrowIfCancellationRequested();
                            ChangeLoadState(PlayerLoadState.Upload);
                            await UploadCardPoolPlan(plan);
                        }
                        uploadPlans.RemoveAll(p => p.completed);

                        retryCounts++;
                        if (retryCounts > MAX_RETRY_COUNT)
                        {
                            throw new TaskNotCompleteException("重试次数过多，上传卡池失败。");
                        }
                        ChangeLoadState(PlayerLoadState.None);
                        await Task.Delay(retryCooldown);
                    }
                    while (uploadPlans.Count > 0);
                    onUploadCompleted?.Invoke();


                    token.ThrowIfCancellationRequested();

                    retryCounts = 0;
                    do
                    {
                        foreach (var plan in downloadPlans)
                        {
                            token.ThrowIfCancellationRequested();
                            ChangeLoadState(PlayerLoadState.Download);

                            bool loadPlan = true;
                            // 如果没有下载并加载卡池。
                            if (!game.card.tryGetCardPool(plan.poolPack.cardPoolId, out _))
                            {
                                // 首先下载并读取卡池。
                                bool poolLoaded = await DownloadPlanCardPool(plan);
                                // 如果卡池读取失败，则不读取卡图。
                                if (!poolLoaded)
                                {
                                    loadPlan = false;
                                }
                            }
                            if (loadPlan)
                            {
                                await DownloadPlan(plan);
                            }
                        }
                        downloadPlans.RemoveAll(p => p.completed);

                        retryCounts++;
                        if (retryCounts > MAX_RETRY_COUNT)
                        {
                            throw new TaskNotCompleteException("重试次数过多，下载卡池失败。");
                        }
                        ChangeLoadState(PlayerLoadState.None);
                        await Task.Delay(retryCooldown);
                    }
                    while (downloadPlans.Count > 0);
                    onDownloadCompleted?.Invoke();
                }
                catch (OperationCanceledException e)
                {
                    logger.logWarn("卡池传输已取消。");
                }
                catch (TaskNotCompleteException e)
                {
                    logger.logError(e.ToString());
                    throw e;
                }
                finally
                {
                    Reset();
                }
            }, token);
            return task;
        }
        #endregion
        #region 构造器
        public RoomCardPoolManager(MainManager main)
        {
            game = main;
        }
        #endregion
        #region 私有方法
        /// <summary>
        /// 获取卡池内的所有图片，并转换为图片资源计划。
        /// </summary>
        /// <param name="cardPoolId">卡池ID。</param>
        /// <returns>图片资源计划数组。</returns>
        private PictureFilePlan[] GetCardPoolPictures(long cardPoolId)
        {
            List<PictureFilePlan> pictures = new List<PictureFilePlan>();
            if (game.card.tryGetCardPool(cardPoolId, out var cardPool))
            {
                foreach (var skin in cardPool.cardSkinList)
                {
                    if (string.IsNullOrEmpty(skin.imagePath))
                    {
                        continue;
                    }
                    var picPlan = new PictureFilePlan(skin.imagePath);
                    picPlan.size = GetPlanFileSize(picPlan);
                    pictures.Add(picPlan);
                }
                foreach (var skin in cardPool.buffSkinList)
                {
                    if (string.IsNullOrEmpty(skin.imagePath))
                    {
                        continue;
                    }
                    var picPlan = new PictureFilePlan(skin.imagePath);
                    picPlan.size = GetPlanFileSize(picPlan);
                    pictures.Add(picPlan);
                }
            }
            return pictures.ToArray();
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
            onProgressChanged(value);
        }
        /// <summary>
        /// 获取资源计划的文件大小。
        /// </summary>
        /// <param name="plan">资源计划。</param>
        /// <returns>文件大小。</returns>
        private long GetPlanFileSize(ResourcePlan plan)
        {
            string path = game.cache.GetCachePath(plan.type, plan.id);
            if (game.fileManager.isFileExist(path))
            {
                return new FileInfo(path).Length;
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
                if (bytes == null)
                    return;
                await game.network.UploadResourceAsync(plan.type, plan.id, bytes);
                plan.completed = true;
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
            // 获取资源的大小，以评估上传的进度。
            var notCompleted = plan.resPlans.Where(p => !p.completed);
            long totalSize = plan.resPlans.Sum(p => p.size);
            long uploadedSize = 0;

            // 上传这些ResourcePlan。
            foreach (var resPlan in notCompleted)
            {
                await UploadResourcePlan(resPlan);
                uploadedSize += resPlan.size;
                ChangeProgress((float)((double)uploadedSize / totalSize));
            }

            // 如果所有ResourcePlan都已完成，将该卡池计划设置为完成。
            if (notCompleted.All(p => p.completed))
            {
                plan.completed = true;
                return true;
            }
            return false;
        }

        private async Task<bool> DownloadPlanCardPool(CardPoolPlan plan)
        {
            long poolId = plan.poolPack.cardPoolId;
            // 先下载计划中的卡池文件资源。
            foreach (var resPlan in plan.resPlans.Where(p => !p.completed))
            {
                byte[] bytes = await DownloadResourcePlan(resPlan);
                // 字节数组为空，下载失败。
                if (bytes == null)
                {
                    continue;
                }
                // 将资源保存为缓存。
                game.cache.SaveCache(game.fileManager, resPlan.type, resPlan.id, bytes);
            }

            // 如果有任何未完成的ResourcePlan，视为下载失败。
            if (plan.resPlans.Any(p => !p.completed))
            {
                return false;
            }


            // 缓存保存后，读取该卡池。
            var cardPoolData = await game.card.loadCachedCardPoolAsync(poolId, plan.poolPack.version);
            CardPool cardPool = cardPoolData.cardPool;
            // 如果获得的卡池对象是null，返回false，下载失败。
            if (cardPool == null)
            {
                return false;
            }
            // 获取该卡池的所有卡图和buff图片id，添加ResourcePlan。
            plan.resPlans.AddRange(GetCardPoolPictures(poolId));
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
            long totalSize = notCompleted.Sum(p => p.size);
            long downloadedSize = 0;

            // 下载这些ResourcePlan。
            foreach (var resPlan in notCompleted)
            {
                byte[] downloaded = await DownloadResourcePlan(resPlan);
                if (downloaded != null)
                    await game.cache.SaveCache(game.fileManager, resPlan.type, resPlan.id, downloaded);
                downloadedSize += resPlan.size;
                ChangeProgress((float)((double)downloadedSize / totalSize));
            }

            // 如果所有计划都已完成，将该卡池计划设置为完成。
            if (notCompleted.All(p => p.completed))
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
        public event Action onUploadCompleted;
        public event Action onDownloadCompleted;

        #endregion
        #region 字段属性
        private MainManager game;
        private ILogger logger;
        /// <summary>
        /// 重试传输的冷却时间（秒）。
        /// </summary>
        public float retryCooldown;
        /// <summary>
        /// 所有卡池的上传计划。
        /// </summary>
        private List<CardPoolPlan> uploadPlans = new List<CardPoolPlan>();
        /// <summary>
        /// 所有卡池的下载计划。
        /// </summary>
        private List<CardPoolPlan> downloadPlans = new List<CardPoolPlan>();
        /// <summary>
        /// 卡池传输的进度。
        /// </summary>
        private float _progress;
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
        public CardPoolPlan(params ResourcePlan[] plans)
        {
            this.resPlans.AddRange(plans);
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
            return await game.fileManager.readBinaryFile(game.card.getCardPoolPath(poolPack.cardPoolId));
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
                return null;

            var fileManager = game.fileManager;
            bool fullPath = Path.GetFullPath(path) == path;
            if (fullPath && fileManager.isFileExist(path))
            {
                //如果直接给出完整路径就直接从完整路径上读
                return await fileManager.readBinaryFile(path);
            }
            else if (fileManager.isPersistentFileExist(path))
            {
                // 从外部图片路径读取图片。
                return await fileManager.readPersistentBinaryFile(path);
            }
            return null;
        }
    }
}