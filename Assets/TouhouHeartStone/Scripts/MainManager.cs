using UnityEngine;
using BJSYGameCore;
using UI;
using System.Threading.Tasks;
using BJSYGameCore.UI;
using TouhouHeartstone;
namespace Game
{
    public class MainManager : Manager
    {
        Task _loadCardTask;
        public UIManager ui => getManager<UIManager>();
        public NetworkingManager network => getManager<NetworkingManager>();
        public GameManager game => getManager<GameManager>();
        protected override void onAwake()
        {
            base.onAwake();
            Main main = ui.getObject<Main>();
            main.MainMenu.ManMachineButton.onClick.set(() =>
            {
                main.display(main.Game);
                game.startLocalGame();
            });
            main.MainMenu.NetworkButton.onClick.set(() =>
            {
                main.display(main.NetworkingPage);
                network.displayLANPanel();
            });
        }
        protected void Start()
        {
            _loadCardTask = getManager<CardManager>().load(new string[] { "Cards/Cards.xls", "Cards/Patchouli.xls" }, new System.Reflection.Assembly[] { typeof(THHGame).Assembly });
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
    }
}
