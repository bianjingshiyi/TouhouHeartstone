using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.BattleTable
{
    public class CardDisplayer : MonoBehaviour
    {
        #region 公有方法
        public void setVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
        public void setBuffs(CardBuffListItemViewData[] buffs)
        {
            _buffList.updateBuffs(buffs);
        }
        public void limitCanvasHeight()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rootTransform);
            var rectTrans = transform as RectTransform;
            var parentTrans = transform.parent as RectTransform;

            var limitYMin = parentTrans.rect.yMin - rectTrans.localPosition.y;
            var yMin = rectTrans.pivot.y * rectTrans.rect.height * _rootTransform.anchorMin.y + _rootTransform.offsetMin.y;
            if (yMin < limitYMin)
            {
                rectTrans.anchoredPosition += Vector2.up * (limitYMin - yMin);
            }

            var limitYMax = parentTrans.rect.yMax - rectTrans.localPosition.y;
            var yMax = rectTrans.pivot.y * rectTrans.rect.height * _rootTransform.anchorMax.y + _rootTransform.offsetMax.y;
            if (yMax > limitYMax)
            {
                rectTrans.anchoredPosition += Vector2.down * (yMax - limitYMax);
            }
        }
        #endregion
        #region 属性字段
        public TableCard card => _card;
        [SerializeField]
        private RectTransform _rootTransform;
        [SerializeField]
        private TableCard _card;
        [SerializeField]
        private CardBuffList _buffList;
        #endregion
    }
}
