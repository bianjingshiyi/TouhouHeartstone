
using UnityEngine;
using System;

namespace TouhouHeartstone.Frontend
{
    public class UIPopup : MonoBehaviour
    {
        public event Action OnShow;
        public event Action OnHide;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            OnHide?.Invoke();
        }

        private void OnEnable()
        {
            OnShow?.Invoke();
        }
    }
}
