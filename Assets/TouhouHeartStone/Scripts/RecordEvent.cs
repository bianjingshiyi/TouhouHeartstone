using System;
using System.Collections.Generic;

using UnityEngine.Events;

namespace TouhouHeartstone
{
    [Serializable]
    public class RecordEvent : UnityEvent<Record> { }
    [Serializable]
    public class WitnessByPlayerEvent : UnityEvent<Dictionary<int, Witness>> { }
}