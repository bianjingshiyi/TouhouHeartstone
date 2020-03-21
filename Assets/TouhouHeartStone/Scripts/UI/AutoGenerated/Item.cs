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
    
    public partial class Item : UIObject
    {
        protected override void Awake()
        {
            base.Awake();
            this.autoBind();
            this.onAwake();
        }
        public void autoBind()
        {
            this._Mask = this.transform.Find("Mask").GetComponent<Mask>();
            this._Image = this.transform.Find("Mask").Find("Image").GetComponent<Image>();
            this._FrameImage = this.transform.Find("Frame").GetComponent<Image>();
            this._AttackImage = this.transform.Find("Attack").GetComponent<Image>();
            this._AttackText = this.transform.Find("Attack").Find("AttackText").GetComponent<Text>();
            this._DurabilityImage = this.transform.Find("Durability").GetComponent<Image>();
            this._DurabilityText = this.transform.Find("Durability").Find("DurabilityText").GetComponent<Text>();
        }
        [SerializeField()]
        private Mask _Mask;
        public Mask Mask
        {
            get
            {
                if ((this._Mask == null))
                {
                    this._Mask = this.transform.Find("Mask").GetComponent<Mask>();
                }
                return this._Mask;
            }
        }
        [SerializeField()]
        private Image _Image;
        public Image Image
        {
            get
            {
                if ((this._Image == null))
                {
                    this._Image = this.transform.Find("Mask").Find("Image").GetComponent<Image>();
                }
                return this._Image;
            }
        }
        [SerializeField()]
        private Image _FrameImage;
        public Image FrameImage
        {
            get
            {
                if ((this._FrameImage == null))
                {
                    this._FrameImage = this.transform.Find("Frame").GetComponent<Image>();
                }
                return this._FrameImage;
            }
        }
        [SerializeField()]
        private Image _AttackImage;
        public Image AttackImage
        {
            get
            {
                if ((this._AttackImage == null))
                {
                    this._AttackImage = this.transform.Find("Attack").GetComponent<Image>();
                }
                return this._AttackImage;
            }
        }
        [SerializeField()]
        private Text _AttackText;
        public Text AttackText
        {
            get
            {
                if ((this._AttackText == null))
                {
                    this._AttackText = this.transform.Find("Attack").Find("AttackText").GetComponent<Text>();
                }
                return this._AttackText;
            }
        }
        [SerializeField()]
        private Image _DurabilityImage;
        public Image DurabilityImage
        {
            get
            {
                if ((this._DurabilityImage == null))
                {
                    this._DurabilityImage = this.transform.Find("Durability").GetComponent<Image>();
                }
                return this._DurabilityImage;
            }
        }
        [SerializeField()]
        private Text _DurabilityText;
        public Text DurabilityText
        {
            get
            {
                if ((this._DurabilityText == null))
                {
                    this._DurabilityText = this.transform.Find("Durability").Find("DurabilityText").GetComponent<Text>();
                }
                return this._DurabilityText;
            }
        }
        partial void onAwake();
    }
}
