using TouhouHeartstone;
using TouhouCardEngine;
using UnityEngine;
namespace UI
{
    partial class Card
    {
        public TouhouCardEngine.Card card { get; private set; }
        public void update(TouhouCardEngine.Card card, CardSkinData skin)
        {
            this.card = card;

            CostText.text = card.getCost().ToString();
            if (card.define.type == CardDefineType.SERVANT)
            {
                TypeController = Type.Servant;
                AttackText.text = card.getAttack().ToString();
                LifeText.text = card.getLife().ToString();
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
            CostText.text = card.getCost().ToString();
            AttackText.text = card.getAttack().ToString();
            LifeText.text = card.getLife().ToString();

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
