using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CardBuffListItem : MonoBehaviour
    {
        public void setBuff(CardBuffListItemViewData viewData)
        {
            _image.sprite = viewData.sprite;
            _nameText.text = viewData.name;
            _descText.text = viewData.desc;
        }

        [SerializeField]
        Image _image;
        [SerializeField]
        Text _nameText;
        [SerializeField]
        Text _descText;
    }
    public struct CardBuffListItemViewData
    {
        public Sprite sprite;
        public string name;
        public string desc;
    }
}
