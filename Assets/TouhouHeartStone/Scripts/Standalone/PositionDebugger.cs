using UnityEngine;

namespace IGensoukyo.Utilities
{
    public class PositionDebugger : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log($"Position: G({transform.position}), L({transform.localPosition})");
        }
    }
}