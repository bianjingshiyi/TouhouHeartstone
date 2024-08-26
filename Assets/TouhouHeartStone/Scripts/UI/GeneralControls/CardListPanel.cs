using System;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    /// <summary>
    /// 卡池浏览界面
    /// </summary>
    public class CardListPanel : MonoBehaviour
    {
        #region 公有方法
        /// <summary>
        /// 显示左边的卡牌总览
        /// </summary>
        /// <param name="value"></param>
        public void setCardListActive(bool value)
        {
            _cardList.SetActive(value);
        }
        /// <summary>
        /// 设置卡牌列表数量（会引发卡牌列表刷新事件）
        /// </summary>
        /// <param name="count"></param>
        public void updateCardListCount(int count)
        {
            _cardListLayoutGroup.setCount(count);
        }
        public int getCardItemIndex(RectTransform rect)
        {
            return _cardListLayoutGroup.getItemIndex(rect);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            if (_cardListLayoutGroup.cellPrefab.transform.parent == _cardListLayoutGroup.transform)
            {
                _cardListLayoutGroup.cellPrefab.gameObject.SetActive(false);
            }
        }
        #endregion
        #endregion
        #region 事件
        public event Action<int, RectTransform> onEnableCardListItem
        {
            add { _cardListLayoutGroup.onEnableItem += value; }
            remove { _cardListLayoutGroup.onEnableItem -= value; }
        }
        public event Action<int, RectTransform> onDisableCardListItem
        {
            add { _cardListLayoutGroup.onDisableItem += value; }
            remove { _cardListLayoutGroup.onDisableItem += value; }
        }
        /// <summary>
        /// 当更新卡牌列表，参数为更新的列表项索引和UI物体
        /// </summary>
        public event Action<int, RectTransform> onUpdateCardListItem
        {
            add { _cardListLayoutGroup.onUpdateItem += value; }
            remove { _cardListLayoutGroup.onUpdateItem -= value; }
        }
        #endregion
        #region 属性字段
        public ScrollRect cardListScrollRect => _cardListScrollRect;
        [SerializeField]
        GameObject _cardList;
        [SerializeField]
        ScrollRect _cardListScrollRect;
        [SerializeField]
        VirtualListLayoutGroup _cardListLayoutGroup;
        #endregion
    }
}