using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    class Player
    {
        public Player(int id)
        {
            this.id = id;
        }
        public void addCrystal(int count, CrystalState state)
        {
            for (int i = 0; i < count; i++)
            {
                crystalList.Add(state);
            }
        }
        public void removeCrystal(int count)
        {
            for (int i = 0; i < count; i++)
            {
                crystalList.RemoveAt(crystalList.Count - 1);
            }
        }
        public int crystalCount
        {
            get { return crystalList.Count; }
        }
        List<CrystalState> crystalList { get; } = new List<CrystalState>();
        public Region hand { get; } = new Region();
        public Region deck { get; } = new Region();
        public Region grave { get; } = new Region();
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