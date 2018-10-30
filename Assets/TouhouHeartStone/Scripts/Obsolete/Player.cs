using UnityEngine;

namespace TouhouHeartstone
{
    class PlayerLogic
    {
        public PlayerLogic(int id)
        {
            this.id = id;
        }
        public RegionLogic hand { get; } = new RegionLogic();
        public RegionLogic deck { get; } = new RegionLogic();
        public int id { get; private set; }
        public override int GetHashCode()
        {
            return id;
        }
        public override bool Equals(object obj)
        {
            return obj is PlayerLogic && (obj as PlayerLogic).id == id;
        }
    }
}