using System;
using System.Collections.Generic;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;

namespace Game.Replays
{
    public class ReplayData
    {
        public int version;
        public HearthstoneOption option;
        public ReplayPlayerData playerA;
        public ReplayPlayerData playerB;
        public List<IResponse> responses;
        public List<float> responseTimes;
    }
    [Serializable]
    public class SerializableReplayData
    {
        public SerializableReplayData(ReplayData replayData)
        {
            version = replayData.version;
            option = new SerializableHearthstoneOption(replayData.option);
            playerA = new SerializableReplayPlayerData(replayData.playerA);
            playerB = new SerializableReplayPlayerData(replayData.playerB);
            responses = replayData.responses;
            responseTimes = replayData.responseTimes;
        }
        public ReplayData toData()
        {
            return new ReplayData()
            {
                version = version,
                option = option.toOption(),
                playerA = playerA.toData(),
                playerB = playerB.toData(),
                responses = responses,
                responseTimes = responseTimes,
            };
        }
        public int version;
        public SerializableHearthstoneOption option;
        public SerializableReplayPlayerData playerA;
        public SerializableReplayPlayerData playerB;
        public List<IResponse> responses;
        public List<float> responseTimes;
    }
}