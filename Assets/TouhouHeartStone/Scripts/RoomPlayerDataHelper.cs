using NitoriNetwork.Common;

namespace Game
{
    public enum PlayerLoadState
    {
        None,
        Checking,
        Upload,
        Download
    }
    public static class RoomPlayerDataHelper
    {
        public const string PROP_LOAD_STATE = "load_state";
        public static PlayerLoadState getLoadState(this IRoomPlayer playerData)
        {
            if (!playerData.hasProp(PROP_LOAD_STATE))
                return 0;
            else
                return (PlayerLoadState)playerData.getProp<int>(PROP_LOAD_STATE);
        }

        public static void setLoadState(this IRoomPlayer playerData, PlayerLoadState val)
        {
            playerData.setProp(PROP_LOAD_STATE, (int)val);
        }
    }
}
