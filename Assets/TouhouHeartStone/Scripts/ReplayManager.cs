using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UnityEngine;

namespace Game
{
    public class ReplayManager : Manager
    {
        #region 公有方法
        public string[] getPaths()
        {
            return fileManager.getPersistentFiles(DIRECTORY, $"*.{EXTENSION}", true);
        }
        public static string getReplayFolder()
        {
            return Path.Combine(Application.persistentDataPath, DIRECTORY);
        }
        public async void Play(string fileName)
        {
            string str = File.ReadAllText(Path.Combine(getReplayFolder(), fileName));
            Replay readRep = BsonSerializer.Deserialize(str, typeof(Replay)) as Replay;
#if UNITY_EDITOR
            _currentReplay = readRep;
            _currentReplay.showContents();
#endif
            await getManager<GameManager>().startReplayGame(readRep);
        }
        #endregion

        #region 私有方法

        #region 生命周期
        protected override void onAwake()
        {
            foreach (var item in typeof(Response).Assembly.GetTypes())
            {
                if (item.IsSubclassOf(typeof(Response)))
                    BsonClassMap.LookupClassMap(item);
            }
            gm.onGameEnd += onGameEnd;
            gm.onGameStart += onGameStart;
        }
        #endregion

        #region 事件回调
        private void onGameStart()
        {
            // gm.game.onPlayerResponse += onPlayerResponse;
            rep = new Replay(gm.game);
            startTime = Time.time;
            UberDebug.LogChannel("Replay", $"Log Start!");
        }

        private void onGameEnd()
        {
            UberDebug.LogChannel("Replay", $"Log End!");

            var str = rep.ToJson();
            if (!Directory.Exists(getReplayFolder()))
            {
                Directory.CreateDirectory(getReplayFolder());
            }
            File.WriteAllText(Path.Combine(getReplayFolder(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "." + EXTENSION), str);
        }
        void onPlayerResponse(int id, IResponse response)
        {
            UberDebug.LogChannel("Replay", $"[{id}] LOG: {response}");
            rep.logs.Add(new Replay.ReplayLogLine(id, response, Time.time - startTime));
        }
        #endregion

        #endregion

        #region 属性字段
        public const string DIRECTORY = "replay";
        public const string EXTENSION = "ddrep";
        private FileManager fileManager => main.fileManager;
        public GameManager gm => main.game;
        private MainManager main => this.findInstance<MainManager>();
        Replay rep;
        float startTime = 0;
        [SerializeField]
        Replay _currentReplay;
        #endregion

    }

    public class ReplayPlayer
    {
        Replay replay;
        CardEngine game;
        float startTime;
        bool waitForTime = true;

        public ReplayPlayer(Replay rep, CardEngine game, bool wait = true)
        {
            replay = rep;
            this.game = game;

            startTime = Time.time;
            this.waitForTime = wait;

            game.answers.onRequest += Answers_onRequest;
        }

        int lineCounter = 0;
        Replay.ReplayLogLine currentLog
        {
            get
            {
                if (lineCounter < replay.logs.Count)
                    return replay.logs[lineCounter];
                return null;
            }
        }

        Task<bool> answerCurrentLog()
        {
            UberDebug.LogChannel("Replay", $"[{currentLog.player}] Resp: {currentLog.response}");
            return game.answers.answer(currentLog.player, currentLog.response);
        }

        private async void Answers_onRequest(IRequest obj)
        {
            // 等待时间
            if (waitForTime && currentLog.time > (Time.time - startTime))
            {
                await Task.Delay((int)((currentLog.time - (Time.time - startTime)) * 1000));
            }

            // 判断是否是自己的行动]
            while (!obj.playersId.Contains(currentLog.player))
            {
                UberDebug.LogChannel("Replay", $"顺序不正确，等待其他线程执行。当前requestID: {obj.playersId[0]}, logID: {currentLog.player}");
                lineCounter++;
            }
#if false
            _ = answerCurrentLog();
            lineCounter++;

            while (currentLog.response is InitReplaceResponse)
            {
                _ = answerCurrentLog();
                lineCounter++;
            }

            return;
#else
            switch (obj)
            {
                case InitReplaceRequest ip:
                    while (currentLog.response is InitReplaceResponse)
                    {
                        _ = answerCurrentLog();
                        lineCounter++;
                    }
                    break;
                case DiscoverRequest disc:
                    if (!(currentLog.response is DiscoverResponse))
                    {
                        UberDebug.LogChannel("Replay", $"请求Discover，但即将播放的是{currentLog.response.GetType().ToString()}");
                        break;
                    }
                    _ = answerCurrentLog();
                    lineCounter++;
                    break;
                case FreeActRequest fa:
                    _ = answerCurrentLog();
                    lineCounter++;
                    break;
                default:
                    _ = answerCurrentLog();
                    lineCounter++;
                    break;
            }
#endif
        }
    }
}
