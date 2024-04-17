using System.Threading.Tasks;
using Game;
using Game.BattleTable.UI;
using TouhouCardEngine;
using UI;
using UnityEngine;

namespace UI
{
    public abstract class CardPopUp : MonoBehaviour
    {
        #region 公有方法
        public async Task show(Vector3 worldPosition, CardDefine tokenDefine, CardSkinData skin, CardManager cardManager, Camera camera, string[] keywords)
        {
            if (tokenDefine != null && skin != null)
            {
                await setTokenCardDefine(tokenDefine, skin, cardManager);
            }
            gameObject.SetActive(true);
            setTokenCardVisible(tokenDefine != null, worldPosition.x < _cardMinLeftSpace);
            setPosition(worldPosition);

            Vector3 centerPosition = camera.ScreenToWorldPoint(camera.pixelRect.center);
            Vector3 popupCenter = centerPosition;
            bool atRight = popupCenter.x <= centerPosition.x;
            setKeywordPopUpPositions(atRight);
            cardManager.getKeywordTexts(keywords, out var titles, out var descs);
            updateKeywordPopUps(titles, descs);
        }
        public void hide()
        {
            gameObject.SetActive(false);
        }
        public void setPosition(Vector3 position)
        {
            var inversePos = transform.parent.InverseTransformPoint(position);
            var pos = transform.localPosition;
            pos.x = inversePos.x;
            pos.y = inversePos.y;
            transform.localPosition = pos;
        }
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

        protected abstract Task setTokenCardDefine(CardDefine card, CardSkinData skin, CardManager cardManager);

        #region 属性字段
        public Vector3 centerPosition => _centerTransform.position;
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
        [SerializeField]
        private float _cardMinLeftSpace = 4.5f;

        private const float leftKeywordsPopUpPivotX = 1;
        private const float rightKeywordsPopUpPivotX = 0;
        private const float leftTokenPopUpPivotX = 1;
        private const float rightTokenPopUpPivotX = 0;
        private const float leftTokenPopUpAnchorX = 1;
        private const float rightTokenPopUpAnchorX = 0;
        #endregion
    }
}