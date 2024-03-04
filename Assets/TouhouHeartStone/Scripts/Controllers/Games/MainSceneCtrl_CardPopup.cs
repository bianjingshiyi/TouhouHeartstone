using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using UI;
using UnityEngine;
using Timer = BJSYGameCore.Timer;
using UICard = UI.Card;

namespace Controllers
{
    partial class MainSceneCtrl
    {
        #region 公有方法
        public void addLocalCardPopup(UICard card, CardDefine cardDefine)
        {
            addCardPopup(card.centerTransform, cardDefine, game.card.getLocalCardPoolReferences());
        }
        public void addCardPopup(UICard card, CardDefine cardDefine, IEnumerable<ICardPoolReference> env)
        {
            addCardPopup(card.centerTransform, cardDefine, env);
        }
        public void addCardPopup(Transform targetTransform, CardDefine cardDefine, IEnumerable<ICardPoolReference> env)
        {
            var tokenDefine = game.card.evaluateCardTokens(cardDefine, env).FirstOrDefault();
            var skin = game.card.getCardSkin(cardDefine);
            addCardPopup(targetTransform, tokenDefine, skin);
        }
        public void addCardPopup(Transform targetTransform, CardDefine tokenDefine, CardSkinData skin)
        {
            _cardPopupTimer.reset();
            _cardPopupTimer.start();
            _cardPopupTarget = targetTransform;
            _cardPopupDefine = tokenDefine;
            _cardPopupSkin = skin;
            hideCardPopup();
        }
        public void removeCardPopup(UICard card)
        {
            removeCardPopup(card.centerTransform);
        }
        public void removeCardPopup(Transform targetTransform)
        {
            if (targetTransform != _cardPopupTarget)
                return;
            _cardPopupTimer.reset();
            hideCardPopup();

            _cardPopupTarget = null;
            _cardPopupDefine = null;
            _cardPopupSkin = null;
        }
        #endregion

        #region 私有方法
        private async Task updateCardPopup()
        {
            if (!_cardPopupTarget || !_cardPopupTarget.gameObject.activeInHierarchy)
            {
                hideCardPopup();
                _cardPopupTimer.reset();
                return;
            }
            if (_cardPopupTimer.isExpired())
            {
                await showCardPopup(_cardPopupTarget.position, _cardPopupDefine, _cardPopupSkin);
            }
            if (isShowingCardPopup)
            {
                _cardPopup.setPosition(_cardPopupTarget.position);
            }
        }
        private async Task showCardPopup(Vector3 position, CardDefine tokenDefine, CardSkinData skin)
        {
            if (isShowingCardPopup)
                return;
            isShowingCardPopup = true;
            var keywords = game.card.getKeywordsInDescription(skin.desc);
            await _cardPopup.show(position, tokenDefine, skin, game.card, game.camera, keywords);
        }
        private void hideCardPopup()
        {
            if (!isShowingCardPopup)
                return;
            isShowingCardPopup = false;
            _cardPopup.hide();
        }
        #endregion

        #region 属性字段
        Timer _cardPopupTimer = new Timer()
        {
            duration = 0.5f
        };
        bool isShowingCardPopup = false;
        Transform _cardPopupTarget;
        CardDefine _cardPopupDefine;
        CardSkinData _cardPopupSkin;
        [SerializeField]
        CardPopUpUI _cardPopup;
        #endregion
    }
}
