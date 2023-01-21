using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using UnityEngine;
using System;
using TouhouHeartstone;
using System.Numerics;
using System.Threading;

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
                var picPlans = new List<ResourcePlan>();
                if (game.card.tryGetCardPool(pool.cardPoolId, out var cardPool))
                {
                    foreach (var skin in cardPool.cardSkinList)
                    {
                        if (string.IsNullOrEmpty(skin.imagePath))
                        {
                            continue;
                        }
                        var picPlan = new PictureFilePlan(pool.cardPoolId, skin.imagePath);
                        picPlan.size = GetPlanFileSize(picPlan);
                        picPlans.Add(picPlan);
                    }
                    foreach (var skin in cardPool.buffSkinList)
                    {
                        if (string.IsNullOrEmpty(skin.imagePath))
                        {
                            continue;
                        }
                        var picPlan = new PictureFilePlan(pool.cardPoolId, skin.imagePath);
                        picPlans.Add(picPlan);
                    }
                }
                var plan = new CardPoolPlan(cardPoolPlan, picPlans.ToArray())
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
                var plan = new CardPoolPlan(cardPoolPlan, null)
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
                    do
                    {
                        foreach (var plan in uploadPlans)
                        {
                            token.ThrowIfCancellationRequested();
                            ChangeLoadState(PlayerLoadState.Upload);
                            await UploadCardPoolPlan(plan);
                        }
                        uploadPlans.RemoveAll(p => p.completed);
                    }
                    while (uploadPlans.Count > 0);
                    onUploadCompleted?.Invoke();


                    token.ThrowIfCancellationRequested();
                    do
                    {
                        foreach (var plan in downloadPlans)
                        {
                            token.ThrowIfCancellationRequested();
                            ChangeLoadState(PlayerLoadState.Upload);
                            await DownloadPlan(plan);
                        }
                        downloadPlans.RemoveAll(p => p.completed);
                    }
                    while (downloadPlans.Count > 0);
                    onDownloadCompleted?.Invoke();
                }
                catch (OperationCanceledException e)
                {
                    Debug.LogWarning("卡池传输已取消。");
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
                await game.network.UploadResourceAsync(plan.type, plan.id, bytes);
                plan.completed = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
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
                Debug.LogException(e);
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
            // 先评估卡池文件，以及所有要上传的卡图和buff图片。
            List<ResourcePlan> plans = new List<ResourcePlan>();
            if (plan.cardPoolPlan != null && !plan.cardPoolPlan.completed)
            {
                plans.Add(plan.cardPoolPlan);
            }
            plans.AddRange(plan.picturePlans.Where(p => !p.completed));

            // 获取资源的大小，以评估上传的进度。
            long totalSize = plans.Sum(p => p.size);
            long uploadedSize = 0;

            // 上传这些ResourcePlan。
            foreach (var resPlan in plans)
            {
                await UploadResourcePlan(resPlan);
                uploadedSize += resPlan.size;
                ChangeProgress((float)((double)uploadedSize / totalSize));
            }

            // 如果所有计划都已完成，将该卡池计划设置为完成。
            if (plans.All(p => p.completed))
            {
                plan.completed = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 下载一个卡池计划。
        /// </summary>
        /// <param name="plan">卡池计划。</param>
        /// <returns>是否下载成功。</returns>
        private async Task<bool> DownloadPlan(CardPoolPlan plan)
        {
            long poolId = plan.poolPack.cardPoolId;
            // 先下载卡池文件。
            byte[] cardPoolBytes = null;
            if (plan.cardPoolPlan != null && !plan.cardPoolPlan.completed)
            {
                cardPoolBytes = await DownloadResourcePlan(plan.cardPoolPlan);
                // 卡池字节为空，下载失败。
                if (cardPoolBytes == null)
                {
                    return false;
                }
                // 将卡池文件保存为缓存。
                game.cache.SaveCache(game.fileManager, plan.cardPoolPlan.type, plan.cardPoolPlan.id, cardPoolBytes);
            }
            // 缓存保存后，如果该卡池计划的图片计划为空，读取该卡池。
            if (plan.picturePlans == null)
            {
                var cardPoolData = game.card.loadCachedCardPoolAsync(poolId, plan.poolPack.version);
                CardPool cardPool = cardPoolData.cardPool;
                // 如果获得的卡池对象是null，返回false，下载失败。
                if (cardPool == null)
                {
                    return false;
                }
                // 获取该卡池的所有卡图和buff图片id，建立ResourcePlan列表。
                List<PictureFilePlan> picturePlans = new List<PictureFilePlan>();
                foreach (var skin in cardPool.cardSkinList)
                {
                    if (string.IsNullOrEmpty(skin.imagePath))
                        continue;
                    var picPlan = new PictureFilePlan(poolId, skin.imagePath);
                    picPlan.size = GetPlanFileSize(picPlan);
                    picturePlans.Add(picPlan);
                }
                foreach (var skin in cardPool.buffSkinList)
                {
                    if (string.IsNullOrEmpty(skin.imagePath))
                        continue;
                    var picPlan = new PictureFilePlan(poolId, skin.imagePath);
                    picPlan.size = GetPlanFileSize(picPlan);
                    picturePlans.Add(picPlan);
                }
                plan.picturePlans = picturePlans.ToArray();
            }

            // 获取资源的大小，以评估下载的进度。
            List<ResourcePlan> plans = new List<ResourcePlan>();
            plans.AddRange(plan.picturePlans.Where(p => !p.completed));
            long totalSize = plan.cardPoolPlan.size + plans.Sum(p => p.size);
            long uploadedSize = plan.cardPoolPlan.size;

            // 下载这些ResourcePlan。
            foreach (var resPlan in plans)
            {
                byte[] downloaded = await DownloadPlan(resPlan);
                await game.cache.SaveCache(game.fileManager, resPlan.type, resPlan.id, downloaded);
                uploadedSize += resPlan.size;
                ChangeProgress((float)((double)uploadedSize / totalSize));
            }

            // 如果所有计划都已完成，将该卡池计划设置为完成。
            if (plans.All(p => p.completed))
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
        #endregion
        #region 字段属性
        private MainManager game;
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
        public CardPoolFilePlan cardPoolPlan;
        public PictureFilePlan[] picturePlans;
        public bool completed;
        public CardPoolPlan(CardPoolFilePlan cardPool, PictureFilePlan[] pictures)
        {
            cardPoolPlan = cardPool;
            picturePlans = pictures;
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
        public long cardPoolId;
        public string path;
        public override ResourceType type => ResourceType.Picture;
        public override string id => Path.GetFileNameWithoutExtension(path);

        public PictureFilePlan(long poolId, string path)
        {
            cardPoolId = poolId;
            this.path = path;
        }
        public async override Task<byte[]> GetBytes(MainManager game)
        {
            if (path == null)
                return null;

            // 读取皮肤的图像字节数组。
            Sprite spr = await game.card.loadImage(path, cardPoolId);
            if (spr == null)
                return null;

            return spr.texture.EncodeToJPG();
        }
    }
}