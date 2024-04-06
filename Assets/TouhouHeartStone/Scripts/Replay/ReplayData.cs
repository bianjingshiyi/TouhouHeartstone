using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
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
    [MessagePackObject]
    [Serializable]
    public class SerializableReplayData
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableReplayData()
        {
        }
        public SerializableReplayData(ReplayData replayData)
        {
            version = replayData.version;
            option = new SerializableHearthstoneOption(replayData.option);
            players = replayData.players.Select(p => new SerializableReplayPlayerData(p)).ToArray();
            playerAId = replayData.playerAId;
            playerBId = replayData.playerBId;
            responseList = replayData.responses.ConvertAll(r => r as Response);
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
                responses = responseList != null ? responseList.ConvertAll(r => r as IResponse) : responses,
            };
        }
        [Key(0)]
        public int version;
        [Key(1)]
        public SerializableHearthstoneOption option;
        [Key(2)]
        public SerializableReplayPlayerData[] players;
        [Key(3)]
        public int playerAId;
        [Key(4)]
        public int playerBId;
        [IgnoreMember]
        [Obsolete]
        [BsonIgnoreIfNull]
        public SerializableReplayPlayerData playerA;
        [IgnoreMember]
        [Obsolete]
        [BsonIgnoreIfNull]
        public SerializableReplayPlayerData playerB;
        [IgnoreMember]
        [Obsolete]
        public List<IResponse> responses;
        [Key(5)]
        public List<Response> responseList;
        [IgnoreMember]
        [Obsolete]
        [BsonIgnoreIfNull]
        public List<float> responseTimes;
    }
}