using TouhouHeartstone;
using BJSYGameCore;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using MongoDB.Bson;
using System.IO;
using UnityEngine;
using TouhouCardEngine.Interfaces;

namespace Game
{
    public class ReplayManager : Manager
    {
        [Serializable]
        public class ReplayLogLine
        {
            public int player;
            public float time;
            public IResponse response;

            public ReplayLogLine(int id, IResponse response, float time)
            {
                player = id;
                this.response = response;
                this.time = time;
            }
        }

        List<ReplayLogLine> logs = new List<ReplayLogLine>();
        float startTime = 0;

        private GameManager _gm;
        public GameManager gm => _gm = _gm ?? getManager<GameManager>();

        private void Start()
        {
            gm.onGameEnd += onGameEnd;
            gm.onGameStart += onGameStart;
        }

        private void onGameStart()
        {
            gm.game.onPlayerResponse += onPlayerResponse;
            logs.Clear();
            startTime = Time.time;
        }

        private void onGameEnd()
        {
            var str = logs.ToJson();
            var folder = Path.Combine(Application.persistentDataPath, "replay");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            File.WriteAllText(Path.Combine(folder, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".zrep"), str);
        }

        void onPlayerResponse(int id, IResponse response)
        {
            logs.Add(new ReplayLogLine(id, response, Time.time - startTime));
        }
    }
}
