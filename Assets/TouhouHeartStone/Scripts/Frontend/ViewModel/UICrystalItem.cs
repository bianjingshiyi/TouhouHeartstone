using UnityEngine;

namespace TouhouHeartstone.Frontend.ViewModel
{
    public class UICrystalItem : MonoBehaviour
    {
        [SerializeField]
        Sprite normal;

        [SerializeField]
        Sprite highlight;

        [SerializeField]
        Sprite used;

        [SerializeField]
        Sprite disable;

        UnityEngine.UI.Image image;

        private void Awake()
        {
            image = GetComponent<UnityEngine.UI.Image>();
        }

        private CrystalState _State;

        public CrystalState State
        {
            get { return _State; }
            set
            {
                _State = value;
                gameObject.SetActive(CrystalState.hidden != value);

                switch(value)
                {
                    case CrystalState.disable:
                        image.sprite = disable;
                        break;
                    case CrystalState.highlight:
                        image.sprite = highlight;
                        break;
                    case CrystalState.normal:
                        image.sprite = normal;
                        break;
                    case CrystalState.used:
                        image.sprite = used;
                        break;
                }
            }
        }
    }
}
