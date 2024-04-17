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
        public int indexOf(MulliganCardItem item)
        {
            return _choices.IndexOf(item);
        }
        public MulliganCardItem addChoice()
        {
            var choice = Instantiate(_itemTemplate, _itemRoot).GetComponent<MulliganCardItem>();
            choice.gameObject.SetActive(true);
            _choices.Add(choice);
            choice.onPointerEnter += onItemPointerEnterCallback;
            choice.onPointerExit += onItemPointerExitCallback;
            choice.onClick += onItemClickCallback;
            return choice;
        }
        public void clearChoices()
        {
            foreach (var choice in _choices)
            {
                choice.onPointerEnter -= onItemPointerEnterCallback;
                choice.onPointerExit -= onItemPointerExitCallback;
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
            _itemTemplate.SetActive(false);
        }
        #endregion
        private void onItemPointerEnterCallback(MulliganCardItem card, PointerEventData pointer)
        {
            onItemPointerEnter?.Invoke(_choices.IndexOf(card));
        }
        private void onItemPointerExitCallback(MulliganCardItem card, PointerEventData pointer)
        {
            onItemPointerExit?.Invoke(_choices.IndexOf(card));
        }
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
        public Action<int> onItemPointerEnter;
        public Action<int> onItemPointerExit;
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
