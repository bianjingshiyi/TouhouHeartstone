using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    abstract class Record
    {
        public abstract Dictionary<int, IWitness> apply(CardEngine game);
        public abstract Dictionary<int, IWitness> revert(CardEngine game);
    }
}