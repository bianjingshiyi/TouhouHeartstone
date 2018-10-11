using UnityEngine;

namespace TouhouHeartstone
{
    public abstract class Player : MonoBehaviour
    {
        public abstract int id
        {
            get;
        }
    }
}