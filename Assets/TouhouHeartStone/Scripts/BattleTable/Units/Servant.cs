using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class Servant : TableUnit, IPointerClickHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region 公有方法
        public void setHighlight(bool highlight, bool yellow = false)
        {
            _animator.SetBool("HightLight_G", highlight && !yellow);
            _animator.SetBool("HightLight_Y", highlight && yellow);
        }
        public void setAttack(int value)
        {
            value = Mathf.Max(value, 0);
            _attackText.text = value.ToString();
        }
        public void setHidden(bool value)
        {
            _animator.SetBool("Hidden", value);
        }
        public void setLife(int value)
        {
            _lifeText.text = value.ToString();
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
            _animator.SetBool("isSeal", enabled);
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
        public void triggerDamaged()
        {
            _animator.SetTrigger("Damaged");
        }
        public void triggerHealed()
        {
            _animator.SetTrigger("Healed");
        }
        public void setDead(bool dead)
        {
            _animator.SetBool("isDead", dead);
        }
        public void showDamageText(int damage)
        {
            _damageText.text = "-" + damage;
            onDamage.invokeBeforeAnim();
        }
        public void showHealText(int heal)
        {
            _healText.text = "+" + heal;
            onHeal.invokeBeforeAnim();
        }
        public void setImageSprite(Sprite sprite)
        {
            _servantImage.setTableImageSprite(sprite, propertyBlock);
        }
        #endregion

        #region 接口实现
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(this, eventData);
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            onPointerUp?.Invoke(this, eventData);
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
        public event Action<Servant, PointerEventData> onPointerUp;
        public event Action<Servant, PointerEventData> onBeginDrag;
        public event Action<Servant, PointerEventData> onDrag;
        public event Action<Servant, PointerEventData> onEndDrag;
        public event Action<Servant, PointerEventData> onPointerEnter;
        public event Action<Servant, PointerEventData> onPointerExit;
        #endregion

        #region 属性字段
        public PropertyNumber attackNumber => _attackText;
        public PropertyNumber lifeNumber => _lifeText;
        public SimpleAnim onSummon;
        public SimpleAnim onTargeted;
        public SimpleAnim onAttack;
        public SimpleAnim onAttacked;
        public SimpleAnim onDamage;
        public SimpleAnim onHeal;
        public SimpleAnim onSetDefine;
        public SimpleAnim onSilenceTrue;
        public SimpleAnim onSilenceFalse;
        public SimpleAnim onDeath;
        [SerializeField]
        private PropertyNumber _attackText;
        [SerializeField]
        private PropertyNumber _lifeText;
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
        [SerializeField]
        private TextMeshPro _damageText;
        [SerializeField]
        private TextMeshPro _healText;
        [SerializeField]
        private MeshRenderer _servantImage;
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