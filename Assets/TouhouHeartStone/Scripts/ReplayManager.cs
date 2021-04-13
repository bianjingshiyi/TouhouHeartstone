using TouhouHeartstone;
using BJSYGameCore;
using System;
using Newtonsoft.Json;
using MongoDB.Bson;
using System.IO;
using UnityEngine;
using TouhouCardEngine.Interfaces;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using UI;

namespace Game
{
    public class ReplayManager : Manager
    {
        Replay rep;
        float startTime = 0;

        private GameManager _gm;
        public GameManager gm => _gm = _gm ?? getManager<GameManager>();

        protected override void onAwake()
        {
            foreach (var item in typeof(Response).Assembly.GetTypes())
            {
                if (item.IsSubclassOf(typeof(Response)))
                    BsonClassMap.LookupClassMap(item);
            }
        }

        private void Start()
        {
            gm.onGameEnd += onGameEnd;
            gm.onGameStart += onGameStart;
        }

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
            if (!Directory.Exists(ReplayFolder()))
            {
                Directory.CreateDirectory(ReplayFolder());
            }
            File.WriteAllText(Path.Combine(ReplayFolder(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "." + Extension), str);
        }

        void onPlayerResponse(int id, IResponse response)
        {
            UberDebug.LogChannel("Replay", $"[{id}] LOG: {response}");
            rep.logs.Add(new Replay.ReplayLogLine(id, response, Time.time - startTime));
        }

        public static string ReplayFolder()
        {
            return Path.Combine(Application.persistentDataPath, "replay");
        }

        public const string Extension = "zgrep";
        [SerializeField]
        Replay _currentReplay;
        public void Play(string fileName)
        {
            string str = File.ReadAllText(Path.Combine(ReplayFolder(), fileName));
            Replay readRep = BsonSerializer.Deserialize(str, typeof(Replay)) as Replay;
            getManager<GameManager>().startReplayGame(readRep);
#if UNITY_EDITOR
            _currentReplay = readRep;
            _currentReplay.showContents();
#endif
        }
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
                        UberDebug.LogChannel("Replay", "请求Discover，但即将播放的是" + currentLog.response.GetType().ToString());
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
