using UnityEngine;

namespace IGensoukyo.Utilities
{
    public static class Utilities
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : MonoBehaviour
        {
            T comp = go.GetComponent<T>();
            if (comp == null)
                comp = go.AddComponent<T>();
            return comp;
        }
    }
}
