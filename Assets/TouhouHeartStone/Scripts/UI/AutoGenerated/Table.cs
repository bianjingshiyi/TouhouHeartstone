//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using BJSYGameCore.UI;
    
    public partial class Table : UIObject
    {
        protected override void Awake()
        {
            base.Awake();
            this.autoBind();
            this.onAwake();
        }
        public void autoBind()
        {
            this.m_as_Image = this.GetComponent<Image>();
            this._FieldsImage = this.transform.Find("Fields").GetComponent<Image>();
            this._TurnEndButton = this.transform.Find("Fields").Find("TurnEnd").GetComponent<Button>();
            this._SelfFieldList = this.transform.Find("Fields").Find("SelfField").GetComponent<FieldList>();
            this._Weather = this.transform.Find("Fields").Find("Weather").GetComponent<Weather>();
            this._EnemyFieldList = this.transform.Find("Fields").Find("EnemyField").GetComponent<FieldList>();
            this._TimeoutSlider = this.transform.Find("Fields").Find("Timeout").GetComponent<Slider>();
            this._ServantPlaceHolder = this.transform.Find("Fields").Find("ServantPlaceHolder").GetComponent<ServantPlaceHolder>();
            this._AttackArrowImage = this.transform.Find("Fields").Find("AttackArrow").GetComponent<Image>();
            this._EnemyHandList = this.transform.Find("EnemyHand").GetComponent<HandList>();
            this._EnemyGem = this.transform.Find("EnemyGem").GetComponent<Gem>();
            this._EnemyMaster = this.transform.Find("EnemyMaster").GetComponent<Master>();
            this._EnemyDeck = this.transform.Find("EnemyDeck").GetComponent<Deck>();
            this._EnemyGraveDeck = this.transform.Find("EnemyGrave").GetComponent<Deck>();
            this._EnemySkill = this.transform.Find("EnemySkill").GetComponent<Skill>();
            this._EnemyItem = this.transform.Find("EnemyItem").GetComponent<Item>();
            this._SelfDeck = this.transform.Find("SelfDeck").GetComponent<Deck>();
            this._SelfGraveDeck = this.transform.Find("SelfGrave").GetComponent<Deck>();
            this._SelfMaster = this.transform.Find("SelfMaster").GetComponent<Master>();
            this._SelfSkill = this.transform.Find("SelfSkill").GetComponent<Skill>();
            this._SelfItem = this.transform.Find("SelfItem").GetComponent<Item>();
            this._SelfGem = this.transform.Find("SelfGem").GetComponent<Gem>();
            this._BlockerButton = this.transform.Find("Blocker").GetComponent<Button>();
            this._SelfHandList = this.transform.Find("SelfHand").GetComponent<HandList>();
            this._InitReplaceDialog = this.transform.Find("InitReplaceDialog").GetComponent<InitReplaceDialog>();
            this._TipText = this.transform.Find("TipText").GetComponent<Text>();
            this._LargeCard = this.transform.Find("LargeCard").GetComponent<Card>();
        }
        [SerializeField()]
        private Image m_as_Image;
        public Image asImage
        {
            get
            {
                if ((this.m_as_Image == null))
                {
                    this.m_as_Image = this.GetComponent<Image>();
                }
                return this.m_as_Image;
            }
        }
        [SerializeField()]
        private Image _FieldsImage;
        public Image FieldsImage
        {
            get
            {
                if ((this._FieldsImage == null))
                {
                    this._FieldsImage = this.transform.Find("Fields").GetComponent<Image>();
                }
                return this._FieldsImage;
            }
        }
        [SerializeField()]
        private Button _TurnEndButton;
        public Button TurnEndButton
        {
            get
            {
                if ((this._TurnEndButton == null))
                {
                    this._TurnEndButton = this.transform.Find("Fields").Find("TurnEnd").GetComponent<Button>();
                }
                return this._TurnEndButton;
            }
        }
        [SerializeField()]
        private FieldList _SelfFieldList;
        public FieldList SelfFieldList
        {
            get
            {
                if ((this._SelfFieldList == null))
                {
                    this._SelfFieldList = this.transform.Find("Fields").Find("SelfField").GetComponent<FieldList>();
                }
                return this._SelfFieldList;
            }
        }
        [SerializeField()]
        private Weather _Weather;
        public Weather Weather
        {
            get
            {
                if ((this._Weather == null))
                {
                    this._Weather = this.transform.Find("Fields").Find("Weather").GetComponent<Weather>();
                }
                return this._Weather;
            }
        }
        [SerializeField()]
        private FieldList _EnemyFieldList;
        public FieldList EnemyFieldList
        {
            get
            {
                if ((this._EnemyFieldList == null))
                {
                    this._EnemyFieldList = this.transform.Find("Fields").Find("EnemyField").GetComponent<FieldList>();
                }
                return this._EnemyFieldList;
            }
        }
        [SerializeField()]
        private Slider _TimeoutSlider;
        public Slider TimeoutSlider
        {
            get
            {
                if ((this._TimeoutSlider == null))
                {
                    this._TimeoutSlider = this.transform.Find("Fields").Find("Timeout").GetComponent<Slider>();
                }
                return this._TimeoutSlider;
            }
        }
        [SerializeField()]
        private ServantPlaceHolder _ServantPlaceHolder;
        public ServantPlaceHolder ServantPlaceHolder
        {
            get
            {
                if ((this._ServantPlaceHolder == null))
                {
                    this._ServantPlaceHolder = this.transform.Find("Fields").Find("ServantPlaceHolder").GetComponent<ServantPlaceHolder>();
                }
                return this._ServantPlaceHolder;
            }
        }
        [SerializeField()]
        private Image _AttackArrowImage;
        public Image AttackArrowImage
        {
            get
            {
                if ((this._AttackArrowImage == null))
                {
                    this._AttackArrowImage = this.transform.Find("Fields").Find("AttackArrow").GetComponent<Image>();
                }
                return this._AttackArrowImage;
            }
        }
        [SerializeField()]
        private HandList _EnemyHandList;
        public HandList EnemyHandList
        {
            get
            {
                if ((this._EnemyHandList == null))
                {
                    this._EnemyHandList = this.transform.Find("EnemyHand").GetComponent<HandList>();
                }
                return this._EnemyHandList;
            }
        }
        [SerializeField()]
        private Gem _EnemyGem;
        public Gem EnemyGem
        {
            get
            {
                if ((this._EnemyGem == null))
                {
                    this._EnemyGem = this.transform.Find("EnemyGem").GetComponent<Gem>();
                }
                return this._EnemyGem;
            }
        }
        [SerializeField()]
        private Master _EnemyMaster;
        public Master EnemyMaster
        {
            get
            {
                if ((this._EnemyMaster == null))
                {
                    this._EnemyMaster = this.transform.Find("EnemyMaster").GetComponent<Master>();
                }
                return this._EnemyMaster;
            }
        }
        [SerializeField()]
        private Deck _EnemyDeck;
        public Deck EnemyDeck
        {
            get
            {
                if ((this._EnemyDeck == null))
                {
                    this._EnemyDeck = this.transform.Find("EnemyDeck").GetComponent<Deck>();
                }
                return this._EnemyDeck;
            }
        }
        [SerializeField()]
        private Deck _EnemyGraveDeck;
        public Deck EnemyGraveDeck
        {
            get
            {
                if ((this._EnemyGraveDeck == null))
                {
                    this._EnemyGraveDeck = this.transform.Find("EnemyGrave").GetComponent<Deck>();
                }
                return this._EnemyGraveDeck;
            }
        }
        [SerializeField()]
        private Skill _EnemySkill;
        public Skill EnemySkill
        {
            get
            {
                if ((this._EnemySkill == null))
                {
                    this._EnemySkill = this.transform.Find("EnemySkill").GetComponent<Skill>();
                }
                return this._EnemySkill;
            }
        }
        [SerializeField()]
        private Item _EnemyItem;
        public Item EnemyItem
        {
            get
            {
                if ((this._EnemyItem == null))
                {
                    this._EnemyItem = this.transform.Find("EnemyItem").GetComponent<Item>();
                }
                return this._EnemyItem;
            }
        }
        [SerializeField()]
        private Deck _SelfDeck;
        public Deck SelfDeck
        {
            get
            {
                if ((this._SelfDeck == null))
                {
                    this._SelfDeck = this.transform.Find("SelfDeck").GetComponent<Deck>();
                }
                return this._SelfDeck;
            }
        }
        [SerializeField()]
        private Deck _SelfGraveDeck;
        public Deck SelfGraveDeck
        {
            get
            {
                if ((this._SelfGraveDeck == null))
                {
                    this._SelfGraveDeck = this.transform.Find("SelfGrave").GetComponent<Deck>();
                }
                return this._SelfGraveDeck;
            }
        }
        [SerializeField()]
        private Master _SelfMaster;
        public Master SelfMaster
        {
            get
            {
                if ((this._SelfMaster == null))
                {
                    this._SelfMaster = this.transform.Find("SelfMaster").GetComponent<Master>();
                }
                return this._SelfMaster;
            }
        }
        [SerializeField()]
        private Skill _SelfSkill;
        public Skill SelfSkill
        {
            get
            {
                if ((this._SelfSkill == null))
                {
                    this._SelfSkill = this.transform.Find("SelfSkill").GetComponent<Skill>();
                }
                return this._SelfSkill;
            }
        }
        [SerializeField()]
        private Item _SelfItem;
        public Item SelfItem
        {
            get
            {
                if ((this._SelfItem == null))
                {
                    this._SelfItem = this.transform.Find("SelfItem").GetComponent<Item>();
                }
                return this._SelfItem;
            }
        }
        [SerializeField()]
        private Gem _SelfGem;
        public Gem SelfGem
        {
            get
            {
                if ((this._SelfGem == null))
                {
                    this._SelfGem = this.transform.Find("SelfGem").GetComponent<Gem>();
                }
                return this._SelfGem;
            }
        }
        [SerializeField()]
        private Button _BlockerButton;
        public Button BlockerButton
        {
            get
            {
                if ((this._BlockerButton == null))
                {
                    this._BlockerButton = this.transform.Find("Blocker").GetComponent<Button>();
                }
                return this._BlockerButton;
            }
        }
        [SerializeField()]
        private HandList _SelfHandList;
        public HandList SelfHandList
        {
            get
            {
                if ((this._SelfHandList == null))
                {
                    this._SelfHandList = this.transform.Find("SelfHand").GetComponent<HandList>();
                }
                return this._SelfHandList;
            }
        }
        [SerializeField()]
        private InitReplaceDialog _InitReplaceDialog;
        public InitReplaceDialog InitReplaceDialog
        {
            get
            {
                if ((this._InitReplaceDialog == null))
                {
                    this._InitReplaceDialog = this.transform.Find("InitReplaceDialog").GetComponent<InitReplaceDialog>();
                }
                return this._InitReplaceDialog;
            }
        }
        [SerializeField()]
        private Text _TipText;
        public Text TipText
        {
            get
            {
                if ((this._TipText == null))
                {
                    this._TipText = this.transform.Find("TipText").GetComponent<Text>();
                }
                return this._TipText;
            }
        }
        [SerializeField()]
        private Card _LargeCard;
        public Card LargeCard
        {
            get
            {
                if ((this._LargeCard == null))
                {
                    this._LargeCard = this.transform.Find("LargeCard").GetComponent<Card>();
                }
                return this._LargeCard;
            }
        }
        partial void onAwake();
    }
}
