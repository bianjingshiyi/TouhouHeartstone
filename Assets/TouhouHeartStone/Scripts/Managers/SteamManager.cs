#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX
#define STEAM_PLATFORM
#endif

#if DEVELOPMENT_BUILD || UNITY_EDITOR
#define STEAM_DEVELOPMENT
#endif

#if STEAM_PLATFORM && (STEAM_DEVELOPMENT || STEAM_BUILD)
#define STEAM_VALID
#endif

using UnityEngine;
using System;
using System.Collections.Generic;
using BJSYGameCore;
using UI;
#if STEAM_VALID
using Steamworks;
using TicketCallback = Steamworks.Callback<Steamworks.GetTicketForWebApiResponse_t>;
#endif
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class SteamManager : Manager
    {
        #region 公有方法

#if STEAM_VALID
        public async static Task<string> getAuthTicketForWebApi()
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            TicketCallback callback = null;
            callback = TicketCallback.Create(data =>
            {
                string ticket = BitConverter.ToString(data.m_rgubTicket).Replace("-", string.Empty);
                tcs.SetResult(ticket);
                callback.Dispose();
                callback = null;
            });
            var authTicket = SteamUser.GetAuthTicketForWebApi(SteamManager.client);
            return await tcs.Task;
        }
#else
        public static Task<string> getAuthTicketForWebApi() => throw new SteamworksNotValidException();
#endif

        #endregion

        #region 私有方法

        #region 生命周期

        protected override void onAwake()
        {
            base.onAwake();
            if (disableSteamWorks) 
                return;
            initialize();
        }

        // This should only ever get called on first load and after an Assembly reload, You should never Disable the Steamworks Manager yourself.
        protected virtual void OnEnable()
        {
#if STEAM_VALID
            if (!initialized)
                return;

            if (SteamAPIWarningMessageHook == null)
            {
                // Set up our callback to receive warning messages from Steam.
                // You must launch with "-debug_steamapi" in the launch args to receive warnings.
                SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
                SteamClient.SetWarningMessageHook(SteamAPIWarningMessageHook);
            }
#endif
        }

        // OnApplicationQuit gets called too early to shutdown the SteamAPI.
        // Because the SteamManager should be persistent and never disabled or destroyed we can shutdown the SteamAPI here.
        // Thus it is not recommended to perform any Steamworks work in other OnDestroy functions as the order of execution can not be garenteed upon Shutdown. Prefer OnDisable().
        protected virtual void OnDestroy()
        {
#if STEAM_VALID
            if (!initialized)
                return;

            SteamAPI.Shutdown();
#endif
        }

        protected virtual void Update()
        {
#if STEAM_VALID
            if (!initialized)
                return;

            // Run Steam client callbacks
            SteamAPI.RunCallbacks();
#endif
        }

        #endregion

        #region 事件回调
#if STEAM_VALID
        [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
        protected static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
        {
            Debug.LogWarning(pchDebugText);
        }
#endif
        #endregion

        // In case of disabled Domain Reload, reset static members before entering Play Mode.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitOnPlayMode()
        {
            everInitialized = false;
        }
        private void initialize()
        {
#if STEAM_VALID
            if (everInitialized)
            {
                // This is almost always an error.
                // The most common case where this happens is when SteamManager gets destroyed because of Application.Quit(),
                // and then some Steamworks code in some other OnDestroy gets called afterwards, creating a new SteamManager.
                // You should never call Steamworks functions in OnDestroy, always prefer OnDisable if possible.
                throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
            }

            if (!Packsize.Test())
            {
                Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
            }

            if (!DllCheck.Test())
            {
                Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
            }

#if !STEAM_DEVELOPMENT
            try
            {
                if (SteamAPI.RestartAppIfNecessary((AppId_t)appId))
                {
                    Debug.LogError("[Steamworks.NET] Not valid appId, restarting.", this);
                    Application.Quit();
                    return;
                }
            }
            catch (DllNotFoundException)
            {
                Debug.LogError("[Steamworks.NET] Could not find steam dll.", this);
                return;
            }
#endif

            // Initializes the Steamworks API.
            // If this returns false then this indicates one of the following conditions:
            // [*] The Steam client isn't running. A running Steam client is required to provide implementations of the various Steamworks interfaces.
            // [*] The Steam client couldn't determine the App ID of game. If you're running your application from the executable or debugger directly then you must have a [code-inline]steam_appid.txt[/code-inline] in your game directory next to the executable, with your app ID in it and nothing else. Steam will look for this file in the current working directory. If you are running your executable from a different directory you may need to relocate the [code-inline]steam_appid.txt[/code-inline] file.
            // [*] Your application is not running under the same OS user context as the Steam client, such as a different user or administration access level.
            // [*] Ensure that you own a license for the App ID on the currently active Steam account. Your game must show up in your Steam library.
            // [*] Your App ID is not completely set up, i.e. in Release State: Unavailable, or it's missing default packages.
            // Valve's documentation for this is located here:
            // https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
            initialized = SteamAPI.Init();
            if (!initialized)
            {
                Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
                return;
            }
            Debug.Log("[Steamworks.NET] SteamAPI is successfully initialized.", this);
            everInitialized = true;
#endif
        }

        #endregion


        #region 事件
#if STEAM_VALID
        protected SteamAPIWarningMessageHook_t SteamAPIWarningMessageHook;
#endif
        #endregion

        #region 属性字段
        public const string clientNormal = "zmcs";
        public const string clientPlaytest = "zmcs-test";
        public uint appId
        {
            get
            {
#if SNAPSHOT_BUILD
                return appIdPlaytest;
#else
                return appIdNormal;
#endif
            }
        }
        public static string client
        {
            get
            {
#if SNAPSHOT_BUILD
                return clientPlaytest;
#else
                return clientNormal;
#endif
            }
        }
        public static bool disableSteamWorks
        {
            get
            {
#if STEAM_VALID
                return false;
#else
                return true;
#endif
            }
        }
        public static bool initialized { get; private set;} = false;
        public static bool isValid => initialized;
        protected static bool everInitialized = false;
        [SerializeField]
        private uint appIdNormal = 1477010;
        [SerializeField]
        private uint appIdPlaytest = 2825850;
        #endregion
    }
    public class SteamworksNotValidException : Exception 
    { 
        public SteamworksNotValidException() : base("Steam is not valid on this platform.")
        {

        }
    }
}
