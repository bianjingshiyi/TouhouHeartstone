using BJSYGameCore.UI;
using TouhouHeartstone;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class Card : UIObject
    {
        //[Obsolete]
        //public void update(TouhouCardEngine.Card card, CardSkinData skin)
        //{
        //    this.card = card;

        //    CostPropNumber.text.text = card.getCost(null).ToString();
        //    if (card.define.type == DEFINE.SERVANT)
        //    {
        //        type = CATEGORY.SERVANT;
        //        // TypeController = Type.Servant;
        //        //onTypeControllerServant?.Invoke();
        //        AttackPropNumber.text.text = card.getAttack(null).ToString();
        //        LifePropNumber.text.text = card.getLife(null).ToString();
        //    }
        //    else if (card.define.type == DEFINE.SPELL)
        //    {
        //        // TypeController = Type.Spell;
        //        type = CATEGORY.SPELL;
        //    }

        //    if (skin != null)
        //    {
        //        Image.sprite = skin.image;
        //        NameText.text = skin.name;
        //        //{card:damage}
        //        DescText.text = skin.desc;
        //        // IsFaceupController = IsFaceup.True;
        //        isFaceup = true;
        //    }
        //    else
        //    {
        //        // IsFaceupController = IsFaceup.False;
        //        isFaceup = false;
        //    }
        //}
        //[Obsolete]
        //public void update(CardDefine card, CardSkinData skin)
        //{
        //    CostPropNumber.text.text = card.getCost().ToString();
        //    AttackPropNumber.text.text = card.getAttack().ToString();
        //    LifePropNumber.text.text = card.getLife().ToString();

        //    Image.sprite = skin.image;
        //    NameText.text = skin.name;
        //    DescText.text = skin.desc;
        //    // IsFaceupController = IsFaceup.True;
        //    isFaceup = true;
        //}
        public void setTypeByName(string typeName)
        {
            switch (typeName)
            {
                case DEFINE.MASTER:
                    type = CATEGORY.MASTER;
                    break;
                case DEFINE.SPELL:
                    type = CATEGORY.SPELL;
                    break;
                case DEFINE.ITEM:
                    type = CATEGORY.ITEM;
                    break;
                case DEFINE.SKILL:
                    type = CATEGORY.SKILL;
                    break;
                default://SERVANT
                    type = CATEGORY.SERVANT;
                    break;
            }
        }
        public void resetPosition()
        {
            rectTransform.anchoredPosition = Vector2.zero;
        }
        [SerializeField]
        AnimationCurve _useCurve = new AnimationCurve();
        public AnimationCurve useCurve
        {
            get { return _useCurve; }
        }
        [SerializeField]
        AnimationCurve _drawCurve = new AnimationCurve();
        public AnimationCurve drawCurve
        {
            get { return _drawCurve; }
        }
        public bool isFaceup
        {
            set
            {
                _cardFaceObj.SetActive(value);
                _cardBackObj.SetActive(!value);
            }
        }
        public int type
        {
            get
            {
                return _type;
            }
            set
            {
                bool isItem = value == CATEGORY.ITEM;
                bool isSpell = value == CATEGORY.SPELL || value == CATEGORY.SKILL;
                bool isServant = !isItem && !isSpell;
                servantCardFace.gameObject.SetActive(isServant);
                spellCardFace.gameObject.SetActive(isSpell);
                itemCardFace.gameObject.SetActive(isItem);
            }
        }
        public string _flipToFaceAnimName;
        public string _flipToBackAnimName;
        public string _notifyAnimName;
        public SimpleAnim onCast;
        public SimpleAnim ShowAnim;
        public ServantCardFace servantCardFace => _servantCardFace;
        public SpellCardFace spellCardFace => _spellCardFace;
        public ItemCardFace itemCardFace => _itemCardFace;
        private int _type;
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
    }
}
