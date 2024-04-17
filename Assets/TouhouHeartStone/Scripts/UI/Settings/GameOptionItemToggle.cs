using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameOptionItemToggle : UIObject
    {
        public Toggle Toggle => _toggle;
        [SerializeField]
        private Toggle _toggle;
    }
}
