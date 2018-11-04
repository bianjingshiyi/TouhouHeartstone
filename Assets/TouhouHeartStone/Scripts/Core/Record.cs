using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    abstract class Record
    {
        public abstract Dictionary<int, Witness> apply(Game game);
        public abstract Dictionary<int, Witness> revert(Game game);
    }
    [Serializable]
    public abstract class Witness
    {
        public int number { get; set; }
    }
}