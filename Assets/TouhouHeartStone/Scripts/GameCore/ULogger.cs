namespace TouhouHeartstone
{
    public class ULogger : TouhouCardEngine.Interfaces.ILogger
    {
        public ULogger()
        {
            log("ULogger Init");
        }
        public bool enable { get; set; } = true;
        public void log(string msg)
        {
            if (!enable)
                return;
            UberDebug.Log(msg);
        }
        public void log(string channel, string msg)
        {
            if (!enable)
                return;
            if (channel == "Debug")
                UberDebug.Log(msg);
            else if (channel == "Warning")
                UberDebug.LogWarning(msg);
            else if (channel == "Error")
                UberDebug.LogError(msg);
            else
                UberDebug.LogChannel(channel, msg);
        }
    }
}