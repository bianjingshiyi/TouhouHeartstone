
using UnityEngine;

namespace TouhouHeartstone.Frontend
{
    public class UIPopup : MonoBehaviour
    {
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
