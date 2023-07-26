using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.BattleTable
{
    public class DiscoverDialog : MonoBehaviour
    {
        public void setActive(bool active)
        {
            gameObject.SetActive(active);
            isShowing = true;
        }
        public void setHideButtonText(string text)
        {
            _hideButtonText.text = text;
        }
        public void setTitleText(string title)
        {
            _title.text = title;
        }
        public DiscoverItem addChoice()
        {
            var choice = Instantiate(_choiceTemplate, _choiceRoot).GetComponent<DiscoverItem>();
            _choices.Add(choice);
            choice.onClick += onItemClickCallback;
            return choice;
        }
        public void clearChoices()
        {
            foreach (var choice in _choices)
            {
                choice.onClick -= onItemClickCallback;
                choice.destroy();
            }
            _choices.Clear();
        }
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _hideButton.onClick.AddListener(onHideButtonClickCallback);
        }
        #endregion
        private void onItemClickCallback(DiscoverItem card, PointerEventData pointer)
        {
            onItemClicked?.Invoke(_choices.IndexOf(card));
        }
        private void onHideButtonClickCallback()
        {
            onHideButtonClicked?.Invoke();
        }
        #endregion

        #region 事件
        public Action<int> onItemClicked;
        public Action onHideButtonClicked;
        #endregion

        #region 属性字段
        public bool isShowing
        {
            get => _isShowing;
            set
            {
                _isShowing = value;
                _panelRoot.SetActive(value);
            }
        }
        private bool _isShowing;
        [SerializeField]
        private GameObject _choiceTemplate;
        [SerializeField]
        private Transform _choiceRoot;
        [SerializeField]
        private GameObject _panelRoot;
        [SerializeField]
        private List<DiscoverItem> _choices = new List<DiscoverItem>();
        [SerializeField]
        private Text _title;
        [SerializeField]
        private Button _hideButton;
        [SerializeField]
        private Text _hideButtonText;
        #endregion
    }
}
