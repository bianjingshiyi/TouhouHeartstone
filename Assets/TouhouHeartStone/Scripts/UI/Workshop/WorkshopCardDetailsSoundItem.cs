using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class WorkshopCardDetailsSoundItem : MonoBehaviour
    {
        public void setText(string str)
        {
            text.text = str;
        }
        private void Awake()
        {
            button.onClick.AddListener(() => onButtonClick?.Invoke(this));
        }
        public event Action<WorkshopCardDetailsSoundItem> onButtonClick;
        [SerializeField]
        private Button button;
        [SerializeField]
        private TextMeshProUGUI text;
    }
}
