using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class WorkshopCommentList : MonoBehaviour
    {
        public void updateComments(WorkshopCommentViewData[] comments, bool error)
        {
            _commentList.updateList(comments.Length, (i, rect) =>
            {
                var comp = rect.GetComponent<WorkshopCommentItem>();
                comp.updateComment(comments[i]);
            });
            _listPage.SetActive(!error);
            _errorPage.SetActive(error);
        }
        public void setPageText(string page)
        {
            _pageText.text = page;
        }
        public void setPageButtonInteractable(bool prev, bool next)
        {
            _prevPageButton.interactable = prev;
            _nextPageButton.interactable = next;
        }
        private void Awake()
        {
            _prevPageButton.onClick.AddListener(() => onPageButtonClick?.Invoke(false));
            _nextPageButton.onClick.AddListener(() => onPageButtonClick?.Invoke(true));
        }

        public event Action<bool> onPageButtonClick;

        [SerializeField]
        private ElementList _commentList;
        [SerializeField]
        private GameObject _listPage;
        [SerializeField]
        private GameObject _errorPage;
        [SerializeField]
        private TextMeshProUGUI _pageText;
        [SerializeField]
        private Button _prevPageButton;
        [SerializeField]
        private Button _nextPageButton;
    }
}
