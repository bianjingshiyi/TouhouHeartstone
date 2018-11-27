using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    abstract class Record
    {
        public abstract Dictionary<int, IWitness> apply(Game game);
        public abstract Dictionary<int, IWitness> revert(Game game);
    }
    public interface IWitness
    {
        int number { get; set; }
    }
}