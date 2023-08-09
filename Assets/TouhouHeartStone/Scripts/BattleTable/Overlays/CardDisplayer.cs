using UI;
using UnityEngine;

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
        #endregion
        #region 属性字段
        public TableCard card => _card;
        [SerializeField]
        private TableCard _card;
        [SerializeField]
        private CardBuffList _buffList;
        #endregion
    }
}
