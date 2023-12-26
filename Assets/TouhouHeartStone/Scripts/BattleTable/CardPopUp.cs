using UI;
using UnityEngine;

namespace Game.BattleTable.UI
{
    public class CardPopUp : MonoBehaviour
    {
        #region 公有方法
        
        public void setKeywordPopUpPositions(bool atRight)
        {
            var parent = atRight ? _keywordsPopUpRightRegionTrans : _keywordsPopUpLeftRegionTrans;
            var pivotX = atRight ? rightKeywordsPopUpPivotX : leftKeywordsPopUpPivotX;
            _keywordsPopUpTrans.SetParent(parent, false);
            var pivot = _keywordsPopUpTrans.pivot;
            pivot.x = pivotX;
            _keywordsPopUpTrans.pivot = pivot;
            _keywordsPopUpTrans.anchoredPosition = Vector2.zero;
        }

        public void updateKeywordPopUps(string[] titles, string[] descs)
        {
            _keywordsPopUp.update(titles, descs);
        }
        public void setTokenCardVisible(bool value, bool atRight)
        {
            _leftTokenCardRegionTrans.gameObject.SetActive(value && !atRight);
            _rightTokenCardRegionTrans.gameObject.SetActive(value && atRight);

            if (!value)
                return;
            var parent = atRight ? _rightTokenCardRegionTrans : _leftTokenCardRegionTrans;
            var pivotX = atRight ? rightTokenPopUpPivotX : leftTokenPopUpPivotX;
            var anchorX = atRight ? rightTokenPopUpAnchorX : leftTokenPopUpAnchorX;
            _tokenCardTransform.SetParent(parent, false);

            var pivot = _popupRootTransform.pivot;
            pivot.x = pivotX;
            _popupRootTransform.pivot = pivot;

            var anchorMin = _popupRootTransform.anchorMin;
            anchorMin.x = anchorX;
            _popupRootTransform.anchorMin = anchorMin;

            var anchorMax = _popupRootTransform.anchorMax;
            anchorMax.x = anchorX;
            _popupRootTransform.anchorMax = anchorMax;

            _popupRootTransform.anchoredPosition = Vector2.zero;
        }


        #endregion

        #region 属性字段
        public TableCard tokenCard => _tokenCard;
        public Vector3 centerPosition => _centerTransform.position;
        [SerializeField]
        private TableCard _tokenCard;
        [SerializeField]
        private RectTransform _popupRootTransform;
        [SerializeField]
        private Transform _tokenCardTransform;
        [SerializeField]
        private RectTransform _leftTokenCardRegionTrans;
        [SerializeField]
        private RectTransform _rightTokenCardRegionTrans;
        [SerializeField]
        private RectTransform _keywordsPopUpLeftRegionTrans;
        [SerializeField]
        private RectTransform _keywordsPopUpRightRegionTrans;
        [SerializeField]
        private RectTransform _keywordsPopUpTrans;
        [SerializeField]
        private RectTransform _centerTransform;
        [SerializeField]
        private KeywordPopUp _keywordsPopUp;

        private const float leftKeywordsPopUpPivotX = 1;
        private const float rightKeywordsPopUpPivotX = 0;
        private const float leftTokenPopUpPivotX = 1;
        private const float rightTokenPopUpPivotX = 0;
        private const float leftTokenPopUpAnchorX = 1;
        private const float rightTokenPopUpAnchorX = 0;
        #endregion
    }
}