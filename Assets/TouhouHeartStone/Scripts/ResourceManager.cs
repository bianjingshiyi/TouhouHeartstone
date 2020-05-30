using System.IO;
using UnityEngine;
using BJSYGameCore;
using ExcelLibrary.SpreadSheet;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Net.Http;

namespace Game
{
    public class ResourceManager : Manager
    {
        public Task<Workbook> loadExcel(string path, RuntimePlatform? platform = null)
        {
            platform = getPlatform(platform);
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return loadExcelByWebRequest(path);
                default:
                    return loadExcelBySystemIO(path);
            }
        }
        public Task<Texture2D> loadTexture(string path, RuntimePlatform? platform = null)
        {
            platform = getPlatform(platform);
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return loadTextureByWebRequestWithFallback(path);
                default:
                    return loadTextureBySystemIOWithFallback(path);
            }
        }
        public async Task<Texture2D> loadTextureByWebRequestWithFallback(string path)
        {
            var task = loadTextureByWebRequest(path);
            var result = await task;
            if (task.IsCompleted)
                return result;

            string ext = "";

            if (Path.GetExtension(path).ToLower() == "png")
                ext = "jpg";
            if (Path.GetExtension(path).ToLower() == "jpg")
                ext = "png";

            path = Path.ChangeExtension(path, ext);
            return await loadTextureByWebRequest(path);
        }
        public async Task<Texture2D> loadTextureByWebRequest(string path)
        {
            Texture2D texture = new Texture2D(512, 512);
            byte[] data = await loadBytesByWebRequest(path);
            texture.LoadImage(data);
            return texture;
        }
        public async Task<Texture2D> loadTextureBySystemIOWithFallback(string path)
        {
            try
            {
                return await loadTextureBySystemIO(path);
            }
            catch (FileNotFoundException)
            {
                string ext = "";
                if (Path.GetExtension(path).ToLower() == "png")
                    ext = "jpg";
                if (Path.GetExtension(path).ToLower() == "jpg")
                    ext = "png";

                path = Path.ChangeExtension(path, ext);
                return await loadTextureBySystemIO(path);
            }
        }
        public async Task<Texture2D> loadTextureBySystemIO(string path)
        {
            Texture2D texture = new Texture2D(512, 512);
            texture.LoadImage(await loadBytesBySystemIO(path));
            return texture;
        }
        private static RuntimePlatform? getPlatform(RuntimePlatform? platform)
        {
#if UNITY_ANDROID
            if (platform == null)
                platform = RuntimePlatform.Android;
#endif
            return platform;
        }

        public Task<Workbook> loadExcelBySystemIO(string path)
        {
            using (FileStream stream = getFileStream(path))
            {
                return Task.FromResult(Workbook.Load(stream));
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
            throw new FileNotFoundException(path);
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
                        tcs.SetException(new FileNotFoundException());
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
