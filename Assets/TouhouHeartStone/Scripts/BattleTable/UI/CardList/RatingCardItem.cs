using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.BattleTable
{
    public class RatingCardItem : CardUIItem
    {
        public void setRating(bool upvote)
        {
            _upvoteImage.color = upvote ? _upvoteColor : _darkColor;
            _downvoteImage.color = upvote ? _darkColor : _downvoteColor;
        }
        public void cancelRating()
        {
            _upvoteImage.color = _normalColor;
            _downvoteImage.color = _normalColor;
        }
        protected override void Awake()
        {
            base.Awake();
            _upvoteButton.onClick.AddListener(() => onVoteClick?.Invoke(this, true));
            _downvoteButton.onClick.AddListener(() => onVoteClick?.Invoke(this, false));
        }
        
        public event Action<RatingCardItem, bool> onVoteClick;
        [SerializeField]
        private Button _upvoteButton;
        [SerializeField]
        private Button _downvoteButton;
        [SerializeField]
        private Image _upvoteImage;
        [SerializeField]
        private Image _downvoteImage;
        [SerializeField]
        private Color _normalColor = Color.white;
        [SerializeField]
        private Color _upvoteColor = Color.green;
        [SerializeField]
        private Color _downvoteColor = Color.red;
        [SerializeField]
        private Color _darkColor = Color.gray;
    }
}