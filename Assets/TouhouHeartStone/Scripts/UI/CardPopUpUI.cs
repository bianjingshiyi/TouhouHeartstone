using System.Threading.Tasks;
using Game;
using TouhouCardEngine;
using UnityEngine;

namespace UI
{
    public class CardPopUpUI : CardPopUp
    {
        protected override Task setTokenCardDefine(CardDefine card, CardSkinData skin, CardManager cardManager)
        {
            return _tokenCard.setCardDefine(card, cardManager);
        }
        #region 属性字段
        [SerializeField]
        private Card _tokenCard;
        #endregion
    }
}