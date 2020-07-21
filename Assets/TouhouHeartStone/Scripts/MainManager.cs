using UnityEngine;
using BJSYGameCore;
using UI;
using System.Threading.Tasks;
using BJSYGameCore.UI;
using TouhouHeartstone;
using TouhouCardEngine;

namespace Game
{
    public class MainManager : Manager
    {
        Task _loadCardTask;
        public UIManager ui => getManager<UIManager>();
        public NetworkingManager network => getManager<NetworkingManager>();
        public GameManager game => getManager<GameManager>();

        Main mainUI => ui.getObject<Main>();
        protected override void onAwake()
        {
            base.onAwake();
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            Main main = ui.getObject<Main>();
            main.MainMenu.ManMachineButtonButtonBlack.asButton.onClick.set(() =>
            {
                main.display(main.Game);
                game.startLocalGame();
            });
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
                new string[] { "Cards/Cards.xls", "Cards/Patchouli.xls" },
                new System.Reflection.Assembly[] { typeof(THHGame).Assembly },
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
                _loadCardTask = null;
            }
            else if (_loadCardTask.IsFaulted || _loadCardTask.IsCanceled)
            {
                UIManager ui = getManager<UIManager>();
                ui.getObject<Dialog>().display("游戏资源加载失败！请重新启动游戏", () => Application.Quit());
                _loadCardTask = null;
            }
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
