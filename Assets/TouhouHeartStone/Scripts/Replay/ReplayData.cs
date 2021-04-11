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
        public HearthstoneOption option;
        public RoomPlayerData[] players;
        public List<IResponse> responses;
    }
}