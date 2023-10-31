using System;
using System.Threading.Tasks;
using Game;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PVECardChoice : MonoBehaviour
    {
        public void setSelected(bool selected)
        {
        }
        public async Task update(CardDefine card, CardManager cardManager)
        {
            await _card.setCardDefine(card, cardManager);
        }
        private void Awake()
        {
            _button.onClick.AddListener(onClickedCallback);
        }
        private void onClickedCallback()
        {
            onClicked?.Invoke(this);
        }
        public event Action<PVECardChoice> onClicked;
        [SerializeField]
        Card _card;
        [SerializeField]
        Button _button;
    }
}