namespace TouhouHeartstone
{
    public class ULogger : TouhouCardEngine.Interfaces.ILogger
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
        public void log(string msg)
        {
            if (!enable)
                return;
            UberDebug.Log((string.IsNullOrEmpty(name) ? null : (name + ":")) + msg);
        }
        public void log(string channel, string msg)
        {
            if (!enable)
                return;
            if (channel == "Debug")
                UberDebug.Log((string.IsNullOrEmpty(name) ? null : (name + ":")) + msg);
            else if (channel == "Warning")
                UberDebug.LogWarning((string.IsNullOrEmpty(name) ? null : (name + ":")) + msg);
            else if (channel == "Error")
                UberDebug.LogError((string.IsNullOrEmpty(name) ? null : (name + ":")) + msg);
            else
                UberDebug.LogChannel(channel, (string.IsNullOrEmpty(name) ? null : (name + ":")) + msg);
        }
    }
}