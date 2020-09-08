using System.IO;
using UnityEngine;
using ExcelLibrary.SpreadSheet;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Data;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using ExcelDataReader;
namespace BJSYGameCore
{
    public class ResourceManager : Manager
    {
        #region 公开接口
        public T load<T>(string path)
        {
            T res;
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("路径不能为空", nameof(path));
            else if (loadFromCache<T>(path, out var cachedRes))//尝试从缓存中加载
            {
                return cachedRes;
            }
            else if (path.StartsWith("res:"))//尝试从资源中加载
            {
                var uRes = Resources.Load(path.Substring(4, path.Length - 4));
                if (uRes == null)
                    res = default;
                else if (uRes is T t)
                    res = t;
                else
                    throw new InvalidCastException("资源\"" + path + "\"" + uRes + "不是" + typeof(T).Name);
            }
            else if (path.StartsWith("ab:"))
            {
                res = loadFromBundle<T>(path.Substring(3, path.Length - 3));
            }
            else
                throw new InvalidOperationException("无法加载类型为" + typeof(T).Name + "的资源" + path);
            saveToCache(path, res);
            return res;
        }
        #endregion
        #region AssetBundle
        T loadFromBundle<T>(string path)
        {
            //如果有，需要做一些关于热更新的处理，但是现在没有。
            AssetBundle bundle;
            if (!loadBundleFromCache(path, out bundle))
            {

            }
            else
            {

            }
            return default;
        }
        bool loadBundleFromCache(string path, out AssetBundle bundle)
        {
            if (bundleCacheDic.TryGetValue(path, out var item))
            {
                bundle = item.bundle;
                return true;
            }
            bundle = null;
            return false;
        }
        Dictionary<string, BundleCacheItem> bundleCacheDic { get; } = new Dictionary<string, BundleCacheItem>();
        class BundleCacheItem
        {
            public AssetBundle bundle;
        }
        #endregion
        #region 缓存
        bool loadFromCache<T>(string path, out T res)
        {
            if (cacheDic.TryGetValue(path, out var item))
            {
                if (item.wref.IsAlive)
                {
                    if (item.wref.Target is T t)
                    {
                        res = t;
                        return true;
                    }
                    else
                        throw new InvalidCastException("资源\"" + path + "\"" + item.wref.Target + "不是" + typeof(T).Name);
                }
                else
                {
                    cacheDic.Remove(path);
                    res = default;
                    return false;
                }
            }
            else
            {
                res = default;
                return false;
            }
        }
        void saveToCache(string path, object res)
        {
            if (cacheDic.TryGetValue(path, out var item) && ReferenceEquals(item.wref.Target, res))
            {
                //Debug.LogWarning("路径" + path + "已经缓存了同一个资源" + item.wref.Target + "，取消缓存" + res);
                return;
            }
            cacheDic[path] = new CacheItem() { wref = new WeakReference(res) };
        }
        Dictionary<string, CacheItem> cacheDic { get; } = new Dictionary<string, CacheItem>();
        class CacheItem
        {
            /// <summary>
            /// 对资源的弱引用。
            /// </summary>
            /// <remarks>
            /// 这么做是为了不影响GC和Unity自动卸载没有被引用的资源。
            /// </remarks>
            public WeakReference wref;
        }
        #endregion
        [Obsolete("Use loadExcelAsDataSet instead.")]
        public Task<Workbook> loadExcel(string path, PlatformCompability platform = null, string curDir = null)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            platform = platform ?? PlatformCompability.Current;
            if (platform.RequireWebRequest)
                return loadExcelByWebRequest(path, curDir);

            return loadExcelBySystemIO(path, curDir);
        }
        /// <summary>
        /// 加载贴图
        /// </summary>
        /// <param name="path">
        /// 贴图的路径。可以是相对于当前路径的相对路径，或是StreamingAssets下的绝对路径。
        /// 以res:开头的请求将读取Resource文件夹
        /// </param>
        /// <param name="curDir">当前路径</param>
        /// <param name="platform"></param>
        /// <returns></returns>
        public Task<Texture2D> loadTexture(string path, string curDir, PlatformCompability platform = null)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (path.StartsWith("res:"))
                return Task.FromResult(Resources.Load<Texture2D>(path.Replace("res:", "")));

            platform = platform ?? PlatformCompability.Current;
            if (platform.RequireWebRequest)
                return loadTextureByWebRequestWithFallback(path, curDir);

            return loadTextureBySystemIOWithFallback(path, curDir);
        }
        /// <summary>
        /// 加载数据集
        /// </summary>
        /// <param name="path"></param>
        /// <param name="platform"></param>
        /// <param name="curDir"></param>
        /// <returns></returns>
        public Task<DataSet> loadDataSet(string path, PlatformCompability platform = null, string curDir = null)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            if (Path.GetExtension(path).ToLower() == ".xls" || Path.GetExtension(path).ToLower() == ".xlsx")
            {
                Debug.Log("尝试直接读取Excel文件，程序将读取对应的dataset文件");
                path += ".dataset";
            }

            platform = platform ?? PlatformCompability.Current;

            if (platform.RequireWebRequest)
                return loadDataSetByWebRequest(path, curDir);

            return loadDataSetBySystemIO(path, curDir);
        }

        public Task<DataSet> loadExcelAsDataSet(string path, PlatformCompability platform = null, string curDir = null)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            platform = platform ?? PlatformCompability.Current;
            if (platform.RequireWebRequest)
                return loadExcelAsDataSetByWebRequest(path, curDir);

            return loadExcelAsDataSetBySystemIO(path, curDir);
        }
        public Task<T> loadObject<T>(string path, string curDir = null, PlatformCompability platform = null) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
                return default;
            if (path.StartsWith("res:"))
                return Task.FromResult(Resources.Load<T>(path.Replace("res:", "")));
            throw new NotImplementedException();
        }
        /// <summary>
        /// 尝试修改材质的扩展名，以便做Fallback
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string textureChangeExt(string path)
        {
            string origExt = Path.GetExtension(path).ToLower();
            string ext = origExt;
            switch (origExt)
            {
                case ".png":
                    ext = ".jpg";
                    break;
                case ".jpg":
                    ext = ".png";
                    break;
                default:
                    Debug.Log($"{ext} has no fallback. path: {path}");
                    break;
            }

            path = Path.ChangeExtension(path, ext);
            return path;
        }

        /// <summary>
        /// 查找指定路径的文件列表
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string[] GetDirectoryFiles(string path)
        {
            var dir = Path.GetDirectoryName(path);
            var name = Path.GetFileName(path);

            var files = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, dir), name);
            Uri streaming = new Uri(Application.streamingAssetsPath + "/");

            List<string> encodedFiles = new List<string>();
            foreach (var file in files)
            {
                Uri abs = new Uri(file);
                encodedFiles.Add(streaming.MakeRelativeUri(abs).ToString());
            }
            return encodedFiles.ToArray();
        }
        public async Task<Texture2D> loadTextureByWebRequestWithFallback(string path, string currDir)
        {
            try
            {
                return await loadTextureByWebRequest(path, currDir);
            }
            catch (FileNotFoundException)
            {
                path = textureChangeExt(path);
                return await loadTextureByWebRequest(path, currDir);
            }
        }

        public async Task<Texture2D> loadTextureByWebRequest(string path, string basePath)
        {
            try
            {
                return await loadTextureByWebRequest(path);
            }
            catch (FileNotFoundException e)
            {
                if (string.IsNullOrEmpty(basePath))
                {
                    throw e;
                }
                return await loadTextureByWebRequest(Path.Combine(basePath, path));
            }
        }

        public Task<Texture2D> loadTextureByWebRequest(string path)
        {
            TaskCompletionSource<Texture2D> tcs = new TaskCompletionSource<Texture2D>();
            UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(Application.streamingAssetsPath + "/" + path);
            uwr.SendWebRequest().completed += op =>
            {
                var uop = op as UnityWebRequestAsyncOperation;
                if (uop.webRequest.isNetworkError)
                {
                    tcs.SetException(new HttpRequestException(uop.webRequest.error));
                    return;
                }
                if (uop.webRequest.isHttpError)
                {
                    if (uop.webRequest.responseCode == 404)
                    {
                        tcs.SetException(new FileNotFoundException());
                    }
                    else
                    {
                        tcs.SetException(new HttpRequestException(uop.webRequest.error));
                    }
                    return;
                }
                tcs.SetResult(DownloadHandlerTexture.GetContent(uwr));
                uwr.Dispose();
            };
            return tcs.Task;
        }
        public async Task<Texture2D> loadTextureBySystemIOWithFallback(string path, string curDir)
        {
            try
            {
                return await loadTextureBySystemIO(path, curDir);
            }
            catch (FileNotFoundException)
            {
                path = textureChangeExt(path);
                return await loadTextureBySystemIO(path, curDir);
            }
        }
        public async Task<Texture2D> loadTextureBySystemIO(string path, string curDir)
        {
            Texture2D texture = new Texture2D(512, 512);
            texture.LoadImage(await loadBytesBySystemIO(path, curDir));
            return texture;
        }

        public Task<Workbook> loadExcelBySystemIO(string path, string curDir)
        {
            using (FileStream stream = getFileStream(path, curDir))
            {
                return Task.FromResult(Workbook.Load(stream));
            }
        }
        public Task<DataSet> loadDataSetBySystemIO(string path, string curDir)
        {
            using (FileStream stream = getFileStream(path, curDir))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return Task.FromResult(bf.Deserialize(stream) as DataSet);
            }
        }
        public Task<DataSet> loadExcelAsDataSetBySystemIO(string path, string curDir)
        {
            using (FileStream stream = getFileStream(path, curDir))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            // 使用第一行的内容作为列索引
                            // 其他选项的说明见Github的ReadMe
                            UseHeaderRow = true,
                        }
                    });
                    return Task.FromResult(result);
                }
            }
        }
        private async Task<Workbook> loadExcelByWebRequest(string path, string currDir)
        {
            byte[] data = await loadBytesByWebRequest(path, currDir);
            using (MemoryStream stream = new MemoryStream(data))
            {
                return Workbook.Load(stream);
            }
        }
        private async Task<DataSet> loadDataSetByWebRequest(string path, string currDir)
        {
            byte[] data = await loadBytesByWebRequest(path, currDir);
            using (MemoryStream stream = new MemoryStream(data))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return bf.Deserialize(stream) as DataSet;
            }
        }
        private async Task<DataSet> loadExcelAsDataSetByWebRequest(string path, string currDir)
        {
            byte[] data = await loadBytesByWebRequest(path, currDir);
            using (MemoryStream stream = new MemoryStream(data))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            // 使用第一行的内容作为列索引
                            // 其他选项的说明见Github的ReadMe
                            UseHeaderRow = true,
                        }
                    });
                    return result;
                }
            }
        }
        private async Task<byte[]> loadBytesBySystemIO(string path, string curDir)
        {
            using (FileStream stream = getFileStream(path, curDir))
            {
                byte[] data = new byte[stream.Length];
                await stream.ReadAsync(data, 0, (int)stream.Length);
                return data;
            }
        }
        private FileStream getFileStream(string path, string curDir)
        {
            string filePath = path;
            if (File.Exists(filePath))
                return new FileStream(filePath, FileMode.Open);
            filePath = Application.streamingAssetsPath + "/" + path;
            if (File.Exists(filePath))
                return new FileStream(filePath, FileMode.Open);
            if (!string.IsNullOrEmpty(curDir))
            {
                filePath = Path.Combine(Application.streamingAssetsPath, curDir, path);
                if (File.Exists(filePath))
                    return new FileStream(filePath, FileMode.Open);
            }
            throw new FileNotFoundException("File not found.", path);
        }
        private async Task<byte[]> loadBytesByWebRequest(string path, string currDir)
        {
            try
            {
                return await loadBytesByWebRequest(path);
            }
            catch (FileNotFoundException e)
            {
                if (string.IsNullOrEmpty(currDir))
                {
                    throw e;
                }
                return await loadBytesByWebRequest(Path.Combine(currDir, path));
            }
        }
        private Task<byte[]> loadBytesByWebRequest(string path)
        {
            TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();
            UnityWebRequest.Get(Application.streamingAssetsPath + "/" + path).SendWebRequest().completed += op =>
            {
                var uop = op as UnityWebRequestAsyncOperation;
                if (uop.webRequest.isNetworkError)
                {
                    tcs.SetException(new HttpRequestException(uop.webRequest.error));
                    return;
                }
                if (uop.webRequest.isHttpError)
                {
                    if (uop.webRequest.responseCode == 404)
                    {
                        tcs.SetException(new FileNotFoundException($"Unable to load file {path}", path));
                    }
                    else
                    {
                        tcs.SetException(new HttpRequestException(uop.webRequest.error));
                    }
                    return;
                }
                tcs.SetResult(uop.webRequest.downloadHandler.data);
            };
            return tcs.Task;
        }
    }
}
