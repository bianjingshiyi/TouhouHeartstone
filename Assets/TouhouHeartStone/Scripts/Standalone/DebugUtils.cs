using UberLogger;

namespace IGensoukyo.Utilities
{
    public class DebugUtils
    {
        [StackTraceIgnore]
        public static void Log(string message, UnityEngine.Object context = null)
        {
            UberDebug.LogChannel("Frontend", message, context);
        }

        [StackTraceIgnore]
        public static void Debug(string message, UnityEngine.Object context = null)
        {
            UberDebug.LogDebugChannel("Frontend", message, context);
        }

        [StackTraceIgnore]
        public static void Trace(string message, UnityEngine.Object context = null)
        {
            UberDebug.LogTraceChannel("Frontend", message, context);
        }

        [StackTraceIgnore]
        public static void LogNoImpl(string message, UnityEngine.Object context = null)
        {
            UberDebug.LogWarningChannel("Frontend", $"[NoImpl]{message}", context);
        }

        [StackTraceIgnore]
        public static void Warning(string message, UnityEngine.Object context = null)
        {
            UberDebug.LogWarningChannel("Frontend", message, context);
        }

        [StackTraceIgnore]
        public static void Error(string message, UnityEngine.Object context = null)
        {
            UberDebug.LogErrorChannel("Frontend", message, context);
        }

        [StackTraceIgnore]
        public static void NullCheck(object obj, string name)
        {
            if (obj == null)
            {
                Warning($"{name} 为空");
            }
        }
    }
}
