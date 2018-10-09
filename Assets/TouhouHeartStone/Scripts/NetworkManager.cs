
using UnityEngine;

namespace TouhouHeartstone
{
    public abstract class NetworkManager : MonoBehaviour
    {
        public abstract bool isClient { get; }
    }
}