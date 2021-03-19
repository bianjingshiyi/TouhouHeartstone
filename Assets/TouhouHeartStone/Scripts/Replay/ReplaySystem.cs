using TouhouHeartstone;
using System;
using TouhouCardEngine.Interfaces;
using MongoDB.Bson.Serialization;
using System.Linq;
using TouhouCardEngine;
using System.Collections.Generic;

namespace Game.Replays
{
    public class ReplaySystem
    {
        public void startRecord(THHGame game, RoomPlayerData[] players)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            this.game = game;
            replay = new ReplayData()
            {
                option = game.option,
                players = players,
                responses = new List<IResponse>()
            };
            game.onPlayerResponse += onPlayerResponse;
            game.onEnd += onEnd;
        }
        public void stopRecord()
        {
            game.onPlayerResponse -= onPlayerResponse;
            game.onEnd -= onEnd;
            game = null;
        }
        public string getSaveDir()
        {
            return "Replays";
        }
        public string getSaveFileName(ReplayData replay)
        {
            return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        }
        public string getSaveFileExtension()
        {
            return "zmrep";
        }
        public bool isRecording
        {
            get { return game != null; }
        }
        #region 私有成员
        static ReplaySystem()
        {
            foreach (var item in typeof(Response).Assembly.GetTypes())
            {
                if (item.IsSubclassOf(typeof(Response)) || item.GetInterfaces().Contains(typeof(IResponse)))
                    BsonClassMap.LookupClassMap(item);
            }
        }
        private void onPlayerResponse(int playerId, IResponse response)
        {
            replay.responses.Add(response);
        }
        private void onEnd()
        {
            stopRecord();
        }
        THHGame game { get; set; } = null;
        public ReplayData replay { get; private set; } = null;
        #endregion
    }
}
