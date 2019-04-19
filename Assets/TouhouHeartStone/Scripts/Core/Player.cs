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
        public Player(int id, Pile[] piles)
        {
            this.id = id;
            foreach (Pile pile in piles)
            {
                pile.owner = this;
            }
            pileList.AddRange(piles);
        }
        public T getProp<T>(string propName)
        {
            if (dicProp.ContainsKey(propName) && dicProp[propName] is T)
                return (T)dicProp[propName];
            return default(T);
        }
        public void setProp<T>(string propName, T value)
        {
            dicProp[propName] = value;
        }
        public void setProp(string propName, PropertyChangeType changeType, int value)
        {
            if (changeType == PropertyChangeType.set)
                dicProp[propName] = value;
            else if (changeType == PropertyChangeType.add)
                dicProp[propName] = getProp<int>(propName) + value;
        }
        public void setProp(string propName, PropertyChangeType changeType, float value)
        {
            if (changeType == PropertyChangeType.set)
                dicProp[propName] = value;
            else if (changeType == PropertyChangeType.add)
                dicProp[propName] = getProp<int>(propName) + value;
        }
        public void setProp(string propName, PropertyChangeType changeType, string value)
        {
            if (changeType == PropertyChangeType.set)
                dicProp[propName] = value;
            else if (changeType == PropertyChangeType.add)
                dicProp[propName] = getProp<string>(propName) + value;
        }
        internal Dictionary<string, object> dicProp { get; } = new Dictionary<string, object>();
        public Pile this[string pileName]
        {
            get { return getPile(pileName); }
        }
        public Pile getPile(string name)
        {
            return pileList.FirstOrDefault(e => { return e.name == name; });
        }
        List<Pile> pileList { get; } = new List<Pile>();
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
        public Pile hand { get; }
        public Pile deck { get; }
        public Pile grave { get; }
        public int id { get; private set; }
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