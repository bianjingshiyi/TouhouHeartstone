using UnityEngine;
using BJSYGameCore;
using UI;
using System.Threading.Tasks;
using BJSYGameCore.UI;
using TouhouHeartstone;
using TouhouCardEngine;
using UnityEngine.Events;
using NitoriNetwork.Common;
using System;
using MukioI18n;
using CustomBuilder;

namespace Game
{
    class StringTable
    {
        [TranslateMsg("所有按钮上通用的确认")]
        public const string CONFIRM = "确认";
        [TranslateMsg("所有按钮通用的是")]
        public const string YES = "是";
        [TranslateMsg("所有按钮通用的否")]
        public const string NO = "否";
        [TranslateMsg("所有提示通用的错误")]
        public const string ERROR = "错误";
        [TranslateMsg("发现了新版本的游戏，询问玩家是否要打开一个外部链接进行下载")]
        public const string WANNA_DOWNLOAD_NEW_VERSION = "发现了更新版本的游戏，是否下载？";
    }
    public class MainManager : Manager
    {
        public UIManager ui => getManager<UIManager>();
        public NetworkingManager network => getManager<NetworkingManager>();
        public GameManager game => getManager<GameManager>();

        LangManager lang => getManager<LangManager>();

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
                _ = network.displayPanelForLAN();
            });
            main.MainMenu.LobbyButtonButtonBlack.asButton.onClick.set(() =>
            {
                if (network.client.account != null)
                {
                    main.display(main.NetworkingPage);
                    _ = network.displayPanelForLobby();
                }
                else
                {
                    main.MainMenu.RegisterPanel.hide();
                    main.MainMenu.LoginPanel.display();
                }
            });
            main.display(main.Loading);
        }
        protected void Start()
        {
            StageLogger logger = new StageLogger(new int[] { 1, 2, 50 });
            logger.OnStageChange += (progress, text) =>
            {
                mainUI.Loading.LoggerText.text = text;
                mainUI.Loading.ProgressBarSlider.value = progress;
            };
            //加载卡片资源
            _loadCardTask = getManager<CardManager>().load(
                new System.Reflection.Assembly[] { typeof(THHGame).Assembly },
                new string[] { "TouhouHeartstone", "TouhouHeartstone.Builtin" },
                false,
                null,
                logger
            );
        }
        protected void Update()
        {
            if (_loadCardTask != null)
            {
                if (_loadCardTask.IsCompleted)
                {
                    //加载卡片资源成功
                    _loadCardTask.Dispose();
                    _loadCardTask = null;
                    UIManager ui = getManager<UIManager>();
                    Main main = ui.getObject<Main>();
                    main.display(main.MainMenu);
                    onLoadingFinish?.Invoke();
                    //开始检查新版本
                    _getNewVersionTask = network.getLatestVersion();
                }
                else if (_loadCardTask.IsFaulted || _loadCardTask.IsCanceled)
                {
                    _loadCardTask.Dispose();
                    _loadCardTask = null;
                    UIManager ui = getManager<UIManager>();
                    ui.getObject<UIDialog>().ShowConfirm(lang._(StringTable.ERROR), lang._(MMANAGE_GAME_FAIL_LOAD), () => Application.Quit());
                }
            }
            if (_getNewVersionTask != null)
            {
                if (_getNewVersionTask.IsCompleted)
                {
                    ClientUpdateInfo latestVersion;
                    try
                    {
                        latestVersion = _getNewVersionTask.Result;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("获取新版本游戏失败");
                        throw e;
                    }
                    finally
                    {
                        _getNewVersionTask.Dispose();
                        _getNewVersionTask = null;
                    }
                    if (SematicTagVersion.Parse(getCurrentVersion()) < SematicTagVersion.Parse(latestVersion.Version) && !string.IsNullOrEmpty(latestVersion.FullPackageUrl))
                    {
                        //如果有新版本，询问玩家要不要下载
                        getManager<UIManager>().getObject<UIDialog>().ShowQuestion(null, lang._(StringTable.WANNA_DOWNLOAD_NEW_VERSION)
                        , () => { Application.OpenURL(latestVersion.FullPackageUrl); });
                    }
                }
                else if (_getNewVersionTask.IsFaulted || _getNewVersionTask.IsCanceled)
                {
                    _getNewVersionTask.Dispose();
                    _getNewVersionTask = null;
                }
            }
        }

        [TranslateMsg]
        private const string MMANAGE_GAME_FAIL_LOAD = "游戏资源加载失败！请重新启动游戏";
        [TranslateMsg]
        private const string MMANGGE_MYSELF = "自己";

        private void showLocalRoom()
        {
            Main main = ui.getObject<Main>();

            currentRoomInfo = new RoomInfo();
            var selfPlayer = new RoomPlayerInfo() { name = lang._(MMANGGE_MYSELF) };
            currentRoomInfo.playerList.Add(selfPlayer);
            GameOption option = new GameOption();
            var rand = new byte[4];
            System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(rand);
            option.randomSeed = BitConverter.ToInt32(rand, 0);
            currentRoomInfo.setOption(option);

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
        #region 私有成员
        string getCurrentVersion()
        {
            return BuildInformation.Instance.Version;
        }
        Task _loadCardTask = null;
        Task<ClientUpdateInfo> _getNewVersionTask = null;
        #endregion
    }
}
