using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using Controllers;
using TouhouCardEngine;
using UI;
using UnityEngine;
using FileItem = System.Tuple<string, long>;

namespace Game
{
    /// <summary>
    /// 用于管理本地的卡池、图片缓存的管理器
    /// 同时也作为局域网的资源提供器，提供卡池、图片等资源的下载。
    /// </summary>
    public class CacheManager : Manager, IResourceProvider
    {
        #region 公共方法
        /// <summary>
        /// 根据资源类型字符串和资源ID，获取缓存文件路径。
        /// </summary>
        /// <param name="type">资源类型。</param>
        /// <param name="id">资源ID。</param>
        /// <returns>缓存文件路径。</returns>
        public string GetCachePath(string type, string id)
        {
            return Path.Combine(CacheDir, type, id);
        }
        /// <summary>
        /// 根据资源类型和资源ID，获取缓存文件路径。
        /// </summary>
        /// <param name="type">资源类型。</param>
        /// <param name="id">资源ID。</param>
        /// <returns>缓存文件路径。</returns>
        public string GetCachePath(TouhouCardEngine.ResourceType type, string id)
        {
            return GetCachePath(type.GetString(), id);
        }
        /// <summary>
        /// 获取缓存文件是否存在。
        /// </summary>
        /// <param name="type">资源类型。</param>
        /// <param name="id">资源ID。</param>
        /// <returns>缓存文件是否存在。</returns>
        public bool FileExists(FileManager fileManager, TouhouCardEngine.ResourceType type, string id)
        {
            return fileManager.isFileExist(GetCachePath(type, id));
        }
        /// <summary>
        /// 将字节数组保存为缓存文件资源。
        /// </summary>
        /// <param name="type">资源类型。</param>
        /// <param name="id">资源ID。</param>
        /// <param name="bytes">字节数组。</param>
        /// <returns></returns>
        public Task SaveCache(FileManager fileManager, TouhouCardEngine.ResourceType type, string id, byte[] bytes)
        {
            string path = GetCachePath(type, id);
            addLRU(path, bytes.LongLength);
            return fileManager.saveFile(path, bytes);
        }
        /// <summary>
        /// 从缓存文件资源中读取字节数组。
        /// </summary>
        /// <param name="type">资源类型。</param>
        /// <param name="id">资源ID。</param>
        /// <returns>读取出的字节数组。</returns>
        public Task<byte[]> ReadCache(FileManager fileManager, TouhouCardEngine.ResourceType type, string id)
        {
            string path = GetCachePath(type, id);
            visitLRU(path);
            return fileManager.readBinaryFile(path);
        }
        /// <summary>
        /// 清除所有缓存文件。
        /// </summary>
        public void ClearCache(FileManager fileManager)
        {
            foreach (var cache in cacheItems)
            {
                try
                {
                    fileManager.deleteFile(cache.Item1);
                }
                catch { }
            }
            rebuildLRU();
        }
        /// <summary>
        /// 获取资源类型是否有效。
        /// </summary>
        /// <param name="resType">资源类型。</param>
        /// <returns>是否有效。</returns>
        public bool ResourceIsValid(string resType)
        {
            switch (resType)
            {
                case "card":
                case "pic":
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 尝试将卡池资源ID拆分为卡池ID和卡池版本。
        /// </summary>
        /// <param name="id">资源ID。</param>
        /// <param name="cardPoolId">拆分出的卡池ID。</param>
        /// <param name="cardPoolVersion">拆分出的卡池版本。</param>
        /// <returns>拆分是否成功。</returns>
        public bool TryParseCardPool(string id, out long cardPoolId, out int cardPoolVersion)
        {
            cardPoolId = 0;
            cardPoolVersion = 0;
            int sepIndex = id.IndexOf('_');
            string cardPoolIdStr = id.Substring(0, sepIndex);
            string cardPoolVersionStr = id.Substring(sepIndex + 1);
            if (long.TryParse(cardPoolIdStr, out cardPoolId) && int.TryParse(cardPoolVersionStr, out cardPoolVersion))
            {
                return true;
            }
            return false;
        }
        public bool GetResourceInfo(TouhouCardEngine.ResourceType resType, string id, out long size)
        {
            return GetResourceInfo(resType.ToString(), id, out size);
        }
        public bool GetResourceInfo(string resType, string resID, out long size)
        {
            size = 0;
            if (!ResourceIsValid(resType))
            {
                return false;
            }

            // 查找本地的卡池。
            if (resType == "card")
            {
                if (TryParseCardPool(resID, out long cardPoolId, out int cardPoolVersion))
                {
                    var cardPoolInfo = card.getCardPoolInfo(cardPoolId);
                    if (cardPoolInfo != null && cardPoolInfo.editTime == cardPoolVersion)
                    {
                        string deckPath = card.getCardPoolPath(cardPoolInfo);
                        var deckFileInfo = new FileInfo(deckPath);
                        size = deckFileInfo.Length;
                        return true;
                    }
                }
            }
            else if (resType == "pic")
            {
                // 查找本地的图片资源。
                string fileName = string.Format("{0}.jpg", resID);
                var picPath = card.getExternalImagePath(fileName);
                if (File.Exists(picPath))
                {
                    var picFileInfo = new FileInfo(picPath);
                    size = picFileInfo.Length;
                    return true;
                }
            }
            //查找上传的别人的卡池缓存。
            var path = GetCachePath(resType, resID);

            if (!File.Exists(path))
                return false;

            var info = new FileInfo(path);
            size = info.Length;
            return true;
        } 
        #endregion

        #region LRU
        LinkedList<FileItem> cacheItems = new LinkedList<FileItem>();
        Dictionary<string, LinkedListNode<FileItem>> cacheIndexer = new Dictionary<string, LinkedListNode<FileItem>>();
        /// <summary>
        /// 缓存总大小
        /// </summary>
        long totalSize = 0;

        /// <summary>
        /// 重建LRU引用缓存
        /// </summary>
        void rebuildLRU()
        {
            cacheIndexer.Clear();
            cacheItems.Clear();
            totalSize = 0;

            var files = Directory.EnumerateFiles(CacheDir, "*", SearchOption.AllDirectories);
            var fileInfos = files.Select(p => new FileInfo(p)).OrderByDescending(f => f.LastAccessTime);
            foreach (var item in fileInfos)
            {
                addLRU(item.FullName, item.Length);
            }
        }
        /// <summary>
        /// 添加LRU引用
        /// </summary>
        /// <param name="path"></param>
        /// <param name="size"></param>
        void addLRU(string path, long size)
        {
            if (cacheIndexer.ContainsKey(path))
            {
                visitLRU(path);
                return;
            }

            // 清理过大的文件
            while (totalSize + size > MaxSize && cacheItems.Count > 0 && MaxSize > 0)
            {
                LinkedListNode<FileItem> lastNode = cacheItems.Last;
                FileItem remove = null;
                if (lastNode != null)
                {
                    remove = lastNode.Value;
                }
                if (remove == null)
                {
                    cacheItems.RemoveLast();
                    continue;
                }
                cacheIndexer.Remove(remove.Item1);
                cacheItems.RemoveLast();
                totalSize -= remove.Item2;

                try
                {
                    File.Delete(Path.Combine(CacheDir, remove.Item1));
                }
                catch { }
            }

            // 添加新项目
            cacheIndexer[path] = cacheItems.AddFirst(new FileItem(path, size));
            totalSize += size;
        }

        /// <summary>
        /// 访问LRU引用
        /// </summary>
        /// <param name="path"></param>
        void visitLRU(string path)
        {
            if (!cacheIndexer.ContainsKey(path)) return;

            var node = cacheIndexer[path];
            cacheItems.Remove(node);
            cacheItems.AddFirst(node);
        }
        #endregion

        private void OnConfigChange(ConfigManager.PlayerConfig config)
        {
            if (config.CacheSize < 0)
            {
                MaxSize = -1;
            }
            else
            {
                MaxSize = (long)Mathf.RoundToInt(config.CacheSize * 2) * 1024 * 1024 * 512;//512MB
            }
        }
        private void prepareDirectory(string name)
        {
            string path = Path.Combine(CacheDir, name);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        #region 生命周期
        protected override void onAwake()
        {
            base.onAwake();
            CacheDir = Path.Combine(Application.persistentDataPath, dirName);
            config.OnConfigChange += OnConfigChange;

            prepareDirectory(TouhouCardEngine.ResourceType.CardDefine.GetString());
            prepareDirectory(TouhouCardEngine.ResourceType.Picture.GetString());
            rebuildLRU();
        }
        #endregion

        #region 接口实现
        Stream IResourceProvider.OpenWriteResource(string resType, string resID, long size)
        {
            // 检查资源类型
            if (!ResourceIsValid(resType))
                return null;

            var folder = Path.Combine(CacheDir, resType);
            var path = Path.Combine(folder, resID);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            // 更新存储资源
            addLRU(path, size);

            return new FileStream(path, FileMode.Create, FileAccess.Write);
        }

        Stream IResourceProvider.OpenReadResource(string resType, string resID)
        {
            // 资源类型
            if (!ResourceIsValid(resType))
                return null;

            // 查找本地的卡池。
            if (resType == "card")
            {
                if (TryParseCardPool(resID, out long cardPoolId, out int cardPoolVersion))
                {
                    var cardPoolInfo = card.getCardPoolInfo(cardPoolId);
                    if (cardPoolInfo != null && cardPoolInfo.editTime == cardPoolVersion)
                    {
                        string deckPath = card.getCardPoolPath(cardPoolInfo);
                        return File.OpenRead(deckPath);
                    }
                }
            }
            else if(resType == "pic")
            {
                // 查找本地的图片资源。
                string fileName = string.Format("{0}.jpg", resID);
                if (card.isExternalImageExists(fileName))
                {
                    return File.OpenRead(card.getExternalImagePath(fileName));
                }
            }

            //查找上传的别人的卡池缓存。
            var path = GetCachePath(resType, resID);
            if (!File.Exists(path))
                return null;

            // 更新LRU
            visitLRU(path);

            return File.OpenRead(path);
        }

        bool IResourceProvider.ResourceInfo(string resType, string resID, out long size)
        {
            return GetResourceInfo(resType, resID, out size);
        }
        #endregion

        #region 字段属性
        [SerializeField]
        private ConfigManager config;
        [SerializeField]
        private CardManager card;
        [SerializeField]
        private string dirName = "cache";
        public long MaxSize { get; set; }
        public string CacheDir { get; private set; }

        #endregion
    }
}
