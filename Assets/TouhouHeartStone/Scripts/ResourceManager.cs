using System.IO;
using UnityEngine;
using BJSYGameCore;
using ExcelLibrary.SpreadSheet;
using UnityEngine.Networking;
using System.Threading.Tasks;
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
                    return loadTextureByWebRequest(path);
                default:
                    return loadTextureBySystemIO(path);
            }
        }
        public async Task<Texture2D> loadTextureByWebRequest(string path)
        {
            Texture2D texture = new Texture2D(512, 512);
            texture.LoadImage(await loadBytesByWebRequest(path));
            return texture;
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
                tcs.SetResult((op as UnityWebRequestAsyncOperation).webRequest.downloadHandler.data);
            };
            return tcs.Task;
        }
    }
}
