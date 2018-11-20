using System;
using UnityEngine;

namespace TouhouHeartstone.Frontend
{
    public class CardSelector : MonoBehaviour
    {
        CardFace card;

        [SerializeField]
        GameObject removeMark;

        public bool SelectState => selectState;

        bool selectState;

        public void Awake()
        {
            card = GetComponentInParent<CardFace>();
            card.OnClick += OnSelect;
            updateEffect();
        }

        private void OnDestroy()
        {
            if (card != null)
                card.OnClick -= OnSelect;
        }

        void OnSelect(CardFace cf)
        {
            selectState = !selectState;
            updateEffect();
            OnCardSelectStateChange?.Invoke(card, SelectState);
        }

        /// <summary>
        /// 卡片选择状态改变
        /// </summary>
        public event Action<CardFace, bool> OnCardSelectStateChange;

        private void updateEffect()
        {
            removeMark.SetActive(SelectState);
        }
    }
}
