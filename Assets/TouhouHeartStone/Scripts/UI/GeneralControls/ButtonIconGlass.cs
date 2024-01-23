using UnityEngine;
using UnityEngine.UI;
using BJSYGameCore.UI;

namespace UI
{
    public class ButtonIconGlass : UIObject
    {
        public void setNewMarkVisible(bool visible)
        {
            _newMark.SetActive(visible);
        }
        public Button button => _button;
        [SerializeField]
        private Button _button;
        [SerializeField]
        private Image _image;
        [SerializeField]
        private GameObject _newMark;
    }
}
