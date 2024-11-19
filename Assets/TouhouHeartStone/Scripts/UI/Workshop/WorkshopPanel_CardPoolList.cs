using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public partial class WorkshopPanel
    {
        #region 公有方法
        public void setViewingUser(bool viewingUser)
        {
            _viewingUserTopBar.SetActive(viewingUser);
            _homeTopBar.SetActive(!viewingUser);
            _viewMineButton.gameObject.SetActive(!viewingUser);
            _homeButton.gameObject.SetActive(viewingUser);
        }
        public void setViewingUserLabel(string label)
        {
            _viewingUserLabelText.text = label;
        }
        public void setBrowseComponentsInteractable(bool interactable)
        {
            _viewMineButton.interactable = interactable;
            _searchFilter.interactable = interactable;
            _sortBy.interactable = interactable;
        }

        #region 筛选
        public void setSortItems(string[] sortNames)
        {
            _sortBy.ClearOptions();
            _sortBy.AddOptions(sortNames.ToList());
        }
        #endregion

        #region 卡池列表
        public void updateHotCardPools(WorkshopCardPoolItemViewData[] viewData)
        {
            _browseCardPoolsPage.SetActive(false);
            _hotCardPoolsPage.SetActive(true);
            for (int i = 0; i < _hotWorkshopCardPoolItems.Length; i++)
            {
                var item = _hotWorkshopCardPoolItems[i];
                if (i >= viewData.Length)
                {
                    item.gameObject.SetActive(false);
                    continue;
                }
                item.gameObject.SetActive(true);
                item.updateView(viewData[i]);
            }
            
        }
        public void updateBrowseCardPools(WorkshopCardPoolItemViewData[] viewData)
        {
            _browseCardPoolsPage.SetActive(true);
            _hotCardPoolsPage.SetActive(false);
            _browseCardPoolItemList.updateList(viewData.Length, (index, rect) =>
            {
                var deckListItem = rect.GetComponent<WorkshopCardPoolItem>();
                deckListItem.updateView(viewData[index]);
            },
            rect =>
            {
                var deckListItem = rect.GetComponent<WorkshopCardPoolItem>();
                deckListItem.onClick += onBrowseCardPoolItemClickCallback;
            },
            rect =>
            {
                var deckListItem = rect.GetComponent<WorkshopCardPoolItem>();
                deckListItem.onClick -= onBrowseCardPoolItemClickCallback;
            });
        }
        public void setLoadingState(LoadingState state)
        {
            _browseContentObj.SetActive(state == LoadingState.Normal);
            _loadingObj.SetActive(state == LoadingState.Loading);
            _warningObj.SetActive(state == LoadingState.Warning);
        }
        #endregion

        #region 页码
        public void setPageText(string text)
        {
            _pageText.text = text;
        }
        public void setPageButtonInteractable(bool prev, bool next)
        {
            _prevPageButton.interactable = prev;
            _nextPageButton.interactable = next;
        }
        #endregion

        #endregion

        #region 私有方法
        private void Awake_CardPoolList()
        {
            _sortBy.onValueChanged.AddListener(sort => onSortChanged?.Invoke(sort));
            _searchFilter.onEndEdit.AddListener(value => onSearchFilterChanged?.Invoke(value));
            _refreshButton.onClick.AddListener(() => onRefreshClicked?.Invoke());
            _prevPageButton.onClick.AddListener(() => onPageButtonClicked?.Invoke(false));
            _nextPageButton.onClick.AddListener(() => onPageButtonClicked?.Invoke(true));

            _viewMineButton.onClick.AddListener(() => onViewMineClicked?.Invoke());
            _homeButton.onClick.AddListener(() => onHomeClicked?.Invoke());
            _uploadButton.onClick.AddListener(() => onUploadClick?.Invoke());
        }
        private void onHotCardPoolItemClickCallback(WorkshopCardPoolItem item)
        {
            onHotCardPoolItemClick?.Invoke(Array.IndexOf(_hotWorkshopCardPoolItems, item));
        }
        private void onBrowseCardPoolItemClickCallback(WorkshopCardPoolItem item)
        {
            onBrowseCardPoolItemClick?.Invoke(_browseCardPoolItemList.indexOf(item));
        }
        #endregion

        #region 事件
        public event Action<int> onSortChanged;
        public event Action<string> onSearchFilterChanged;
        public event Action onRefreshClicked;
        public event Action onViewMineClicked;
        public event Action onHomeClicked;
        public event Action<int> onHotCardPoolItemClick;
        public event Action<int> onBrowseCardPoolItemClick;
        public event Action<bool> onPageButtonClicked;
        public event Action onUploadClick;
        #endregion

        #region 属性字段
        [Header("Filters")]
        [SerializeField]
        private GameObject _homeTopBar;
        [SerializeField]
        private Dropdown _sortBy;
        [SerializeField]
        private InputField _searchFilter;
        [SerializeField]
        private Button _viewMineButton;
        [SerializeField]
        private Button _homeButton;
        [SerializeField]
        private Button _uploadButton;

        [Header("Viewing User")]
        [SerializeField]
        private GameObject _viewingUserTopBar;
        [SerializeField]
        private TextMeshProUGUI _viewingUserLabelText;


        [Header("Hot CardPools")]
        [SerializeField]
        private GameObject _hotCardPoolsPage;
        [SerializeField]
        private WorkshopCardPoolItem[] _hotWorkshopCardPoolItems;

        [Header("Browse CardPools")]
        [SerializeField]
        private GameObject _browseCardPoolsPage;
        [SerializeField]
        private ElementList _browseCardPoolItemList;
        [SerializeField]
        private GameObject _browseContentObj;
        [SerializeField]
        private GameObject _loadingObj;
        [SerializeField]
        private GameObject _warningObj;
        [SerializeField]
        private Button _refreshButton;
        [SerializeField]
        private Button _prevPageButton;
        [SerializeField]
        private Button _nextPageButton;
        [SerializeField]
        private TextMeshProUGUI _pageText;
        #endregion

        #region 内嵌类
        public enum LoadingState
        {
            Normal,
            Loading,
            Warning
        }
        #endregion

    }
}