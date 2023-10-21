using System.Collections.Generic;
using UberLogger;

namespace TouhouHeartstone
{
    public class ULogger : TouhouCardEngine.Shared.ILogger
    {
        string name { get; } = null;
        public ULogger()
        {
            log("ULogger Init");
        }
        public ULogger(string name)
        {
            log("ULogger Init");
            this.name = name;
        }
        public bool enable { get; set; } = true;

        public List<string> blackList { get; set; } = new List<string>();

        [StackTraceIgnore]
        public void log(string msg)
        {
            log("Debug", msg);
        }
        [StackTraceIgnore]
        public void log(string channel, string msg)
        {
            if (!enable)
                return;
            if (blackList.Contains(channel))
                return;
            if (channel == "Debug")
                UberDebug.Log($"{(string.IsNullOrEmpty(name) ? null : $"{name}:")}{msg}");
            else if (channel == "Warning")
                UberDebug.LogWarning($"{(string.IsNullOrEmpty(name) ? null : $"{name}:")}{msg}");
            else if (channel == "Error")
                UberDebug.LogError($"{(string.IsNullOrEmpty(name) ? null : $"{name}:")}{msg}");
            else
                UberDebug.LogChannel(channel, $"{(string.IsNullOrEmpty(name) ? null : $"{name}:")}{msg}");
        }

        [StackTraceIgnore]
        public void logWarn(string msg)
        {
            logWarn(null, msg);
        }

        [StackTraceIgnore]
        public void logWarn(string channel, string msg)
        {
            if (!enable)
                return;
            if (blackList.Contains(channel))
                return;
            if (string.IsNullOrEmpty(channel))
                UberDebug.LogWarning($"{(string.IsNullOrEmpty(name) ? null : $"{name}:")}{msg}");
            else
                UberDebug.LogWarningChannel(channel, $"{(string.IsNullOrEmpty(name) ? null : $"{name}:")}{msg}");
        }

        [StackTraceIgnore]
        public void logError(string msg)
        {
            logError(null, msg);
        }

        [StackTraceIgnore]
        public void logError(string channel, string msg)
        {
            if (!enable)
                return;
            if (blackList.Contains(channel))
                return;
            if (string.IsNullOrEmpty(channel))
                UberDebug.LogError($"{(string.IsNullOrEmpty(name) ? null : $"{name}:")}{msg}");
            else
                UberDebug.LogErrorChannel(channel, $"{(string.IsNullOrEmpty(name) ? null : $"{name}:")}{msg}");
        }

        [StackTraceIgnore]
        public void logTrace(string msg)
        {
            logTrace(null, msg);
        }

        [StackTraceIgnore]
        public void logTrace(string channel, string msg)
        {
            if (!enable)
                return;
            if (blackList.Contains(channel))
                return;
            if (string.IsNullOrEmpty(channel))
                UberDebug.LogTrace($"{(string.IsNullOrEmpty(name) ? null : $"{name}:")}{msg}");
            else
                UberDebug.LogTraceChannel(channel, $"{(string.IsNullOrEmpty(name) ? null : $"{name}:")}{msg}");
        }
    }
}