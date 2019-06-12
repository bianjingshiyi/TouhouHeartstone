using TouhouHeartstone.Frontend.ViewModel;
using UnityEngine;

namespace TouhouHeartstone.Frontend.View
{
    public class UICrystalItem : MonoBehaviour
    {
        [SerializeField]
        Sprite normal = null;

        [SerializeField]
        Sprite highlight = null;

        [SerializeField]
        Sprite used = null;

        [SerializeField]
        Sprite disable = null;

        UnityEngine.UI.Image image;

        private void Awake()
        {
            image = GetComponent<UnityEngine.UI.Image>();
        }

        private CrystalItemState _State;

        public CrystalItemState State
        {
            get { return _State; }
            set
            {
                _State = value;
                gameObject.SetActive(CrystalItemState.hidden != value);

                switch (value)
                {
                    case CrystalItemState.disable:
                        image.sprite = disable;
                        break;
                    case CrystalItemState.highlight:
                        image.sprite = highlight;
                        break;
                    case CrystalItemState.normal:
                        image.sprite = normal;
                        break;
                    case CrystalItemState.used:
                        image.sprite = used;
                        break;
                }
            }
        }
    }
}
