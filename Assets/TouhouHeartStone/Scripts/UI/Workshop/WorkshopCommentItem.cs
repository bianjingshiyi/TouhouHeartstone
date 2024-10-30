using TMPro;
using UnityEngine;

namespace UI
{
    public class WorkshopCommentItem : MonoBehaviour
    {
        public void updateComment(WorkshopCommentViewData viewData)
        {
            usernameText.text = viewData.userName;
            comment.text = viewData.comment;
            positiveRating.SetActive(viewData.rating);
            negativeRating.SetActive(!viewData.rating);
        }
        [SerializeField]
        private TextMeshProUGUI usernameText;
        [SerializeField]
        private TextMeshProUGUI comment;
        [SerializeField]
        private GameObject positiveRating;
        [SerializeField]
        private GameObject negativeRating;
    }

    public struct WorkshopCommentViewData
    {
        public string userName;
        public string comment;
        public bool rating;
    }
}
