using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public partial class WorkshopPanel
    {
        #region 公有方法

        #region 卡池详细信息
        public void setCardPoolDetailsPage(CardPoolDetailPage page)
        {
            _cardPoolDetailsCommentsPage.SetActive(page == CardPoolDetailPage.Comments);
            _cardPoolDetailsCardsPage.SetActive(page == CardPoolDetailPage.Info);
        }
        public void setDetails(DeckListItemViewData viewData, WorkshopCardPoolDetailsViewData details)
        {
            _cardPoolDetailsHeader.updateView(viewData);
            _cardPoolDetailsCreatorText.text = details.creator;
            _cardPoolDetailsCreateTimeText.text = details.creationTime;
            _cardPoolDetailsCardCountText.text = details.cardCount;
            _cardPoolDetailsDescriptionText.text = details.description;
            _cardPoolDetailsVersionText.text = details.version;
            _cardPoolDetailsDependenciesText.text = details.dependencies;
        }
        public void setDetailsState(Color color, string state)
        {
            _cardPoolDetailsStateText.color = color;
            _cardPoolDetailsStateText.text = state;
        }
        public void setDetailsStateVisible(bool visible)
        {
            _cardPoolDetailsStateObj.SetActive(visible);
        }
        public void setReviewsText(string reviewText)
        {
            _cardPoolDetailsReviewText.text = reviewText;
        }
        public void setReviewsVisible(bool visible)
        {
            _cardPoolDetailsReviewObj.SetActive(visible);
        }
        public void setDownloadProgressVisible(bool visible)
        {
            _downloadProgressSlider.gameObject.SetActive(visible);
        }
        public void setDownloadProgress(float progress)
        {
            _downloadProgressSlider.value = progress;
        }
        public void setDownloadButtonInteractable(bool value)
        {
            _downloadButton.interactable = value;
        }
        public void setDownloadButtonText(string text)
        {
            _downloadButtonText.text = text;
        }
        #endregion

        #endregion

        #region 私有方法
        private void Awake_CardPoolDetails()
        {
            _cardPoolDetailsHeader.onClickDeleteButton += item => onCardPoolDetailsDeleteClicked?.Invoke();
            _cardPoolDetailsCreatorButton.onClick.AddListener(() => onCardPoolDetailsCreatorClicked?.Invoke());
            _cardPoolDetailsInfoTabButton.onClick.AddListener(() => onCardPoolDetailsTabClick?.Invoke(CardPoolDetailPage.Info));
            _cardPoolDetailsCommentsTabButton.onClick.AddListener(() => onCardPoolDetailsTabClick?.Invoke(CardPoolDetailPage.Comments));

            _downloadButton.onClick.AddListener(() => onCardPoolDetailsDownloadClick?.Invoke());
        }
        #endregion

        #region 事件
        public event Action onCardPoolDetailsDeleteClicked;
        public event Action onCardPoolDetailsCreatorClicked;
        public event Action onCardPoolDetailsDownloadClick;
        public event Action<CardPoolDetailPage> onCardPoolDetailsTabClick;
        #endregion

        #region 属性字段


        [Header("Tasks")]
        [SerializeField]
        private Text _downloadButtonText;
        [SerializeField]
        private Slider _downloadProgressSlider;
        [SerializeField]
        private Button _downloadButton;

        [Header("Card Pool Details")]
        [SerializeField]
        private DeckListItem _cardPoolDetailsHeader;
        // 卡池信息
        [SerializeField]
        private TextMeshProUGUI _cardPoolDetailsCreatorText;
        [SerializeField]
        private Button _cardPoolDetailsCreatorButton;
        [SerializeField]
        private TextMeshProUGUI _cardPoolDetailsDescriptionText;
        [SerializeField]
        private TextMeshProUGUI _cardPoolDetailsCardCountText;
        [SerializeField]
        private TextMeshProUGUI _cardPoolDetailsCreateTimeText;
        [SerializeField]
        private TextMeshProUGUI _cardPoolDetailsVersionText;
        [SerializeField]
        private TextMeshProUGUI _cardPoolDetailsDependenciesText;

        // 选项卡
        [SerializeField]
        private Button _cardPoolDetailsInfoTabButton;
        [SerializeField]
        private Button _cardPoolDetailsCommentsTabButton;

        // 底部页面
        [SerializeField]
        private GameObject _cardPoolDetailsCommentsPage;
        [SerializeField]
        private GameObject _cardPoolDetailsCardsPage;

        // 卡池当前状态
        [SerializeField]
        private GameObject _cardPoolDetailsStateObj;
        [SerializeField]
        private TextMeshProUGUI _cardPoolDetailsStateText;

        // 审核状态
        [SerializeField]
        private GameObject _cardPoolDetailsReviewObj;
        [SerializeField]
        private TextMeshProUGUI _cardPoolDetailsReviewText;
        #endregion


        #region 内嵌类
        public enum CardPoolDetailPage
        {
            Comments,
            Info
        }
        #endregion

    }
    public struct WorkshopCardPoolDetailsViewData
    {
        public string creator;
        public string creationTime;
        public string cardCount;
        public string description;
        public string version;
        public string dependencies;
    }
}