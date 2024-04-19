using System;
using System.Linq;
using BJSYGameCore.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class WorkshopPanel : UIObject
    {
        #region 公有方法
        public void setSortItems(string[] sortNames)
        {
            _sortBy.ClearOptions();
            _sortBy.AddOptions(sortNames.ToList());
        }
        public void hideDetails()
        {
            _detailsNothingPage.SetActive(true);
            _detailsPreviewPage.SetActive(false);
        }
        public void showDetails(DeckListItemViewData viewData, string creator, string time, string cardCount, string description)
        {
            _detailsNothingPage.SetActive(false);
            _detailsPreviewPage.SetActive(true);
            _detailsPoolItem.updateView(viewData);
            _detailsCreatorText.text = creator;
            _detailsCreateTimeText.text = time;
            _detailsCardCountText.text = cardCount;
            _detailsDescriptionText.text = description;
        }
        public void setDownloadProgress(float progress)
        {
            var parent = _downloadProgressBar.parent as RectTransform;
            var sizeDelta = _downloadProgressBar.sizeDelta;
            sizeDelta.x = Mathf.Lerp(0, parent.rect.size.x, progress);
            _downloadProgressBar.sizeDelta = sizeDelta;
        }
        public void setDownloadButtonText(string text)
        {
            _downloadButtonText.text = text;
        }
        public void updateCardPools(DeckListItemViewData[] viewData)
        {
            _cardPoolItemList.updateList(viewData.Length, (index, rect) =>
            {
                var deckListItem = rect.GetComponent<DeckListItem>();
                deckListItem.updateView(viewData[index]);
            },
            rect =>
            {
                var deckListItem = rect.GetComponent<DeckListItem>();
                deckListItem.onClick += onCardPoolItemClickCallback;
            },
            rect =>
            {
                var deckListItem = rect.GetComponent<DeckListItem>();
                deckListItem.onClick -= onCardPoolItemClickCallback;
            });
        }
        public void setPageText(string text)
        {
            _pageText.text = text;
        }
        public void setPageButtonInteractable(bool prev, bool next)
        {
            _prevPageButton.interactable = prev;
            _nextPageButton.interactable = next;
        }

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
        public void setUploadingPool(DeckListItemViewData viewData, int cardCount, int version)
        {
            _uploadPoolHeader.updateView(viewData);
            _uploadPoolCardCount.text = cardCount.ToString();
            _uploadPoolVersion.text = version.ToString();
            _uploadPoolDescInputField.text = string.Empty;
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
        protected override void Awake()
        {
            base.Awake();
            _returnButton.onClick.AddListener(() => onReturnClick?.Invoke());

            _sortBy.onValueChanged.AddListener(sort => onSortChanged?.Invoke(sort));
            _onlyMine.onValueChanged.AddListener(value => onOnlyMineChanged?.Invoke(value));
            _creatorFilter.onEndEdit.AddListener(value => onCreatorFilterChanged?.Invoke(value));
            _searchFilter.onEndEdit.AddListener(value => onSearchFilterChanged?.Invoke(value));

            _prevPageButton.onClick.AddListener(() => onPageButtonClicked?.Invoke(false));
            _nextPageButton.onClick.AddListener(() => onPageButtonClicked?.Invoke(true));

            _downloadButton.onClick.AddListener(() => onDownloadClick?.Invoke());
            _uploadButton.onClick.AddListener(() => onUploadClick?.Invoke());

            _selectUploadPoolView.onClickItemButton += i => onClickSelectUploadPoolItem?.Invoke(i);
            _selectUploadPoolCloseButton.onClick.AddListener(() => onClickSelectUploadPoolCloseButton?.Invoke());

            _uploadWorkshopRuleViewButton.onClick.AddListener(() => onClickWorkshopRuleViewButton?.Invoke());
            _uploadConfirmButton.onClick.AddListener(() => onClickUploadConfirmButton?.Invoke());
            _uploadCloseButton.onClick.AddListener(() => onClickUploadCloseButton?.Invoke()); 
        }
        private void onCardPoolItemClickCallback(DeckListItem item)
        {
            onCardPoolItemClick?.Invoke(_cardPoolItemList.indexOf(item));
        }
        #endregion

        #region 事件
        public event Action onReturnClick;

        public event Action<int> onSortChanged;
        public event Action<bool> onOnlyMineChanged;
        public event Action<string> onCreatorFilterChanged;
        public event Action<string> onSearchFilterChanged;

        public event Action<int> onCardPoolItemClick;

        public event Action<bool> onPageButtonClicked;

        public event Action onDownloadClick;
        public event Action onUploadClick;

        public event Action<int> onClickSelectUploadPoolItem;
        public event Action onClickSelectUploadPoolCloseButton;

        public event Action onClickWorkshopRuleViewButton;
        public event Action onClickUploadConfirmButton;
        public event Action onClickUploadCloseButton;
        #endregion

        #region 属性字段
        [SerializeField]
        private Button _returnButton;

        [Header("Filters")]
        [SerializeField]
        private Dropdown _sortBy;
        [SerializeField]
        private Toggle _onlyMine;
        [SerializeField]
        private InputField _creatorFilter;
        [SerializeField]
        private InputField _searchFilter;

        [Header("CardPools")]
        [SerializeField]
        private ElementList _cardPoolItemList;

        [Header("Pages")]
        [SerializeField]
        private TextMeshProUGUI _pageText;
        [SerializeField]
        private Button _prevPageButton;
        [SerializeField]
        private Button _nextPageButton;


        [Header("Details")]
        [SerializeField]
        private GameObject _detailsNothingPage;
        [SerializeField]
        private GameObject _detailsPreviewPage;
        [SerializeField]
        private DeckListItem _detailsPoolItem;
        [SerializeField]
        private TextMeshProUGUI _detailsCreatorText;
        [SerializeField]
        private TextMeshProUGUI _detailsCreateTimeText;
        [SerializeField]
        private TextMeshProUGUI _detailsCardCountText;
        [SerializeField]
        private TextMeshProUGUI _detailsDescriptionText;

        [Header("Tasks")]
        [SerializeField]
        private Text _downloadButtonText;
        [SerializeField]
        private RectTransform _downloadProgressBar;
        [SerializeField]
        private Button _downloadButton;
        [SerializeField]
        private Button _uploadButton;

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
        private TMP_InputField _uploadPoolDescInputField;
        [SerializeField]
        private Toggle _uploadPoolRuleAgreeToggle;
        [SerializeField]
        private Button _uploadWorkshopRuleViewButton;
        [SerializeField]
        private Button _uploadConfirmButton;
        [SerializeField]
        private Button _uploadCloseButton;

        [Header("Rule Panel")]
        [SerializeField]
        private GameObject _rulePanelObj;
        [SerializeField]
        private EULAPanel _ruleEulaPanel;
        #endregion
    }
}