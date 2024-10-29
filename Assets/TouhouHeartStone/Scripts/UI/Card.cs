using System;
using BJSYGameCore.UI;
using Game;
using Game.BattleTable;
using TouhouHeartstone;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class Card : UIObject, IPointerEnterHandler, IPointerExitHandler
    {
        public void setType(UICardType type)
        {
            bool isItem = type == UICardType.Item;
            bool isSpell = type == UICardType.Spell;
            bool isServant = !isItem && !isSpell;
            servantCardFace.gameObject.SetActive(isServant);
            spellCardFace.gameObject.SetActive(isSpell);
            itemCardFace.gameObject.SetActive(isItem);
        }
        public void setFaceup(bool value)
        {
            _cardFaceObj.SetActive(value);
            _cardBackObj.SetActive(!value);
        }
        public void resetPosition()
        {
            rectTransform.anchoredPosition = Vector2.zero;
        }


        private void OnDisable()
        {
            exitHover();
        }
        private void enterHover()
        {
            if (_hovered)
                return;
            _hovered = true;
            if (_playHoverSound)
                _sePlayer?.Play();
            _animator.SetBool("isHovered", true);
            onHoverEnter?.Invoke(this);
        }
        private void exitHover()
        {
            if (!_hovered)
                return;
            _hovered = false;
            _animator.SetBool("isHovered", false);
            onHoverExit?.Invoke(this);
        }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (!_canHover)
                return;
            enterHover();
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            exitHover();
        }
        public event Action<Card> onHoverEnter;
        public event Action<Card> onHoverExit;

        public ServantCardFace servantCardFace => _servantCardFace;
        public SpellCardFace spellCardFace => _spellCardFace;
        public ItemCardFace itemCardFace => _itemCardFace;
        public Transform centerTransform => _centerTransform;
        private int _type;
        private bool _hovered;
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private ServantCardFace _servantCardFace;
        [SerializeField]
        private SpellCardFace _spellCardFace;
        [SerializeField]
        private ItemCardFace _itemCardFace;
        [SerializeField]
        private GameObject _cardFaceObj;
        [SerializeField]
        private GameObject _cardBackObj;
        [SerializeField]
        private Transform _centerTransform;

        [Header("Pointer Hover")]
        [SerializeField]
        private bool _canHover;
        [SerializeField]
        private bool _playHoverSound;
        [SerializeField]
        private SEPlayer _sePlayer;
    }
    public enum UICardType
    {
        Servant,
        Spell,
        Item
    }
    public struct UICardViewData
    {
        public UICardType type;
        public bool isFaceUp;
        public bool costVisible;
        public string cost;
        public string attack;
        public string life;
        public PropertyNumberColor costColor;
        public PropertyNumberColor attackColor;
        public PropertyNumberColor lifeColor;
        public string tag;
        public string name;
        public string subname;
        public string desc;
    }
}
