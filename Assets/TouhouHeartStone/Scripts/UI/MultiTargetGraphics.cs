using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MultiTargetGraphics : MonoBehaviour
    {
        [SerializeField] 
        private Graphic[] targetGraphics;
        public Graphic[] TargetGraphics => targetGraphics;
    }
}