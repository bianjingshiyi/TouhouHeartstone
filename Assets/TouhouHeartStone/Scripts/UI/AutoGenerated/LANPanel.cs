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
    
    public partial class LANPanel : UIObject
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
            this._TitleBarImage = this.transform.Find("TitleBar").GetComponent<Image>();
            this._TitleText = this.transform.Find("TitleBar").Find("Title").GetComponent<Text>();
            this._IP_FieldImage = this.transform.Find("IP_Field").GetComponent<Image>();
            this._IP_Field_TitleText = this.transform.Find("IP_Field").Find("Title").GetComponent<Text>();
            this._IPInputField = this.transform.Find("IP_Field").Find("IPInputField").GetComponent<InputField>();
            this._ConnectButtonButtonBlack = this.transform.Find("IP_Field").Find("ConnectButton").GetComponent<ButtonBlack>();
            this._CreateRoomButtonButtonBlack = this.transform.Find("CreateRoomButton").GetComponent<ButtonBlack>();
            this._FlushRoomButton = this.transform.Find("FlushRoomButton").GetComponent<Button>();
            this._RoomListPanelImage = this.transform.Find("RoomListPanel").GetComponent<Image>();
            this._RoomScrollView = this.transform.Find("RoomListPanel").Find("RoomScrollView").GetComponent<RoomScrollView>();
            this._RoomInfoPanelImage = this.transform.Find("RoomInfoPanel").GetComponent<Image>();
            this._ContainerVerticalLayoutGroup = this.transform.Find("RoomInfoPanel").Find("Container").GetComponent<VerticalLayoutGroup>();
            this._NameLabelText = this.transform.Find("RoomInfoPanel").Find("Container").Find("NameLabel").GetComponent<Text>();
            this._NameText = this.transform.Find("RoomInfoPanel").Find("Container").Find("NameText").GetComponent<Text>();
            this._IPLabelText = this.transform.Find("RoomInfoPanel").Find("Container").Find("IPLabel").GetComponent<Text>();
            this._IPText = this.transform.Find("RoomInfoPanel").Find("Container").Find("IPText").GetComponent<Text>();
            this._DescLabelText = this.transform.Find("RoomInfoPanel").Find("Container").Find("DescLabel").GetComponent<Text>();
            this._DescText = this.transform.Find("RoomInfoPanel").Find("Container").Find("DescText").GetComponent<Text>();
            this._ReturnBtnButtonBlack = this.transform.Find("ReturnBtn").GetComponent<ButtonBlack>();
            this._LoadingPanelImage = this.transform.Find("LoadingPanel").GetComponent<Image>();
            this._LoadingImage = this.transform.Find("LoadingPanel").Find("LoadingImage").GetComponent<Image>();
        }
        private Main _parent;
        public Main parent
        {
            get
            {
                return this.transform.parent.parent.parent.GetComponent<Main>();
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
        private Image _TitleBarImage;
        public Image TitleBarImage
        {
            get
            {
                if ((this._TitleBarImage == null))
                {
                    this._TitleBarImage = this.transform.Find("TitleBar").GetComponent<Image>();
                }
                return this._TitleBarImage;
            }
        }
        [SerializeField()]
        private Text _TitleText;
        public Text TitleText
        {
            get
            {
                if ((this._TitleText == null))
                {
                    this._TitleText = this.transform.Find("TitleBar").Find("Title").GetComponent<Text>();
                }
                return this._TitleText;
            }
        }
        [SerializeField()]
        private Image _IP_FieldImage;
        public Image IP_FieldImage
        {
            get
            {
                if ((this._IP_FieldImage == null))
                {
                    this._IP_FieldImage = this.transform.Find("IP_Field").GetComponent<Image>();
                }
                return this._IP_FieldImage;
            }
        }
        [SerializeField()]
        private Text _IP_Field_TitleText;
        public Text IP_Field_TitleText
        {
            get
            {
                if ((this._IP_Field_TitleText == null))
                {
                    this._IP_Field_TitleText = this.transform.Find("IP_Field").Find("Title").GetComponent<Text>();
                }
                return this._IP_Field_TitleText;
            }
        }
        [SerializeField()]
        private InputField _IPInputField;
        public InputField IPInputField
        {
            get
            {
                if ((this._IPInputField == null))
                {
                    this._IPInputField = this.transform.Find("IP_Field").Find("IPInputField").GetComponent<InputField>();
                }
                return this._IPInputField;
            }
        }
        [SerializeField()]
        private ButtonBlack _ConnectButtonButtonBlack;
        public ButtonBlack ConnectButtonButtonBlack
        {
            get
            {
                if ((this._ConnectButtonButtonBlack == null))
                {
                    this._ConnectButtonButtonBlack = this.transform.Find("IP_Field").Find("ConnectButton").GetComponent<ButtonBlack>();
                }
                return this._ConnectButtonButtonBlack;
            }
        }
        [SerializeField()]
        private ButtonBlack _CreateRoomButtonButtonBlack;
        public ButtonBlack CreateRoomButtonButtonBlack
        {
            get
            {
                if ((this._CreateRoomButtonButtonBlack == null))
                {
                    this._CreateRoomButtonButtonBlack = this.transform.Find("CreateRoomButton").GetComponent<ButtonBlack>();
                }
                return this._CreateRoomButtonButtonBlack;
            }
        }
        [SerializeField()]
        private Button _FlushRoomButton;
        public Button FlushRoomButton
        {
            get
            {
                if ((this._FlushRoomButton == null))
                {
                    this._FlushRoomButton = this.transform.Find("FlushRoomButton").GetComponent<Button>();
                }
                return this._FlushRoomButton;
            }
        }
        [SerializeField()]
        private Image _RoomListPanelImage;
        public Image RoomListPanelImage
        {
            get
            {
                if ((this._RoomListPanelImage == null))
                {
                    this._RoomListPanelImage = this.transform.Find("RoomListPanel").GetComponent<Image>();
                }
                return this._RoomListPanelImage;
            }
        }
        [SerializeField()]
        private RoomScrollView _RoomScrollView;
        public RoomScrollView RoomScrollView
        {
            get
            {
                if ((this._RoomScrollView == null))
                {
                    this._RoomScrollView = this.transform.Find("RoomListPanel").Find("RoomScrollView").GetComponent<RoomScrollView>();
                }
                return this._RoomScrollView;
            }
        }
        [SerializeField()]
        private Image _RoomInfoPanelImage;
        public Image RoomInfoPanelImage
        {
            get
            {
                if ((this._RoomInfoPanelImage == null))
                {
                    this._RoomInfoPanelImage = this.transform.Find("RoomInfoPanel").GetComponent<Image>();
                }
                return this._RoomInfoPanelImage;
            }
        }
        [SerializeField()]
        private VerticalLayoutGroup _ContainerVerticalLayoutGroup;
        public VerticalLayoutGroup ContainerVerticalLayoutGroup
        {
            get
            {
                if ((this._ContainerVerticalLayoutGroup == null))
                {
                    this._ContainerVerticalLayoutGroup = this.transform.Find("RoomInfoPanel").Find("Container").GetComponent<VerticalLayoutGroup>();
                }
                return this._ContainerVerticalLayoutGroup;
            }
        }
        [SerializeField()]
        private Text _NameLabelText;
        public Text NameLabelText
        {
            get
            {
                if ((this._NameLabelText == null))
                {
                    this._NameLabelText = this.transform.Find("RoomInfoPanel").Find("Container").Find("NameLabel").GetComponent<Text>();
                }
                return this._NameLabelText;
            }
        }
        [SerializeField()]
        private Text _NameText;
        public Text NameText
        {
            get
            {
                if ((this._NameText == null))
                {
                    this._NameText = this.transform.Find("RoomInfoPanel").Find("Container").Find("NameText").GetComponent<Text>();
                }
                return this._NameText;
            }
        }
        [SerializeField()]
        private Text _IPLabelText;
        public Text IPLabelText
        {
            get
            {
                if ((this._IPLabelText == null))
                {
                    this._IPLabelText = this.transform.Find("RoomInfoPanel").Find("Container").Find("IPLabel").GetComponent<Text>();
                }
                return this._IPLabelText;
            }
        }
        [SerializeField()]
        private Text _IPText;
        public Text IPText
        {
            get
            {
                if ((this._IPText == null))
                {
                    this._IPText = this.transform.Find("RoomInfoPanel").Find("Container").Find("IPText").GetComponent<Text>();
                }
                return this._IPText;
            }
        }
        [SerializeField()]
        private Text _DescLabelText;
        public Text DescLabelText
        {
            get
            {
                if ((this._DescLabelText == null))
                {
                    this._DescLabelText = this.transform.Find("RoomInfoPanel").Find("Container").Find("DescLabel").GetComponent<Text>();
                }
                return this._DescLabelText;
            }
        }
        [SerializeField()]
        private Text _DescText;
        public Text DescText
        {
            get
            {
                if ((this._DescText == null))
                {
                    this._DescText = this.transform.Find("RoomInfoPanel").Find("Container").Find("DescText").GetComponent<Text>();
                }
                return this._DescText;
            }
        }
        [SerializeField()]
        private ButtonBlack _ReturnBtnButtonBlack;
        public ButtonBlack ReturnBtnButtonBlack
        {
            get
            {
                if ((this._ReturnBtnButtonBlack == null))
                {
                    this._ReturnBtnButtonBlack = this.transform.Find("ReturnBtn").GetComponent<ButtonBlack>();
                }
                return this._ReturnBtnButtonBlack;
            }
        }
        [SerializeField()]
        private Image _LoadingPanelImage;
        public Image LoadingPanelImage
        {
            get
            {
                if ((this._LoadingPanelImage == null))
                {
                    this._LoadingPanelImage = this.transform.Find("LoadingPanel").GetComponent<Image>();
                }
                return this._LoadingPanelImage;
            }
        }
        [SerializeField()]
        private Image _LoadingImage;
        public Image LoadingImage
        {
            get
            {
                if ((this._LoadingImage == null))
                {
                    this._LoadingImage = this.transform.Find("LoadingPanel").Find("LoadingImage").GetComponent<Image>();
                }
                return this._LoadingImage;
            }
        }
        partial void onAwake();
    }
}
