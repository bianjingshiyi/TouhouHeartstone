using System;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public partial class WorkshopPanel : UIObject
    {
        #region 公有方法

        public void setWorkshopPage(WorkshopPage page)
        {
            _workshopBrowsePage.SetActive(page == WorkshopPage.Browse);
            _workshopCardPoolDetailsPage.SetActive(page == WorkshopPage.CardPoolDetails);
            _workshopCardDetailsPage.SetActive(page == WorkshopPage.CardDetails);
            _workshopLoadingPage.SetActive(page == WorkshopPage.Loading);
        }
        public void switchUploadOrDownloadButton(bool download)
        {
            _uploadButton.gameObject.SetActive(!download);
            _downloadButton.gameObject.SetActive(download);
        }
        public void showHintMessage(string text)
        {
            _hintMessage.show(text);
        }
        public void hideHintMessage()
        {
            _hintMessage.hide();
        }

        #endregion

        #region 私有方法
        protected override void Awake()
        {
            base.Awake();
            _returnButton.onClick.AddListener(() => onReturnClick?.Invoke());
            Awake_CardPoolList();
            Awake_CardPoolDetails();
            Awake_UploadPanel();
        }
        #endregion

        #region 事件
        public event Action onReturnClick;
        #endregion

        #region 属性字段
        [SerializeField]
        private Button _returnButton;
        [SerializeField]
        private GameObject _workshopBrowsePage;
        [SerializeField]
        private GameObject _workshopCardPoolDetailsPage;
        [SerializeField]
        private GameObject _workshopCardDetailsPage;
        [SerializeField]
        private GameObject _workshopLoadingPage;
        [SerializeField]
        private HintMessage _hintMessage;

        [Header("Rule Panel")]
        [SerializeField]
        private GameObject _rulePanelObj;
        [SerializeField]
        private EULAPanel _ruleEulaPanel;
        #endregion

        #region 内嵌类
        public enum WorkshopPage
        {
            Browse,
            CardPoolDetails,
            CardDetails,
            Loading,
        }
        #endregion
    }
}