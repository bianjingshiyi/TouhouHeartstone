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
    public class MainManager : Manager, IAppManager
    {
        public UIManager ui => getManager<UIManager>();
        public NetworkingManager network => getManager<NetworkingManager>();
        public GameManager game => getManager<GameManager>();
        public FileManager fileManager => new FileManager();
        public LangManager lang => getManager<LangManager>();
        public ILanguageManager langManager => getManager<LangManager>();
        MainCtrl _mainCtrl;
        [SerializeField]
        UnityEvent onLoadingFinish;
        Main mainUI => ui.getObject<Main>();

        RoomData currentRoomData;

        protected override void onAwake()
        {
            base.onAwake();
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            Main main = ui.getObject<Main>();
            main.MainMenu.vsBotButton.onClick.set(showLocalRoom);
            main.MainMenu.lANButton.onClick.set(() =>
            {
                main.display(main.NetworkingPage);
                network.displayPanelForLAN();
            });
            main.MainMenu.serverButton.onClick.set(() =>
            {
                if (network.logic.Logined)
                {
                    main.display(main.NetworkingPage);
                    network.displayPanelForLobby();
                }
                else
                {
                    main.MainMenu.registerPanel.hide();
                    main.MainMenu.loginPanel.display();
                }
            });
            main.display(main.Loading);
            _mainCtrl = new MainCtrl(this);
            _mainCtrl.onAwake();
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
            _mainCtrl.onStart();
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
                        Debug.LogError($"获取新版本游戏失败");
                        throw e;
                    }
                    finally
                    {
                        _getNewVersionTask.Dispose();
                        _getNewVersionTask = null;
                    }
                    if (SematicTagVersion.Parse(getCurrentVersion()) < SematicTagVersion.Parse(latestVersion.Version))
                    {
                        var pf = PlatformCompability.Current;
                        string downloadURL = null;
                        switch (pf.Platform)
                        {
                            case RuntimePlatform.WindowsPlayer:
                            case RuntimePlatform.WindowsEditor:
                                downloadURL = pf.Is64Bit ? latestVersion.Win64?.PackageUrl : latestVersion.Win32?.PackageUrl;
                                break;
                            case RuntimePlatform.Android:
                                downloadURL = latestVersion.Android?.PackageUrl;
                                break;
                            case RuntimePlatform.LinuxPlayer:
                            case RuntimePlatform.LinuxEditor:
                                downloadURL = latestVersion.Linux?.PackageUrl;
                                break;
                            default:
                                break;
                        }
                        //如果有新版本，询问玩家要不要下载
                        var dialog = getManager<UIManager>().getObject<UIDialog>();
                        if (downloadURL != null)
                        {
                            dialog.ShowQuestion(StringTable.HINT_TITLE, lang._(StringTable.WANNA_DOWNLOAD_NEW_VERSION), () =>
                            {
                                Application.OpenURL(downloadURL);
                            });
                        }
                        else
                        {
                            dialog.ShowConfirm(StringTable.HINT_TITLE, lang._(StringTable.INVALID_DOWNLOAD_URL), null);
                        }
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
            _ = network.logic.CreateLocalRoom();
            currentRoomData = network.logic.CurrentRoom;
            currentRoomData.maxPlayerCount = 2; // 炉石两个人

            HearthstoneOption option = new HearthstoneOption();
            var rand = new byte[4];
            System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(rand);
            option.randomSeed = BitConverter.ToInt32(rand, 0);
            currentRoomData.setOption(option);

            main.RoomPanel.ResetEvents();
            main.RoomPanel.SetMode(RoomPanel.Mode.Local);
            main.RoomPanel.SetSelfID(network.logic.localPlayer.id);
            main.RoomPanel.SetRightText("");
            main.RoomPanel.SetInfo(currentRoomData);

            main.display(main.RoomPanel);
            main.RoomPanel.OnStartClick += () =>
            {
                game.startLocalGame(currentRoomData);
            };
            main.RoomPanel.OnQuitClick += () =>
            {
                currentRoomData = null;
                network.quitRoom();

                main.display(main.MainMenu);
            };
            main.RoomPanel.OnRoomPropUpdate += (name, obj) =>
            {
                currentRoomData.setProp(name, obj);
                main.RoomPanel.SetInfo(currentRoomData);
            };
            main.RoomPanel.OnPlayerRemove += (id) =>
            {
                for (int i = 0; i < currentRoomData.playerDataList.Count; i++)
                {
                    if (currentRoomData.playerDataList[i].id == id)
                    {
                        currentRoomData.playerDataList.RemoveAt(i--);
                    }
                }
                main.RoomPanel.SetInfo(currentRoomData);
            };
            main.RoomPanel.OnAIPlayerAdd += () =>
            {
                network.logic.AddAIPlayer();
                main.RoomPanel.SetInfo(currentRoomData);
            };
            main.RoomPanel.OnSetPlayerDeck += (id, deck) =>
            {
                for (int i = 0; i < currentRoomData.playerDataList.Count; i++)
                {
                    var player = currentRoomData.playerDataList[i];
                    if (player.id == id)
                    {
                        currentRoomData.playerDataList[i].setDeck(deck);
                        if (player.type == RoomPlayerType.ai)
                        {
                            player.setReady(true);
                        }
                    }
                }
                main.RoomPanel.SetInfo(currentRoomData);
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
