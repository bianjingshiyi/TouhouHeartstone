using System;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class TableCard : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public void setVisible(bool visible)
        {
            _cardRoot.SetActive(visible);
        }
        public void animateFaceup(bool faceup)
        {
            setFaceup(faceup, false);
        }
        public void setFaceup(bool faceup, bool immediate = true)
        {
            animator.SetBool("isFaceUp", faceup);
            if (immediate)
            {
                animator.Play(faceup ? _faceFrontStateName : _faceBackStateName, -1, 0);
            }
            _isFaceup = faceup;
        }
        public void setType(string type)
        {
            _servantCardFace.setVisible(type == DEFINE.SERVANT || type == DEFINE.MASTER);
            _spellCardFace.setVisible(type == DEFINE.SPELL || type == DEFINE.SKILL);
            _itemCardFace.setVisible(type == DEFINE.ITEM);
        }
        public PropertyNumber getAttackPropNum(string cardType)
        {
            switch (cardType)
            {
                case DEFINE.SERVANT:
                case DEFINE.MASTER:
                    return _servantCardFace.attackPropNumber;
                case DEFINE.ITEM:
                    return _itemCardFace.attackPropNumber;
            }
            return null;
        }
        public PropertyNumber getLifePropNum(string cardType)
        {
            switch (cardType)
            {
                case DEFINE.SERVANT:
                case DEFINE.MASTER:
                    return _servantCardFace.lifePropNumber;
                case DEFINE.ITEM:
                    return _itemCardFace.lifePropNumber;
            }
            return null;
        }
        public PropertyNumber getCostPropNum(string cardType)
        {
            switch (cardType)
            {
                case DEFINE.SERVANT:
                case DEFINE.MASTER:
                    return _servantCardFace.costPropNumber;
                case DEFINE.SPELL:
                case DEFINE.SKILL:
                    return _spellCardFace.costPropNumber;
                case DEFINE.ITEM:
                    return _itemCardFace.costPropNumber;
            }
            return null;
        }
        public void setPreheat(bool preheat)
        {
            _preheatEffectFaceObj.SetActive(preheat);
            _preheatEffectBackObj.SetActive(preheat);
        }
        public void setTag(string name)
        {
            _servantCardFace.setTag(name);
        }
        public void setName(string name)
        {
            _servantCardFace.nameText.text = name;
            _spellCardFace.nameText.text = name;
            _itemCardFace.nameText.text = name;
        }
        public void setSubname(string name)
        {
            _servantCardFace.subnameText.text = name;
        }
        public void setDesc(string desc)
        {
            _servantCardFace.descText.text = desc;
            _spellCardFace.descText.text = desc;
            _itemCardFace.descText.text = desc;
        }
        public void setCost(int value)
        {
            _servantCardFace.costPropNumber.text = value.ToString();
            _spellCardFace.costPropNumber.text = value.ToString();
            _itemCardFace.costPropNumber.text = value.ToString();
        }
        public void setAttack(int value)
        {
            value = Math.Max(0, value);
            _servantCardFace.attackPropNumber.text = value.ToString();
            _itemCardFace.attackPropNumber.text = value.ToString();
        }
        public void setLife(int value)
        {
            _servantCardFace.lifePropNumber.text = value.ToString();
            _itemCardFace.lifePropNumber.text = value.ToString();
        }
        public void setImageSprite(Sprite sprite)
        {
            _servantCardFace.setImage(sprite);
            _spellCardFace.setImage(sprite);
            _itemCardFace.setImage(sprite);
        }
        public void destroy()
        {
            Destroy(gameObject);
        }

        #region 事件回调
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
        public event Action<TableCard, PointerEventData> onClick;
        public event Action<TableCard, PointerEventData> onBeginDrag;
        public event Action<TableCard, PointerEventData> onDrag;
        public event Action<TableCard, PointerEventData> onEndDrag;
        public event Action<TableCard, PointerEventData> onPointerEnter;
        public event Action<TableCard, PointerEventData> onPointerExit;
        #endregion

        #region 属性字段
        public Animator animator => _animator;
        public PositionLerp positionLerp => _positionLerp;
        public float width => _width;
        public float height => _height;
        public bool isFaceup => _isFaceup;
        public SimpleAnim onSummon;
        public SimpleAnim onDisappear;
        public SimpleAnim onShowAnim;
        public SimpleAnim onFlipToFace;
        public SimpleAnim onFlipToBack;
        public SimpleAnim onNotify;
        [SerializeField]
        private GameObject _cardRoot;
        [SerializeField]
        private string _faceFrontStateName;
        [SerializeField]
        private string _faceBackStateName;

        [SerializeField]
        private float _width;
        [SerializeField]
        private float _height;
        [SerializeField]
        private GameObject _preheatEffectFaceObj;
        [SerializeField]
        private GameObject _preheatEffectBackObj;
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private PositionLerp _positionLerp;
        [SerializeField]
        private GameObject cardBack;
        [SerializeField]
        private GameObject cardFront;
        [SerializeField]
        private ServantCardFace _servantCardFace;
        [SerializeField]
        private SpellCardFace _spellCardFace;
        [SerializeField]
        private ItemCardFace _itemCardFace;

        private bool _isFaceup = true;
        #endregion
    }
}