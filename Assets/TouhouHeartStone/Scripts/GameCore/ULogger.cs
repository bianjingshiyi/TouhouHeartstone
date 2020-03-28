namespace TouhouHeartstone
{
    public class ULogger : TouhouCardEngine.Interfaces.ILogger
    {
        public void log(string msg)
        {
            UberDebug.Log(msg);
        }
        public void log(string channel, string msg)
        {
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