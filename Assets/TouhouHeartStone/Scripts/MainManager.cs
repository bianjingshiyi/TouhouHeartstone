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
using UnityEngine.SceneManagement;
using Game.Replays;
using Controllers;
using MongoDB.Bson.Serialization;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    partial class MainCtrl : IController<MainCtrl>
    {
        public MainCtrl(IAppManager appManager)
        {
            app = appManager;
        }
        public void onAwake()
        {

        }
        public void onStart()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == "NewUI")
                    lobbySceneCtrl = new LobbySceneCtrl(app, this, scene);
            }
        }

        public ReplaySystem replays { get; } = new ReplaySystem();

        public LobbySceneCtrl lobbySceneCtrl { get; private set; }
        public IAppManager app { get; }
        public MainCtrl main => this;
    }
    class LobbySceneCtrl : IController<MainCtrl>
    {
        public LobbySceneCtrl(IAppManager app, MainCtrl main, Scene scene)
        {
            this.app = app;
            this.main = main;
            mainMenuCtrl = new MainMenuCtrl(app, main, scene.findInstanceAt<Transform>("Canvas/Main/MainMenu").gameObject.GetComponent<MainMenu>());
            replaySelectCtrl = new ReplaySelectPanelCtrl(app, main, scene.findInstanceAt<Transform>("Canvas/Main/Replay").gameObject.GetComponent<ReplaySelectPanel>());
            //editorCtrl = new EditorCtrl(app, main, scene.findInstanceAt<Transform>("Canvas/Main/EditorPanel").gameObject.GetComponent<EditorPanel>());
        }
        public IAppManager app { get; }
        public MainCtrl main { get; }
        public MainMenuCtrl mainMenuCtrl { get; }
        public ReplaySelectPanelCtrl replaySelectCtrl { get; }
        public EditorCtrl editorCtrl { get; }
        public CardEditorCtrl cardEditorCtrl { get; }
        public EffectEditorCtrl effectEditorCtrl { get; }
        public ActionNodeGraphCtrl actionNodeGraphCtrl { get; }
    }
    class MainMenuCtrl : IController<MainCtrl>
    {
        public MainMenuCtrl(IAppManager app, MainCtrl main, MainMenu ui)
        {
            this.app = app;
            this.main = main;
            _ui = ui;
            _ui.onReplayButtonClick += onReplayButtonClick;
        }
        public void display()
        {
            _ui.gameObject.SetActive(true);
        }
        private async void onReplayButtonClick()
        {
            _ui.gameObject.SetActive(false);
            await main.lobbySceneCtrl.replaySelectCtrl.displayAsync();
        }
        void onEditorButtonClick()
        {
            //_ui.gameObject.SetActive(false);
            main.lobbySceneCtrl.editorCtrl.display();
        }
        public MainCtrl main { get; }
        public IAppManager app { get; }
        MainMenu _ui;
    }
    partial class ReplaySelectPanelCtrl : IController<MainCtrl>
    {
        public ReplaySelectPanelCtrl(IAppManager app, MainCtrl main, ReplaySelectPanel ui)
        {
            this.app = app;
            this.main = main;
            _ui = ui;
            _ui.onAwake();
            _ui.onPlayButtonClick += onPlayButtonClick;
            _ui.onReplayListClick += onReplayListClick;
            _ui.onBackButtonClick += onReturnButtonClick;
        }
        private void onReturnButtonClick(ReplaySelectPanel ui)
        {
            ui.gameObject.SetActive(false);
            main.lobbySceneCtrl.mainMenuCtrl.display();
        }
        public async Task displayAsync()
        {
            //读取Replay文件头
            _paths = app.fileManager.getPersistentFiles(main.replays.getSaveDir(), "*." + main.replays.getSaveFileExtension(), true);
            string[][] fileHeads = await app.fileManager.readPersistentTextFiles(_paths);
            //转化为显示数据并刷新显示
            ReplayItemViewData[] viewDatas = new ReplayItemViewData[fileHeads.Length];
            for (int i = 0; i < fileHeads.Length; i++)
            {
                if (BsonSerializer.Deserialize<int[]>(fileHeads[i][0]) is int[] masterIds)
                {
                    viewDatas[i] = new ReplayItemViewData()
                    {
                        name = Path.GetFileNameWithoutExtension(_paths[i]),
                        playerAMasterDefRef = new DefineReference(0, masterIds[0]),
                        playerBMasterDefRef = new DefineReference(0, masterIds[1])
                    };
                }
                else if (BsonSerializer.Deserialize<DefineReference[]>(fileHeads[i][0]) is DefineReference[] masterDefRefs)
                {
                    viewDatas[i] = new ReplayItemViewData()
                    {
                        name = Path.GetFileNameWithoutExtension(_paths[i]),
                        playerAMasterDefRef = masterDefRefs[0],
                        playerBMasterDefRef = masterDefRefs[1]
                    };
                }
            }
            _ui.updateView(new ReplaySelectViewData()
            {
                replayItemViewDatas = viewDatas
            });
            _ui.gameObject.SetActive(true);
        }
        public void hide()
        {
            _ui.gameObject.SetActive(false);
        }
        private async void onPlayButtonClick(ReplaySelectPanel ui)
        {
            string path = _paths[_selectIndex];
            var content = await app.fileManager.readPersistentTextFile(path, 1, 1);
            string json = content[0];
            ReplayData replay = BsonSerializer.Deserialize<ReplayData>(json);
            hide();
            _ = (app as MainManager).getManager<GameManager>().startReplayGame(replay);
        }
        private void onReplayListClick(ReplaySelectPanel ui, int index)
        {
            _selectIndex = index;
            _ui.updateView(new ReplaySelectViewData()
            {
                selectIndex = index
            });
        }
        public MainCtrl main { get; }
        public IAppManager app { get; }
        string[] _paths;
        int _selectIndex = -1;
        ReplaySelectPanel _ui;
    }
    public class MainManager : Manager, IAppManager
    {
        public long NewSnowflakeId()
        {
            return _snowflakeGen.NewId();
        }
        protected override void onAwake()
        {
            base.onAwake();
            //初始化序列化设置
            JsonHelper.init();
            // 初始化节点图的类型转换。
            GraphTypeHelper.init();

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            if (ui != null)
            {
                Main main = ui.getObject<Main>();
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
            }
            //_mainCtrl = new MainCtrl(this);
            //_mainCtrl.onAwake();

            // 初始化玩家Key.
            InitPlayerKey();

        }
        protected async void Start()
        {
            StageLogger logger = new StageLogger(new int[] { 1, 2, 50 });
            logger.OnStageChange += (progress, text) =>
            {
                if (mainUI == null)
                    return;
                mainUI.Loading.LoggerText.text = text;
                mainUI.Loading.ProgressBarSlider.value = progress;
            };
            //_mainCtrl.onStart();
            if (_mainSceneCtrl == null)
            {
                _mainSceneCtrl = TransformHelper.findComponentInAllScenes<MainSceneCtrl>();
            }
            if (_mainSceneCtrl != null)
            {
                _mainSceneCtrl.menu.hide();
            }
            //加载内置卡片数据
            _loadCardTask = card.loadBuiltinCards(
                new System.Reflection.Assembly[] { typeof(THHGame).Assembly },
                new string[] { "TouhouHeartstone", "TouhouHeartstone.Builtin" },
                false,
                null,
                logger,
                false
            );
            anims.loadBuiltinAnimations();
            //加载外部卡片数据
            await card.loadCardPoolInfosAsync();
            //foreach (var pair in cardPoolInfos)
            //{
            //    try
            //    {
            //        await card.loadCardPool(pair.Key);
            //    }
            //    catch (Exception e)
            //    {
            //        Debug.LogError("加载外部卡池" + pair.Key + "失败：" + e);
            //    }
            //}
            if (_loadCardTask != null)
                await _loadCardTask;
            if (_mainSceneCtrl != null)
            {
                _mainSceneCtrl.menu.display();
            }
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
        #endregion

        #region 属性字段
        public UIManager ui => getManager<UIManager>();
        public NetworkingManager network => getManager<NetworkingManager>();
        public GameManager game => getManager<GameManager>();
        public SEManager seManager => getManager<SEManager>();
        public FileManager fileManager => new FileManager();
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
        public IResourceManager resourceManager => getManager<ResourceManager>();
        public CardManager card => getManager<CardManager>();
        public DeckManager deck => getManager<DeckManager>();
        public BGMManager bgm => getManager<BGMManager>();
        public TriggerManager triggers => this.findInstance<TriggerManager>();
        public CacheManager cache => getManager<CacheManager>();
        public AnimationManager anims => getManager<AnimationManager>();
        public EditorManager editor => _editor;
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
        RoomData currentRoomData;
        Task _loadCardTask = null;
        Task<ClientUpdateInfo> _getNewVersionTask = null;
        int _playerKey;
        SnowflakeGenerator _snowflakeGen;
        #endregion
    }
}
