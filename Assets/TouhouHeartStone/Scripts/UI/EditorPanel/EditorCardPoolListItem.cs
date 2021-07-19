using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    class EditorCardPoolListItem : MonoBehaviour
    {
        public void updateView(string name, Sprite sprite)
        {
            _nameText.text = name;
            _image.sprite = sprite;
        }
        [SerializeField]
        Text _nameText;
        [SerializeField]
        Image _image;
    }
}