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
        public void setImage(Sprite sprite)
        {
            _image.setTableImageSprite(sprite, propertyBlock);
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
        [SerializeField]
        private MeshRenderer _image;
        private MaterialPropertyBlock propertyBlock
        {
            get
            {
                if (_propertyBlock == null)
                {
                    _propertyBlock = new MaterialPropertyBlock();
                }
                return _propertyBlock;
            }
        }
        private MaterialPropertyBlock _propertyBlock;
        #endregion
    }
}