using System.IO;
using UnityEngine;
using BJSYGameCore;
using ExcelLibrary.SpreadSheet;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Data;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using ExcelDataReader;

namespace Game
{
    public class ResourceManager : Manager
    {
        [Obsolete("Use loadExcelAsDataSet instead.")]
        public Task<Workbook> loadExcel(string path, PlatformCompability platform = null)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            platform = platform ?? PlatformCompability.Current;
            if (platform.RequireWebRequest)
                return loadExcelByWebRequest(path);

            return loadExcelBySystemIO(path);
        }
        public Task<Texture2D> loadTexture(string path, PlatformCompability platform = null)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            platform = platform ?? PlatformCompability.Current;
            if (platform.RequireWebRequest)
                return loadTextureByWebRequestWithFallback(path);

            return loadTextureBySystemIOWithFallback(path);
        }
        public Task<DataSet> loadDataSet(string path, PlatformCompability platform = null)
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
                return loadDataSetByWebRequest(path);

            return loadDataSetBySystemIO(path);

        }

        public Task<DataSet> loadExcelAsDataSet(string path, PlatformCompability platform = null)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            platform = platform ?? PlatformCompability.Current;
            if (platform.RequireWebRequest)
                return loadExcelAsDataSetByWebRequest(path);

            return loadExcelAsDataSetBySystemIO(path);
        }

        public async Task<Texture2D> loadTextureByWebRequestWithFallback(string path)
        {
            var task = loadTextureByWebRequest(path);
            var result = await task;
            if (task.IsCompleted)
                return result;

            path = textureChangeExt(path);
            return await loadTextureByWebRequest(path);
        }

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
        public async Task<Texture2D> loadTextureBySystemIOWithFallback(string path)
        {
            try
            {
                return await loadTextureBySystemIO(path);
            }
            catch (FileNotFoundException)
            {
                path = textureChangeExt(path);
                return await loadTextureBySystemIO(path);
            }
        }
        public async Task<Texture2D> loadTextureBySystemIO(string path)
        {
            Texture2D texture = new Texture2D(512, 512);
            texture.LoadImage(await loadBytesBySystemIO(path));
            return texture;
        }

        public Task<Workbook> loadExcelBySystemIO(string path)
        {
            using (FileStream stream = getFileStream(path))
            {
                return Task.FromResult(Workbook.Load(stream));
            }
        }
        public Task<DataSet> loadDataSetBySystemIO(string path)
        {
            using (FileStream stream = getFileStream(path))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return Task.FromResult(bf.Deserialize(stream) as DataSet);
            }
        }
        public Task<DataSet> loadExcelAsDataSetBySystemIO(string path)
        {
            using (FileStream stream = getFileStream(path))
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
        private async Task<Workbook> loadExcelByWebRequest(string path)
        {
            byte[] data = await loadBytesByWebRequest(path);
            using (MemoryStream stream = new MemoryStream(data))
            {
                return Workbook.Load(stream);
            }
        }
        private async Task<DataSet> loadDataSetByWebRequest(string path)
        {
            byte[] data = await loadBytesByWebRequest(path);
            using (MemoryStream stream = new MemoryStream(data))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return bf.Deserialize(stream) as DataSet;
            }
        }
        private async Task<DataSet> loadExcelAsDataSetByWebRequest(string path)
        {
            byte[] data = await loadBytesByWebRequest(path);
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
        private async Task<byte[]> loadBytesBySystemIO(string path)
        {
            using (FileStream stream = getFileStream(path))
            {
                byte[] data = new byte[stream.Length];
                await stream.ReadAsync(data, 0, (int)stream.Length);
                return data;
            }
        }
        private FileStream getFileStream(string path)
        {
            string filePath = path;
            if (File.Exists(filePath))
                return new FileStream(filePath, FileMode.Open);
            filePath = Application.streamingAssetsPath + "/" + path;
            if (File.Exists(filePath))
                return new FileStream(filePath, FileMode.Open);
            throw new FileNotFoundException("File not found.", path);
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
