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
using UnityEngine.Experimental.Rendering;
using Game;

namespace BJSYGameCore
{
    public class THHResourceManager : ResourceManager
    {
        #region 公有方法
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

            if (path.isResourcesPath(out var resourcesPath))
                return Task.FromResult(Resources.Load<Texture2D>(resourcesPath));

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
            if (path.isResourcesPath(out var resourcesPath))
                return Task.FromResult(Resources.Load<T>(resourcesPath));
            throw new NotImplementedException();
        }
        public async override Task<T> loadAsync<T>(string path, string dir)
        {
            T res;
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("路径不能为空", nameof(path));
            if (loadFromCache<T>(path, out var cachedRes))
            {
                //有缓存资源，直接返回缓存资源
                return cachedRes;
            }
            else if (tryGetLoadOperation(path, typeof(T), out LoadResourceOperationBase operation))
            {
                //正在加载这个资源，等待加载过程完成并返回，避免重复加载
                await operation.task;
                res = (T)operation.resource;
            }
            else if (path.StartsWith(PATH_RES_PREFIX))
            {
                //从资源中加载
                ResourceRequest request = Resources.LoadAsync(path.Substring(PATH_RES_PREFIX.Length, path.Length - PATH_RES_PREFIX.Length), typeof(T));
                operation = new LoadResourceOperation(path, typeof(T), request);
                addLoadOperation(operation);
                await operation.task;
                res = (T)operation.resource;
            }
            else
            {
                //尝试从外部文件中加载
                if (typeof(Sprite).IsAssignableFrom(typeof(T)))
                {
                    //加载Sprite
                    Texture2D texture = await loadTexture(path, dir);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100);
                    res = (T)(object)sprite;
                }
                else if (typeof(Texture2D).IsAssignableFrom(typeof(T)))
                {
                    //加载Texture
                    res = (T)(object)await loadTexture(path, dir);
                }
                else
                    throw new InvalidOperationException($"无法加载资源{path}");
            }
            saveToCache(path, res);
            return res;
        }
        #endregion
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
                if (uop.webRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    tcs.SetException(new HttpRequestException(uop.webRequest.error));
                    return;
                }
                if (uop.webRequest.result == UnityWebRequest.Result.ProtocolError)
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
                Texture2D texture = new Texture2D(512, 512, TextureFormat.RGB24, true);
                if (uwr.downloadHandler.data != null)
                    texture.LoadImage(uwr.downloadHandler.data);
                else
                    texture = null;
                tcs.SetResult(texture);
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
            Texture2D texture = new Texture2D(512, 512, TextureFormat.RGB24, true);
            var bytes = await loadBytesBySystemIO(path, curDir);
            if (bytes == null)
            {
                throw new ArgumentNullException($"Fail to load {path}: data readed is null");
            }
            try
            {
                texture.LoadImage(bytes);
            }
            catch (ArgumentNullException)
            {
                // 不知道为啥会炸，先来个dirtyfix再说
                texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                texture.LoadImage(bytes);
            }
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
            throw new FileNotFoundException($"File not found {path} at {curDir}", path);
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
                if (uop.webRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    tcs.SetException(new HttpRequestException(uop.webRequest.error));
                    return;
                }
                if (uop.webRequest.result == UnityWebRequest.Result.ProtocolError)
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
