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
        public void setTokenCardVisible(bool value)
        {
            _tokenCardRegionTrans.gameObject.SetActive(value);
        }


        #endregion

        #region 属性字段
        public TableCard tokenCard => _tokenCard;
        public Vector3 centerPosition => _centerTransform.position;
        [SerializeField]
        private TableCard _tokenCard;
        [SerializeField]
        private RectTransform _tokenCardRegionTrans;
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
        #endregion
    }
}