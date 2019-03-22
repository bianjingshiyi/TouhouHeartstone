using System.ComponentModel;
using UnityEngine;
using UnityWeld.Binding;

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
            set
            {
                imageResource = value;
                NotifyPropertyChange("SpriteMain");
                NotifyPropertyChange("SpriteBkg");
                NotifyPropertyChange("SpriteRibbon");
            }
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
            set
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
    }
}
