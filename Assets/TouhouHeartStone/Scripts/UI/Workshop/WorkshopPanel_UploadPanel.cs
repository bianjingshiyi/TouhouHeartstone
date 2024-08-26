using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public partial class WorkshopPanel
    {
        #region 公有方法

        #region 选择上传卡池面板
        public void setSelectUploadPoolPanelVisible(bool visible)
        {
            _selectUploadPoolObj.SetActive(visible);
        }
        public void setSelectingUploadPools(DeckListItemViewData[] viewData)
        {
            _selectUploadPoolView.update(viewData);
        }
        #endregion

        #region 上传卡池面板
        public void setUploadingPool(DeckListItemViewData viewData, int cardCount, int version, string dependencies)
        {
            _uploadPoolHeader.updateView(viewData);
            _uploadPoolCardCount.text = cardCount.ToString();
            _uploadPoolVersion.text = version.ToString();
            _uploadPoolDescInputField.text = string.Empty;
            _uploadPoolDependencyText.text = dependencies;
            _uploadPoolRuleAgreeToggle.isOn = false;
        }
        public void setUploadPoolPanelVisible(bool value)
        {
            _uploadPoolObj.SetActive(value);
        }
        public void showWorkshopRule(string content)
        {
            _ruleEulaPanel.Show(content);
        }
        #endregion

        #endregion

        #region 私有方法
        private void Awake_UploadPanel()
        {
            _selectUploadPoolView.onClickItemButton += i => onUploadPanelCardPoolItemClick?.Invoke(i);
            _selectUploadPoolCloseButton.onClick.AddListener(() => onUploadPanelSelectCardPoolCloseButtonClick?.Invoke());

            _uploadConfirmButton.onClick.AddListener(() => onUploadPanelConfirmButtonClick?.Invoke(_uploadPoolDescInputField.text, _uploadPoolRuleAgreeToggle.isOn));
            _uploadCloseButton.onClick.AddListener(() => onUploadPanelCloseButtonClick?.Invoke());
            _uploadWorkshopRuleViewButton.onClick.AddListener(() => onClickWorkshopRuleViewButton?.Invoke());
        }
        #endregion

        #region 事件
        public event Action<int> onUploadPanelCardPoolItemClick;
        public event Action onUploadPanelSelectCardPoolCloseButtonClick;
        public event Action<string, bool> onUploadPanelConfirmButtonClick;
        public event Action onUploadPanelCloseButtonClick;

        public event Action onClickWorkshopRuleViewButton;
        #endregion

        #region 属性字段
        [Header("Select Upload Pool")]
        [SerializeField]
        private GameObject _selectUploadPoolObj;
        [SerializeField]
        private DeckViewPanel _selectUploadPoolView;
        [SerializeField]
        private Button _selectUploadPoolCloseButton;

        [Header("Upload Pool")]
        [SerializeField]
        private GameObject _uploadPoolObj;
        [SerializeField]
        private DeckListItem _uploadPoolHeader;
        [SerializeField]
        private TextMeshProUGUI _uploadPoolCardCount;
        [SerializeField]
        private TextMeshProUGUI _uploadPoolVersion;
        [SerializeField]
        private TextMeshProUGUI _uploadPoolDependencyText;
        [SerializeField]
        private TMP_InputField _uploadPoolDescInputField;
        [SerializeField]
        private Toggle _uploadPoolRuleAgreeToggle;
        [SerializeField]
        private Button _uploadWorkshopRuleViewButton;
        [SerializeField]
        private Button _uploadConfirmButton;
        [SerializeField]
        private Button _uploadCloseButton;
        #endregion

    }
}