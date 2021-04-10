using TouhouHeartstone;
using System;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.Events;
namespace UI
{
    partial class Card
    {
        protected override void Awake()
        {
            base.Awake();
            init();
        }
        [Obsolete("Use table.getCard instead")]
        public TouhouCardEngine.Card card { get; set; }
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
                if (value)
                {
                    _onFaceupControllerTrue.Invoke();
                }
                else
                {
                    _onFaceupControllerFalse.Invoke();
                }
            }
        }
        public int type
        {
            set
            {
                switch (value)
                {
                    case CATEGORY.MASTER:
                        _onTypeControllerMaster.Invoke();
                        break;
                    case CATEGORY.SPELL:
                        asAnimator.SetBool(ANIM_PARAM_ISSERVANT, false);
                        asAnimator.SetBool(ANIM_PARAM_ISSPELL, true);
                        asAnimator.SetBool(ANIM_PARAM_ISITEM, false);
                        _onTypeControllerSpell.Invoke();
                        break;
                    case CATEGORY.ITEM:
                        asAnimator.SetBool(ANIM_PARAM_ISSERVANT, false);
                        asAnimator.SetBool(ANIM_PARAM_ISSPELL, false);
                        asAnimator.SetBool(ANIM_PARAM_ISITEM, true);
                        _onTypeControllerItem.Invoke();
                        break;
                    case CATEGORY.SKILL:
                        _onTypeControllerSkill.Invoke();
                        break;
                    default://SERVANT
                        asAnimator.SetBool(ANIM_PARAM_ISSERVANT, true);
                        asAnimator.SetBool(ANIM_PARAM_ISSPELL, false);
                        asAnimator.SetBool(ANIM_PARAM_ISITEM, false);
                        _onTypeControllerServant.Invoke();
                        break;
                }
            }
        }
        [SerializeField]
        private UnityEvent _onTypeControllerMaster;
        [SerializeField]
        private UnityEvent _onTypeControllerSpell;
        [SerializeField]
        private UnityEvent _onTypeControllerItem;
        [SerializeField]
        private UnityEvent _onTypeControllerSkill;
        [SerializeField]
        private UnityEvent _onTypeControllerServant;
        [SerializeField]
        private UnityEvent _onFaceupControllerFalse;
        [SerializeField]
        private UnityEvent _onFaceupControllerTrue;
        public string _flipToFaceAnimName;
        public string _flipToBackAnimName;
        public string _notifyAnimName;
        public SimpleAnim onCast;
        public SimpleAnim ShowAnim;
    }
}
