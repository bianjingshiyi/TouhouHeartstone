using Game;
using TouhouCardEngine;
namespace UI
{
    public partial class LANPanel
    {
        partial void onAwake()
        {
            CreateRoomButton.onClick.AddListener(() =>
            {
                _ = ui.getManager<NetworkingManager>().createRoom();
            });
        }
        protected override void onDisplay()
        {
            base.onDisplay();
            ui.getManager<NetworkingManager>().updateLANRooms();
        }
        public void refreshRoomInfo(RoomInfo room)
        {
            if (room == null)
            {
                NameText.text = null;
                IPText.text = null;
                PortText.text = null;
                DescText.text = null;
            }
            else
            {
                NameText.text = "房间";
                IPText.text = "IP:" + room.ip;
                PortText.text = "端口:" + room.port;
                DescText.text = "描述:";
            }
        }
    }
}