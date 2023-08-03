using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.XR;

namespace Game.BattleTable
{
    public class HandList : MonoBehaviour
    {
        public bool containsPosition(Vector3 position)
        {
            if (transform is RectTransform rectTrans)
            {
                return rectTrans.rect.Contains(position);
            }
            return false;
        }
        public void zoomIn()
        {
            _isZoomIn = true;
        }
        public void zoomOut()
        {
            _isZoomIn = false;
        }
        public void updateHand(float deltaTime)
        {

        }
        public HandItem insertHand(int position)
        {
            var hand = Instantiate(_handTemplate, _handRoot).GetComponent<HandItem>();
            hand.gameObject.SetActive(true);
            hand.onClick += onHandItemClickCallback;
            hand.onBeginDrag += onHandItemBeginDragCallback;
            hand.onDrag += onHandItemDragCallback;
            hand.onEndDrag += onHandItemEndDragCallback;
            hand.onPointerEnter += onHandItemPointerEnterCallback;
            hand.onPointerExit += onHandItemPointerExitCallback;
            _hands.Insert(position, hand);
            hand.transform.SetParent(_handRoot, false);
            return hand;
        }
        public HandItem addHand()
        {
            return insertHand(_hands.Count);
        }
        public bool contains(HandItem item)
        {
            return _hands.Contains(item);
        }
        public bool removeHand(HandItem hand)
        {
            if (_hands.Remove(hand))
            {
                hand.onClick -= onHandItemClickCallback;
                hand.onBeginDrag -= onHandItemBeginDragCallback;
                hand.onDrag -= onHandItemDragCallback;
                hand.onEndDrag -= onHandItemEndDragCallback;
                hand.onPointerEnter -= onHandItemPointerEnterCallback;
                hand.onPointerExit -= onHandItemPointerExitCallback;
                hand.destroy();
                return true;
            }
            return false;
        }
        public void clear()
        {
            foreach (var hand in _hands)
            {
                hand.onClick -= onHandItemClickCallback;
                hand.onBeginDrag -= onHandItemBeginDragCallback;
                hand.onDrag -= onHandItemDragCallback;
                hand.onEndDrag -= onHandItemEndDragCallback;
                hand.onPointerEnter -= onHandItemPointerEnterCallback;
                hand.onPointerExit -= onHandItemPointerExitCallback;
                hand.destroy();
            }
            _hands.Clear();
        }
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _handTemplate.SetActive(false);
        }
        #endregion

        #region 事件回调
        private void onHandItemClickCallback(HandItem servant, PointerEventData pointer)
        {
            onHandItemClick?.Invoke(servant, this, pointer);
        }
        private void onHandItemBeginDragCallback(HandItem servant, PointerEventData pointer)
        {
            onHandItemBeginDrag?.Invoke(servant, this, pointer);
        }
        private void onHandItemDragCallback(HandItem servant, PointerEventData pointer)
        {
            onHandItemDrag?.Invoke(servant, this, pointer);
        }
        private void onHandItemEndDragCallback(HandItem servant, PointerEventData pointer)
        {
            onHandItemEndDrag?.Invoke(servant, this, pointer);
        }
        private void onHandItemPointerEnterCallback(HandItem servant, PointerEventData pointer)
        {
            onHandItemPointerEnter?.Invoke(servant, this, pointer);
        }
        private void onHandItemPointerExitCallback(HandItem servant, PointerEventData pointer)
        {
            onHandItemPointerExit?.Invoke(servant, this, pointer);
        }
        #endregion

        #endregion


        #region 事件
        public Action<HandItem, HandList, PointerEventData> onHandItemClick;
        public Action<HandItem, HandList, PointerEventData> onHandItemBeginDrag;
        public Action<HandItem, HandList, PointerEventData> onHandItemDrag;
        public Action<HandItem, HandList, PointerEventData> onHandItemEndDrag;
        public Action<HandItem, HandList, PointerEventData> onHandItemPointerEnter;
        public Action<HandItem, HandList, PointerEventData> onHandItemPointerExit;
        #endregion

        public bool isZoomIn => _isZoomIn;
        [SerializeField]
        private GameObject _handTemplate;
        [SerializeField]
        private Transform _handRoot;
        [SerializeField]
        private List<HandItem> _hands;
        [SerializeField]
        private bool _isZoomIn;
    }
}