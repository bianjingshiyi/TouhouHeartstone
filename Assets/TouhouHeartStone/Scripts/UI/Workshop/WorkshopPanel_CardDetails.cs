using System;
using Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public partial class WorkshopPanel
    {
        #region 公有方法

        #region 卡池详细信息
        public void setCardDetailsPage(CardDetailPage page)
        {
            _cardDetailsCommentsPage.SetActive(page == CardDetailPage.Comments);
            _cardDetailsCardPage.SetActive(page == CardDetailPage.Card);
        }
        public void setCardDetails(WorkshopCardDetailsViewData details)
        {
            _cardDetailsPreview.updateCard(details.card);
            _cardDetailsPreview.setImage(details.cardImage);
            _cardDetailsCreatorText.text = details.creator;
            _cardDetailsCardPoolText.text = details.cardPoolName;
            _cardDetailsCharacterItem.updateView(details.characterName, details.characterIcon);
            _cardDetailsFlavorText.text = details.flavor;
            _cardDetailsSoundList.updateList(details.sounds.Length, (i, rect) =>
            {
                var comp = rect.GetComponent<WorkshopCardDetailsSoundItem>();
                comp.setText(details.sounds[i]);
            },
            rect =>
            {
                var comp = rect.GetComponent<WorkshopCardDetailsSoundItem>();
                comp.onButtonClick += onCardDetailsSoundPlayClickedCallback;
            },
            rect =>
            {
                var comp = rect.GetComponent<WorkshopCardDetailsSoundItem>();
                comp.onButtonClick -= onCardDetailsSoundPlayClickedCallback;
            });
        }
        #endregion

        #endregion

        #region 私有方法
        private void Awake_CardDetails()
        {
            _cardDetailsCardTabButton.onClick.AddListener(() => onCardDetailsTabClick?.Invoke(CardDetailPage.Card));
            _cardDetailsCommentsTabButton.onClick.AddListener(() => onCardDetailsTabClick?.Invoke(CardDetailPage.Comments));
            _cardDetailsCommentButton.onClick.AddListener(() => onCardDetailsCommentButtonClick?.Invoke());
        }
        private void onCardDetailsSoundPlayClickedCallback(WorkshopCardDetailsSoundItem item)
        {
            onCardDetailsSoundPlayClicked?.Invoke(_cardDetailsSoundList.indexOf(item));
        }

        #endregion

        #region 事件
        public event Action<int> onCardDetailsSoundPlayClicked;
        public event Action<CardDetailPage> onCardDetailsTabClick;
        public event Action onCardDetailsCommentButtonClick;
        #endregion

        #region 属性字段


        [Header("Card Details")]
        [SerializeField]
        private Card _cardDetailsPreview;
        // 卡池信息
        [SerializeField]
        private TextMeshProUGUI _cardDetailsCreatorText;
        [SerializeField]
        private TextMeshProUGUI _cardDetailsCardPoolText;
        [SerializeField]
        private CardViewDeckCardListItem _cardDetailsCharacterItem;
        [SerializeField]
        private TextMeshProUGUI _cardDetailsFlavorText;
        [SerializeField]
        private ElementList _cardDetailsSoundList;

        // 选项卡
        [SerializeField]
        private Button _cardDetailsCardTabButton;
        [SerializeField]
        private Button _cardDetailsCommentsTabButton;
        [SerializeField]
        private Button _cardDetailsCommentButton;

        // 页面
        [SerializeField]
        private GameObject _cardDetailsCardPage;
        [SerializeField]
        private GameObject _cardDetailsCommentsPage;
        #endregion


        #region 内嵌类
        public enum CardDetailPage
        {
            Card,
            Comments
        }
        #endregion
    }
    public struct WorkshopCardDetailsViewData
    {
        public UICardViewData card;
        public Sprite cardImage;
        public string creator;
        public string cardPoolName;
        public string characterName;
        public Sprite characterIcon;
        public string flavor;
        public string[] sounds;
    }
}