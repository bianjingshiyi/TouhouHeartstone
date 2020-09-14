using UnityEngine;
using BJSYGameCore;
using UI;
using System.Threading.Tasks;
using BJSYGameCore.UI;
using TouhouHeartstone;
using TouhouCardEngine;
using UnityEngine.Events;
using NitoriNetwork.Common;
namespace Game
{
    public class MainManager : Manager
    {
        Task _loadCardTask;
        public UIManager ui => getManager<UIManager>();
        public NetworkingManager network => getManager<NetworkingManager>();
        public GameManager game => getManager<GameManager>();

        [SerializeField]
        UnityEvent onLoadingFinish;

        Main mainUI => ui.getObject<Main>();

        RoomInfo currentRoomInfo = new RoomInfo();



        protected override void onAwake()
        {
            base.onAwake();
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            Main main = ui.getObject<Main>();
            main.MainMenu.ManMachineButtonButtonBlack.asButton.onClick.set(showLocalRoom);
            main.MainMenu.NetworkButtonButtonBlack.asButton.onClick.set(() =>
            {
                main.display(main.NetworkingPage);
                network.displayLANPanel();
            });
            main.display(main.Loading);
        }
        protected void Start()
        {
            _loadCardTask = getManager<CardManager>().load(
                new System.Reflection.Assembly[] { typeof(THHGame).Assembly },
                new string[] { "TouhouHeartstone", "TouhouHeartstone.Builtin" },
                false,
                null,
                mainUI.Loading.LoggerText.GetComponent<ScreenLogger>()
            );
        }
        protected void Update()
        {
            if (_loadCardTask == null)
                return;
            if (_loadCardTask.IsCompleted)
            {
                UIManager ui = getManager<UIManager>();
                Main main = ui.getObject<Main>();
                main.display(main.MainMenu);
                onLoadingFinish?.Invoke();
                _loadCardTask = null;
            }
            else if (_loadCardTask.IsFaulted || _loadCardTask.IsCanceled)
            {
                UIManager ui = getManager<UIManager>();
                ui.getObject<Dialog>().display("游戏资源加载失败！请重新启动游戏", () => Application.Quit());
                _loadCardTask = null;
            }
        }

        private void showLocalRoom()
        {
            Main main = ui.getObject<Main>();

            currentRoomInfo = new RoomInfo();
            var selfPlayer = new RoomPlayerInfo() { name = "自己" };
            currentRoomInfo.playerList.Add(selfPlayer);
            currentRoomInfo.setOption(GameOption.Default);

            main.RoomPanel.ResetEvents();
            main.RoomPanel.SetMode(RoomPanel.Mode.Local);
            main.RoomPanel.SetSelfID(selfPlayer.PlayerID);
            main.RoomPanel.SetRightText("");
            main.RoomPanel.SetInfo(currentRoomInfo);

            main.display(main.RoomPanel);
            main.RoomPanel.OnStartClick += () =>
            {
                game.startLocalGame(currentRoomInfo);
            };
            main.RoomPanel.OnQuitClick += () =>
            {
                main.display(main.MainMenu);
            };
            main.RoomPanel.OnRoomInfoUpdate += (info) =>
            {
                currentRoomInfo = info;
                main.RoomPanel.SetInfo(currentRoomInfo);
            };
            main.RoomPanel.OnPlayerRemove += (id) =>
            {
                for (int i = 0; i < currentRoomInfo.playerList.Count; i++)
                {
                    if (currentRoomInfo.playerList[i].PlayerID == id)
                    {
                        currentRoomInfo.playerList.RemoveAt(i--);
                    }
                }
                main.RoomPanel.SetInfo(currentRoomInfo);
            };
            main.RoomPanel.OnAIPlayerAdd += (player) =>
            {
                currentRoomInfo.playerList.Add(player);
                main.RoomPanel.SetInfo(currentRoomInfo);
            };
            main.RoomPanel.OnSetPlayerDeck += (id, deck) =>
            {
                for (int i = 0; i < currentRoomInfo.playerList.Count; i++)
                {
                    if (currentRoomInfo.playerList[i].PlayerID == id)
                    {
                        currentRoomInfo.playerList[i].setDeck(deck);
                    }
                }
                main.RoomPanel.SetInfo(currentRoomInfo);
            };
        }
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                if (e.Exception.InnerException != null)
                    Debug.LogError(e.Exception.InnerException);
                else if (e.Exception.InnerExceptions != null)
                {
                    foreach (var exception in e.Exception.InnerExceptions)
                    {
                        Debug.LogError(exception);
                    }
                }
                else
                    Debug.LogError(e.Exception);
            }
            else
                Debug.LogError(e);
        }
    }
}
