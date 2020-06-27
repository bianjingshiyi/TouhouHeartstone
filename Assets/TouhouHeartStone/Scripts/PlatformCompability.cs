using UnityEngine;

namespace Game
{
    public class PlatformCompability
    {
        public static PlatformCompability Current => new PlatformCompability();

        PlatformCompability()
        {
            currentPlatform = Application.platform;
        }

        readonly RuntimePlatform currentPlatform;

        public PlatformCompability(RuntimePlatform rtp)
        {
            currentPlatform = rtp;
        }

        /// <summary>
        /// 读取文件是否需要WebRequest
        /// </summary>
        /// <returns></returns>
        public bool RequireWebRequest => currentPlatform == RuntimePlatform.Android;

        /// <summary>
        /// 是否支持直接读取Excel文件
        /// </summary>
        public bool SupportExcelReading => currentPlatform != RuntimePlatform.Android;
    }
}
