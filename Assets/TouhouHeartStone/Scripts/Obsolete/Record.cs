using System;
using System.Collections.Generic;

using UnityEngine;

namespace TouhouHeartstone
{
    [Serializable]
    public abstract class Record
    {
        public abstract Dictionary<int, Witness> apply(GameContainer game);
        public abstract Dictionary<int, Witness> revert(GameContainer game);
    }
    [Serializable]
    public abstract class Witness
    {
        public int number
        {
            get { return _number; }
            set { _number = value; }
        }
        [SerializeField]
        int _number;
    }
}