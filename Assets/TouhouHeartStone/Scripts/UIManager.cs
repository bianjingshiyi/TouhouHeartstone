using UnityEngine;
using UI;

namespace Game
{
    public class UIManager : BJSYGameCore.UI.UIManager
    {
        public UIDialog dialog => _dialog;
        [SerializeField]
        UIDialog _dialog;
    }
}
