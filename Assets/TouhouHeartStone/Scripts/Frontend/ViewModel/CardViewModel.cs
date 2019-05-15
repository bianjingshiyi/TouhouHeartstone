using System.ComponentModel;
using System;
using UnityEngine;
using UnityWeld.Binding;
using TouhouHeartstone.Frontend.View;
using TouhouHeartstone.Frontend.Model;

namespace TouhouHeartstone.Frontend.ViewModel
{
    /// <summary>
    /// 卡的VM
    /// </summary>
    [Binding]
    public class CardViewModel : MonoBehaviour, INotifyPropertyChanged
    {
        #region cardface
        CardImageResource imageResource = new CardImageResource();
        public CardImageResource ImageResource
        {
            get
            {
                return imageResource;
            }
            protected set
            {
                imageResource = value;
                NotifyPropertyChange("SpriteMain");
                NotifyPropertyChange("SpriteBkg");
                NotifyPropertyChange("SpriteRibbon");
            }
        }


        private CardType _CardType;
        public CardType CardType
        {
            get { return _CardType; }
            set { _CardType = value; NotifyPropertyChange("CardType"); }
        }

        private int _CardID;
        /// <summary>
        /// 卡片类型ID
        /// </summary>
        public int CardID
        {
            get { return _CardID; }
            set
            {
                _CardID = value;
                var gv = GetComponentInParent<GlobalView>();
                ImageResource = gv.GetCardImageResource(_CardID);
                TextResource = gv.GetCardTextResource(_CardID);
                _CardType = CardType.PositionArg;
            }
        }

        private int _RuntimeID;
        /// <summary>
        /// 运行时ID
        /// </summary>
        public int RuntimeID
        {
            get { return _RuntimeID; }
            set { _RuntimeID = value; name = $"Card({value})"; }
        }

        [Binding]
        public Sprite SpriteMain => ImageResource.SpriteMain;

        [Binding]
        public Sprite SpriteBkg => ImageResource.SpriteBkg;

        [Binding]
        public Sprite SpriteRibbon => ImageResource.SpriteRibbon;

        CardTextResource textResource = new CardTextResource();
        public CardTextResource TextResource
        {
            get { return textResource; }
            protected set
            {
                textResource = value;
                NotifyPropertyChange("NameText");
                NotifyPropertyChange("DescText");
                NotifyPropertyChange("TypeText");
            }
        }

        [Binding]
        public string NameText => textResource.Name;
        [Binding]
        public string DescText => textResource.Description;
        [Binding]
        public string TypeText => textResource.Type;
        #endregion

        #region carddata
        CardSpecification cardSpec = new CardSpecification();
        public CardSpecification CardSpec
        {
            get
            {
                return cardSpec;
            }

            set
            {
                if (cardSpec != null)
                    cardSpec.PropertyChanged -= PropertyChanged;
                cardSpec = value;
                if (cardSpec != null)
                    cardSpec.PropertyChanged += PropertyChanged;

                NotifyPropertyChange("HP");
                NotifyPropertyChange("Atk");
                NotifyPropertyChange("Cost");
            }
        }

        [Binding]
        public int HP => cardSpec.HP;
        [Binding]
        public int Atk => cardSpec.Atk;
        [Binding]
        public int Cost => cardSpec.Cost;

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        void Awake()
        {
            if (cardSpec != null)
                cardSpec.PropertyChanged += PropertyChanged;
        }

        /// <summary>
        /// 当前卡片的位置
        /// </summary>
        public int Index { get; set; }

        public event Action<CardViewModel> OnDestroyEvent;

        void OnDestroy()
        {
            OnDestroyEvent?.Invoke(this);
        }

        /// <summary>
        /// 使用这张卡
        /// </summary>
        public void Use(UseCardEventArgs arg)
        {
            UberDebug.LogDebugChannel("Frontend", $"使用卡{this}");
            DoAction(arg);
        }

        public override string ToString()
        {
            return $"CardVM {RuntimeID}(Type {CardID})";
        }


        /// <summary>
        /// 卡片接收事件，监听此事件以获取系统指令
        /// </summary>
        public event CallbackEvent OnRecvActionEvent;

        /// <summary>
        /// 卡片动作的事件，监听此事件以获取卡片操作
        /// </summary>
        public event GenericAction OnActionEvent;

        /// <summary>
        /// VM 发布的Action
        /// </summary>
        public void RecvAction(EventArgs args, GenericAction callback = null)
        {
            if (args is ICardID)
            {
                if (CardID <= 0)
                {
                    CardID = (args as ICardID).CardDID;
                }
            }
            OnRecvActionEvent?.Invoke(this, args, callback);
        }

        /// <summary>
        /// View 发出的Action
        /// </summary>
        public void DoAction(EventArgs args)
        {
            if (args is ICardID)
            {
                (args as ICardID).CardRID = RuntimeID;
                (args as ICardID).CardDID = CardID;
            }

            OnActionEvent?.Invoke(this, args);
        }
    }

    public enum CardType
    {
        /// <summary>
        /// 无目标法术卡
        /// </summary>
        NoArg,
        /// <summary>
        /// 随从卡
        /// </summary>
        PositionArg,
        /// <summary>
        /// 有目标的法术卡
        /// </summary>
        TargetArg,
        /// <summary>
        /// 有目标的随从卡
        /// </summary>
        PositionTargerArg,
    }
}
