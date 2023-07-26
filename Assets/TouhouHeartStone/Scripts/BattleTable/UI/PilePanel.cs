using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
            _cards.Add(choice);
            return choice;
        }
        public void clearCards()
        {
            foreach (var choice in _cards)
            {
                choice.destroy();
            }
            _cards.Clear();
        }
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _closeButton.onClick.AddListener(onCloseButtonClickCallback);
        }
        #endregion
        private void onCloseButtonClickCallback()
        {
            onCloseButtonClicked?.Invoke();
        }
        #endregion

        #region 事件
        public Action onCloseButtonClicked;
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
