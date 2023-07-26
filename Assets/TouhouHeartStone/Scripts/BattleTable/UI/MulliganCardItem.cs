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
        #region 生命周期
        private void Awake()
        {
            card.onClick += OnPointerClickCallback;
            card.onBeginDrag += OnBeginDragCallback;
            card.onDrag += OnDragCallback;
            card.onEndDrag += OnEndDragCallback;
            card.onPointerEnter += OnPointerEnterCallback;
            card.onPointerExit += OnPointerExitCallback;
        }
        #endregion

        #region 事件回调
        private void OnPointerClickCallback(TableCard card, PointerEventData eventData)
        {
            onClick?.Invoke(this, eventData);
        }

        private void OnBeginDragCallback(TableCard card, PointerEventData eventData)
        {
            onBeginDrag?.Invoke(this, eventData);
        }

        private void OnDragCallback(TableCard card, PointerEventData eventData)
        {
            onDrag?.Invoke(this, eventData);
        }

        private void OnEndDragCallback(TableCard card, PointerEventData eventData)
        {
            onEndDrag?.Invoke(this, eventData);
        }
        private void OnPointerEnterCallback(TableCard card, PointerEventData eventData)
        {
            onPointerEnter?.Invoke(this, eventData);
        }

        private void OnPointerExitCallback(TableCard card, PointerEventData eventData)
        {
            onPointerExit?.Invoke(this, eventData);
        }
        #endregion

        #region 事件
        public event Action<MulliganCardItem, PointerEventData> onClick;
        public event Action<MulliganCardItem, PointerEventData> onBeginDrag;
        public event Action<MulliganCardItem, PointerEventData> onDrag;
        public event Action<MulliganCardItem, PointerEventData> onEndDrag;
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