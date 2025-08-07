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
            addCardPopup(targetTransform, cardDefine, tokenDefine, env);
        }
        public void addCardPopup(Transform targetTransform, CardDefine card, CardDefine token, IEnumerable<ICardPoolReference> env)
        {
            _cardPopupTimer.reset();
            _cardPopupTimer.start();
            _cardPopupTarget = targetTransform;
            _cardPopupEnv = env;
            _cardPopupDefine = card;
            _cardPopupTokenDefine = token;
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
            _cardPopupTokenDefine = null;
            _cardPopupDefine = null;
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
                await showCardPopup(_cardPopupTarget.position, _cardPopupDefine, _cardPopupTokenDefine, _cardPopupEnv);
            }
            if (isShowingCardPopup)
            {
                _cardPopup.setPosition(_cardPopupTarget.position);
            }
        }
        private async Task showCardPopup(Vector3 position, CardDefine card, CardDefine token, IEnumerable<ICardPoolReference> env)
        {
            if (isShowingCardPopup)
                return;
            isShowingCardPopup = true;
            ICardPoolReference mainPoolRef = game.card.getCardPoolReferenceInEnv(card.cardPoolId, env);
            CardSkinData skin = game.card.getCardSkin(mainPoolRef, card.id);
            string cardDesc = game.card.getLocalizedString(mainPoolRef, skin.descKey);
            string[] keywords = game.card.getKeywordsInDescription(cardDesc);

            CardSkinData tokenSkin = null;
            if (token != null)
            {
                ICardPoolReference tokenPoolRef = game.card.getCardPoolReferenceInEnv(token.cardPoolId, env);
                tokenSkin = game.card.getCardSkin(tokenPoolRef, token.id);
            }
            await _cardPopup.show(position, token, tokenSkin, game.card, game.camera, keywords);
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
        IEnumerable<ICardPoolReference> _cardPopupEnv;
        CardDefine _cardPopupDefine;
        CardDefine _cardPopupTokenDefine;
        [SerializeField]
        CardPopUpUI _cardPopup;
        #endregion
    }
}
