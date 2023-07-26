using UnityEngine;

namespace Game.BattleTable
{
    public class ItemCardFace : CardFace
    {
        #region 属性字段
        public PropertyNumber attackPropNumber => _attackNumber;
        public PropertyNumber lifePropNumber => _lifeNumber;
        [SerializeField]
        private PropertyNumber _attackNumber;
        [SerializeField]
        private PropertyNumber _lifeNumber;
        #endregion
    }
}