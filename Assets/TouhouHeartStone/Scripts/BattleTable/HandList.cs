using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
            _animator.SetBool("IsExpanded", true);
            _blocker.gameObject.SetActive(true);
        }
        public void zoomOut()
        {
            _isZoomIn = false;
            _animator.SetBool("IsExpanded", false);
            _blocker.gameObject.SetActive(false);
        }
        public void updateHand(float deltaTime)
        {
            alignHands(deltaTime);
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

            Vector3 targetPosition = getHandTargetPosition(position, _hands.Count);
            hand.transform.localPosition = targetPosition;

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
            _rectTransform = transform as RectTransform;
            _blocker.onClick += onBlockerClickCallback;
            _handTemplate.SetActive(false);
        }
        #endregion

        #region 事件回调
        private void onBlockerClickCallback()
        {
            onBlockerClick?.Invoke(this);
        }
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

        private void alignHands(float deltaTime)
        {
            int count = _hands.Count;
            for (int i = 0; i < _hands.Count; i++)
            {
                var hand = _hands[i];
                if (hand != ignoreAlignHand)
                {
                    var targetPosition = getHandTargetPosition(i, count);
                    var distance = (targetPosition - hand.transform.localPosition).magnitude;
                    hand.transform.SetSiblingIndex(i);
                    hand.transform.localPosition = Vector3.MoveTowards(hand.transform.localPosition, targetPosition, distance * deltaTime * _alignSpeed);
                }
            }
        }

        private Vector3 getHandTargetPosition(int position, int count)
        {
            float rectWidth = _rectTransform?.rect.width ?? 10;
            float pivotX = _rectTransform?.pivot.x ?? 0.5f;
            float singleWidth = Mathf.Min(rectWidth / count, _maxHandWidth);
            var width = count * singleWidth;
            float startX = width * pivotX - singleWidth * 0.5f;
            float x = startX - singleWidth * position;
            return new Vector3(x, position * _cardThickness, 0);
        }

        #endregion


        #region 事件
        public Action<HandList> onBlockerClick;
        public Action<HandItem, HandList, PointerEventData> onHandItemClick;
        public Action<HandItem, HandList, PointerEventData> onHandItemBeginDrag;
        public Action<HandItem, HandList, PointerEventData> onHandItemDrag;
        public Action<HandItem, HandList, PointerEventData> onHandItemEndDrag;
        public Action<HandItem, HandList, PointerEventData> onHandItemPointerEnter;
        public Action<HandItem, HandList, PointerEventData> onHandItemPointerExit;
        #endregion

        public bool isZoomIn => _isZoomIn;
        public HandItem ignoreAlignHand { get; set; }
        [SerializeField]
        private RectTransform _rectTransform;
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private MeshButton _blocker;
        [SerializeField]
        private GameObject _handTemplate;
        [SerializeField]
        private Transform _handRoot;
        [SerializeField]
        private List<HandItem> _hands;
        [SerializeField]
        private bool _isZoomIn;
        [SerializeField]
        private float _maxHandWidth = 4;
        [SerializeField]
        private float _cardThickness = 0.1f;
        [SerializeField]
        private float _alignSpeed = 20;
    }
}