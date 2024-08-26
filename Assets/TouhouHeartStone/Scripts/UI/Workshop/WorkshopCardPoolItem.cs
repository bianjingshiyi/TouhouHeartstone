using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class WorkshopCardPoolItem : MonoBehaviour
    {
        #region 公有方法
        public void updateView(WorkshopCardPoolItemViewData viewData)
        {
            var itemViewData = new DeckListItemViewData()
            {
                hasPrivatePools = viewData.hasPrivatePools,
                canEditName = false,

                name = viewData.name,
                image = viewData.image,

                isError = viewData.isError,
                isWaiting = viewData.isWaiting,
                isWarning = viewData.isWarning,

                mode = DeckListItem.Mode.Default,
            };
            _deckListItem.updateView(itemViewData);
            _numberText.text = viewData.number.ToString();
            _ratingText.text = viewData.rating.ToString();
            _authorText.text = viewData.author;
        }
        #endregion

        #region 私有方法

        #region 生命周期
        private void Awake()
        {
            _deckListItem.onClick += (item) => onClick?.Invoke(this);
        }
        #endregion

        #endregion

        #region 事件
        public event Action<WorkshopCardPoolItem> onClick;
        #endregion

        #region 属性字段
        public DeckListItem _deckListItem;
        public TextMeshProUGUI _numberText;
        public TextMeshProUGUI _ratingText;
        public TextMeshProUGUI _authorText;
        #endregion
    }
    public struct WorkshopCardPoolItemViewData
    {
        public string name;
        public Sprite image;
        public bool isWarning;
        public bool isError;
        public bool hasPrivatePools;
        public bool isWaiting;
        public int number;
        public int rating;
        public string author;
    }
}