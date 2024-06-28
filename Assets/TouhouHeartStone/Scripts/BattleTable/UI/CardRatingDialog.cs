using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.BattleTable
{
    public class CardRatingDialog : MonoBehaviour
    {
        public void setActive(bool active)
        {
            gameObject.SetActive(active);
        }
        public RatingCardItem getItem(int index)
        {
            return _cardList.getItem(index);
        }
        public int indexOf(RatingCardItem item)
        {
            return _cardList.indexOf(item);
        }
        public RatingCardItem addChoice()
        {
            return _cardList.add();
        }
        public void clearChoices()
        {
            _cardList.clear();
        }
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _acceptButton.onClick.AddListener(onAcceptButtonClickCallback);
            _cardList.onItemPointerEnter += onItemPointerEnterCallback;
            _cardList.onItemPointerExit += onItemPointerExitCallback;
            _cardList.onItemClicked += onItemClickCallback;
            _cardList.onVoteClick += (index, vote) => onItemVoteClick?.Invoke(index, vote);
        }
        #endregion
        private void onItemPointerEnterCallback(int index)
        {
            onItemPointerEnter?.Invoke(index);
        }
        private void onItemPointerExitCallback(int index)
        {
            onItemPointerExit?.Invoke(index);
        }
        private void onItemClickCallback(int index)
        {
            onItemClicked?.Invoke(index);
        }
        private void onAcceptButtonClickCallback()
        {
            onAccept?.Invoke();
        }
        #endregion

        #region 事件
        public Action<int> onItemPointerEnter;
        public Action<int> onItemPointerExit;
        public Action<int> onItemClicked;
        public Action<int, bool> onItemVoteClick;
        public Action onAccept;
        #endregion

        #region 属性字段
        [SerializeField]
        private RatingCardList _cardList;
        [SerializeField]
        private Button _acceptButton;
        #endregion
    }
}
