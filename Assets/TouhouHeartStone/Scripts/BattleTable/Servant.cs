using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class Servant : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region 公有方法
        public void setHighlight(bool highlight)
        {
            _highlight.SetActive(highlight);
        }
        public void setAttack(int value)
        {
            value = Mathf.Max(value, 0);
            _attackText.text = value.ToString();
        }
        public void setAttackColor(Color color)
        {
            _attackText.color = color;
        }
        public void setLife(int value)
        {
            _lifeText.text = value.ToString();
        }
        public void setLifeColor(Color color)
        {
            _lifeText.color = color;
        }
        public void setTaunt(bool enabled)
        {
            _animator.SetBool("isTaunt", enabled);
        }
        public void setStealth(bool enabled)
        {
            _animator.SetBool("isStealth", enabled);
        }
        public void setShackle(bool enabled)
        {
            _animator.SetBool("isShackle", enabled);
        }
        public void setSealed(bool enabled)
        {
            _animator.SetBool("isSilence", enabled);
        }
        public void setShield(bool enabled)
        {
            _animator.SetBool("isShield", enabled);
        }
        public void setLastword(bool enabled)
        {
            _animator.SetBool("isDyingWish", enabled);
        }
        public void setLifeDrain(bool enabled)
        {
            _animator.SetBool("isLifeSteal", enabled);
        }
        public void setLethal(bool enabled)
        {
            _animator.SetBool("isLethal", enabled);
        }
        public void setImageSprite(Sprite sprite)
        {
        }
        public void destroy()
        {
            Destroy(gameObject);
        }
        #endregion

        #region 接口实现
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(this, eventData);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            onBeginDrag?.Invoke(this, eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            onDrag?.Invoke(this, eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            onEndDrag?.Invoke(this, eventData);
        }
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
        public event Action<Servant, PointerEventData> onClick;
        public event Action<Servant, PointerEventData> onBeginDrag;
        public event Action<Servant, PointerEventData> onDrag;
        public event Action<Servant, PointerEventData> onEndDrag;
        public event Action<Servant, PointerEventData> onPointerEnter;
        public event Action<Servant, PointerEventData> onPointerExit;
        #endregion

        #region 属性字段
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private GameObject _highlight;
        [SerializeField]
        private TextMeshPro _attackText;
        [SerializeField]
        private TextMeshPro _lifeText;
        [SerializeField]
        private GameObject _stealthEffect;
        [SerializeField]
        private GameObject _shackleEffect;
        [SerializeField]
        private GameObject _sealEffect;
        [SerializeField]
        private GameObject _shieldEffect;
        [SerializeField]
        private GameObject _lastwordsIcon;
        [SerializeField]
        private GameObject _lifeDrainIcon;
        [SerializeField]
        private GameObject _lethalIcon;
        #endregion
    }
}