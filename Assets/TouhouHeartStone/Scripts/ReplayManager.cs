using System;
using System.IO;
using System.Threading.Tasks;
using BJSYGameCore;
using Game.Replays;
using MongoDB.Bson;
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
        public static string getNowReplayFileName()
        {
            return $"{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.{EXTENSION}";
        }
        public async Task saveReplay(ReplayMeta meta, ReplayData replay)
        {
            var path = Path.Combine(DIRECTORY, getNowReplayFileName());
            var seriMeta = new SerializableReplayMeta(meta);
            var seriData = new SerializableReplayData(replay);
            var head = seriMeta.ToJson();
            var content = seriData.ToJson();
            await main.fileManager.savePersistentFile(path, new string[]
            {
                head,
                content
            });
        }
        #endregion

        #region 属性字段
        public const string DIRECTORY = "replay";
        public const string EXTENSION = "ddrep";
        private FileManager fileManager => main.fileManager;
        public GameManager gm => main.game;
        private MainManager main => this.findInstance<MainManager>();
        #endregion

    }

    //    public class ReplayPlayer
    //    {
    //        ReplayData replay;
    //        CardEngine game;
    //        float startTime;
    //        bool waitForTime = true;

    //        public ReplayPlayer(ReplayData rep, CardEngine game, bool wait = true)
    //        {
    //            replay = rep;
    //            this.game = game;

    //            startTime = Time.time;
    //            this.waitForTime = wait;

    //            game.answers.onRequest += Answers_onRequest;
    //        }

    //        int lineCounter = 0;
    //        Replay.ReplayLogLine currentLog
    //        {
    //            get
    //            {
    //                if (lineCounter < replay.logs.Count)
    //                    return replay.logs[lineCounter];
    //                return null;
    //            }
    //        }

    //        Task<bool> answerCurrentLog()
    //        {
    //            UberDebug.LogChannel("Replay", $"[{currentLog.player}] Resp: {currentLog.response}");
    //            return game.answers.answer(currentLog.player, currentLog.response);
    //        }

    //        private async void Answers_onRequest(IRequest obj)
    //        {
    //            // 等待时间
    //            if (waitForTime && currentLog.time > (Time.time - startTime))
    //            {
    //                await Task.Delay((int)((currentLog.time - (Time.time - startTime)) * 1000));
    //            }

    //            // 判断是否是自己的行动]
    //            while (!obj.playersId.Contains(currentLog.player))
    //            {
    //                UberDebug.LogChannel("Replay", $"顺序不正确，等待其他线程执行。当前requestID: {obj.playersId[0]}, logID: {currentLog.player}");
    //                lineCounter++;
    //            }
    //#if false
    //            _ = answerCurrentLog();
    //            lineCounter++;

    //            while (currentLog.response is InitReplaceResponse)
    //            {
    //                _ = answerCurrentLog();
    //                lineCounter++;
    //            }

    //            return;
    //#else
    //            switch (obj)
    //            {
    //                case InitReplaceRequest ip:
    //                    while (currentLog.response is InitReplaceResponse)
    //                    {
    //                        _ = answerCurrentLog();
    //                        lineCounter++;
    //                    }
    //                    break;
    //                case DiscoverRequest disc:
    //                    if (!(currentLog.response is DiscoverResponse))
    //                    {
    //                        UberDebug.LogChannel("Replay", $"请求Discover，但即将播放的是{currentLog.response.GetType().ToString()}");
    //                        break;
    //                    }
    //                    _ = answerCurrentLog();
    //                    lineCounter++;
    //                    break;
    //                case FreeActRequest fa:
    //                    _ = answerCurrentLog();
    //                    lineCounter++;
    //                    break;
    //                default:
    //                    _ = answerCurrentLog();
    //                    lineCounter++;
    //                    break;
    //            }
    //#endif
    //        }
    //    }
}
