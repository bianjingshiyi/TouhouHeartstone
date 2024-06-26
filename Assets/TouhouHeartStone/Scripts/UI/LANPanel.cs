using System;
using BJSYGameCore.UI;
using ExcelDataReader.Log;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public partial class LANPanel : UIObject
    {
        #region 公有方法
        public void clearRooms()
        {
            _roomScrollView.RoomList.clearItems();
        }
        public RoomListItem addRoomItem()
        {
            return _roomScrollView.RoomList.addItem();
        }
        public void setRoomInfos(string name, string ip, string desc)
        {
            _nameText.text = name;
            _IPText.text = ip;
            _descText.text = desc;
        }
        public void ShowPasswordDialog()
        {
            _passwordDialog.SetActive(true);
        }
        public void HidePasswordDialog()
        {
            _passwordDialog.SetActive(false);
            _errorMessageObj.SetActive(false);
            _passwordInputField.text = string.Empty;
        }
        public void ShowErrorMessage(string message)
        {
            _errorMessageObj.SetActive(true);
            _errorMessageText.text = message;
        }
        public void setLoadingImageVisible(bool on)
        {
            _loadingPanelImage.gameObject.SetActive(on);
        }
        public void setCreateRoomButtonInteractable(bool interactable)
        {
            _createRoomButton.interactable = interactable;
        }
        public void setIPFieldVisible(bool visible)
        {
            _IPFieldImage.gameObject.SetActive(visible);
        }
        public void setRoomRuleDropdownListVisible(bool visible)
        {
            _createRoomDropdownListObj.SetActive(visible);
        }
        public void switchRoomRuleDropdownListVisible()
        {
            setRoomRuleDropdownListVisible(!_createRoomDropdownListObj.activeSelf);
        }
        #endregion
        #region 私有方法

        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            _confirmPasswordButton.onClick.AddListener(onConfirmPasswordClick);
            _cancelPasswordButton.onClick.AddListener(onCancelPasswordClick);
            _refreshRoomButton.onClick.AddListener(onRefreshRoomButtonClickedCallback);
            _createRoomButton.onClick.AddListener(onCreateRoomClickedCallback);
            _returnButton.onClick.AddListener(onReturnButtonClickedCallback);
            _connectButton.onClick.AddListener(onConnectButtonClickedCallback);

            for (int i = 0; i < _createRoomDropdownListItems.Length; i++)
            {
                var button = _createRoomDropdownListItems[i];
                button.onClick.AddListener(() => onCreateRoomDropdownListItemClicked?.Invoke(Array.IndexOf(_createRoomDropdownListItems, button)));
            }
        }
        #endregion

        #region 事件回调
        private void onCancelPasswordClick()
        {
            onCancelPassword?.Invoke();
        }
        private void onConfirmPasswordClick()
        {
            onConfirmPassword?.Invoke(_passwordInputField.text);
        }
        private void onRefreshRoomButtonClickedCallback()
        {
            onRefreshRoomList?.Invoke();
        }
        private void onCreateRoomClickedCallback()
        {
            onCreateRoomClicked?.Invoke();
        }
        private void onReturnButtonClickedCallback()
        {
            onReturnButtonClicked?.Invoke();
        }
        private void onConnectButtonClickedCallback()
        {
            onConnectButtonClicked?.Invoke(_IPInputField.text);
        }
        #endregion




        #endregion
        #region 事件
        public event Action<string> onConfirmPassword;
        public event Action onCancelPassword;
        public event Action onRefreshRoomList;
        public event Action onCreateRoomClicked;
        public event Action onReturnButtonClicked;
        public event Action<string> onConnectButtonClicked;
        public event Action<int> onCreateRoomDropdownListItemClicked;
        #endregion
        #region 私有字段
        [SerializeField]
        private Image _IPFieldImage;
        [SerializeField]
        private InputField _IPInputField;
        [SerializeField]
        private Button _connectButton;
        [SerializeField]
        private Button _createRoomButton;
        [SerializeField]
        private GameObject _createRoomDropdownListObj;
        [SerializeField]
        private Button[] _createRoomDropdownListItems;

        [SerializeField]
        private Button _refreshRoomButton;
        [SerializeField]
        private RoomScrollView _roomScrollView;
        [SerializeField]
        private Text _nameText;
        [SerializeField]
        private Text _IPText;
        [SerializeField]
        private Text _descText;
        [SerializeField]
        private Button _returnButton;
        [SerializeField]
        private Image _loadingPanelImage;

        [Header("密码")]
        [SerializeField]
        private GameObject _passwordDialog;
        [SerializeField]
        private InputField _passwordInputField;
        [SerializeField]
        private GameObject _errorMessageObj;
        [SerializeField]
        private Text _errorMessageText;
        [SerializeField]
        private Button _confirmPasswordButton;
        [SerializeField]
        private Button _cancelPasswordButton;
        #endregion
    }
}
