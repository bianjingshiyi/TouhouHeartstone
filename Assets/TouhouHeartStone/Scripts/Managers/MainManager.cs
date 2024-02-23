using System;
using System.IO;
using System.Threading.Tasks;
using BJSYGameCore;
using Controllers;
using CustomBuilder;
using Game.PVE;
using MukioI18n;
using NitoriNetwork.Common;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class MainManager : Manager
    {
        public long NewSnowflakeId()
        {
            return _snowflakeGen.NewId();
        }
        protected override void onAwake()
        {
            base.onAwake();
            checkOldPersistentData();
            //初始化序列化设置
            JsonHelper.init();
            // 初始化节点图的类型转换。
            GraphTypeHelper.init();

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            if (ui != null)
            {
                Main main = ui.getObject<Main>();
                main.display(main.Loading);
            }

            // 初始化玩家Key.
            InitPlayerKey();
            database.init();
            cache.init();
            deck.init();
            network.start();
        }
        protected async void Start()
        {
            //_mainCtrl.onStart();
            if (_mainSceneCtrl == null)
            {
                _mainSceneCtrl = TransformHelper.findComponentInAllScenes<MainSceneCtrl>();
            }
            if (_mainSceneCtrl != null)
            {
                _mainSceneCtrl.menu.hide();
            }
            anims.loadBuiltinAnimations();
            //加载外部卡片数据
            await card.loadAllCardPoolInfosAsync();
            await deck.checkPresetDecks();

            //加载卡片资源成功
            UIManager ui = getManager<UIManager>();
            Main main = ui.getObject<Main>();
            main.display(main.MainMenu);
            onLoadingFinish?.Invoke();
            if (SteamManager.disableSteamWorks)
            {
                //开始检查新版本
                _getNewVersionTask = network.getLatestVersion();
            }
            else
            {
                steam.initialize();
            }

            if (_mainSceneCtrl != null)
            {
                _mainSceneCtrl.menu.display();
            }
        }
        protected void Update()
        {
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

        //private void showLocalRoom()
        //{
        //    Main main = ui.getObject<Main>();
        //    _ = network.logic.CreateLocalRoom();
        //    currentRoomData = network.logic.CurrentRoom;
        //    currentRoomData.maxPlayerCount = 2; // 炉石两个人

        //    HearthstoneOption option = new HearthstoneOption();
        //    var rand = new byte[4];
        //    System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(rand);
        //    option.randomSeed = BitConverter.ToInt32(rand, 0);
        //    currentRoomData.setOption(option);

        //    main.RoomPanel.ResetEvents();
        //    main.RoomPanel.SetMode(RoomPanel.Mode.Local);
        //    main.RoomPanel.SetSelfID(network.logic.localPlayer.id);
        //    main.RoomPanel.SetRightText("");
        //    main.RoomPanel.SetInfo(currentRoomData);

        //    main.display(main.RoomPanel);
        //    main.RoomPanel.onStartClick += () =>
        //    {
        //        game.startLocalGame(currentRoomData);
        //    };
        //    main.RoomPanel.onQuitClick += () =>
        //    {
        //        currentRoomData = null;
        //        network.quitRoom();

        //        main.display(main.MainMenu);
        //    };
        //    main.RoomPanel.onRoomPropUpdate += (name, obj) =>
        //    {
        //        currentRoomData.setProp(name, obj);
        //        main.RoomPanel.SetInfo(currentRoomData);
        //    };
        //    main.RoomPanel.onPlayerRemove += (id) =>
        //    {
        //        for (int i = 0; i < currentRoomData.playerDataList.Count; i++)
        //        {
        //            if (currentRoomData.playerDataList[i].id == id)
        //            {
        //                currentRoomData.playerDataList.RemoveAt(i--);
        //            }
        //        }
        //        main.RoomPanel.SetInfo(currentRoomData);
        //    };
        //    main.RoomPanel.onAIPlayerAdd += () =>
        //    {
        //        network.logic.AddAIPlayer();
        //        main.RoomPanel.SetInfo(currentRoomData);
        //    };
        //    main.RoomPanel.onSetPlayerDeck += (id, cardsId) =>
        //    {
        //        for (int i = 0; i < currentRoomData.playerDataList.Count; i++)
        //        {
        //            var player = currentRoomData.playerDataList[i];
        //            if (player.id == id)
        //            {
        //                Deck deck = currentRoomData.playerDataList[i].getDeck();
        //                if (deck != null)
        //                {
        //                    deck.masterId = cardsId[0];
        //                    deck.setCards(cardsId.Skip(1));
        //                }
        //                else
        //                {
        //                    deck = new Deck(cardsId[0], cardsId.Skip(1));
        //                }
        //                currentRoomData.playerDataList[i].setDeck(deck);
        //                if (player.type == RoomPlayerType.ai)
        //                {
        //                    player.setReady(true);
        //                }
        //            }
        //        }
        //        main.RoomPanel.SetInfo(currentRoomData);
        //    };
        //}
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
        #region 私有方法
        private void InitPlayerKey()
        {
            if (PlayerPrefs.HasKey("PlayerKey"))
            {
                _playerKey = PlayerPrefs.GetInt("PlayerKey");
            }
            else
            {
                _playerKey = Guid.NewGuid().GetHashCode();
                PlayerPrefs.SetInt("PlayerKey", _playerKey);
            }
            _snowflakeGen = new SnowflakeGenerator(_playerKey);
        }
        string getCurrentVersion()
        {
            return BuildInformation.Instance.Version;
        }
        private void checkOldPersistentData()
        {
            var newDir = Application.persistentDataPath;
            string folderName = "醉梦传说";
#if DEVELOPMENT_BUILD
            folderName = "醉梦传说测试版";
#elif SNAPSHOT_BUILD
            folderName = "醉梦传说快照";
#endif
            var oldDir = Path.Combine(Path.GetDirectoryName(newDir), folderName);
            if (!Directory.Exists(oldDir))
                return;
            // 检测新的目录下有没有卡组、卡池或者贴图等关键数据，如果存在则不执行，防止新版本数据被旧版本覆盖。
            foreach (var folder in importantPersistentFolders)
            {
                if (Directory.Exists(Path.Combine(newDir, folder)))
                    return;
            }
            foreach (var file in importantPersistentFiles)
            {
                if (File.Exists(Path.Combine(newDir, file)))
                    return;
            }
            // 开始移动。
            foreach (var folder in importantPersistentFolders)
            {
                var oldFolder = Path.Combine(oldDir, folder);
                var newFolder = Path.Combine(newDir, folder);
                if (!Directory.Exists(oldFolder))
                    continue;
                Directory.Move(oldFolder, newFolder);
            }
            foreach (var file in importantPersistentFiles)
            {
                var oldFile = Path.Combine(oldDir, file);
                var newFile = Path.Combine(newDir, file);
                if (!File.Exists(oldFile))
                    continue;
                File.Move(oldFile, newFile);
            }
        }
        #endregion

        #region 属性字段
        private static readonly string[] importantPersistentFolders = new string[]
        {
            "cache",
            "CardPools",
            "pve",
            "replay",
            "Textures"
        };
        private static readonly string[] importantPersistentFiles = new string[]
        {
            "token",
            "data.db"
        };
        public UIManager ui => getManager<UIManager>();
        public NetworkingManager network => getManager<NetworkingManager>();
        public GameManager game => getManager<GameManager>();
        public EventManager eventManager => getManager<EventManager>();
        public SEManager seManager => getManager<SEManager>();
        public ConfigManager config => getManager<ConfigManager>();
        public ReplayManager replays => getManager<ReplayManager>();
        public FileManager fileManager => new FileManager();
        public TutorialManager tutorial => getManager<TutorialManager>();
        public SteamManager steam => getManager<SteamManager>();
        /// <summary>
        /// 语言管理器，仅用于控制语言的修改。如果你需要翻译，请使用 i18n._();
        /// </summary>
        public LangManager lang => getManager<LangManager>();
        /// <summary>
        /// 翻译提供者
        /// </summary>
        public ITranslationProvider i18n => lang;
        /// <summary>
        /// 语言管理器，仅用于控制语言的修改。如果你需要翻译，请使用 i18n._();
        /// </summary>
        public ILanguageManager langManager => getManager<LangManager>();
        public CardManager card => getManager<CardManager>();
        public DatabaseManager database => getManager<DatabaseManager>();
        public DeckManager deck => getManager<DeckManager>();
        public BGMManager bgm => getManager<BGMManager>();
        public CacheManager cache => getManager<CacheManager>();
        public AnimationManager anims => getManager<AnimationManager>();
        public EditorManager editor => _editor;
        public PVEManager pve => getManager<PVEManager>();
        public Camera camera => _camera;
        public int playerKey => _playerKey;
        [SerializeField]
        Camera _camera;
        [SerializeField]
        EditorManager _editor;
        [SerializeField]
        MainSceneCtrl _mainSceneCtrl;
        //MainCtrl _mainCtrl;
        [SerializeField]
        UnityEvent onLoadingFinish;
        Main mainUI => ui.getObject<Main>();
        Task<ClientUpdateInfo> _getNewVersionTask = null;
        int _playerKey;
        SnowflakeGenerator _snowflakeGen;
        #endregion
    }
}
