using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    class CardViewDeckCardListItem : MonoBehaviour
    {
        public void updateView(string name, Sprite sprite)
        {
            updateView(name, sprite, 0);
        }
        public void updateView(string name, Sprite sprite, int count)
        {
            _nameText.text = name;
            _image.sprite = sprite;
            if (count > 0)
            {
                _numberText.gameObject.SetActive(true);
                _numberText.text = count.ToString();
            }
            else
            {
                _numberText.gameObject.SetActive(false);
            }
        }
        [SerializeField]
        Text _nameText;
        [SerializeField]
        Image _image;
        [SerializeField]
        Text _numberText;
    }
}