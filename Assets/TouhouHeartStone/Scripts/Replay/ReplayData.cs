using TouhouHeartstone;
using System;
using System.Collections.Generic;
using TouhouCardEngine.Interfaces;
using NitoriNetwork.Common;
using TouhouCardEngine;

namespace Game.Replays
{
    [Serializable]
    public class ReplayData
    {
        public GameOption option;
        public RoomPlayerData[] players;
        public List<IResponse> responses;
    }
}