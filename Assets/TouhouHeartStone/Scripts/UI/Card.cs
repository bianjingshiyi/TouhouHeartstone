using TouhouHeartstone;
using System;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.Events;
namespace UI
{
    partial class Card
    {
        [Obsolete]
        public TouhouCardEngine.Card card { get; set; }
        [Obsolete]
        public void update(TouhouCardEngine.Card card, CardSkinData skin)
        {
            this.card = card;

            CostPropNumber.asText.text = card.getCost(null).ToString();
            if (card.define.type == CardDefineType.SERVANT)
            {
                type = CardCategory.SERVANT;
                // TypeController = Type.Servant;
                //onTypeControllerServant?.Invoke();
                AttackPropNumber.asText.text = card.getAttack(null).ToString();
                LifePropNumber.asText.text = card.getLife(null).ToString();
            }
            else if (card.define.type == CardDefineType.SPELL)
            {
                // TypeController = Type.Spell;
                type = CardCategory.SPELL;
            }

            if (skin != null)
            {
                Image.sprite = skin.image;
                NameText.text = skin.name;
                //{card:damage}
                DescText.text = skin.desc;
                // IsFaceupController = IsFaceup.True;
                isFaceup = true;
            }
            else
            {
                // IsFaceupController = IsFaceup.False;
                isFaceup = false;
            }
        }
        public void update(CardDefine card, CardSkinData skin)
        {
            CostPropNumber.asText.text = card.getCost().ToString();
            AttackPropNumber.asText.text = card.getAttack().ToString();
            LifePropNumber.asText.text = card.getLife().ToString();

            Image.sprite = skin.image;
            NameText.text = skin.name;
            DescText.text = skin.desc;
            // IsFaceupController = IsFaceup.True;
            isFaceup = true;
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
                if (value)
                {
                    _onFaceupControllerTrue.Invoke();
                    AttackPropNumber.display();
                    LifePropNumber.display();
                }
                else
                {
                    _onFaceupControllerFalse.Invoke();
                    AttackPropNumber.hide();
                    LifePropNumber.hide();
                }
            }
        }
        public int type
        {
            set
            {
                switch (value)
                {
                    case CardCategory.SPELL:
                        _onTypeControllerSpell.Invoke();
                        break;
                    default://SERVANT
                        _onTypeControllerServant.Invoke();
                        break;
                }
            }
        }
        [SerializeField]
        private UnityEvent _onTypeControllerSpell;
        [SerializeField]
        private UnityEvent _onTypeControllerServant;
        [SerializeField]
        private UnityEvent _onFaceupControllerFalse;
        [SerializeField]
        private UnityEvent _onFaceupControllerTrue;
        public SimpleAnim onCast;
    }
}
