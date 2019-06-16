using UnityEngine;
using System;
using System.ComponentModel;

using UnityWeld.Binding;

namespace TouhouHeartstone.Frontend
{
    /// <summary>
    /// 卡片图像资源类
    /// </summary>
    [Serializable]
    public class CardImageResource: ICardResource, Ii18n
    {
        [SerializeField]
        string id = "";

        /// <summary>
        /// 资源对应ID
        /// </summary>
        public string ID => id;

        [SerializeField]
        string lang = "";

        /// <summary>
        /// 资源对应语言
        /// </summary>
        public string Lang => lang;

        [SerializeField]
        Sprite mainSprite = null;

        /// <summary>
        /// 主（中心）图像
        /// </summary>
        public Sprite SpriteMain => mainSprite;

        [SerializeField]
        Sprite bkgSprite = null;

        /// <summary>
        /// 卡片背景图像
        /// </summary>
        public Sprite SpriteBkg => bkgSprite;

        [SerializeField]
        Sprite ribbonSprite = null;

        /// <summary>
        /// 卡片装饰图像
        /// </summary>
        public Sprite SpriteRibbon => ribbonSprite;
    }

    /// <summary>
    /// 卡片文本资源类
    /// </summary>
    [Serializable]
    public class CardTextResource : ICardResource, Ii18n
    {
        [SerializeField]
        string id = "";

        public string ID => id;

        [SerializeField]
        string lang = "";

        public string Lang => lang;

        [SerializeField]
        string name = "";

        /// <summary>
        /// 卡片主标题文字
        /// </summary>
        public string Name => name;

        [SerializeField]
        string desc = "";

        /// <summary>
        /// 卡片描述文字
        /// </summary>
        public string Description => desc;

        [SerializeField]
        string type = "";

        /// <summary>
        /// 卡片种类文字
        /// </summary>
        public string Type => type;
    }

    public class CardImageRes : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Sprite _MainSprite;
        public Sprite MainSprite
        {
            get { return _MainSprite; }
            set { _MainSprite = value; NotifyPropertyChange("MainSprite"); }
        }

        private Sprite _BkgSprite;
        public Sprite BkgSprite
        {
            get { return _BkgSprite; }
            set { _BkgSprite = value; NotifyPropertyChange("BkgSprite"); }
        }

        private Sprite _RibbonSprite;
        public Sprite RibbonSprite
        {
            get { return _RibbonSprite; }
            set { _RibbonSprite = value; NotifyPropertyChange("RibbonSprite"); }
        }
    }

    /// <summary>
    /// 卡片当前数据
    /// </summary>
    public class CardSpecification : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 通知 Property 改变
        /// </summary>
        void NotifyPropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _HP;
        public int HP
        {
            get { return _HP; }
            set { _HP = value; NotifyPropertyChange("HP"); }
        }

        private int _Atk;
        public int Atk
        {
            get { return _Atk; }
            set { _Atk = value; NotifyPropertyChange("Atk"); }
        }

        private int _Cost;
        public int Cost
        {
            get { return _Cost; }
            set { _Cost = value; NotifyPropertyChange("Cost"); }
        }
    }

    public interface Ii18n
    {
        string Lang { get; }
    }

    public interface ICardResource
    {
        /// <summary>
        /// 卡片类型的ID
        /// </summary>
        string ID { get; }
    }

    public interface Ii18nSelector<T> where T : Ii18n
    {
        void SetFallbackLang(string lang);
        void SetDefaultLang(string lang);
        T Get();
        T Get(string lang);
        bool Exist(string lang);
    }

    public abstract class I18nSelector<T> : Ii18nSelector<T> where T : Ii18n
    {
        string fallbackLang = "en-US";
        string defaultLang = "en-US";
        public void SetFallbackLang(string lang)
        {
            fallbackLang = lang;
        }

        public void SetDefaultLang(string lang)
        {
            defaultLang = lang;
        }

        protected abstract T InnerGet(string lang);
        public abstract bool Exist(string lang);
        public virtual T Get()
        {
            if (Exist(defaultLang))
                return InnerGet(defaultLang);
            
            if (Exist(fallbackLang))
                return InnerGet(fallbackLang);
            
            throw new I18nLangNotFoundException(fallbackLang);
        }
        public virtual T Get(string lang)
        {
            if (Exist(lang))
                return InnerGet(lang);

            if (Exist(fallbackLang))
                return InnerGet(fallbackLang);
            
            throw new I18nLangNotFoundException(fallbackLang);
        }
    }

    [System.Serializable]
    public class I18nLangNotFoundException : System.Exception
    {
        public I18nLangNotFoundException() { }
        public I18nLangNotFoundException(string lang) : base($"{lang} 语言资源未找到") { }
        public I18nLangNotFoundException(string message, System.Exception inner) : base(message, inner) { }
        protected I18nLangNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class ResourcesIDNotFoundException : Exception
    {
        public ResourcesIDNotFoundException() { }
        public ResourcesIDNotFoundException(string id) : base($"{id} not found.") { }
        public ResourcesIDNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected ResourcesIDNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
