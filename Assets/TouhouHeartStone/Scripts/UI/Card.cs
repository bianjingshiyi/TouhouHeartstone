using BJSYGameCore.UI;
using Game;
using TouhouHeartstone;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class Card : UIObject, IPointerEnterHandler, IPointerExitHandler
    {
        public void setTypeByName(string typeName)
        {
            var type = UICardType.Servant;
            switch (typeName)
            {
                case DEFINE.ITEM:
                    type = UICardType.Item;
                    break;
                case DEFINE.SPELL:
                case DEFINE.SKILL:
                    type = UICardType.Spell;
                    break;
                default:
                    type = UICardType.Servant;
                    break;
            }
            setType(type);
        }
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
        }
        private void exitHover()
        {
            if (!_hovered)
                return;
            _hovered = false;
            _animator.SetBool("isHovered", false);
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
        public ServantCardFace servantCardFace => _servantCardFace;
        public SpellCardFace spellCardFace => _spellCardFace;
        public ItemCardFace itemCardFace => _itemCardFace;
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
}
