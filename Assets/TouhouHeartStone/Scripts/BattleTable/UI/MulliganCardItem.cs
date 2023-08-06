using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class MulliganCardItem : MonoBehaviour
    {
        public void setMarkVisible(bool visible)
        {
            _mark.SetActive(visible);
        }
        public void destroy()
        {
            Destroy(gameObject);
        }
        private void Awake()
        {
            _card.onClick += onPointerClickCallback;
            _card.onPointerEnter += onPointerEnterCallback;
            _card.onPointerExit += onPointerExitCallback;
        }
        #region 事件回调
        void onPointerClickCallback(TableCard card, PointerEventData eventData)
        {
            onClick?.Invoke(this, eventData);
        }

        void onPointerEnterCallback(TableCard card, PointerEventData eventData)
        {
            onPointerEnter?.Invoke(this, eventData);
        }

        void onPointerExitCallback(TableCard card, PointerEventData eventData)
        {
            onPointerExit?.Invoke(this, eventData);
        }
        #endregion

        #region 事件
        public event Action<MulliganCardItem, PointerEventData> onClick;
        public event Action<MulliganCardItem, PointerEventData> onPointerEnter;
        public event Action<MulliganCardItem, PointerEventData> onPointerExit;
        #endregion

        public TableCard card => _card;
        [SerializeField]
        private TableCard _card;
        [SerializeField]
        private GameObject _mark;
    }
}