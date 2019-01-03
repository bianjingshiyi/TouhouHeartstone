using System;
using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    public class Player
    {
        public Player(int id)
        {
            this.id = id;
        }
        /// <summary>
        /// 获取玩家的套牌。
        /// </summary>
        /// <returns></returns>
        public int[] getDeck()
        {
            return new int[30];
        }
        public void createPile(string name)
        {
            regions.Add(new Pile(name));
        }
        public Pile this[string pileName]
        {
            get { return getPile(pileName); }
        }
        public Pile getPile(string name)
        {
            return regions.FirstOrDefault(e => { return e.name == name; });
        }
        List<Pile> regions { get; } = new List<Pile>();
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
        public Pile hand { get; } = new Pile("Hand");
        public Pile deck { get; } = new Pile("Deck");
        public Pile grave { get; } = new Pile("Grave");
        public int id { get; private set; }
        public override int GetHashCode()
        {
            return id;
        }
        public override bool Equals(object obj)
        {
            return obj is Player && (obj as Player).id == id;
        }
        public override string ToString()
        {
            return "Player(" + id + ")";
        }
        public static implicit operator Player[] (Player player)
        {
            return new Player[] { player };
        }
    }
}