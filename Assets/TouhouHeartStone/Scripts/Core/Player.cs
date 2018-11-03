using UnityEngine;

namespace TouhouHeartstone
{
    class Player
    {
        public Player(int id)
        {
            this.id = id;
        }
        public Region hand { get; } = new Region();
        public Region deck { get; } = new Region();
        public int id { get; private set; }
        public override int GetHashCode()
        {
            return id;
        }
        public override bool Equals(object obj)
        {
            return obj is Player && (obj as Player).id == id;
        }
    }
}