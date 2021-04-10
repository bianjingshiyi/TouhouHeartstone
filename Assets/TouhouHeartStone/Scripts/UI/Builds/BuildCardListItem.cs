using UnityEngine;
using System;
using UnityEngine.EventSystems;
using TouhouCardEngine;
using UnityEngine.UI;
using BJSYGameCore.UI;
using Game;

namespace UI
{
    partial class BuildCardListItem : IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        Vector2 _dragStartPosition;
        [SerializeField]
        float _dragThreshold = 50f;
        [SerializeField]
        bool _isDragging = false;
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _dragStartPosition = eventData.position;
            _isDragging = false;
            GetComponentInParent<ScrollRect>()?.SendMessage("OnBeginDrag", eventData, SendMessageOptions.DontRequireReceiver);
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            var delta = _dragStartPosition - eventData.position;
            if (Math.Abs(delta.x) > _dragThreshold && Math.Abs(delta.x) > Math.Abs(delta.y))
            {
                _isDragging = true;
            }
            if (_isDragging)
            {
                Build build = GetComponentInParent<Build>();
                build.onDragCardItem(this, eventData);
            }
            else
            {
                GetComponentInParent<ScrollRect>()?.SendMessage("OnDrag", eventData, SendMessageOptions.DontRequireReceiver);
            }
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (_isDragging)
            {
                //结束拖拽
                Build build = GetComponentInParent<Build>();
                build.onReleaseCardItem(this, eventData);
            }
            _isDragging = false;
            GetComponentInParent<ScrollRect>()?.SendMessage("OnEndDrag", eventData, SendMessageOptions.DontRequireReceiver);
        }

        public int CardID { get; private set; }

        /// <summary>
        /// 设置剩下可用的牌的提示
        /// </summary>
        /// <param name="now"></param>
        /// <param name="count"></param>
        public void setRemainText(int now, int count)
        {
            Image.gameObject.SetActive(now != 0);
            RemainHintText.text = $"{now} / {count}";
        }

        /// <summary>
        /// 设置卡的显示数据
        /// </summary>
        /// <param name="data"></param>
        public void SetCardData(BuildCardViewData data)
        {
            Card.isFaceup = true; // Animator默认状态是false，重置之后得还原
            Card.setCard(data.define, data.skin);
            setRemainText(data.useCount, data.maxCount);
            CardID = data.define.id;
        }
    }

    partial class BuildCardList
    {
        VirtualList<BuildCardListItem> cardVList;
        VirtualList<BuildCardListItem> CardVList
        {
            get
            {
                if (cardVList == null)
                {
                    cardVList = new VirtualList<BuildCardListItem>(addItem, asGridLayoutGroup);
                    cardVList.onDisplayUIObj += setItemArgs;
                }
                return cardVList;
            }
        }

        partial void onAwake()
        {
            cardVList = new VirtualList<BuildCardListItem>(addItem, asGridLayoutGroup);
            cardVList.onDisplayUIObj += setItemArgs;
        }

        BuildCardViewData[] listData;

        public void SetList(BuildCardViewData[] list)
        {
            CardVList.reset();
            listData = list;
            CardVList.TotalDataCount = list.Length;

            for (int i = 0; i < list.Length; i++)
                cardVList.addItem();
        }

        [Obsolete("Use SetList Instead")]
        public void SetContent(int index, BuildCardViewData d)
        {
            listData[index] = d;
            SetList(listData);
        }

        private void setItemArgs(int index, BuildCardListItem item)
        {
            item.SetCardData(listData[index]);
        }

        private void OnDestroy()
        {
            CardVList.onDisplayUIObj -= setItemArgs;
        }
    }

    public struct BuildCardViewData
    {
        public CardDefine define;
        public CardSkinData skin;
        public int maxCount;
        public int useCount;
    }
}