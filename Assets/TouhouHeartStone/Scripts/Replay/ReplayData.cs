using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;

namespace Game.Replays
{
    public class ReplayData
    {
        public int version;
        public HearthstoneOption option;
        public ReplayPlayerData[] players;
        public int playerAId;
        public int playerBId;
        public List<IResponse> responses;
    }
    [Serializable]
    public class SerializableReplayData
    {
        public SerializableReplayData(ReplayData replayData)
        {
            version = replayData.version;
            option = new SerializableHearthstoneOption(replayData.option);
            players = replayData.players.Select(p => new SerializableReplayPlayerData(p)).ToArray();
            playerAId = replayData.playerAId;
            playerBId = replayData.playerBId;
            responses = replayData.responses;
        }
        public ReplayData toData()
        {
            ReplayPlayerData[] players;
            int playerAId;
            int playerBId;
            if (this.players != null)
            {
                players = this.players.Select(p => p.toData()).ToArray();
                playerAId = this.playerAId;
                playerBId = this.playerBId;
            }
            else if (playerA != null && playerB != null)
            {
                players = new ReplayPlayerData[]
                {
                    playerA.toData(),
                    playerB.toData()
                };
                playerAId = playerA.id;
                playerBId = playerB.id;
            }
            else
            {
                players = Array.Empty<ReplayPlayerData>();
                playerAId = 0;
                playerBId = 0;
            }
            return new ReplayData()
            {
                version = version,
                option = option.toOption(),
                players = players,
                playerAId = playerAId,
                playerBId = playerBId,
                responses = responses,
            };
        }
        public int version;
        public SerializableHearthstoneOption option;
        public SerializableReplayPlayerData[] players;
        public int playerAId;
        public int playerBId;
        [Obsolete]
        [BsonIgnoreIfNull]
        public SerializableReplayPlayerData playerA;
        [Obsolete]
        [BsonIgnoreIfNull]
        public SerializableReplayPlayerData playerB;
        public List<IResponse> responses;
        [Obsolete]
        [BsonIgnoreIfNull]
        public List<float> responseTimes;
    }
}