using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BJSYGameCore;
using Controllers;
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
        public async Task<ReplayMeta[]> loadMetas(string[] paths)
        {
            var metas = new List<ReplayMeta>();
            foreach (var path in paths)
            {
                ReplayMeta meta;
                try
                {
                    using Stream stream = fileManager.getPersistentFileStream(path);
                    byte[] bytes = new byte[5];
                    stream.Read(bytes, 0, bytes.Length);
                    if (bytes[0] == REPLAY_HEADER_BYTE0 &&
                        bytes[1] == REPLAY_HEADER_BYTE1)
                    {
                        short headerLength = BitConverter.ToInt16(bytes, 2);
                        bytes = new byte[headerLength];
                        stream.Read(bytes, 0, bytes.Length);

                        var seri = JsonHelper.fromBytes<SerializableReplayMeta>(bytes);
                        meta = seri.toMeta();
                    }
                    else
                    {
                        string[] fileHeads = await fileManager.readPersistentTextFile(path, 0, 1);
                        string fileHead = fileHeads[0];
                        var seri = JsonHelper.fromJson<SerializableReplayMeta>(fileHead);
                        meta = seri.toMeta();
                    }
                }
                catch
                {
                    meta = new InvalidReplayMeta()
                    {
                        name = Path.GetFileName(path)
                    };
                }
                meta.path = path;
                metas.Add(meta);
            }
            return metas.ToArray();
        }
        public async Task<ReplayData> loadReplay(string path)
        {
            try
            {
                using Stream stream = fileManager.getPersistentFileStream(path);
                byte[] bytes = new byte[5];
                stream.Read(bytes, 0, bytes.Length);
                if (bytes[0] == REPLAY_HEADER_BYTE0 &&
                    bytes[1] == REPLAY_HEADER_BYTE1)
                {
                    short headerLength = BitConverter.ToInt16(bytes, 2);
                    bytes = new byte[stream.Length - headerLength - 5];
                    stream.Position += headerLength;
                    stream.Read(bytes, 0, bytes.Length);

                    var seri = JsonHelper.fromBytes<SerializableReplayData>(bytes);
                    return seri.toData();
                }
                else
                {
                    var content = await fileManager.readPersistentTextFile(path, 1, 1);
                    string json = content[0];
                    var seri = JsonHelper.fromJson<SerializableReplayData>(json);
                    return seri.toData();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"回放{path}加载错误：{e}");
            }
            return null;
        }
        public static string getReplayFolder()
        {
            return Path.Combine(Application.persistentDataPath, DIRECTORY);
        }
        public static string getNowReplayFileName()
        {
            return $"{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.{EXTENSION}";
        }
        public bool deleteReplay(string path)
        {
            return main.fileManager.deletePersistentFile(path);
        }
        public async Task saveReplay(ReplayMeta meta, ReplayData replay)
        {
            var path = Path.Combine(DIRECTORY, getNowReplayFileName());
            var seriMeta = new SerializableReplayMeta(meta);
            var seriData = new SerializableReplayData(replay);
            var head = seriMeta.toBytes();
            var content = seriData.toBytes();
            byte[] bytes = new byte[5 + head.Length + content.Length];
            bytes[0] = REPLAY_HEADER_BYTE0;
            bytes[1] = REPLAY_HEADER_BYTE1;
            BitConverter.GetBytes((short)head.Length).CopyTo(bytes, 2);
            bytes[4] = 0x92;
            head.CopyTo(bytes, 5);
            content.CopyTo(bytes, 5 + head.Length);
            await main.fileManager.savePersistentFile(path, bytes);
        }
        #endregion

        #region 属性字段
        public const byte REPLAY_HEADER_BYTE0 = (byte)'z';
        public const byte REPLAY_HEADER_BYTE1 = (byte)'r';
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
