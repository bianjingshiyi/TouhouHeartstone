using TouhouHeartstone;
using System;
using TouhouCardEngine.Interfaces;
using MongoDB.Bson.Serialization;
using System.Linq;
using TouhouCardEngine;
using System.Collections.Generic;

namespace Game.Replays
{
    public class ReplayPlayer
    {

    }
    public class ReplaySystem
    {
        public void startRecord(THHGame game, RoomPlayerData[] players)
        {
            this.game = game ?? throw new ArgumentNullException(nameof(game));
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
        ReplayData replay { get; set; } = null;
        #endregion
    }
}
