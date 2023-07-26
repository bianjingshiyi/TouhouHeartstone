using TMPro;
using UnityEngine;

namespace Game.BattleTable
{
    public class ServantCardFace : CardFace
    {
        public void setTag(string tag)
        {
            _tagText.text = tag;
        }
        #region 属性字段
        public TextMeshPro subnameText => _subnameText;
        public PropertyNumber attackPropNumber => _attackNumber;
        public PropertyNumber lifePropNumber => _lifeNumber;
        [SerializeField]
        private PropertyNumber _attackNumber;
        [SerializeField]
        private PropertyNumber _lifeNumber;
        [SerializeField]
        private TextMeshPro _subnameText;
        [SerializeField]
        private TextMeshPro _tagText;
        #endregion
    }
}