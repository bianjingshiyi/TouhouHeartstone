using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.BattleTable
{
    public class MulliganDialog : MonoBehaviour
    {
        public void setActive(bool active)
        {
            gameObject.SetActive(active);
        }
        public void setAcceptButtonText(string text)
        {
            _acceptButtonText.text = text;
        }
        public void setWaiting(bool waiting)
        {
            _waitingText.gameObject.SetActive(waiting);
            _acceptButton.gameObject.SetActive(!waiting);
        }
        public void setTitleText(string title)
        {
            _title.text = title;
        }
        public MulliganCardItem getItem(int index)
        {
            return _choices[index];
        }
        public MulliganCardItem addChoice()
        {
            var choice = Instantiate(_itemTemplate, _itemRoot).GetComponent<MulliganCardItem>();
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
            _acceptButton.onClick.AddListener(onAcceptButtonClickCallback);
        }
        #endregion
        private void onItemClickCallback(MulliganCardItem card, PointerEventData pointer)
        {
            onItemClicked?.Invoke(_choices.IndexOf(card));
        }
        private void onAcceptButtonClickCallback()
        {
            onAccept?.Invoke();
        }
        #endregion

        #region 事件
        public Action<int> onItemClicked;
        public Action onAccept;
        #endregion

        #region 属性字段
        public bool isShowing => gameObject.activeSelf;
        [SerializeField]
        private GameObject _itemTemplate;
        [SerializeField]
        private Transform _itemRoot;
        [SerializeField]
        private List<MulliganCardItem> _choices = new List<MulliganCardItem>();
        [SerializeField]
        private Text _title;
        [SerializeField]
        private Text _waitingText;
        [SerializeField]
        private Button _acceptButton;
        [SerializeField]
        private Text _acceptButtonText;
        #endregion
    }
}
