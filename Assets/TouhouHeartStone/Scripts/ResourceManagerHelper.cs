using UnityEngine;
using BJSYGameCore;
using System.Threading.Tasks;

namespace Game
{
    public static class ResourceManagerHelper
    {
        /// <summary>
        /// 加载音频。
        /// </summary>
        /// <param name="path">
        /// 路径。当前支持两种格式:
        /// res:Name, 指从Resource文件夹读取对应的音频文件
        /// se:Name, 指从SEManager读取对应的音效。
        /// 建议不要使用se:Name读取，而是转而直接用SEManager播放对应音效。
        /// </param>
        /// <param name="curDir"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static Task<AudioClip> loadAudio(this THHResourceManager manager, string path, string curDir = null, PlatformCompability platform = null)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (path.StartsWith("res:"))
            {
                return Task.FromResult(Resources.Load<AudioClip>(path.Replace("res:", "")));
            }

            if (path.StartsWith("se:"))
            {
                return Task.FromResult(manager.getManager<SEManager>().GetSE(path.Replace("se:", "")));
            }

            return null;
        }
    }
}
