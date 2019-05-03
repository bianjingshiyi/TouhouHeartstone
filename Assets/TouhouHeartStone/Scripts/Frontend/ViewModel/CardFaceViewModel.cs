using System.ComponentModel;
using System;
using UnityEngine;
using UnityWeld.Binding;
using TouhouHeartstone.Frontend.View;

namespace TouhouHeartstone.Frontend.ViewModel
{
    /// <summary>
    /// 卡面的VM
    /// </summary>
    [Binding]
    public class CardFaceViewModel : MonoBehaviour, INotifyPropertyChanged
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

        /// <summary>
        /// 动画播放的事件，监听此事件用于处理各种奇奇怪怪的动画
        /// </summary>
        public event CallbackEvent OnAnimationPlay;

        /// <summary>
        /// 播放动画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="callback"></param>
        public void PlayAnimation(object sender, CardAnimationEventArgs args, GenericAction callback)
        {
            OnAnimationPlay?.Invoke(sender, args, callback);
        }

        /// <summary>
        /// 卡片动作的事件，监听此事件以获取卡片操作
        /// </summary>
        public event CallbackEvent OnCardAction;

        public void DoCardAction(string name, EventArgs args)
        {
            // todo: 把卡片事件名称给封装进去
            OnCardAction?.Invoke(gameObject, args);
        }

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

        /// <summary>
        /// 位置改变事件（给VM监听）
        /// </summary>
        public event Action OnIndexChangeEvent;

        /// <summary>
        /// 通知位置改变了
        /// </summary>
        public void OnIndexChange()
        {
            OnIndexChangeEvent?.Invoke();
        }

        public event GenericAction DrawCallback;

        /// <summary>
        /// 卡片抽到手上了
        /// </summary>
        public void OnDrawCard()
        {
            DrawCallback?.Invoke(this, null);
        }

        public event Action<CardFaceViewModel> OnDestroyEvent;

        void OnDestroy()
        {
            OnDestroyEvent?.Invoke(this);
        }

        /// <summary>
        /// 使用这张卡
        /// </summary>
        public void Use()
        {
            // 通常效果卡
            UberDebug.LogDebugChannel("Frontend", $"使用卡{this}");

            // debug: 假设这是张随从卡
            OnCardUse(this, new Model.UseCardWithPositionArgs(0));
        }

        /// <summary>
        /// 卡片被使用的事件
        /// </summary>
        public event Action<CardFaceViewModel, Model.UseCardEventArgs> OnCardUse;


        /// <summary>
        /// 确定卡被使用的事件
        /// </summary>
        public event Action<Model.UseCardEventArgs, GenericAction> OnCardUseComfirm;
        /// <summary>
        /// 被使用了
        /// </summary>
        public void OnUse(Model.UseCardEventArgs args, GenericAction callback)
        {
            OnCardUseComfirm?.Invoke(args, callback);
        }

        public override string ToString()
        {
            return $"CardVM {RuntimeID}(Type {CardID})";
        }
    }
}
