using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using TouhouCardEngine;
using UI;
using UnityEngine;
using FileItem = System.Tuple<string, long>;

namespace Game
{
    public class CacheManager : Manager, IResourceProvider
    {
        #region 公共方法
        public string GetCachePath(string type, string id)
        {
            return Path.Combine(CacheDir, type, id);
        }
        public string GetCachePath(TouhouCardEngine.ResourceType type, string id)
        {
            return GetCachePath(type.GetString(), id);
        }
        public bool FileExists(TouhouCardEngine.ResourceType type, string id)
        {
            return File.Exists(GetCachePath(type, id));
        }
        public Task SaveCache(FileManager fileManager, TouhouCardEngine.ResourceType type, string id, byte[] bytes)
        {
            string path = GetCachePath(type, id);
            addLRU(path, bytes.LongLength);
            return fileManager.saveFile(path, bytes);
        }
        public Task<byte[]> ReadCache(FileManager fileManager, TouhouCardEngine.ResourceType type, string id)
        {
            string path = GetCachePath(type, id);
            visitLRU(path);
            return fileManager.readBinaryFile(path);
        }
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

            var path = GetCachePath(resType, resID);
            if (!File.Exists(path))
                return null;

            // 更新LRU
            visitLRU(path);

            return File.OpenRead(path);
        }

        bool IResourceProvider.ResourceInfo(string resType, string resID, out long size)
        {
            size = 0;
            var path = GetCachePath(resType, resID);

            if (!ResourceIsValid(resType) || !File.Exists(path))
                return false;

            var info = new FileInfo(path);
            size = info.Length;
            return true;
        }
        #endregion

        #region 字段属性
        [SerializeField]
        private ConfigManager config;
        [SerializeField]
        private string dirName = "cache";
        public long MaxSize { get; set; }
        public string CacheDir { get; private set; }

        #endregion
    }
}
