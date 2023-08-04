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
        public void setImage(Texture texture)
        {
            var propertyBlock = new MaterialPropertyBlock();
            _image.GetPropertyBlock(propertyBlock);
            propertyBlock.SetTexture("_Texture0", texture);
            _image.SetPropertyBlock(propertyBlock);
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
        #endregion
    }
}