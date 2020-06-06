using Game;
using TouhouCardEngine;
using System.Text.RegularExpressions;
namespace UI
{
    public partial class IPPanel
    {
        partial void onAwake()
        {
            ConnectButton.onClick.AddListener(() =>
            {
                Match m = Regex.Match(IPInputField.text, @"(?<ip>d+\.d+\.d+\.d+)\:(?<port>d+)");
                if (m.Success)
                {
                    _ = ui.getManager<NetworkingManager>().joinRoom(new RoomInfo()
                    {
                        ip = m.Groups["ip"].Value,
                        port = int.Parse(m.Groups["port"].Value)
                    });
                }
                else
                    ui.getObject<Dialog>().display("输入的地址格式有误", null);
            });
        }
        protected void Update()
        {
            AddressText.text = "你的地址：\n" + ui.getManager<NetworkingManager>().address;
        }
    }
}