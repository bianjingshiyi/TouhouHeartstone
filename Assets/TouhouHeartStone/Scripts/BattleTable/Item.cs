using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class Item : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void setDisplay(bool display)
        {
            gameObject.SetActive(display);
        }
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

        public void setImageSprite(Sprite image)
        {
            // TODO:_imageRenderer.SetTexture(image);
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

        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private GameObject _attackTextObj;
        [SerializeField]
        private PropertyNumber _attackText;
        [SerializeField]
        private PropertyNumber _lifeText;
        [SerializeField]
        private MeshRenderer _imageRenderer;
    }
}