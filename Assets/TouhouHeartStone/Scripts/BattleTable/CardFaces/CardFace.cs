using TMPro;
using UnityEngine;

namespace Game.BattleTable
{
    public class CardFace : MonoBehaviour
    {
        public void setVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
        #region 属性字段
        public PropertyNumber costPropNumber => _costNumber;
        public TextMeshPro nameText => _nameText;
        public TextMeshPro descText => _descText;
        [SerializeField]
        private PropertyNumber _costNumber;
        [SerializeField]
        private TextMeshPro _nameText;
        [SerializeField]
        private TextMeshPro _descText;
        #endregion
    }
}