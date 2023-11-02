using System;
using System.Collections.Generic;
using Game;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class PVECardEditor : MonoBehaviour
    {
        #region 公有方法
        public void setCard(CardDefine define, CardSkinData skin)
        {
            _card.setCardDefine(define, skin);
        }
        public void updateGemInventory(CardGemViewData[] viewDatas)
        {
            _gemInventoryList.Clear();
            _gemInventoryElementList.updateList(viewDatas.Length,
                (i, rect) =>
                {
                    CardGem gem = rect.GetComponent<CardGem>();
                    var viewData = viewDatas[i];
                    gem.update(viewData);
                    _gemInventoryList.Add(gem);
                },
                rect =>
                {
                    CardGem gem = rect.GetComponent<CardGem>();
                    gem.onBeginDrag += onInventoryGemBeginDragCallback;
                    gem.onDrag += onInventoryGemDragCallback;
                    gem.onEndDrag += onInventoryGemEndDragCallback;
                },
                rect =>
                {
                    CardGem gem = rect.GetComponent<CardGem>();
                    gem.onBeginDrag -= onInventoryGemBeginDragCallback;
                    gem.onDrag -= onInventoryGemDragCallback;
                    gem.onEndDrag -= onInventoryGemEndDragCallback;
                });
        }
        public void updateGemSlot(CardGemSlotGroupViewData[] viewDatas)
        {
            _gemSlotGroupElementList.updateList(viewDatas.Length,
                (i, rect) =>
                {
                    CardGemSlotGroup gem = rect.GetComponent<CardGemSlotGroup>();
                    var viewData = viewDatas[i];
                    gem.update(viewData);
                },
                rect =>
                {
                    CardGemSlotGroup gem = rect.GetComponent<CardGemSlotGroup>();
                    gem.onSlotBeginDrag += onSlotGemBeginDragCallback;
                    gem.onSlotDrag += onSlotGemDragCallback;
                    gem.onSlotEndDrag += onSlotGemEndDragCallback;
                },
                rect =>
                {
                    CardGemSlotGroup gem = rect.GetComponent<CardGemSlotGroup>();
                    gem.onSlotBeginDrag -= onSlotGemBeginDragCallback;
                    gem.onSlotDrag -= onSlotGemDragCallback;
                    gem.onSlotEndDrag -= onSlotGemEndDragCallback;
                });
        }

        public void startDragInventoryGem(int index)
        {
            var gem = _gemInventoryList[index];
            gem.startDrag();
        }
        public void releaseDragInventoryGem(int index)
        {
            var gem = _gemInventoryList[index];
            gem.releaseDrag();
        }
        public void moveInventoryGem(int index, Vector2 position)
        {
            var gem = _gemInventoryList[index];
            gem.move(position);
        }
        #endregion

        #region 私有方法

        #region 生命周期
        private void Awake()
        {
            _closeButton.onClick.AddListener(() => onCloseClicked?.Invoke());
        }
        #endregion

        #region 事件回调

        #region 背包宝石
        private void onInventoryGemBeginDragCallback(CardGem gem, PointerEventData eventData)
        {
            onInventoryGemBeginDrag?.Invoke(_gemInventoryElementList.indexOf(gem.transform as RectTransform), eventData);
        }
        private void onInventoryGemDragCallback(CardGem gem, PointerEventData eventData)
        {
            onInventoryGemDrag?.Invoke(_gemInventoryElementList.indexOf(gem.transform as RectTransform), eventData);
        }
        private void onInventoryGemEndDragCallback(CardGem gem, PointerEventData eventData)
        {
            onInventoryGemEndDrag?.Invoke(_gemInventoryElementList.indexOf(gem.transform as RectTransform), eventData);
        }
        #endregion

        #region 插槽宝石
        private void onSlotGemBeginDragCallback(CardGemSlotGroup gem, int index, PointerEventData eventData)
        {
            onSlotGemBeginDrag?.Invoke(_gemSlotGroupElementList.indexOf(gem.transform as RectTransform), index, eventData);
        }
        private void onSlotGemDragCallback(CardGemSlotGroup gem, int index, PointerEventData eventData)
        {
            onSlotGemDrag?.Invoke(_gemSlotGroupElementList.indexOf(gem.transform as RectTransform), index, eventData);
        }
        private void onSlotGemEndDragCallback(CardGemSlotGroup gem, int index, PointerEventData eventData)
        {
            onSlotGemEndDrag?.Invoke(_gemSlotGroupElementList.indexOf(gem.transform as RectTransform), index, eventData);
        }
        #endregion

        #endregion

        #endregion

        #region 事件
        public event Action onCloseClicked;
        public event Action<int, PointerEventData> onInventoryGemBeginDrag;
        public event Action<int, PointerEventData> onInventoryGemDrag;
        public event Action<int, PointerEventData> onInventoryGemEndDrag;
        public event Action<int, int, PointerEventData> onSlotGemBeginDrag;
        public event Action<int, int, PointerEventData> onSlotGemDrag;
        public event Action<int, int, PointerEventData> onSlotGemEndDrag;
        #endregion

        #region 属性字段
        private List<CardGem> _gemInventoryList = new List<CardGem>();
        [SerializeField]
        private Card _card;
        [SerializeField]
        private Button _closeButton;
        [SerializeField]
        private ElementList _gemInventoryElementList;
        [SerializeField]
        private ElementList _gemSlotGroupElementList;
        #endregion
    }
}
