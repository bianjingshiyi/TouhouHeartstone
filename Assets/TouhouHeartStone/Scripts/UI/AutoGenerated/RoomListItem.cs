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
    
    public partial class RoomListItem : UIObject
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
            this._RoomNameText = this.transform.Find("Texts").Find("RoomName").GetComponent<Text>();
            this._RoomDescText = this.transform.Find("Texts").Find("RoomDesc").GetComponent<Text>();
            this._Button = this.transform.Find("Button").GetComponent<Button>();
        }
        private RoomList _parent;
        public RoomList parent
        {
            get
            {
                return this.transform.parent.GetComponent<RoomList>();
            }
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
        private Text _RoomNameText;
        public Text RoomNameText
        {
            get
            {
                if ((this._RoomNameText == null))
                {
                    this._RoomNameText = this.transform.Find("Texts").Find("RoomName").GetComponent<Text>();
                }
                return this._RoomNameText;
            }
        }
        [SerializeField()]
        private Text _RoomDescText;
        public Text RoomDescText
        {
            get
            {
                if ((this._RoomDescText == null))
                {
                    this._RoomDescText = this.transform.Find("Texts").Find("RoomDesc").GetComponent<Text>();
                }
                return this._RoomDescText;
            }
        }
        [SerializeField()]
        private Button _Button;
        public Button Button
        {
            get
            {
                if ((this._Button == null))
                {
                    this._Button = this.transform.Find("Button").GetComponent<Button>();
                }
                return this._Button;
            }
        }
        partial void onAwake();
    }
}