using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class Master : TableUnit, IPointerClickHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region 公有事件
        public void updateMaster(float deltaTime)
        {
            HeadSlots.updateHead(deltaTime);
        }
        public void setHighlight(bool highlight, bool yellow = false)
        {
            _animator.SetBool("HighLightGreen", highlight && !yellow);
            _animator.SetBool("HighLightYellow", highlight && yellow);
        }
        public void setAttackDisplay(bool display)
        {
            _animator.SetBool("ShowAttack", display);
        }
        public void setShackle(bool enabled)
        {
            _animator.SetBool("isShackle", enabled);
        }
        public void setArmorDisplay(bool showArmor)
        {
            _animator.SetBool("hasArmor", showArmor);
        }
        public void setArmor(int value)
        {
            value = Mathf.Max(value, 0);
            _armorText.text = value.ToString();
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
        public void setStealth(bool enabled)
        {
            _stealthEffect?.SetActive(enabled);
        }
        public void setSealed(bool enabled)
        {
            _sealEffect?.SetActive(enabled);
        }
        public void setShield(bool enabled)
        {
            _shieldEffect?.SetActive(enabled);
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
            _imageRenderer.setTableImageSprite(sprite, propertyBlock);
        }
        public void reset()
        {
            _positionLerp._time = 0.0f;
            _positionLerp.enabled = false;
            transform.localScale = new Vector3(1, 1, 1);
            _healingImageObj.SetActive(false);
            HeadSlots.clear();
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
        #endregion

        #region 事件
        public event Action<Master, PointerEventData> onClick;
        public event Action<Master, PointerEventData> onBeginDrag;
        public event Action<Master, PointerEventData> onDrag;
        public event Action<Master, PointerEventData> onEndDrag;
        public event Action<Master, PointerEventData> onPointerUp;
        #endregion

        #region 属性字段
        public PropertyNumber attackNumber => _attackText;
        public PropertyNumber lifeNumber => _lifeText;
        public HeadSlots HeadSlots => _headSlots;
        public SimpleAnim onTargeted;
        public SimpleAnim onAttack;
        public SimpleAnim onAttacked;
        public SimpleAnim onDamage;
        public ParticleSystem damageEffectPrefab;
        public SimpleAnim onSetDefine;
        public SimpleAnim onHeal;
        public ParticleSystem healEffectPrefab;
        public SimpleAnim onDeath;
        [SerializeField]
        private GameObject _armorObj;
        [SerializeField]
        private TextMeshPro _armorText;
        [SerializeField]
        private HeadSlots _headSlots;
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
        private TextMeshPro _damageText;
        [SerializeField]
        private TextMeshPro _healText;
        [SerializeField]
        private GameObject _healingImageObj;

        [SerializeField]
        private MeshRenderer _imageRenderer;
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