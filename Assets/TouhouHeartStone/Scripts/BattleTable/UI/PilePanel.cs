using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.BattleTable
{
    public class PilePanel : MonoBehaviour
    {
        public void setActive(bool active)
        {
            gameObject.SetActive(active);
        }
        public void setCloseButtonText(string text)
        {
            _closeButtonText.text = text;
        }
        public void setTitleText(string title)
        {
            _title.text = title;
        }
        public PilePanelItem addCard()
        {
            var choice = Instantiate(_itemTemplate, _itemRoot).GetComponent<PilePanelItem>();
            choice.gameObject.SetActive(true);
            _cards.Add(choice);
            choice.onPointerEnter += onItemPointerEnterCallback;
            choice.onPointerExit += onItemPointerExitCallback;
            return choice;
        }
        public TableCard getCard(int index)
        {
            return _cards[index].card;
        }
        public int indexOf(TableCard card)
        {
            return _cards.FindIndex(i => i.card == card);
        }
        public void clearCards()
        {
            foreach (var choice in _cards)
            {
                choice.onPointerEnter -= onItemPointerEnterCallback;
                choice.onPointerExit -= onItemPointerExitCallback;
                choice.destroy();
            }
            _cards.Clear();
        }
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _closeButton.onClick.AddListener(onCloseButtonClickCallback);
            _itemTemplate.SetActive(false);
        }
        #endregion
        private void onCloseButtonClickCallback()
        {
            onCloseButtonClicked?.Invoke();
        }
        private void onItemPointerEnterCallback(PilePanelItem item, PointerEventData pointerData)
        {
            onItemPointerEnter?.Invoke(_cards.IndexOf(item));
        }
        private void onItemPointerExitCallback(PilePanelItem item, PointerEventData pointerData)
        {
            onItemPointerExit?.Invoke(_cards.IndexOf(item));
        }
        #endregion

        #region 事件
        public Action onCloseButtonClicked;
        public Action<int> onItemPointerEnter;
        public Action<int> onItemPointerExit;
        #endregion

        #region 属性字段
        public bool isShowing => gameObject.activeSelf;
        [SerializeField]
        private GameObject _itemTemplate;
        [SerializeField]
        private Transform _itemRoot;
        [SerializeField]
        private List<PilePanelItem> _cards = new List<PilePanelItem>();
        [SerializeField]
        private Text _title;
        [SerializeField]
        private Button _closeButton;
        [SerializeField]
        private Text _closeButtonText;
        #endregion
    }
}
