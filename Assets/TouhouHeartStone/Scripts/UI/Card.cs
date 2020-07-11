using TouhouHeartstone;
using System;
using TouhouCardEngine;
using UnityEngine;
namespace UI
{
    partial class Card
    {
        [Obsolete]
        public TouhouCardEngine.Card card { get; private set; }
        [Obsolete]
        public void update(TouhouCardEngine.Card card, CardSkinData skin)
        {
            this.card = card;

            CostPropNumber.asText.text = card.getCost().ToString();
            if (card.define.type == CardDefineType.SERVANT)
            {
                TypeController = Type.Servant;
                AttackPropNumber.asText.text = card.getAttack().ToString();
                LifePropNumber.asText.text = card.getLife().ToString();
            }
            else
            {
                TypeController = Type.Spell;
            }

            if (skin != null)
            {
                Image.sprite = skin.image;
                NameText.text = skin.name;
                DescText.text = skin.desc;
                IsFaceupController = IsFaceup.True;
            }
            else
                IsFaceupController = IsFaceup.False;
        }
        public void update(CardDefine card, CardSkinData skin)
        {
            CostPropNumber.asText.text = card.getCost().ToString();
            AttackPropNumber.asText.text = card.getAttack().ToString();
            LifePropNumber.asText.text = card.getLife().ToString();

            Image.sprite = skin.image;
            NameText.text = skin.name;
            DescText.text = skin.desc;
            IsFaceupController = IsFaceup.True;
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
    }
}
