using System;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class Item : TableUnit, IPointerEnterHandler, IPointerExitHandler
    {
        public void setAttack(int value)
        {
            value = Mathf.Max(value, 0);
            _attackText.text = value.ToString();
        }
        public void setAttackColor(PropertyNumberColor color)
        {
            _attackText.setTextColor(color);
        }
        public void setLife(int value)
        {
            _lifeText.text = value.ToString();
        }
        public void setLifeColor(PropertyNumberColor color)
        {
            _lifeText.setTextColor(color);
        }
        public void setOpen(bool open)
        {
            _animator.SetBool("ItemOff", !open);
        }
        public void setDeath(bool death)
        {
            _animator.SetBool("Death", death);
        }
        public void setExist(bool exist)
        {
            _animator.SetBool("Exists", exist);
        }
        public void setLethal(bool value)
        {
            _lethalIconObj.SetActive(value);
        }
        public void setLifeDrain(bool value)
        {
            _lifeDrainIconObj.SetActive(value);
        }
        public void setLastword(bool value)
        {
            _lastwordIconObj.SetActive(value);
        }

        public void setImageSprite(Sprite image)
        {
            _imageRenderer.setTableImageSprite(image, propertyBlock);
        }

        #region 事件回调
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter?.Invoke(this, eventData);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            onPointerExit?.Invoke(this, eventData);
        }
        #endregion

        #region 事件
        public event Action<Item, PointerEventData> onPointerEnter;
        public event Action<Item, PointerEventData> onPointerExit;
        #endregion

        #region 属性字段
        public PropertyNumber attackNumber => _attackText;
        public PropertyNumber durabilityNumber => _lifeText;
        public SimpleAnim onEquip;
        public SimpleAnim onLifeDown;
        public SimpleAnim onSetDefine;
        public SimpleAnim onDestroy;
        [SerializeField]
        private GameObject _attackTextObj;
        [SerializeField]
        private PropertyNumber _attackText;
        [SerializeField]
        private PropertyNumber _lifeText;
        [SerializeField]
        private MeshRenderer _imageRenderer;
        [SerializeField]
        private GameObject _lethalIconObj;
        [SerializeField]
        private GameObject _lifeDrainIconObj;
        [SerializeField]
        private GameObject _lastwordIconObj;
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