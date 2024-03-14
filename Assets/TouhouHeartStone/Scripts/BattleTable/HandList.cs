using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
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

            return insertHand(position, hand);
        }
        public HandItem insertHand(int position, HandItem hand)
        {
            hand.onClick += onHandItemClickCallback;
            hand.onBeginDrag += onHandItemBeginDragCallback;
            hand.onDrag += onHandItemDragCallback;
            hand.onEndDrag += onHandItemEndDragCallback;
            hand.onPointerEnter += onHandItemPointerEnterCallback;
            hand.onPointerExit += onHandItemPointerExitCallback;

            PileInsertHelper.insertItem(_hands, _emptyPositions, position, hand);

            hand.handList = this;
            hand.transform.SetParent(_handRoot, false);

            Vector3 targetPosition = getHandTargetLocalPosition(position, _hands.Count);
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
        public bool removeHand(HandItem hand, bool destroy = true)
        {
            if (PileInsertHelper.removeItem(_hands, _emptyPositions, hand))
            {
                hand.onClick -= onHandItemClickCallback;
                hand.onBeginDrag -= onHandItemBeginDragCallback;
                hand.onDrag -= onHandItemDragCallback;
                hand.onEndDrag -= onHandItemEndDragCallback;
                hand.onPointerEnter -= onHandItemPointerEnterCallback;
                hand.onPointerExit -= onHandItemPointerExitCallback;
                hand.handList = null;
                if (destroy)
                {
                    hand.destroy();
                }
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
                hand.handList = null;
                hand.destroy();
            }
            _hands.Clear();
            _emptyPositions.Clear();
        }
        public Vector3 getHandTargetPosition(int index)
        {
            var localPos = getHandTargetLocalPosition(index, _hands.Count);
            return _handRoot.TransformPoint(localPos);
        }
        public int indexOf(HandItem hand)
        {
            return _hands.IndexOf(hand);
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
                if (!hand.ignoreAlign)
                {
                    var targetPosition = getHandTargetLocalPosition(i, count);
                    var distance = (targetPosition - hand.transform.localPosition).magnitude;
                    hand.transform.SetSiblingIndex(i);
                    hand.transform.localPosition = Vector3.MoveTowards(hand.transform.localPosition, targetPosition, distance * deltaTime * _alignSpeed);
                }
            }
        }

        private Vector3 getHandTargetLocalPosition(int position, int count)
        {
            float rectWidth = _rectTransform?.rect.width ?? 10;
            float pivotX = _rectTransform?.pivot.x ?? 0.5f;
            float singleWidth = Mathf.Min((rectWidth - _maxHandWidth) / (count - 1), _maxHandWidth);
            float width = singleWidth * (count - 1);
            float centerX = rectWidth * (pivotX - 0.5f);
            float startX = centerX + width * 0.5f;

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
        public int count => _hands.Count;
        public HandItem this[int index] => _hands[index];
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
        /// <summary>
        /// 暂存手牌列表，用于暂时存储缺失的手牌位置。用于解决将手牌插入到大于当前手牌数量的位置的问题。
        /// </summary>
        private List<int> _emptyPositions = new List<int>();
    }
}