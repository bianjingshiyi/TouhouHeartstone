using BJSYGameCore;

namespace Game
{
    public class DebugManager : Manager
    {
#if (DEBUG || DEVELOPMENT_BUILD || UNITY_EDITOR)
        const bool debugMode = true;
#else
        const bool debugMode = false;
#endif
        /// <summary>
        /// 跳过卡组数量检测
        /// </summary>
        public bool SkipCardNumberCheck { get; set; } = debugMode;
    }
}
