using MessagePack;
using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game.Replays
{
    public class ReplayPlayerData
    {
        public int id;
        public string name;
        public DefineReference[] deck;
        public ReplayPlayerData()
        {

        }
        public ReplayPlayerData(RoomPlayerData data)
        {
            id = data.id;
            name = data.name;
            deck = data.getDeck().toDefRefArray();
        }
        public RoomPlayerData toRoomData()
        {
            var data = new RoomPlayerData(id, name, RoomPlayerType.human);
            data.setDeck(new Deck(deck));
            return data;
        }
    }
    [MessagePackObject]
    [Serializable]
    public class SerializableReplayPlayerData
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableReplayPlayerData()
        {
        }
        public SerializableReplayPlayerData(ReplayPlayerData replayData)
        {
            id = replayData.id;
            name = replayData.name;
            deck = replayData.deck;
        }
        public ReplayPlayerData toData()
        {
            return new ReplayPlayerData()
            {
                id = id,
                name = name,
                deck = deck
            };
        }
        [Key(0)]
        public int id;
        [Key(1)]
        public string name;
        [Key(2)]
        public DefineReference[] deck;
    }
}