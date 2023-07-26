using System;
using Boo.Lang;
using TMPro;
using TouhouCardEngine;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class Master : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region 公有事件
        public void updateSecrets()
        {

        }
        public void setHighlight(bool highlight)
        {
            _animator.SetBool("HighlightYellow", highlight);
        }
        public void setAttackDisplay(bool display)
        {
            _animator.SetBool("ShowAttack", display);
            _attackText.gameObject.SetActive(display);
        }
        public void setShackle(bool enabled)
        {
            _animator.SetBool("isShackle", enabled);
        }
        public void setArmorDisplay(bool showArmor)
        {
            _armorObj.SetActive(showArmor);
        }
        public void addSecret(Secret secret)
        {
            _secretList.Add(secret);
        }
        public void insertSecret(int position, Secret secret)
        {
            _secretList.Insert(position, secret);
        }
        public void removeSecret(Secret secret)
        {
            _secretList.Remove(secret);
        }
        public void clearSecrets()
        {
            _secretList.Clear();
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
        public void setImageSprite(Sprite sprite)
        {

        }
        public void reset()
        {
            _positionLerp._time = 0.0f;
            _positionLerp.enabled = false;
            transform.localPosition = new Vector3(0, 0, 0);
            transform.localScale = new Vector3(1, 1, 1);
            _healingImageObj.SetActive(false);
            _imageRenderer.setColor(new Color(1, 1, 1, 1));
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
        #endregion

        #region 事件
        public event Action<Master, PointerEventData> onClick;
        public event Action<Master, PointerEventData> onBeginDrag;
        public event Action<Master, PointerEventData> onDrag;
        public event Action<Master, PointerEventData> onEndDrag;
        #endregion

        #region 属性字段
        [SerializeField]
        private Animator _animator;

        [SerializeField]
        private GameObject _armorObj;
        [SerializeField]
        private TextMeshPro _armorText;
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
        private GameObject _healingImageObj;

        [SerializeField]
        private List<Secret> _secretList = new List<Secret>();

        [SerializeField]
        private MeshRenderer _imageRenderer;
        [SerializeField]
        private PositionLerp _positionLerp;
        #endregion
    }
}