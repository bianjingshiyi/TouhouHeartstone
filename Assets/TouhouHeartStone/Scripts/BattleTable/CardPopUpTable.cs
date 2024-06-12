using System.Threading.Tasks;
using TouhouCardEngine;
using UI;
using UnityEngine;

namespace Game.BattleTable.UI
{
    public class CardPopUpTable : CardPopUp
    {
        public void initTable(BattleTableManager tbl)
        {
            table = tbl;
        }
        protected override Task setTokenCardDefine(CardDefine card, CardSkinData skin, CardManager cardManager)
        {
            return _tokenCard.setCardDefine(card, skin, cardManager);
        }
        #region 属性字段
        private BattleTableManager table;
        [SerializeField]
        private TableCard _tokenCard;
        #endregion
    }
}