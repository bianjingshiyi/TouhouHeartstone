
using UnityEngine;

namespace TouhouHeartstone
{
    public class LogManager : MonoBehaviour
    {
        public void debug(string msg, UnityEngine.Object obj)
        {
            Debug.Log(msg, obj);
        }
        public void msg(string msg)
        {
            Debug.Log(msg);
        }
    }
}